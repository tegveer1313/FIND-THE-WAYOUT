using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class SpawnPlayer : MonoBehaviour
{

    public GameObject playerPrefab;
    public Transform playerSponer;

    public float minX;
    public float maxX;
    public float minY;
    public float maxY;


    private void Start()
    {
        Vector2 randomPosition = new Vector2(Random.Range(minX, maxX), Random.Range(minY, maxY));
        PhotonNetwork.Instantiate(playerPrefab.name, playerSponer.position, Quaternion.identity);
    }

}
