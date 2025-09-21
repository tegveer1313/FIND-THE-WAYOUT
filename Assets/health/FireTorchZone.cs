using UnityEngine;

public class FireTorchZone : MonoBehaviour
{
    [Header("Oxygen Drain Settings")]
    public float oxygenDrainRate = 10f; // per second

    private bool isHeld = false; // set by your pickup script
    private PlayerOxygen playerOxygen;

    private void Start()
    {
        // Find player oxygen script in scene (optional shortcut)
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerOxygen = player.GetComponent<PlayerOxygen>();
    }

    private void Update()
    {
        if (isHeld && playerOxygen != null)
        {
            // Drain oxygen if player is holding torch
            playerOxygen.ReduceOxygen(oxygenDrainRate * Time.deltaTime);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerOxygen oxy = other.GetComponent<PlayerOxygen>();
            if (oxy != null) oxy.nearFireTorch = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerOxygen oxy = other.GetComponent<PlayerOxygen>();
            if (oxy != null) oxy.nearFireTorch = false;
        }
    }

    // Call this from your pickup script
    public void SetHeld(bool held)
    {
        isHeld = held;
    }

    
}
