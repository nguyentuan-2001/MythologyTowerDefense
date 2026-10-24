using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MythologyTowerDefense.Gameplay.Enemies;
using MythologyTowerDefense.Gameplay.Path;
using MythologyTowerDefense.Gameplay.Economy;

namespace MythologyTowerDefense.Gameplay.Waves
{
    public class WaveManager : MonoBehaviour
    {
        [SerializeField] private GameObject enemyPrefab;
        [SerializeField] private PathDefinition path;
        [SerializeField] private CurrencyManager currencyManager;
        [SerializeField] private BaseHealth baseHealth;
        [SerializeField] private int enemiesPerWave = 10;
        [SerializeField] private float spawnInterval = 1.0f;
        [SerializeField] private int totalWaves = 1;

        private readonly List<EnemyController> activeEnemies = new List<EnemyController>();

        public IReadOnlyList<EnemyController> ActiveEnemies => activeEnemies;
        public int CurrentWave { get; private set; }
        public bool IsWaveInProgress { get; private set; }

        public event Action<int> OnWaveStarted;
        public event Action<int> OnWaveCompleted;
        public event Action OnAllWavesCompleted;

        public void StartNextWave()
        {
            if (IsWaveInProgress || CurrentWave >= totalWaves)
            {
                return;
            }

            StartCoroutine(RunWave());
        }

        private IEnumerator RunWave()
        {
            IsWaveInProgress = true;
            CurrentWave++;
            OnWaveStarted?.Invoke(CurrentWave);

            for (int i = 0; i < enemiesPerWave; i++)
            {
                SpawnEnemy();
                yield return new WaitForSeconds(spawnInterval);
            }

            while (activeEnemies.Count > 0)
            {
                yield return null;
            }

            IsWaveInProgress = false;
            OnWaveCompleted?.Invoke(CurrentWave);

            if (CurrentWave >= totalWaves)
            {
                OnAllWavesCompleted?.Invoke();
            }
        }

        private void SpawnEnemy()
        {
            if (enemyPrefab == null || path == null)
            {
                Debug.LogWarning("WaveManager: missing enemyPrefab or path reference.");
                return;
            }

            GameObject instance = Instantiate(enemyPrefab, path.StartPosition, Quaternion.identity);
            EnemyController enemy = instance.GetComponent<EnemyController>();

            if (enemy == null)
            {
                Debug.LogWarning("WaveManager: spawned enemy prefab has no EnemyController component.");
                Destroy(instance);
                return;
            }

            enemy.Initialize(path);
            activeEnemies.Add(enemy);
            enemy.OnDefeated += HandleEnemyDefeated;
            enemy.OnReachedBase += HandleEnemyReachedBase;
        }

        private void HandleEnemyDefeated(EnemyController enemy, int reward)
        {
            UnregisterEnemy(enemy);
            currencyManager?.Add(reward);
        }

        private void HandleEnemyReachedBase(EnemyController enemy, int damage)
        {
            UnregisterEnemy(enemy);
            baseHealth?.TakeDamage(damage);
        }

        private void UnregisterEnemy(EnemyController enemy)
        {
            enemy.OnDefeated -= HandleEnemyDefeated;
            enemy.OnReachedBase -= HandleEnemyReachedBase;
            activeEnemies.Remove(enemy);
        }
    }
}
