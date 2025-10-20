using UnityEngine;
using UnityEngine.Animations.Rigging;

public class ArmIKController : MonoBehaviour
{
    // (optional) kept for inspector reference but not used to move hand anymore
    public Transform rightHandTarget;

    // Rig component controlling the hand IK (assign in inspector)
    public Rig rig;

    [Tooltip("Weight value when holding an item")]
    public float holdWeight = 1f;
    [Tooltip("Weight value when not holding")]
    public float restWeight = 0f;
    [Tooltip("Lerp speed for weight changes")]
    public float smooth = 5f;

    private bool isHolding;

    void Update()
    {
        if (rig == null) return;

        float target = isHolding ? holdWeight : restWeight;
        rig.weight = Mathf.Lerp(rig.weight, target, Time.deltaTime * smooth);
    }

    // Call this from Player when an item is picked/dropped
    public void SetHolding(bool holding)
    {
        isHolding = holding;
    }
}