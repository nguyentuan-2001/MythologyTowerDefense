using UnityEngine;
using UnityEngine.UI;
using MythologyTowerDefense.Core;
using MythologyTowerDefense.Gameplay.Economy;
using MythologyTowerDefense.Gameplay.Waves;
using MythologyTowerDefense.Gameplay.Towers;

namespace MythologyTowerDefense.UI
{
    public class HUDController : MonoBehaviour
    {
        [Header("Systems")]
        [SerializeField] private GameManager gameManager;
        [SerializeField] private CurrencyManager currencyManager;
        [SerializeField] private BaseHealth baseHealth;
        [SerializeField] private WaveManager waveManager;
        [SerializeField] private TowerSelectionController selectionController;

        [Header("HUD Text")]
        [SerializeField] private Text currencyText;
        [SerializeField] private Text baseHealthText;
        [SerializeField] private Text waveStatusText;

        [Header("Buttons")]
        [SerializeField] private Button startWaveButton;
        [SerializeField] private Button pauseButton;
        [SerializeField] private Text pauseButtonLabel;
        [SerializeField] private Button restartButton;

        [Header("End Panel")]
        [SerializeField] private GameObject endPanel;
        [SerializeField] private Text endPanelText;

        [Header("Selection Panel")]
        [SerializeField] private GameObject selectionPanel;
        [SerializeField] private Text selectionText;

        private void OnEnable()
        {
            if (currencyManager != null) currencyManager.OnCurrencyChanged += HandleCurrencyChanged;
            if (baseHealth != null) baseHealth.OnHealthChanged += HandleBaseHealthChanged;

            if (waveManager != null)
            {
                waveManager.OnWaveStarted += HandleWaveStarted;
                waveManager.OnWaveCompleted += HandleWaveCompleted;
            }

            if (gameManager != null) gameManager.OnStateChanged += HandleStateChanged;
            if (selectionController != null) selectionController.OnSelectionChanged += HandleSelectionChanged;

            if (startWaveButton != null) startWaveButton.onClick.AddListener(HandleStartWaveClicked);
            if (pauseButton != null) pauseButton.onClick.AddListener(HandlePauseClicked);
            if (restartButton != null) restartButton.onClick.AddListener(HandleRestartClicked);
        }

        private void OnDisable()
        {
            if (currencyManager != null) currencyManager.OnCurrencyChanged -= HandleCurrencyChanged;
            if (baseHealth != null) baseHealth.OnHealthChanged -= HandleBaseHealthChanged;

            if (waveManager != null)
            {
                waveManager.OnWaveStarted -= HandleWaveStarted;
                waveManager.OnWaveCompleted -= HandleWaveCompleted;
            }

            if (gameManager != null) gameManager.OnStateChanged -= HandleStateChanged;
            if (selectionController != null) selectionController.OnSelectionChanged -= HandleSelectionChanged;

            if (startWaveButton != null) startWaveButton.onClick.RemoveListener(HandleStartWaveClicked);
            if (pauseButton != null) pauseButton.onClick.RemoveListener(HandlePauseClicked);
            if (restartButton != null) restartButton.onClick.RemoveListener(HandleRestartClicked);
        }

        private void Start()
        {
            if (endPanel != null) endPanel.SetActive(false);
            if (selectionPanel != null) selectionPanel.SetActive(false);
            UpdateWaveStatusText("Press Start Wave");
        }

        private void HandleCurrencyChanged(int amount)
        {
            if (currencyText != null) currencyText.text = $"Gold: {amount}";
        }

        private void HandleBaseHealthChanged(int current, int max)
        {
            if (baseHealthText != null) baseHealthText.text = $"Base HP: {current}/{max}";
        }

        private void HandleWaveStarted(int waveNumber)
        {
            UpdateWaveStatusText($"Wave {waveNumber} incoming...");
        }

        private void HandleWaveCompleted(int waveNumber)
        {
            UpdateWaveStatusText($"Wave {waveNumber} cleared!");
        }

        private void UpdateWaveStatusText(string message)
        {
            if (waveStatusText != null) waveStatusText.text = message;
        }

        private void HandleStateChanged(GameState state)
        {
            switch (state)
            {
                case GameState.Paused:
                    if (pauseButtonLabel != null) pauseButtonLabel.text = "Resume";
                    break;
                case GameState.Playing:
                    if (pauseButtonLabel != null) pauseButtonLabel.text = "Pause";
                    break;
                case GameState.Won:
                    ShowEndPanel("VICTORY", new Color(0.83f, 0.69f, 0.22f));
                    break;
                case GameState.Lost:
                    ShowEndPanel("GAME OVER", new Color(0.72f, 0.14f, 0.10f));
                    break;
            }
        }

        private void ShowEndPanel(string message, Color color)
        {
            UpdateWaveStatusText(message);

            if (endPanel != null) endPanel.SetActive(true);

            if (endPanelText != null)
            {
                endPanelText.text = message;
                endPanelText.color = color;
            }

            if (startWaveButton != null) startWaveButton.interactable = false;
            if (pauseButton != null) pauseButton.interactable = false;
        }

        private void HandleSelectionChanged(TowerController tower)
        {
            if (tower == null)
            {
                if (selectionPanel != null) selectionPanel.SetActive(false);
                return;
            }

            if (selectionPanel != null) selectionPanel.SetActive(true);

            if (selectionText != null)
            {
                string splashLine = tower.SplashRadius > 0f ? $"\nSplash: {tower.SplashRadius:0.0}" : string.Empty;
                selectionText.text = $"{tower.DisplayName}\nRange: {tower.Range:0.0}\nDamage: {tower.Damage}\nInterval: {tower.AttackInterval:0.0}s{splashLine}";
            }
        }

        private void HandleStartWaveClicked() => gameManager?.RequestStartWave();
        private void HandlePauseClicked() => gameManager?.TogglePause();
        private void HandleRestartClicked() => gameManager?.Restart();
    }
}
