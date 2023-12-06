using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

// based on https://www.youtube.com/watch?v=HMAs9_2yTuo&list=PLfhbBaEcybmgidDH3RX_qzFM0mIxWJa21&index=10
// and https://www.youtube.com/watch?v=Ps3Rti-N5T4&list=PLfhbBaEcybmgidDH3RX_qzFM0mIxWJa21&index=11

/// <summary>
/// Updates text to display to the UI
/// </summary>
public class UI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI healthText = default;
    [SerializeField] private TextMeshProUGUI staminaText = default;
    [SerializeField] private TextMeshProUGUI powerText = default;
    [SerializeField] private TextMeshProUGUI batteryText = default;

    /// <summary>
    /// Subscribes to the actions to update text
    /// </summary>
    private void OnEnable()
    {
        FirstPersonController.OnDamage += UpdateHealth;
        FirstPersonController.OnHeal += UpdateHealth;
        FirstPersonController.OnStaminaChange += UpdateStamina;
        FirstPersonController.OnPowerChange += UpdatePower;
        ItemPickup.OnBatteryPickup += UpdateBatteries;
    }

    /// <summary>
    /// Unsubscribes from the actions if the reference ends
    /// </summary>
    private void OnDisable()
    {
        FirstPersonController.OnDamage -= UpdateHealth;
        FirstPersonController.OnHeal -= UpdateHealth;
        FirstPersonController.OnStaminaChange -= UpdateStamina;
        FirstPersonController.OnPowerChange -= UpdatePower;
        ItemPickup.OnBatteryPickup -= UpdateBatteries;
    }

    /// <summary>
    /// Gets the starting values for the text
    /// </summary>
    private void Start()
    {
        UpdateHealth(100);
        UpdateStamina(100);
        UpdatePower(100);
        UpdateBatteries(0);
    }

    /// <summary>
    /// Updates the UI text for health to be the player's current health 
    /// </summary>
    /// <param name="currentHealth">Player's current health</param>
    private void UpdateHealth(float currentHealth)
    {
        healthText.text = currentHealth.ToString("00");
    }

    /// <summary>
    /// Updates the UI text for stamina to be the player's current stamina
    /// </summary>
    /// <param name="currentStamina">Player's current stamina</param>
    private void UpdateStamina(float currentStamina)
    {
        staminaText.text = currentStamina.ToString("00");
    }

    /// <summary>
    /// Updates the UI text for stamina to be the player's current flashlight power
    /// </summary>
    /// <param name="currentPower">Current amount of flashlight power</param>
    private void UpdatePower(float currentPower)
    {
        powerText.text = currentPower.ToString("00");
    }

    /// <summary>
    /// Updates the UI text for batteries held to be the player's current batteries held
    /// </summary>
    /// <param name="batteriesHeld">Current number of batteries held</param>
    private void UpdateBatteries(int batteriesHeld)
    {
        batteryText.text = "Batteries: " + batteriesHeld.ToString();
    }
}
