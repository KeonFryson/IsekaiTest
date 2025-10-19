using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class RuneManager : MonoBehaviour
{
    public Camera playerCamera;
    public RuneData[] runeTypes;
    public float placementRange = 10f;

    private int selectedRune = 0;

    private InputSystem_Actions inputActions;

    void Awake()
    {
        inputActions = new InputSystem_Actions();

        // Use the "Attack" action to place runes (mapped to mouse left / gamepad / touch in the generated asset)
        inputActions.Player.Attack.performed += ctx => PlaceRune();
    }

    void OnEnable()
    {
        inputActions?.Enable();
    }

    void OnDisable()
    {
        inputActions?.Disable();
    }

    void OnDestroy()
    {
        // Dispose the generated asset when this object is destroyed
        inputActions?.Dispose();
    }

    void Update()
    {
        // Switch rune type with number keys (1–9) using the new Input System
        if (Keyboard.current != null)
        {
            int maxKeys = Mathf.Min(runeTypes.Length, 9);
            for (int i = 0; i < maxKeys; i++)
            {
                Key key = Key.Digit1 + i;
                var keyControl = Keyboard.current[key];
                if (keyControl != null && keyControl.wasPressedThisFrame)
                {
                    selectedRune = i;
                    Debug.Log($"Selected Rune: {runeTypes[selectedRune].runeName}");
                }
            }
        }
    }

    void PlaceRune()
    {
        if (playerCamera == null) return;

        // Get mouse/touch position via the new Input System if available, otherwise fall back to (0,0)
        Vector2 screenPos = Vector2.zero;
        if (Mouse.current != null)
            screenPos = Mouse.current.position.ReadValue();
        else if (Pointer.current != null)
            screenPos = Pointer.current.position.ReadValue();

        Ray ray = playerCamera.ScreenPointToRay(screenPos);
        if (Physics.Raycast(ray, out RaycastHit hit, placementRange))
        {
            if (hit.collider.CompareTag("Ground"))
            {
                RuneData rune = runeTypes.Length > 0 ? runeTypes[selectedRune] : null;
                if (rune == null || rune.runePrefab == null) return;

                Quaternion rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
                // Instantiate first
                GameObject newRune = Instantiate(rune.runePrefab, hit.point, rotation);

                // Apply a small offset along the hit normal to avoid z-fighting with the terrain/mesh
                // Note: RuneData has the field "postionOffset" (misspelled in the ScriptableObject) — use that value.
                newRune.transform.position = hit.point + hit.normal * rune.postionOffset;

                // Disable shadow casting / receiving on the rune renderers to avoid dark artifacts on ground
                var renderers = newRune.GetComponentsInChildren<Renderer>();
                foreach (var r in renderers)
                {
                    r.shadowCastingMode = ShadowCastingMode.Off;
                    r.receiveShadows = false;
                }

                RuneBehavior behavior = newRune.AddComponent<RuneBehavior>();
                behavior.runeData = rune;
            }
        }
    }
}