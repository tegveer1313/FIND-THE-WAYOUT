using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class FPSController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 5f;
    public float sprintSpeed = 8f;
    public float crouchSpeed = 2f;
    public float acceleration = 10f;
    public float deceleration = 10f;
    public float jumpHeight = 1.5f;
    public float gravity = -9.81f;

    [Header("Crouch Settings")]
    public float standingHeight = 2f;
    public float crouchingHeight = 1f;
    public bool instantCrouch = true;
    public float crouchTransitionSpeed = 10f;

    [Header("Look Settings")]
    public float lookSensitivity = 1f;
    public float minLookAngle = -80f;
    public float maxLookAngle = 80f;

    [Header("References")]
    public Transform cameraHolder;
    public Transform playerModel;

    private CharacterController controller;
    private Vector2 moveInput;
    private Vector2 lookInput;
    private bool isJumping;
    private bool isCrouching;
    private bool isSprinting;
    private float verticalVelocity;
    private float currentHeight;
    private Vector3 currentVelocity;

    private FPSInputActions inputActions;

    private Vector3 originalModelScale;
    private Vector3 crouchingModelScale;

    private float pitch = 0f;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        inputActions = new FPSInputActions();

        if (playerModel != null)
        {
            originalModelScale = playerModel.localScale;
            crouchingModelScale = new Vector3(originalModelScale.x, originalModelScale.y * (crouchingHeight / standingHeight), originalModelScale.z);
        }
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnEnable()
    {
        if (inputActions != null)
        {
            inputActions.Enable();

            inputActions.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
            inputActions.Player.Move.canceled += ctx => moveInput = Vector2.zero;

            inputActions.Player.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
            inputActions.Player.Look.canceled += ctx => lookInput = Vector2.zero;

            inputActions.Player.Jump.performed += ctx => Jump();

            inputActions.Player.Crouch.performed += ctx => isCrouching = true;
            inputActions.Player.Crouch.canceled += ctx => isCrouching = false;

            inputActions.Player.Sprint.performed += ctx => isSprinting = true;
            inputActions.Player.Sprint.canceled += ctx => isSprinting = false;
        }
    }

    private void OnDisable()
    {
        if (inputActions != null)
        {
            inputActions.Disable();
        }
    }

    private void Update()
    {
        HandleMovement();
        HandleLook();
        HandleCrouch();
    }

    private void HandleMovement()
    {
        Vector3 forward = transform.forward * moveInput.y;
        Vector3 right = transform.right * moveInput.x;
        Vector3 targetMove = (forward + right).normalized;

        float targetSpeed = isCrouching ? crouchSpeed : (isSprinting ? sprintSpeed : walkSpeed);

        if (targetMove.magnitude > 0)
        {
            currentVelocity = Vector3.Lerp(currentVelocity, targetMove * targetSpeed, acceleration * Time.deltaTime);
        }
        else
        {
            currentVelocity = Vector3.Lerp(currentVelocity, Vector3.zero, deceleration * Time.deltaTime);
        }

        if (controller.isGrounded)
        {
            verticalVelocity = -2f;
            if (isJumping)
            {
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
                isJumping = false;
            }
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }

        Vector3 move = currentVelocity;
        move.y = verticalVelocity;

        controller.Move(move * Time.deltaTime);
    }

    private void HandleLook()
    {
        transform.Rotate(Vector3.up * lookInput.x * lookSensitivity * Time.deltaTime);

        pitch -= lookInput.y * lookSensitivity * Time.deltaTime;
        pitch = Mathf.Clamp(pitch, minLookAngle, maxLookAngle);
        cameraHolder.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    private void Jump()
    {
        if (controller.isGrounded)
            isJumping = true;
    }

    private void HandleCrouch()
    {
        float targetHeight = isCrouching ? crouchingHeight : standingHeight;

        if (instantCrouch)
        {
            controller.height = targetHeight;
        }
        else
        {
            currentHeight = Mathf.Lerp(controller.height, targetHeight, crouchTransitionSpeed * Time.deltaTime);
            controller.height = currentHeight;
        }

        Vector3 center = controller.center;
        center.y = controller.height / 2f;
        controller.center = center;

        if (playerModel != null)
        {
            if (instantCrouch)
            {
                playerModel.localScale = isCrouching ? crouchingModelScale : originalModelScale;
            }
            else
            {
                Vector3 targetScale = isCrouching ? crouchingModelScale : originalModelScale;
                playerModel.localScale = Vector3.Lerp(playerModel.localScale, targetScale, crouchTransitionSpeed * Time.deltaTime);
            }
        }
    }
}
