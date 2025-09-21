using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class PickupDestroyTargets
{
    public PickupItem pickupItem;                // Which item this mapping belongs to
    public List<GameObject> destroyTargets;      // Objects to destroy (fire, particles, lights, prefabs, etc.)
}

public class PickupDestroyer : MonoBehaviour
{
    [Header("Destroy Settings")]
    public bool destroyOnPickUp = false;
    public float destroyDelay = 5f; // optional, not required if instant destroy

    [Tooltip("Assign objects to destroy per PickupItem (fire, particles, prefabs, etc.)")]
    public List<PickupDestroyTargets> destroyMappings = new List<PickupDestroyTargets>();

    private Coroutine destroyCoroutine;

    /// <summary>
    /// Instantly destroy all mapped objects for this PickupItem
    /// </summary>
    public void DestroyNow(PickupItem item)
    {
        if (item == null) return;

        var mapping = destroyMappings.Find(m => m.pickupItem == item);

        if (mapping != null && mapping.destroyTargets.Count > 0)
        {
            foreach (var target in mapping.destroyTargets)
            {
                if (target != null)
                    Destroy(target); // instant destroy
            }
        }
    }

    /// <summary>
    /// Start destruction timer (optional)
    /// </summary>
    public void StartDestroyTimer(PickupItem item)
    {
        if (destroyCoroutine != null) StopCoroutine(destroyCoroutine);
        destroyCoroutine = StartCoroutine(DestroyAfterDelay(item));
    }

    private System.Collections.IEnumerator DestroyAfterDelay(PickupItem item)
    {
        yield return new WaitForSeconds(destroyDelay);
        DestroyNow(item);
        destroyCoroutine = null;
    }

    /// <summary>
    /// Cancel any scheduled destruction
    /// </summary>
    public void CancelDestroy()
    {
        if (destroyCoroutine != null)
        {
            StopCoroutine(destroyCoroutine);
            destroyCoroutine = null;
        }
    }
}
