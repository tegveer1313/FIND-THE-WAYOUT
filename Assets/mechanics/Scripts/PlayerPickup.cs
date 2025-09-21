using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class PlayerPickup : MonoBehaviour
{
    [Header("Pickup Layer")]
    public LayerMask HitLayer;

    [Header("Pickup Settings")]
    public Transform holdPosition;
    public float throwForce = 10f;
    public float pickupRange = 3f;

    [Header("Pickable Objects")]
    public List<PickupItem> pickableObjects = new List<PickupItem>();

    [Header("Aiming Settings")]
    public float maxAimAngle = 15f;

    [Header("References")]
    public Camera playerCamera;

    private FPSInputActions inputActions;
    [HideInInspector] public GameObject heldObject;
    [HideInInspector] public Rigidbody heldRigidbody;

    private PickupDestroyer destroyer;   // 🔗 Reference to destroy script

    private void Awake()
    {
        inputActions = new FPSInputActions();
    }

    private void OnEnable()
    {
        inputActions.Enable();
        inputActions.Player.Pickup.performed += OnPickupPerformed;
        inputActions.Player.Throw.performed += OnThrowPerformed;
    }

    private void OnDisable()
    {
        inputActions.Player.Pickup.performed -= OnPickupPerformed;
        inputActions.Player.Throw.performed -= OnThrowPerformed;
        inputActions.Disable();
    }

    private void Start()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;

        if (playerCamera == null)
            Debug.LogError("Player camera not assigned and Camera.main not found!");
    }

    private void OnPickupPerformed(InputAction.CallbackContext ctx)
    {
        PickupOrReleaseOrPlace();
    }

    private void OnThrowPerformed(InputAction.CallbackContext ctx)
    {
        Throw();
    }

    private void PickupOrReleaseOrPlace()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, pickupRange, HitLayer))
        {
            if (hit.collider.CompareTag("PickAble"))
            {
                TorchStand stand = hit.collider.GetComponent<TorchStand>();

                if (heldObject == null)
                {
                    if (stand != null && stand.currentTorch != null)
                    {
                        PickFromStand(stand);
                        return;
                    }
                    else
                    {
                        TryPickup();
                        return;
                    }
                }

                if (stand != null)
                {
                    PlaceOnStand(stand);
                    return;
                }

                Release();
            }
        }
    }

    private void TryPickup()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, pickupRange);

        foreach (var hit in hits)
        {
            PickupItem item = hit.GetComponent<PickupItem>();
            if (item != null && pickableObjects.Contains(item))
            {
                heldObject = item.gameObject;
                heldRigidbody = heldObject.GetComponent<Rigidbody>();
                destroyer = heldObject.GetComponent<PickupDestroyer>();

                if (heldRigidbody != null)
                {
                    heldRigidbody.isKinematic = true;
                    heldRigidbody.detectCollisions = false;
                }

                heldObject.transform.SetParent(holdPosition);
                heldObject.transform.position = holdPosition.position;
                heldObject.transform.rotation = holdPosition.rotation;

                Debug.Log($"{heldObject.name} picked up!");

                // 🔥 Trigger destroyer logic
                if (destroyer != null && destroyer.destroyOnPickUp)
                    destroyer.StartDestroyTimer(item);

                return;
            }
        }
    }

    private void Release()
    {
        if (heldObject != null)
        {
            if (heldRigidbody != null)
            {
                heldRigidbody.isKinematic = false;
                heldRigidbody.detectCollisions = true;
            }

            heldObject.transform.SetParent(null);

            // ❌ Cancel destruction
            if (destroyer != null)
                destroyer.CancelDestroy();

            heldObject = null;
            heldRigidbody = null;
            destroyer = null;
        }
    }

    private void Throw()
    {
        if (heldObject != null && heldRigidbody != null)
        {
            heldObject.transform.SetParent(null);
            heldRigidbody.isKinematic = false;
            heldRigidbody.detectCollisions = true;

            heldRigidbody.linearVelocity = Vector3.zero;
            heldRigidbody.angularVelocity = Vector3.zero;
            heldRigidbody.AddForce(playerCamera.transform.forward * throwForce, ForceMode.Impulse);

            Debug.Log("Item thrown!");

            if (destroyer != null)
                destroyer.CancelDestroy();

            heldObject = null;
            heldRigidbody = null;
            destroyer = null;
        }
    }

    private void PickFromStand(TorchStand stand)
    {
        heldObject = stand.currentTorch;
        heldRigidbody = heldObject.GetComponent<Rigidbody>();
        destroyer = heldObject.GetComponent<PickupDestroyer>();

        if (heldRigidbody != null)
        {
            heldRigidbody.isKinematic = true;
            heldRigidbody.detectCollisions = false;
        }

        heldObject.transform.SetParent(holdPosition);
        heldObject.transform.position = holdPosition.position;
        heldObject.transform.rotation = holdPosition.rotation;

        stand.currentTorch = null;

        Debug.Log($"{heldObject.name} picked up from stand!");

        if (destroyer != null && destroyer.destroyOnPickUp)
            destroyer.StartDestroyTimer(heldObject.GetComponent<PickupItem>());
    }

    private void PlaceOnStand(TorchStand stand)
    {
        if (heldObject != null)
        {
            heldObject.transform.SetParent(stand.standPosition, worldPositionStays: true);
            heldObject.transform.position = stand.standPosition.position;
            heldObject.transform.rotation = stand.standPosition.rotation;

            if (heldRigidbody != null)
            {
                heldRigidbody.isKinematic = true;
                heldRigidbody.detectCollisions = false;
            }

            stand.currentTorch = heldObject;

            if (destroyer != null)
                destroyer.CancelDestroy();

            heldObject = null;
            heldRigidbody = null;
            destroyer = null;
        }
    }
}
