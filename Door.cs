using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;

// based on: https://www.youtube.com/watch?v=6RLmvaPfLdA&list=PLfhbBaEcybmgidDH3RX_qzFM0mIxWJa21&index=12
/// <summary>
/// Allows the player to open most doors and will display text on the screen when they can interact with a door
/// </summary>
public class Door : Interactable
{
    private bool isOpen = false;
    private bool canBeInteractedWith = true;
    private Animator anim;
    [SerializeField] private TextMeshProUGUI interactableText = default;
    [SerializeField] private KeyCode interactKey = default;

    /// <summary>
    /// Gets the animator for the doors, hides the interactable text and sets it as well
    /// </summary>
    private void Start()
    {
        anim = GetComponent<Animator>();
        interactableText.enabled = false;
        interactableText.text = $"Press {interactKey} to interact";
    }

    /// <summary>
    /// Unhides the interactable text
    /// </summary>
    public override void OnFocus()
    {
        interactableText.enabled = true;
    }

    /// <summary>
    /// Sets isOpen to the opposite value, then gets the direction of the door and the direction of the player.
    /// Then gets the dot value, and if it is positive, then the door opens inward, if it's negative then it opens outwards.
    /// </summary>
    public override void OnInteract()
    {
        if (canBeInteractedWith)
        {
            isOpen = !isOpen;

            Vector3 doorTransformDirection = transform.TransformDirection(Vector3.forward);
            Vector3 playerTransformDirection = FirstPersonController.instance.transform.position - transform.position;
            float dot = Vector3.Dot(doorTransformDirection, playerTransformDirection);

            anim.SetFloat("dot", dot);
            anim.SetBool("isOpen", isOpen);
        }
    }

    /// <summary>
    /// Hides the interactable text
    /// </summary>
    public override void OnLoseFocus()
    {
        interactableText.enabled = false;
    }
}
