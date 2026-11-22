using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;

    public float spawnTime;
    // Counted across the whole scene, not per spawner.
    public int maxAlive = 6;

    private float spawnTimer;

    // Start is called before the first frame update
    void Start()
    {
        if (GameFlowManager.Instance != null && GameFlowManager.Instance.IsPaused) return;
        SpawnEnemy();
    }

    // Update is called once per frame
    void Update()
    {
        if (GameFlowManager.Instance != null && GameFlowManager.Instance.IsPaused) return;
        spawnTimer += Time.deltaTime;
        if (spawnTimer > spawnTime)
        {
            spawnTimer = 0;
            SpawnEnemy();
        }
    }

    void SpawnEnemy()
    {
        if (maxAlive > 0 && AliveEnemyCount() >= maxAlive) return;
        GameObject.Instantiate(enemyPrefab, transform.position, Quaternion.identity);
    }

    private static int AliveEnemyCount()
    {
        return Object.FindObjectsOfType<Enemy>().Length;
    }
}
