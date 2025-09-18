using UnityEngine;
using UnityEngine.InputSystem;

public class LightThrowWithE : MonoBehaviour
{
    [Header("References")]
    public PlayerPickup playerPickup;      // Reference to PlayerPickup component
    public float lightThrowForce = 3f;     // Small throw force

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

        // Use E key for light throw
        inputActions.Player.ThrowLight.performed += ctx => DoLightThrow();
    }

    private void OnDisable()
    {
        inputActions.Player.ThrowLight.performed -= ctx => DoLightThrow();
        inputActions.Disable();
    }

    private void DoLightThrow()
    {
        if (playerPickup == null || playerPickup.heldObject == null) return;

        // Check if aiming at a TorchStand
        Ray ray = new Ray(playerPickup.playerCamera.transform.position, playerPickup.playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, playerPickup.pickupRange))
        {
            TorchStand stand = hit.collider.GetComponent<TorchStand>();
            if (stand != null)
            {
                // Don't throw if aiming at a stand
                return;
            }
        }

        GameObject heldObj = playerPickup.heldObject;
        Rigidbody heldRb = playerPickup.heldRigidbody;
        Camera playerCam = playerPickup.playerCamera;

        if (heldObj != null && heldRb != null && playerCam != null)
        {
            heldObj.transform.SetParent(null);
            heldRb.isKinematic = false;
            heldRb.detectCollisions = true;
            heldRb.linearVelocity = Vector3.zero;
            heldRb.angularVelocity = Vector3.zero;

            heldRb.AddForce(playerCam.transform.forward * lightThrowForce, ForceMode.Impulse);

            Debug.Log("Item lightly thrown!");

            playerPickup.heldObject = null;
            playerPickup.heldRigidbody = null;
        }
    }
}
