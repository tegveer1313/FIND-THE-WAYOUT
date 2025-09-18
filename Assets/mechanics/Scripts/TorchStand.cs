using UnityEngine;

public class TorchStand : MonoBehaviour
{
    [Header("Stand Settings")]
    public Transform standPosition;
    [HideInInspector]
    public GameObject currentTorch; // Keep reference to the torch on the stand

    private void OnDrawGizmos()
    {
        if (standPosition != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(standPosition.position, 0.1f);
        }
    }
}
