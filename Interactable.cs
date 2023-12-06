using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// based on https://www.youtube.com/watch?v=AQc-NM2Up3M&list=PLfhbBaEcybmgidDH3RX_qzFM0mIxWJa21&index=8
/// <summary>
/// Allows for the use of functions for when a player interacts with an object, looks at an interactable object, or looks away from an interactable object
/// </summary>
public abstract class Interactable : MonoBehaviour
{
    public virtual void Awake()
    {
        gameObject.layer = 9; // The interactable layer in Unity is layer 9, this is done as a cautionary measure to insure the object is interactable
    }

    /// <summary>
    /// When the player interacts with an interactble object
    /// </summary>
    public abstract void OnInteract();

    /// <summary>
    /// When a player looks at an interactable object
    /// </summary>
    public abstract void OnFocus();

    /// <summary>
    /// When a player stops looking at an interactable object
    /// </summary>
    public abstract void OnLoseFocus();
}