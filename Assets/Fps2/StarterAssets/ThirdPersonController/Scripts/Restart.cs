using System.Collections;
using UnityEngine;

public class Restart : MonoBehaviour
{
    [Header("Assign a start/respawn Transform here")]
    public Transform startPoint;

    [Tooltip("Temporarily disable this trigger after teleporting to avoid immediate re-trigger")]
    public float disableTriggerForSeconds = 0.5f;

    Collider triggerCollider;

    void Awake()
    {
        triggerCollider = GetComponent<Collider>();
        if (triggerCollider == null) Debug.LogWarning("Restart: No collider on this object. Add one and check 'Is Trigger'.");
        else if (!triggerCollider.isTrigger) Debug.LogWarning("Restart: Collider is not marked as 'Is Trigger'.");
        if (startPoint == null) Debug.LogWarning("Restart: startPoint is not assigned in the Inspector.");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (startPoint == null) return; // safety

        // Find the player GameObject (handles colliders that are on child objects)
        GameObject playerGO = null;
        if (other.CompareTag("Player")) playerGO = other.gameObject;
        else if (other.transform.root != null && other.transform.root.CompareTag("Player")) playerGO = other.transform.root.gameObject;
        else if (other.attachedRigidbody != null && other.attachedRigidbody.gameObject.CompareTag("Player")) playerGO = other.attachedRigidbody.gameObject;
        else return; // not the player

        Transform playerT = playerGO.transform;
        CharacterController cc = playerGO.GetComponent<CharacterController>();
        Rigidbody rb = playerGO.GetComponent<Rigidbody>();

        // Handle CharacterController properly (disable before moving to avoid collision issues)
        if (cc != null)
        {
            cc.enabled = false;
            playerT.position = startPoint.position;
            playerT.rotation = startPoint.rotation;
            cc.enabled = true;
        }
        // For non-kinematic rigidbody use MovePosition / MoveRotation + zero velocities
        else if (rb != null && !rb.isKinematic)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.MovePosition(startPoint.position);
            rb.MoveRotation(startPoint.rotation);
        }
        // Fallback - just set transform (works for kinematic rigidbodies or simple setups)
        else
        {
            playerT.position = startPoint.position;
            playerT.rotation = startPoint.rotation;
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }

        Debug.Log($"Restart: Teleported '{playerGO.name}' to start point '{startPoint.name}'.");

        // Prevent immediate retrigger (e.g. if start point overlaps trigger)
        if (disableTriggerForSeconds > 0f && triggerCollider != null)
            StartCoroutine(TemporarilyDisableTrigger());
    }

    IEnumerator TemporarilyDisableTrigger()
    {
        triggerCollider.enabled = false;
        yield return new WaitForSeconds(disableTriggerForSeconds);
        triggerCollider.enabled = true;
    }
}
