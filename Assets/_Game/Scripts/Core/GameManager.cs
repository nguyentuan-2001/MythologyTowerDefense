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

        private void Awake()
        {
            // Never rely on the engine/platform's own boot-time default for this — this project's
            // ProjectSettings/TimeManager.asset was found set to m_TimeScale: 0 (not the usual 1),
            // which the Editor's Play Mode silently papered over but a real WebGL build did not:
            // every enemy spawned registered correctly but never moved (Time.deltaTime stayed 0)
            // until some other action (Pause then Resume) happened to set timeScale to 1 for the
            // first time. Setting it explicitly here makes correctness independent of that project
            // setting entirely.
            Time.timeScale = 1f;
        }

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
