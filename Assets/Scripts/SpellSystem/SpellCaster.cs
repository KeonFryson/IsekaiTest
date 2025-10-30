
using UnityEngine;
using UnityEngine.InputSystem;

public class SpellCaster : MonoBehaviour
{
    public SpellDefinition spell;
    public Transform castOrigin;
    [Tooltip("This component now uses the Player/Attack action to trigger casting.")]
    public bool debugExpose = false; // kept only to show a tooltip in inspector if needed

    private InputSystem_Actions inputActions;
    private InputSystem_Actions.PlayerActions playerActions;

    void Awake()
    {
        inputActions = new InputSystem_Actions();
        playerActions = inputActions.Player;
    }

    void OnEnable()
    {
        inputActions?.Enable();
        playerActions.Attack.performed += OnCastPerformed;
    }

    void OnDisable()
    {
        playerActions.Attack.performed -= OnCastPerformed;
        inputActions?.Disable();
    }

    private void OnCastPerformed(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed || spell == null) return;
        Cast();
    }

    // Call this from other code to cast programmatically
    public void Cast()
    {
        Transform origin = castOrigin != null ? castOrigin : transform;
        spell.Cast(transform, origin.position, origin.forward);
    }

    public void CastTowards(Vector3 worldDirection)
    {
        Transform origin = castOrigin != null ? castOrigin : transform;
        spell.Cast(transform, origin.position, worldDirection.normalized);
    }

    void OnDestroy()
    {
        inputActions?.Dispose();
    }
}