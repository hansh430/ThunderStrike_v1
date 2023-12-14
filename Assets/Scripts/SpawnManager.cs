using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager instance;
    public Transform[] spawnPoints;
    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        foreach (Transform spawn in spawnPoints)
        {
            spawn.gameObject.SetActive(false);
        }
    }
    public Transform GetSpawnPoint()
    {
        int n = Random.Range(0, spawnPoints.Length);
        return spawnPoints[n];
    }
}
