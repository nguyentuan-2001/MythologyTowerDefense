using System;
using System.Collections;
using UnityEngine;

public class WaveSpawner : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float spawnInterval = 1f;
    [SerializeField] private int enemiesPerWave = 10;

    public int CurrentWave { get; private set; }
    public bool WaveInProgress { get; private set; }

    public event Action<int> OnWaveStarted;
    public event Action<int> OnWaveCompleted;

    public void StartWave()
    {
        if (WaveInProgress || (GameManager.Instance != null && GameManager.Instance.IsGameOver))
        {
            return;
        }

        StartCoroutine(SpawnWaveRoutine());
    }

    private IEnumerator SpawnWaveRoutine()
    {
        WaveInProgress = true;
        CurrentWave++;
        OnWaveStarted?.Invoke(CurrentWave);

        for (int i = 0; i < enemiesPerWave; i++)
        {
            if (enemyPrefab != null && spawnPoint != null)
            {
                Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);
            }

            yield return new WaitForSeconds(spawnInterval);
        }

        while (GameManager.Instance != null && GameManager.Instance.ActiveEnemies.Count > 0)
        {
            yield return null;
        }

        WaveInProgress = false;
        OnWaveCompleted?.Invoke(CurrentWave);
    }
}
