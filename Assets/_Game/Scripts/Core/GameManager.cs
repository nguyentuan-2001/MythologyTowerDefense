using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using MythologyTowerDefense.Gameplay.Economy;
using MythologyTowerDefense.Gameplay.Waves;

namespace MythologyTowerDefense.Core
{
    public enum GameState
    {
        Playing,
        Paused,
        Won,
        Lost
    }

    public class GameManager : MonoBehaviour
    {
        [SerializeField] private BaseHealth baseHealth;
        [SerializeField] private WaveManager waveManager;

        public GameState State { get; private set; } = GameState.Playing;

        public event Action<GameState> OnStateChanged;

        private void OnEnable()
        {
            if (baseHealth != null)
            {
                baseHealth.OnDepleted += HandleBaseDepleted;
            }

            if (waveManager != null)
            {
                waveManager.OnAllWavesCompleted += HandleAllWavesCompleted;
            }
        }

        private void OnDisable()
        {
            if (baseHealth != null)
            {
                baseHealth.OnDepleted -= HandleBaseDepleted;
            }

            if (waveManager != null)
            {
                waveManager.OnAllWavesCompleted -= HandleAllWavesCompleted;
            }
        }

        public void RequestStartWave()
        {
            if (State != GameState.Playing)
            {
                return;
            }

            waveManager?.StartNextWave();
        }

        public void TogglePause()
        {
            if (State == GameState.Won || State == GameState.Lost)
            {
                return;
            }

            if (State == GameState.Paused)
            {
                State = GameState.Playing;
                Time.timeScale = 1f;
            }
            else
            {
                State = GameState.Paused;
                Time.timeScale = 0f;
            }

            OnStateChanged?.Invoke(State);
        }

        public void Restart()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        private void HandleBaseDepleted()
        {
            if (State == GameState.Won || State == GameState.Lost)
            {
                return;
            }

            State = GameState.Lost;
            Time.timeScale = 0f;
            OnStateChanged?.Invoke(State);
        }

        private void HandleAllWavesCompleted()
        {
            if (State == GameState.Won || State == GameState.Lost)
            {
                return;
            }

            State = GameState.Won;
            Time.timeScale = 0f;
            OnStateChanged?.Invoke(State);
        }
    }
}
