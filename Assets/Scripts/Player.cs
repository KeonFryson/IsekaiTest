using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class Player : MonoBehaviour
{
    public Camera playerCamera;
    public float walkSpeed = 5f;
    public float runSpeed = 10f;
    public float jumpPower = 7f;
    public float gravity = 10f;
    public float lookSpeed = 2f;
    public float lookXLimit = 45f;
    public float defaultHeight = 2f;
    public float crouchHeight = 1f;
    public float crouchSpeed = 3f;
    public float interactDistance = 5f;

    // Where picked objects are parented (assign in inspector). If left null we'll try to resolve a hand bone at Start.
    public Transform rightHandHoldPoint;
    // Optional default offsets for fine tuning the held object's position & rotation in hand (used when the item doesn't provide its own offsets)
    public Vector3 holdLocalPositionOffset = Vector3.zero;
    public Vector3 holdLocalRotationOffset = Vector3.zero;
    // Force applied to dropped objects (throw)
    public float dropForwardForce = 2f;

    // Arm IK controller (optional) — assign the ArmIKController that controls hand lift/IK
    public ArmIKController armIKController;

    private Vector3 moveDirection = Vector3.zero;
    private float rotationX = 0;
    private float rotationY = 0;
    private CharacterController characterController;
    private Animator anim; // << added
    private bool canMove = true;

    private InputSystem_Actions inputActions;
    private InputSystem_Actions.PlayerActions playerActions;
    private Vector2 moveInput;
    private Vector2 lookInput;
    private bool isRunning;
    private bool isJumping;
    private bool isCrouching;

    /// <summary>
    /// Mana 
    /// </summary>
    public float Mana;
    public float MaxMana = 100;

    public TMP_Text manaText;

    // ---- Pickup state ----
    private GameObject heldObject;
    private Rigidbody heldRb;
    private Transform heldOriginalParent;
    private bool heldOriginalKinematic;
    private bool heldOriginalUseGravity;

    // Per-held-item offsets (populated when picking up an item)
    private Vector3 heldItemLocalPositionOffset = Vector3.zero;
    private Vector3 heldItemLocalRotationOffset = Vector3.zero;

    private ItemHoldOffsets itemHoldOffsets;

    void Awake()
    {
        Mana = MaxMana;

        inputActions = new InputSystem_Actions();
        playerActions = inputActions.Player;

        playerActions.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        playerActions.Move.canceled += ctx => moveInput = Vector2.zero;
        playerActions.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
        playerActions.Look.canceled += ctx => lookInput = Vector2.zero;
        playerActions.Sprint.performed += ctx => isRunning = ctx.ReadValueAsButton();
        playerActions.Sprint.canceled += ctx => isRunning = false;
        playerActions.Jump.performed += ctx => isJumping = ctx.ReadValueAsButton();
        playerActions.Jump.canceled += ctx => isJumping = false;
        playerActions.Crouch.performed += ctx => isCrouching = ctx.ReadValueAsButton();
        playerActions.Crouch.canceled += ctx => isCrouching = false;

        // Interact: press to pick up / drop
        playerActions.Interact.performed += ctx => OnInteractPerformed();
    }

    void OnEnable()
    {
        inputActions.Enable();
    }

    void OnDisable()
    {
        inputActions.Disable();
    }

    void Start()
    {
        if (manaText)
            manaText.text = "Mana: " + Mana.ToString("0") + " / " + MaxMana.ToString("0");

        characterController = GetComponent<CharacterController>();
        anim = GetComponentInChildren<Animator>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // initialize rotations from current transform so we don't snap on start
        rotationY = transform.eulerAngles.y;
        if (playerCamera != null)
            rotationX = playerCamera.transform.localEulerAngles.x;
        // convert rotationX to -180..180 range if needed
        if (rotationX > 180f) rotationX -= 360f;

        // If hold point not assigned, try to resolve a right-hand bone from animator (Humanoid rigs)
        if (rightHandHoldPoint == null && anim != null)
        {
            var hand = anim.GetBoneTransform(HumanBodyBones.RightHand);
            if (hand != null)
            {
                // create a child GameObject used as exact hold point to avoid modifying bone local transforms
                GameObject holdPoint = new GameObject("HandHoldPoint");
                holdPoint.transform.SetParent(hand, false);
                // position slightly forward to sit in hand by default
                holdPoint.transform.localPosition = new Vector3(0.05f, 0f, 0f);
                rightHandHoldPoint = holdPoint.transform;
            }
        }
    }

    void Update()
    {
        if (manaText)
            manaText.text = "Mana: " + Mana.ToString("0") + " / " + MaxMana.ToString("0");


        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        float curSpeedX = canMove ? (isRunning ? runSpeed : walkSpeed) * moveInput.y : 0;
        float curSpeedY = canMove ? (isRunning ? runSpeed : walkSpeed) * moveInput.x : 0;
        float movementDirectionY = moveDirection.y;
        moveDirection = (forward * curSpeedX) + (right * curSpeedY);

        if (isJumping && canMove && characterController.isGrounded)
            moveDirection.y = jumpPower;
        else
            moveDirection.y = movementDirectionY;

        if (!characterController.isGrounded)
            moveDirection.y -= gravity * Time.deltaTime;

        if (isCrouching && canMove)
        {
            characterController.height = crouchHeight;
            walkSpeed = crouchSpeed;
            runSpeed = crouchSpeed;
        }
        else
        {
            characterController.height = defaultHeight;
            walkSpeed = 5f;
            runSpeed = 10f;
        }

        characterController.Move(moveDirection * Time.deltaTime);

        // Update rotation values but defer applying them to LateUpdate to avoid jitter with CharacterController.Move
        if (canMove)
        {
            // Use deltaTime for smooth/framerate-independent mouse look
            rotationX += -lookInput.y * lookSpeed * Time.deltaTime;
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);

            rotationY += lookInput.x * lookSpeed * Time.deltaTime;
            // keep rotationY within 0-360 for numerical stability
            if (rotationY > 360f) rotationY -= 360f;
            else if (rotationY < 0f) rotationY += 360f;
        }

        // -------- Animation Control --------
        float speedParam = new Vector2(moveInput.x, moveInput.y).magnitude * (isRunning ? 3f : 1f);
        anim.SetFloat("Speed", speedParam);   // X-axis for forward/backward
        anim.SetFloat("Strafe", moveInput.x); // Y-axis for left/right strafing
        anim.SetBool("IsJumping", !characterController.isGrounded);
        // -----------------------------------

        // If holding an object keep it aligned (parenting already handles position, but ensure local offsets)
        if (heldObject != null && rightHandHoldPoint != null)
        {
            // Prefer per-item offsets from ItemHoldOffsets if available, otherwise use stored per-item values or player defaults.
            if (itemHoldOffsets != null)
            {
                heldObject.transform.localPosition = itemHoldOffsets.holdLocalPositionOffset;
                heldObject.transform.localRotation = Quaternion.Euler(itemHoldOffsets.holdLocalRotationOffset);
            }
            else
            {
                heldObject.transform.localPosition = heldItemLocalPositionOffset;
                heldObject.transform.localRotation = Quaternion.Euler(heldItemLocalRotationOffset);
            }
        }
    }

    // Apply camera/player rotation in LateUpdate for smoother camera following the final character position
    void LateUpdate()
    {
        if (!canMove) return;

        // Apply yaw to the player root
        transform.rotation = Quaternion.Euler(0f, rotationY, 0f);

        // Ensure camera is not null and apply pitch locally
        if (playerCamera != null)
            playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0f, 0f);
    }

    // Called when Interact action is performed: pick up or drop the nearest valid object
    private void OnInteractPerformed()
    {
        if (heldObject != null)
        {
            DropHeldObject();
            return;
        }

        TryPickup();
    }

    private void TryPickup()
    {
        if (playerCamera == null) return;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance))
        {
            // Prefer objects tagged "Pickup" or that have a Rigidbody
            var candidate = hit.collider.gameObject;
            Rigidbody rb = candidate.GetComponent<Rigidbody>();
            if (candidate.CompareTag("Pickup") || rb != null)
            {
                // pick up the root of the hit transform (if colliders are child objects)
                Transform root = candidate.transform;
                // climb to root that has Rigidbody if child collider was hit
                while (root.parent != null && root.GetComponent<Rigidbody>() == null && root != root.root)
                    root = root.parent;

                Rigidbody pickRb = root.GetComponent<Rigidbody>();
                if (pickRb == null)
                {
                    // if there is no rigidbody, try to pick the exact object hit
                    pickRb = rb;
                }

                // Determine per-item offsets (fallback to Player defaults)
                var offsetsComp = root.GetComponent<ItemHoldOffsets>();
                if (offsetsComp != null)
                {
                    heldItemLocalPositionOffset = offsetsComp.holdLocalPositionOffset;
                    heldItemLocalRotationOffset = offsetsComp.holdLocalRotationOffset;
                    itemHoldOffsets = offsetsComp; // remember the component so Update can use it directly
                }
                else
                {
                    heldItemLocalPositionOffset = holdLocalPositionOffset;
                    heldItemLocalRotationOffset = holdLocalRotationOffset;
                    itemHoldOffsets = null;
                }

                if (pickRb == null)
                {
                    // no rigidbody at all; still allow pickup by parenting the GameObject (non-physics)
                    heldObject = root.gameObject;
                    heldOriginalParent = heldObject.transform.parent;
                    heldObject.transform.SetParent(rightHandHoldPoint, true);
                    heldObject.transform.localPosition = heldItemLocalPositionOffset;
                    heldObject.transform.localRotation = Quaternion.Euler(heldItemLocalRotationOffset);
                    heldRb = null;
                }
                else
                {
                    // store original state
                    heldObject = pickRb.gameObject;
                    heldRb = pickRb;
                    heldOriginalParent = heldObject.transform.parent;
                    heldOriginalKinematic = heldRb.isKinematic;
                    heldOriginalUseGravity = heldRb.useGravity;

                    // disable physics while held
                    heldRb.isKinematic = true;
                    heldRb.useGravity = false;

                    // parent to hand hold point and align
                    heldObject.transform.SetParent(rightHandHoldPoint, true);
                    heldObject.transform.localPosition = heldItemLocalPositionOffset;
                    heldObject.transform.localRotation = Quaternion.Euler(heldItemLocalRotationOffset);
                }

                // Notify ArmIKController to lift the hand when an item is picked
                if (armIKController != null)
                {
                    // If rightHandHoldPoint exists and the ArmIKController doesn't have a target assigned,
                    // assign it so the IK controller moves the same hold point the Player is parenting objects to.
                    if (armIKController.rightHandTarget == null && rightHandHoldPoint != null)
                        armIKController.rightHandTarget = rightHandHoldPoint;

                    armIKController.SetHolding(true);
                }
            }
        }
    }

    private void DropHeldObject()
    {
        if (heldObject == null) return;

        // unparent and restore physics state if applicable
        heldObject.transform.SetParent(heldOriginalParent, true);

        if (heldRb != null)
        {
            heldRb.isKinematic = heldOriginalKinematic;
            heldRb.useGravity = heldOriginalUseGravity;
            // apply a small forward impulse so it drops/throws slightly away from the player
            heldRb.AddForce(playerCamera.transform.forward * dropForwardForce, ForceMode.VelocityChange);
        }

        // Notify ArmIKController to lower the hand when the item is dropped
        if (armIKController != null)
            armIKController.SetHolding(false);

        heldObject = null;
        heldRb = null;
        heldOriginalParent = null;

        // reset per-item offsets to player defaults
        heldItemLocalPositionOffset = holdLocalPositionOffset;
        heldItemLocalRotationOffset = holdLocalRotationOffset;
        itemHoldOffsets = null;
    }
}