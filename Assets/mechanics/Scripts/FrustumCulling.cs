using UnityEngine;
using System.Collections.Generic;

public class AutoFrustumCulling : MonoBehaviour
{
    [Header("References")]
    public Camera targetCamera; // Assign your Cinemachine camera here
    public Transform player;    // Optional, for distance-based checks

    [Header("Settings")]
    public float padding = 1f;          // Extra padding for AABB bounds
    public float checkInterval = 0.2f;  // How often to check visibility
    public float maxCullDistance = 50f; // Max distance to check objects

    private Plane[] frustumPlanes;
    private float timer;

    private Renderer[] sceneRenderers;

    void Start()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        // Automatically find all renderers in the scene (Unity 6.1 compatible)
        sceneRenderers = Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None);
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= checkInterval)
        {
            PerformCulling();
            timer = 0f;
        }
    }

    private void PerformCulling()
    {
        frustumPlanes = GeometryUtility.CalculateFrustumPlanes(targetCamera);

        foreach (Renderer rend in sceneRenderers)
        {
            if (rend == null) continue;

            // Skip objects with the "noCull" tag
            if (rend.gameObject.CompareTag("noCull"))
            {
                rend.enabled = true;
                continue;
            }

            // Skip distant objects for efficiency
            if (player != null)
            {
                float distance = Vector3.Distance(player.position, rend.transform.position);
                if (distance > maxCullDistance)
                {
                    rend.enabled = false;
                    continue;
                }
            }

            // Expand bounds with padding
            Bounds paddedBounds = rend.bounds;
            paddedBounds.Expand(padding);

            // Test if inside camera frustum
            bool isVisible = GeometryUtility.TestPlanesAABB(frustumPlanes, paddedBounds);
            rend.enabled = isVisible;
        }
    }

    // Call this if you spawn new objects dynamically
    public void AddRenderer(Renderer rend)
    {
        var tempList = new List<Renderer>(sceneRenderers);
        if (!tempList.Contains(rend))
        {
            tempList.Add(rend);
            sceneRenderers = tempList.ToArray();
        }
    }

    // Optional: remove renderer from culling if needed
    public void RemoveRenderer(Renderer rend)
    {
        var tempList = new List<Renderer>(sceneRenderers);
        if (tempList.Contains(rend))
        {
            tempList.Remove(rend);
            sceneRenderers = tempList.ToArray();
        }
    }
}
