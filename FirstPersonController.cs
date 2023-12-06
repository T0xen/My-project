using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// based on https://www.youtube.com/playlist?list=PLfhbBaEcybmgidDH3RX_qzFM0mIxWJa21
// Anything not based on a tutorial will say so, otherwise it is based on ^ or the one linked to it
/// <summary>
/// Handles everything the player does by itself
/// </summary>
public class FirstPersonController : MonoBehaviour
{
    // Conditionals for all movement related actions
    public bool CanMove { get; private set; } = true;
    private bool IsSprinting => canSprint && Input.GetKey(sprintKey);
    private bool ShouldJump => Input.GetKeyDown(jumpKey) && characterController.isGrounded;
    private bool ShouldCrouch => Input.GetKeyDown(crouchKey) && !duringCrouchAnimation && characterController.isGrounded;

    // Headers describes the general purpose of the variables
    [Header("Functional Options")]
    [SerializeField] private bool canSprint = true;
    [SerializeField] private bool canJump = true;
    [SerializeField] private bool canCrouch = true;
    [SerializeField] private bool canUseHeadBob = true;
    [SerializeField] private bool canZoom = true;
    [SerializeField] private bool canInteract = true;
    [SerializeField] private bool useFootsteps = true;
    [SerializeField] private bool useStamina = true;
    [SerializeField] private bool canUseFlashlight = true;
    

    [Header("Controls")]
    [SerializeField] private KeyCode sprintKey = KeyCode.LeftShift;
    [SerializeField] private KeyCode jumpKey = KeyCode.Space;
    [SerializeField] private KeyCode crouchKey = KeyCode.LeftControl;
    [SerializeField] private KeyCode zoomKey = KeyCode.Mouse1;
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private KeyCode flashlightKey = KeyCode.Mouse0;
    [SerializeField] private KeyCode reloadKey = KeyCode.R;

    [Header("Movement Parameters")]
    [SerializeField] private float walkSpeed = 4f;
    [SerializeField] private float sprintSpeed = 8f;
    [SerializeField] private float crouchSpeed = 2f;

    [Header("Look Parameters")]
    [SerializeField, Range(1,10)] private float lookSpeedX = 2.0f;
    [SerializeField, Range(1, 10)] private float lookSpeedY = 2.0f;
    [SerializeField, Range(1, 180)] private float upperLookLimit = 80.0f; // Value is a degree and will be clamped with lowerLookLimit
    [SerializeField, Range(1, 180)] private float lowerLookLimit = 80.0f;

    [Header("Health Parameters")]
    [SerializeField] private float maxHealth = 100;
    [SerializeField] private float timeBeforeHealthRegen = 5;
    [SerializeField] private float healthValueIncrement = 1;
    [SerializeField] private float healthTimeIncrement = .2f;
    private float currentHealth;
    private Coroutine regeneratingHealth;
    public static Action<float> OnTakeDamage;
    public static Action<float> OnDamage;
    public static Action<float> OnHeal;

    [Header("Stamina Parameters")]
    [SerializeField] private float maxStamina = 100;
    [SerializeField] private float staminaUseMultiplier = 5;
    [SerializeField] private float timeBeforeStaminaRegen = 4;
    [SerializeField] private float staminaValueIncrement = 2;
    [SerializeField] private float staminaTimeIncrement = .1f;
    private float currentStamina;
    private Coroutine regeneratingStamina;
    public static Action<float> OnStaminaChange;

    [Header("Jumping Parameters")]
    [SerializeField] private float jumpForce = 8f;
    [SerializeField] private float gravity = 30.0f;

    [Header("Crouch Parameters")]
    [SerializeField] private float crouchHeight = .5f;
    [SerializeField] private float standingHeight = 2.0f;
    [SerializeField] private float timeToCrouch = .25f;
    [SerializeField] private Vector3 crouchingCenter = new Vector3(-0.740389f, -0.5925f, -0.25f);
    [SerializeField] private Vector3 standingCenter = new Vector3(-0.740389f, -1.185f, -0.25f); // because of the model used, the center of the capsule collider had to be moved
    private bool isCrouching;
    private bool duringCrouchAnimation;

    [Header("Headbob Parameters")]
    [SerializeField] private float walkBobSpeed = 14f;
    [SerializeField] private float walkBobAmount = .05f;
    [SerializeField] private float sprintBobSpeed = 18f;
    [SerializeField] private float sprintBobAmount = .1f;
    [SerializeField] private float crouchBobSpeed = 8f;
    [SerializeField] private float crouchBobAmount = .025f;
    private float defaultYPos = 0;
    private float timer;

    [Header("Zoom Parameters")]
    [SerializeField] private float timeToZoom = .3f;
    [SerializeField] private float zoomFOV = 30f;
    private float defaultFOV;
    private Coroutine zoomRoutine;

    [Header("Footstep Parameters")]
    [SerializeField] private float baseStepSpeed = .5f;
    [SerializeField] private float crouchStepMultiplier = 1.5f;
    [SerializeField] private float sprintStepMultiplier = .6f;
    [SerializeField] private AudioSource footstepAudioSource = default;
    [SerializeField] private AudioClip[] woodClips = default;
    [SerializeField] private AudioClip[] tileClips = default;
    [SerializeField] private AudioClip[] concreteClips = default;
    private float footstepTimer = 0;
    private float GetCurrentOffset => isCrouching ? (baseStepSpeed * crouchStepMultiplier) : IsSprinting ? (baseStepSpeed * sprintStepMultiplier) : baseStepSpeed;

    [Header("Interaction")]
    [SerializeField] private Vector3 interactionRayPoint = default;
    [SerializeField] private float interactionDistance = default;
    [SerializeField] private LayerMask interactionLayer = default;
    private Interactable currentInteractable;

    // First 2 properties and the basic functionality of the flashlight is from the script included with the Rusty Flashlight Asset
    // Power useage is added by me, not taken from a tutorial
    [Header("Flashlight")]
    public GameObject lightGO; //light gameObject to work with
    private bool isOn = false;
    [SerializeField] private float flashlightBatteryDuration = 100;
    [SerializeField] private float batteryDrain = 2;
    private float currentBatteryDuration;
    public static Action<float> OnPowerChange;

    private Camera playerCamera;
    private CharacterController characterController;

    private Vector3 moveDirection;
    private Vector2 currentInput;

    // Used to store mouse input
    private float rotationX = 0;

    // Allows Door to get FirstPersonController
    public static FirstPersonController instance;

    // Allows for the access of batteriesInHand from ItemPickup
    public static ItemPickup itemPickup;

    /// <summary>
    /// Reserved method that has ApplyDamage get called whenever the action OnTakeDamage occurs
    /// </summary>
    private void OnEnable()
    {
        OnTakeDamage += ApplyDamage;
    }

    /// <summary>
    /// Whenever OnTakeDamage is no longer being referenced, unsubscribe from ApplyDamage to get rid of the reference
    /// </summary>
    private void OnDisable()
    {
        OnTakeDamage -= ApplyDamage;
    }

    /// <summary>
    /// Gets the camera, character controller, default camera position, and default FOV. 
    /// Sets health and stamina to their max values. Also, locks the cursor to the screen 
    /// and hides it when the game is active.
    /// </summary>
    void Awake()
    {
        instance = this;

        itemPickup = new ItemPickup();

        playerCamera = GetComponentInChildren<Camera>();
        characterController = GetComponentInChildren<CharacterController>();
        defaultYPos = playerCamera.transform.localPosition.y; // Used for head bobs
        defaultFOV = playerCamera.fieldOfView;
        currentHealth = maxHealth;
        currentStamina = maxStamina;
        currentBatteryDuration = flashlightBatteryDuration;
        lightGO.SetActive(isOn); //sets flashlight to off
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    /// <summary>
    /// Calls all of the functions to handle any player interaction with the game once per frame
    /// </summary>
    void Update()
    {
        if(CanMove)
        {
            HandleMovementInput();
            HandleMouseLook();

            if (canJump) HandleJump();

            if (canCrouch) HandleCrouch();

            if (canUseHeadBob) HandleHeadBob();

            if (canZoom) HandleZoom();

            if(canInteract)
            {
                HandleInteractionCheck();
                HandleInteractionInput();
            }

            if (useFootsteps) HandleFootsteps();

            if (useStamina) HandleStamina();

            // So without it coded like this, the amount that the battery would go down was literal ten-thousandths of a sec,
            // or it would count down even when the flashlight was off (this logic can probably be cleaned up but idrc atm)
            if(canUseFlashlight)
            {
                HandleFlashlight();
                if (canUseFlashlight && isOn)
                {
                    currentBatteryDuration -= batteryDrain * Time.deltaTime;
                    OnPowerChange?.Invoke(currentBatteryDuration);
                    if (currentBatteryDuration <= 0)
                    {
                        currentBatteryDuration = 0;
                        canUseFlashlight = false;
                        lightGO.SetActive(false);
                    }
                }
            }

            try // This was actually useful
            {
                // Code does not work as a byproduct of the Queue being messed up
                if (itemPickup.batteriesInHand.Count > 0 && Input.GetKeyDown(reloadKey))
                {
                    currentBatteryDuration = itemPickup.batteriesInHand.Dequeue();

                    ItemPickup.OnBatteryPickup?.Invoke(itemPickup.batteriesInHand.Count);
                    OnPowerChange?.Invoke(currentBatteryDuration);
                }
            }
            catch (NullReferenceException)
            {
                print("NullReferenceException -> itemPickup is not properly instanced");
            }
            

            ApplyFinalMovements();
        }
    }

    /// <summary>
    /// Determines the speed of the user based on their inputs
    /// </summary>
    private void HandleMovementInput()
    {
        currentInput = new Vector2((isCrouching ? crouchSpeed : IsSprinting ? sprintSpeed : walkSpeed) * Input.GetAxis("Vertical"), walkSpeed * Input.GetAxis("Horizontal"));

        float moveDirectionY = moveDirection.y;
        moveDirection = (transform.TransformDirection(Vector3.forward) * currentInput.x) + (transform.TransformDirection(Vector3.right) *  currentInput.y);
        moveDirection.y = moveDirectionY;
    }

    /// <summary>
    /// Allows the user to look around with the mouse and prevents them from looking above or below a certain degree
    /// </summary>
    private void HandleMouseLook()
    {
        rotationX -= Input.GetAxis("Mouse Y") * lookSpeedY;
        rotationX = Mathf.Clamp(rotationX, -upperLookLimit, lowerLookLimit);
        playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
        transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeedX, 0);
    }

    /// <summary>
    /// Sets the players vertical movement equal to jumpForce if they can jump
    /// </summary>
    private void HandleJump()
    {
        if (ShouldJump) moveDirection.y = jumpForce;
    }

    /// <summary>
    /// Has the player crouch/uncrouch if they are pressing the crouch input and have height clearance.
    /// </summary>
    private void HandleCrouch()
    {
        if (ShouldCrouch) StartCoroutine(CrouchStand()); // Coroutine apparently allows for execution to be suspended and resumed
    }

    /// <summary>
    /// If the player is not jumping and is moving, then the camera will bob up and down, with the
    /// height and speed of the bob determined on the stance and movement speed of the player.
    /// </summary>
    private void HandleHeadBob()
    {
        if (!characterController.isGrounded) return;

        if(Mathf.Abs(moveDirection.x) > .1f || Mathf.Abs(moveDirection.z) > .1f)
        {
            timer += Time.deltaTime * (isCrouching ? crouchBobSpeed : IsSprinting ? sprintBobSpeed : walkBobSpeed); // If crouching -> crouch bob speed, else if sprinting -> sprint bob speed, else walk bob speed
            playerCamera.transform.localPosition = new Vector3(
                playerCamera.transform.localPosition.x,
                defaultYPos + Mathf.Sin(timer) * (isCrouching ? crouchBobAmount : IsSprinting ? sprintBobAmount : walkBobAmount),
                playerCamera.transform.localPosition.z);
        }
    }

    /// <summary>
    /// If the player is sprinting and moving, stop regenerating stamina and begin using stamina.
    /// Once the player runs out of stamina, they can no longer sprint. Also, if the player is not sprinting,
    /// does not have max stamina, and are not actively regenerating stamina, then they will begin reginning stamina.
    /// </summary>
    private void HandleStamina()
    {
        if(IsSprinting && currentInput != Vector2.zero)
        {
            if(regeneratingStamina != null)
            {
                StopCoroutine(regeneratingStamina);
                regeneratingStamina = null;
            } 
            currentStamina -= staminaUseMultiplier * Time.deltaTime;

            if(currentStamina < 0) currentStamina = 0;

            OnStaminaChange?.Invoke(currentStamina);

            if (currentStamina <= 0) canSprint = false;
        }

        if (!IsSprinting && currentStamina < maxStamina && regeneratingStamina == null) regeneratingStamina = StartCoroutine(RegenerateStamina());
    }

    /// <summary>
    /// If the player is pressing the assigned zoom key, then start zooming in. If they let go, then start zooming out
    /// </summary>
    private void HandleZoom()
    {
        if(Input.GetKeyDown(zoomKey))
        {
            if(zoomRoutine != null)
            {
                StopCoroutine(zoomRoutine);
                zoomRoutine = null;
            }
            zoomRoutine = StartCoroutine(ToggleZoom(true));
        }

        if (Input.GetKeyUp(zoomKey))
        {
            if (zoomRoutine != null)
            {
                StopCoroutine(zoomRoutine);
                zoomRoutine = null;
            }
            zoomRoutine = StartCoroutine(ToggleZoom(false));
        }
    }

    /// <summary>
    /// If the player is looking at an interactable and is close enough to it interact with it, then they can interact with an 
    /// interactable object if they are not currently interacting with any object, including the one they are about to interact with
    /// </summary>
    private void HandleInteractionCheck()
    {
        if(Physics.Raycast(playerCamera.ViewportPointToRay(interactionRayPoint), out RaycastHit hit, interactionDistance))
        {
            if(hit.collider.gameObject.layer == 9 && (currentInteractable == null || hit.collider.gameObject.GetInstanceID() != currentInteractable.gameObject.GetInstanceID()))
            {
                hit.collider.TryGetComponent(out currentInteractable);

                if (currentInteractable) currentInteractable.OnFocus();
            }
        }
        else if (currentInteractable)
        {
            currentInteractable.OnLoseFocus();
            currentInteractable = null;
        }
    }
    
    /// <summary>
    /// If the player is: 
    /// 1. Pressing the interact key 2. They do not currently have an interactable object
    /// 3. They are close enough to interact with the object 4.They are looking at an object that is tagged as interactable
    /// Then interact with the object
    /// </summary>
    private void HandleInteractionInput()
    {
        if(Input.GetKeyDown(interactKey) && currentInteractable != null && 
            Physics.Raycast(playerCamera.ViewportPointToRay(interactionRayPoint), out RaycastHit hit, interactionDistance, interactionLayer))
        {
            currentInteractable.OnInteract();
        }
    }

    /// <summary>
    /// If the player is not jumping and is moving, then play a sound for each footstep based on 
    /// the material of the ground beneath them and how fast they are moving.
    /// </summary>
    private void HandleFootsteps()
    {
        if (!characterController.isGrounded) return;
        if (currentInput == Vector2.zero) return;

        footstepTimer -= Time.deltaTime;

        if(footstepTimer <= 0)
        {
            if(Physics.Raycast(playerCamera.transform.position, Vector3.down, out RaycastHit hit, 3))
            {
                switch (hit.collider.tag)
                {
                    case "Footsteps/TILE": // Specifies UnityEngine.Random b/c when Actions were added, it added System and counted these Random's as ambigous
                        footstepAudioSource.PlayOneShot(tileClips[UnityEngine.Random.Range(0, tileClips.Length - 1)]);
                        break;
                    case "Footsteps/WOOD":
                        footstepAudioSource.PlayOneShot(woodClips[UnityEngine.Random.Range(0, woodClips.Length - 1)]);
                        break;
                    case "Footsteps/CONCRETE":
                        footstepAudioSource.PlayOneShot(concreteClips[UnityEngine.Random.Range(0, concreteClips.Length - 1)]);
                        break;
                    default: // Tile is the most common floor material so it makes sense to have it as the default
                        footstepAudioSource.PlayOneShot(tileClips[UnityEngine.Random.Range(0, tileClips.Length - 1)]);
                        break;
                }
            }
            footstepTimer = GetCurrentOffset;
        }
    }

    /// <summary>
    /// If the player presses the flashlight key, then the flashlight turns on, and drains battery
    /// Parts unrelated to power are from rusty flashlight asset script
    /// </summary>
    private void HandleFlashlight()
    {
        //toggle flashlight on key down
        if (Input.GetKeyDown(flashlightKey)) // By default was X but I changed it to Left Click
        {
            isOn = !isOn; //toggles light

            if (isOn && currentBatteryDuration > 0)
            {
                lightGO.SetActive(true);

                OnPowerChange?.Invoke(currentBatteryDuration);

                if (currentBatteryDuration <= 0)
                {
                    currentBatteryDuration = 0;
                    canUseFlashlight = false;
                    lightGO.SetActive(false);
                }
            }
            else
            {
                lightGO.SetActive(false);
            }

            OnPowerChange?.Invoke(currentBatteryDuration);
        }
    }

    /// <summary>
    /// Reduces the players health by the damage passed in, and cancels health regen if it active or resets the timer if the player is not actively regenerating health
    /// Also, will kill the player if their health reaches 0.
    /// </summary>
    /// <param name="dmg">The amount of damage taken</param>
    private void ApplyDamage(float dmg)
    {
        currentHealth -= dmg;
        OnDamage?.Invoke(currentHealth);

        if (currentHealth <= 0) KillPlayer();
        else if (regeneratingHealth != null) StopCoroutine(regeneratingHealth);

        regeneratingHealth = StartCoroutine(RegenerateHealth());
    }

    /// <summary>
    /// Stops the regeneratingHealth coroutine and will trigger an on-death event
    /// </summary>
    private void KillPlayer()
    {
        currentHealth = 0;

        if (regeneratingHealth != null) StopCoroutine(regeneratingHealth);

        print("Dead"); // Update to be a YOU DIED screen
    }

    /// <summary>
    /// Allows user to jump if grounded and affects the users movement based on time instead of frames
    /// </summary>
    private void ApplyFinalMovements()
    {
        if(!characterController.isGrounded) moveDirection.y -= gravity * Time.deltaTime;

        characterController.Move(moveDirection * Time.deltaTime);
    }

    /// <summary>
    /// Compares the users height and center to the targets for both of those, and interpolates it by the time it is taking to crouch
    /// </summary>
    /// <returns>Only returns null</returns>
    private IEnumerator CrouchStand()
    {
        // If the ceiling is too close to the character, then it will prevent the player from uncrouching
        if (isCrouching && Physics.Raycast(playerCamera.transform.position, Vector3.up, 1f)) yield break; 

        duringCrouchAnimation = true;

        float timeElapsed = 0;
        float targetHeight = isCrouching ? standingHeight : crouchHeight;
        float currentHeight = characterController.height;
        Vector3 targetCenter = isCrouching ? standingCenter : crouchingCenter;
        Vector3 currentCenter = characterController.center;

        while(timeElapsed < timeToCrouch)
        {
            characterController.height = Mathf.Lerp(currentHeight, targetHeight, timeElapsed/timeToCrouch);
            characterController.center = Vector3.Lerp(currentCenter, targetCenter, timeElapsed / timeToCrouch);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        // This is done if there is a case where the height does not reach the desired height due to a slight mismatch in timing during the while loop
        characterController.height = targetHeight;
        characterController.center = targetCenter;

        isCrouching = !isCrouching;

        duringCrouchAnimation = false;
    }

    /// <summary>
    /// Interpolates the users FOV from the current FOV state to the desired FOV state by the time it is taking to zoom in
    /// </summary>
    /// <param name="isEnter">If true, the player is zoomed in, if false then the player is zoomed out</param>
    /// <returns></returns>
    private IEnumerator ToggleZoom(bool isEnter)
    {
        float targetFOV = isEnter ? zoomFOV : defaultFOV;
        float startingFOV = playerCamera.fieldOfView;
        float timeElapsed = 0;

        while (timeElapsed < timeToZoom)
        {
            playerCamera.fieldOfView = Mathf.Lerp(startingFOV, targetFOV, timeElapsed/timeToZoom);
            timeElapsed += Time.deltaTime;
            yield return null; // done to wait for the next frame
        }

        playerCamera.fieldOfView = targetFOV; // This is done if there is a case where the FOV does not reach the desired FOV due to a slight mismatch in timing during the while loop
        zoomRoutine = null;
    }

    /// <summary>
    /// When the player is able to regen health, they will regen health based on the time and health increments until they reach max health
    /// </summary>
    /// <returns>If the player is not able to regen health yet, then it returns the time till they can regen health. 
    /// Otherwise it returns the healthTimeIncrement value </returns>
    private IEnumerator RegenerateHealth()
    {
        yield return new WaitForSeconds(timeBeforeHealthRegen);
        WaitForSeconds timeToWait = new WaitForSeconds(healthTimeIncrement);

        while(currentHealth < maxHealth)
        {
            currentHealth += healthValueIncrement;

            if (currentHealth > maxHealth) currentHealth = maxHealth;

            OnHeal?.Invoke(currentHealth);

            yield return timeToWait;
        }

        regeneratingHealth = null;
    }

    /// <summary>
    /// Once the time before stamina regen begins has passed, the user will begin regenerating stamina based on the time and value increments
    /// until they reach max stamina. As long as there is at least 1 stamina, the user can sprint
    /// </summary>
    /// <returns>If the player is not able to regen stamina yet, then it returns the time till they can regen health. 
    /// Otherwise it returns the staminaTimeIncrement value</returns>
    private IEnumerator RegenerateStamina()
    {
        yield return new WaitForSeconds(timeBeforeStaminaRegen);
        WaitForSeconds timeToWait = new WaitForSeconds(staminaTimeIncrement);

        while(currentStamina < maxStamina)
        {
            if (currentStamina > 0) canSprint = true;

            currentStamina += staminaValueIncrement;

            if(currentStamina > maxStamina) currentStamina = maxStamina;

            OnStaminaChange?.Invoke(currentStamina);

            yield return timeToWait;
        }

        regeneratingStamina = null;
    }
}
