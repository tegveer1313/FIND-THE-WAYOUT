using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class PickupDestroyTargets
{
    public PickupItem pickupItem;                // Which item this mapping belongs to
    public List<GameObject> destroyTargets;      // Objects that should be destroyed with it
}

public class PlayerPickup : MonoBehaviour
{
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

    [Header("Destroy Settings")]
    public bool destroyOnPickUp = false; // ✅ only option now
    public float destroyDelay = 5f;      // how long before destruction while held

    [Tooltip("Each pickup item can have its own destroy targets.")]
    public List<PickupDestroyTargets> destroyMappings = new List<PickupDestroyTargets>();

    private FPSInputActions inputActions;
    [HideInInspector] public GameObject heldObject;
    [HideInInspector] public Rigidbody heldRigidbody;

    // Track if object was picked by the player
    private bool objectPickedByPlayer = false;

    // ✅ coroutine handle for destruction
    private Coroutine destroyCoroutine;

    private void Awake()
    {
        inputActions = new FPSInputActions();
    }

    private void OnEnable()
    {
        inputActions.Enable();
        inputActions.Player.Pickup.performed += ctx => PickupOrReleaseOrPlace();
        inputActions.Player.Throw.performed += ctx => Throw();
    }

    private void OnDisable()
    {
        inputActions.Player.Pickup.performed -= ctx => PickupOrReleaseOrPlace();
        inputActions.Player.Throw.performed -= ctx => Throw();
        inputActions.Disable();
    }

    private void Start()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;

        if (playerCamera == null)
            Debug.LogError("Player camera not assigned and Camera.main not found!");
    }

    private void PickupOrReleaseOrPlace()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        Physics.Raycast(ray, out RaycastHit hit, pickupRange);

        TorchStand stand = hit.collider != null ? hit.collider.GetComponent<TorchStand>() : null;

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

    private void TryPickup()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, pickupRange);

        GameObject closest = null;
        float closestDist = float.MaxValue;

        foreach (var hit in hits)
        {
            PickupItem item = hit.GetComponent<PickupItem>();
            if (item != null && pickableObjects.Contains(item))
            {
                Vector3 directionToObject = (hit.transform.position - playerCamera.transform.position).normalized;
                float angle = Vector3.Angle(playerCamera.transform.forward, directionToObject);

                if (angle <= maxAimAngle)
                {
                    float dist = Vector3.Distance(transform.position, hit.transform.position);
                    if (dist < closestDist)
                    {
                        closest = hit.gameObject;
                        closestDist = dist;
                    }
                }
            }
        }

        if (closest != null)
        {
            heldObject = closest;
            heldRigidbody = heldObject.GetComponent<Rigidbody>();

            if (heldRigidbody != null)
            {
                heldRigidbody.isKinematic = true;
                heldRigidbody.detectCollisions = false;
            }

            heldObject.transform.SetParent(holdPosition);
            heldObject.transform.position = holdPosition.position;
            heldObject.transform.rotation = holdPosition.rotation;

            Debug.Log($"{heldObject.name} picked up!");
            objectPickedByPlayer = true;

            // ✅ Start timed destruction
            if (destroyOnPickUp)
            {
                if (destroyCoroutine != null) StopCoroutine(destroyCoroutine);
                destroyCoroutine = StartCoroutine(DestroyAfterDelay(heldObject.GetComponent<PickupItem>()));
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
            Debug.Log("Item released!");

            // ❌ Cancel destruction
            if (destroyCoroutine != null)
            {
                StopCoroutine(destroyCoroutine);
                destroyCoroutine = null;
            }

            heldObject = null;
            heldRigidbody = null;
            objectPickedByPlayer = false;
        }
    }

    private void Throw()
    {
        if (heldObject != null && heldRigidbody != null && playerCamera != null)
        {
            heldObject.transform.SetParent(null);
            heldRigidbody.isKinematic = false;
            heldRigidbody.detectCollisions = true;

            heldRigidbody.linearVelocity = Vector3.zero;
            heldRigidbody.angularVelocity = Vector3.zero;

            heldRigidbody.AddForce(playerCamera.transform.forward * throwForce, ForceMode.Impulse);

            Debug.Log("Item thrown!");

            // ❌ Cancel destruction
            if (destroyCoroutine != null)
            {
                StopCoroutine(destroyCoroutine);
                destroyCoroutine = null;
            }

            heldObject = null;
            heldRigidbody = null;
            objectPickedByPlayer = false;
        }
    }

    private void PickFromStand(TorchStand stand)
    {
        heldObject = stand.currentTorch;
        heldRigidbody = heldObject.GetComponent<Rigidbody>();

        if (heldRigidbody != null)
        {
            heldRigidbody.isKinematic = true;
            heldRigidbody.detectCollisions = false;
        }

        Vector3 originalScale = heldObject.transform.lossyScale;

        heldObject.transform.SetParent(holdPosition, worldPositionStays: true);
        heldObject.transform.position = holdPosition.position;
        heldObject.transform.rotation = holdPosition.rotation;

        heldObject.transform.localScale = new Vector3(
            originalScale.x / holdPosition.lossyScale.x,
            originalScale.y / holdPosition.lossyScale.y,
            originalScale.z / holdPosition.lossyScale.z
        );

        stand.currentTorch = null;

        Debug.Log($"{heldObject.name} picked up from stand!");
        objectPickedByPlayer = true;

        // ✅ Start timed destruction
        if (destroyOnPickUp)
        {
            if (destroyCoroutine != null) StopCoroutine(destroyCoroutine);
            destroyCoroutine = StartCoroutine(DestroyAfterDelay(heldObject.GetComponent<PickupItem>()));
        }
    }

    private void PlaceOnStand(TorchStand stand)
    {
        if (heldObject != null)
        {
            Vector3 originalScale = heldObject.transform.lossyScale;

            heldObject.transform.SetParent(stand.standPosition, worldPositionStays: true);
            heldObject.transform.position = stand.standPosition.position;
            heldObject.transform.rotation = stand.standPosition.rotation;

            heldObject.transform.localScale = new Vector3(
                originalScale.x / stand.standPosition.lossyScale.x,
                originalScale.y / stand.standPosition.lossyScale.y,
                originalScale.z / stand.standPosition.lossyScale.z
            );

            if (heldRigidbody != null)
            {
                heldRigidbody.isKinematic = true;
                heldRigidbody.detectCollisions = false;
            }

            stand.currentTorch = heldObject;

            Debug.Log("Object placed on stand.");

            // ❌ Cancel destruction
            if (destroyCoroutine != null)
            {
                StopCoroutine(destroyCoroutine);
                destroyCoroutine = null;
            }

            heldObject = null;
            heldRigidbody = null;
            objectPickedByPlayer = false;
        }
    }

    private IEnumerator DestroyAfterDelay(PickupItem item)
    {
        yield return new WaitForSeconds(destroyDelay);

        if (heldObject != null && objectPickedByPlayer) // ✅ Only destroy if still held
        {
            DestroyTargetsWithDelay(item);

            // Optionally also destroy the heldObject if in mapping
            var mapping = destroyMappings.Find(m => m.pickupItem == item);
            if (mapping != null && mapping.destroyTargets.Contains(heldObject))
            {
                Destroy(heldObject);
            }

            heldObject = null;
            heldRigidbody = null;
            objectPickedByPlayer = false;
        }

        destroyCoroutine = null; // clear handle
    }

    private void DestroyTargetsWithDelay(PickupItem item)
    {
        if (item == null) return;
        if (!objectPickedByPlayer) return;

        var mapping = destroyMappings.Find(m => m.pickupItem == item);

        if (mapping != null && mapping.destroyTargets.Count > 0)
        {
            foreach (var target in mapping.destroyTargets)
            {
                if (target != null)
                    Destroy(target, 0f); // immediate when delay already handled by coroutine
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupRange);
    }
}
