using UnityEngine;
using UnityEngine.InputSystem;

public class LightThrowWithE : MonoBehaviour
{
    [Header("References")]
    public PlayerPickup playerPickup;       // Reference to PlayerPickup component
    public float throwForce = 10f;          // Throw force for E key

    private FPSInputActions inputActions;

    private void Awake()
    {
        inputActions = new FPSInputActions();

        if (playerPickup == null)
            playerPickup = GetComponent<PlayerPickup>();
    }

    private void OnEnable()
    {
        inputActions.Enable();

        // Interact key (E) for throw
        inputActions.Player.Interact.performed += ctx => DoThrow();
    }

    private void OnDisable()
    {
        inputActions.Player.Interact.performed -= ctx => DoThrow();
        inputActions.Disable();
    }

    private void DoThrow()
    {
        if (playerPickup == null || playerPickup.heldObject == null) return;

        // Check if aiming at a TorchStand
        Ray ray = new Ray(playerPickup.playerCamera.transform.position, playerPickup.playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, playerPickup.pickupRange))
        {
            TorchStand stand = hit.collider.GetComponent<TorchStand>();
            if (stand != null) return;  // Don't throw if aiming at a stand
        }

        playerPickup.heldObject = null;
        playerPickup.heldRigidbody = null;
    }
}
