using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 4f;
    public float sprintSpeed = 7f;
    public float crouchSpeed = 2f;
    public float jumpHeight = 1.5f;
    public float gravity = -15f;

    [Header("Look Settings")]
    public float lookSensitivity = 0.1f;
    public Transform cameraTransform; 

    private CharacterController controller;
    private PlayerControls inputActions;
    private Vector3 velocity;
    private Vector2 moveInput;
    private Vector2 lookInput;
    private float cameraPitch = 0f;

    // Crouch Variables
    private float standHeight = 1.8f;
    private float crouchHeight = 1.0f;
    private Vector3 standCenter = new Vector3(0, 0.9f, 0);
    private Vector3 crouchCenter = new Vector3(0, 0.5f, 0);
    private float standCamY = 1.6f;
    private float crouchCamY = 0.8f;
    
    private bool isGrounded; // Naya ground check variable

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        inputActions = new PlayerControls();
    }

    void OnEnable() { inputActions.Player.Enable(); }
    void OnDisable() { inputActions.Player.Disable(); }

    void Update()
    {
        // 1. Inputs
        moveInput = inputActions.Player.Move.ReadValue<Vector2>();
        lookInput = inputActions.Player.Look.ReadValue<Vector2>();
        
        bool isSprinting = inputActions.Player.Sprint.IsPressed();
        bool isCrouching = inputActions.Player.Crouch.IsPressed();

        // 2. Look
        transform.Rotate(Vector3.up * lookInput.x * lookSensitivity);
        cameraPitch -= lookInput.y * lookSensitivity;
        cameraPitch = Mathf.Clamp(cameraPitch, -90f, 90f);
        cameraTransform.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);

        // 3. Crouch Logic
        if (isCrouching)
        {
            controller.height = crouchHeight;
            controller.center = crouchCenter;
            cameraTransform.localPosition = new Vector3(0, crouchCamY, 0);
        }
        else
        {
            controller.height = standHeight;
            controller.center = standCenter;
            cameraTransform.localPosition = new Vector3(0, standCamY, 0);
        }

        // 4. Movement
        float currentSpeed = isCrouching ? crouchSpeed : (isSprinting ? sprintSpeed : walkSpeed);
        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
        controller.Move(move * currentSpeed * Time.deltaTime);

        // 5. RAYCAST GROUND CHECK (Yeh naya fix hai)
        // Player ke paon se thora sa neeche laser chorta hai (0.2 meters tak)
        isGrounded = controller.isGrounded || Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, 0.2f);

        // 6. Jump & Gravity
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; 
        }

        if (inputActions.Player.Jump.WasPressedThisFrame() && isGrounded && !isCrouching)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            Debug.Log("Jump chala!");
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}