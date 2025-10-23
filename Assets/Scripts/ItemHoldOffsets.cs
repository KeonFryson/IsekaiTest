using UnityEngine;

/// <summary>
/// Attach to pickup objects to specify per-item hold offsets used by Player when the item is held.
/// </summary>
public class ItemHoldOffsets : MonoBehaviour
{
    [Tooltip("Local position inside the hand hold point when held")]
    public Vector3 holdLocalPositionOffset = Vector3.zero;
    [Tooltip("Local Euler rotation (degrees) inside the hand hold point when held")]
    public Vector3 holdLocalRotationOffset = Vector3.zero;
}