using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Handles all player actions with interactable items (only batteries atm)
/// </summary>
public class ItemPickup : Interactable
{
    private bool canBeInteractedWith = true; 
    [SerializeField] private TextMeshProUGUI interactableText = default;
    [SerializeField] private KeyCode interactKey = default;
    public Queue<float> batteriesInHand = new Queue<float>();
    public static Action<int> OnBatteryPickup;

    // Start is called before the first frame update
    void Start()
    {
        interactableText.enabled = false;
        interactableText.text = $"Press {interactKey} to pick up";
    }

    /// <summary>
    /// Enables text telling the player that they can pick up an item
    /// </summary>
    public override void OnFocus()
    {
        interactableText.enabled = true;
    }

    /// <summary>
    /// If the player can interact with the item, and the player is not holding max batteries,
    /// then it will enqueue a float with a value of 100, update the number of batteries held,
    /// and destroy the item. Also, the interact text will be disabled again in case it is showing.
    /// </summary>
    public override void OnInteract()
    {
        if (canBeInteractedWith && GameObject.FindWithTag("Item"))
        {
            batteriesInHand.Enqueue(100f);
            
            print(batteriesInHand.Count);

            OnBatteryPickup?.Invoke(batteriesInHand.Count);

            Destroy(gameObject);
            interactableText.enabled = false;
        }
    }

    /// <summary>
    /// Disables text telling the player that they can pick up an item
    /// </summary>
    public override void OnLoseFocus()
    {
        interactableText.enabled = false;
    }
}
