using UnityEngine;
using UnityEngine.UI;

public class PlayerOxygen : MonoBehaviour
{
    [Header("Oxygen Settings")]
    public float maxOxygen = 100f;
    public float currentOxygen;
    public float depletionRate = 10f; // per second near fire
    public float recoveryRate = 5f;   // per second when away

    [Header("UI")]
    public Slider oxygenBar;

    [HideInInspector] public bool nearFireTorch = false;

    void Start()
    {
        currentOxygen = maxOxygen;
        if (oxygenBar != null) oxygenBar.maxValue = maxOxygen;
    }

    void Update()
    {
        if (nearFireTorch)
            currentOxygen -= depletionRate * Time.deltaTime;
        else
            currentOxygen += recoveryRate * Time.deltaTime;

        currentOxygen = Mathf.Clamp(currentOxygen, 0f, maxOxygen);

        if (oxygenBar != null)
            oxygenBar.value = currentOxygen;

        if (currentOxygen <= 0f)
        {
            Debug.Log("⚠️ Player suffocated near the fire torch!");
            // Here you can trigger death, damage, fade-out, etc.
        }
    }

    public void ReduceOxygen(float amount)
{
    currentOxygen -= amount;
    currentOxygen = Mathf.Clamp(currentOxygen, 0f, maxOxygen);

    if (oxygenBar != null)
        oxygenBar.value = currentOxygen;
}
}
