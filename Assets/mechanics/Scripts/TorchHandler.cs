using UnityEngine;
using UnityEngine.InputSystem;

public class TorchHandler : MonoBehaviour
{
    [Header("Torch Settings")]
    public GameObject torchPrefab;
    public Transform torchHoldPosition;
    public int maxTorches = 3;
    public float torchBurnDuration = 15f;

    private int currentTorches;
    private GameObject currentTorch;
    private float torchTimer;
    private bool torchActive = false;

    private FPSInputActions inputActions;

    private void Awake()
    {
        currentTorches = maxTorches;
        inputActions = new FPSInputActions();
    }

    private void OnEnable()
    {
        inputActions.Enable();
        inputActions.Player.Interact.performed += OnInteract;
    }

    private void OnDisable()
    {
        if (inputActions != null)
        {
            inputActions.Player.Interact.performed -= OnInteract;
            inputActions.Disable();
        }
    }

    private void Update()
    {
        HandleTorch();
    }

    private void OnInteract(InputAction.CallbackContext ctx)
    {
        if (!torchActive && currentTorches > 0)
        {
            EquipTorch();
        }
    }

    private void EquipTorch()
    {
        if (torchPrefab == null || torchHoldPosition == null) return;

        currentTorch = Instantiate(torchPrefab, torchHoldPosition.position, torchHoldPosition.rotation, torchHoldPosition);
        torchActive = true;
        torchTimer = torchBurnDuration;

        var fireParticles = currentTorch.GetComponentInChildren<ParticleSystem>();
        var torchLight = currentTorch.GetComponentInChildren<Light>();
        if (fireParticles != null) fireParticles.Play();
        if (torchLight != null) torchLight.enabled = true;

        currentTorches--;
    }

    private void HandleTorch()
    {
        if (torchActive && currentTorch != null)
        {
            torchTimer -= Time.deltaTime;
            if (torchTimer <= 0f)
            {
                var fireParticles = currentTorch.GetComponentInChildren<ParticleSystem>();
                var torchLight = currentTorch.GetComponentInChildren<Light>();
                if (fireParticles != null) fireParticles.Stop();
                if (torchLight != null) torchLight.enabled = false;

                Destroy(currentTorch);
                currentTorch = null;
                torchActive = false;
            }
        }
    }

    public int GetCurrentTorches() => currentTorches;
}
