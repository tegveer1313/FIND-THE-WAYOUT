using UnityEngine;
using UnityEngine.InputSystem;

public class TorchHandler : MonoBehaviour
{
    [Header("Torch Settings")]
    public GameObject torchPrefab;
    public Transform torchHoldPosition;      // Player's hand position
    public Camera playerCamera;              // Assign the player's camera
    public int maxTorches = 3;
    public float torchBurnDuration = 15f;
    public float maxPlaceDistance = 5f;      // Max distance to place torch

    private int currentTorches;
    private GameObject currentTorch;
    private float torchTimer;
    private bool torchActive = false;
    private bool placedOnWall = false;

    private void Awake()
    {
        currentTorches = maxTorches;
    }

    private void Update()
    {
        HandleTorch();
        HandleInput();
    }

    private void HandleInput()
    {
        // R key → Equip torch in hand
        if (Keyboard.current.rKey.wasPressedThisFrame && !torchActive && currentTorches > 0)
        {
            EquipTorch();
        }

        // E key → Place torch on wall
        if (Keyboard.current.eKey.wasPressedThisFrame && torchActive && !placedOnWall)
        {
            Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, maxPlaceDistance))
            {
                // Only place on vertical walls
                if (hit.collider != null && hit.collider is MeshCollider && Mathf.Abs(hit.normal.y) < 0.5f)
                {
                    PlaceTorch(hit.point, hit.normal);
                }
            }
        }
    }

    private void EquipTorch()
    {
        if (torchPrefab == null || torchHoldPosition == null) return;

        currentTorch = Instantiate(torchPrefab, torchHoldPosition.position, torchHoldPosition.rotation, torchHoldPosition);
        torchActive = true;
        placedOnWall = false;
        torchTimer = torchBurnDuration;

        EnableTorchVisuals(currentTorch);

        currentTorches--;
    }

    private void PlaceTorch(Vector3 position, Vector3 normal)
    {
        if (currentTorch == null) return;

        // Detach from hand
        currentTorch.transform.parent = null;
        currentTorch.transform.position = position;

        // Rotate torch to face away from the wall
        Quaternion rotation = Quaternion.LookRotation(-normal); // Note the minus to face outward

        // Add slight downward tilt for natural look
        rotation *= Quaternion.Euler(-20f, 0f, 0f);

        currentTorch.transform.rotation = rotation;

        placedOnWall = true;
        torchActive = false;
    }

    private void EnableTorchVisuals(GameObject torch)
    {
        var fireParticles = torch.GetComponentInChildren<ParticleSystem>();
        var torchLight = torch.GetComponentInChildren<Light>();
        if (fireParticles != null) fireParticles.Play();
        if (torchLight != null) torchLight.enabled = true;
    }

    private void HandleTorch()
    {
        if (torchActive && currentTorch != null && !placedOnWall)
        {
            torchTimer -= Time.deltaTime;
            if (torchTimer <= 0f)
            {
                DisableTorchVisuals(currentTorch);
                Destroy(currentTorch);
                currentTorch = null;
                torchActive = false;
            }
        }
    }

    private void DisableTorchVisuals(GameObject torch)
    {
        var fireParticles = torch.GetComponentInChildren<ParticleSystem>();
        var torchLight = torch.GetComponentInChildren<Light>();
        if (fireParticles != null) fireParticles.Stop();
        if (torchLight != null) torchLight.enabled = false;
    }

    public int GetCurrentTorches() => currentTorches;
}
