using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private WaveSpawner waveSpawner;

    [Header("UI Elements")]
    [SerializeField] private Text currencyText;
    [SerializeField] private Text baseHealthText;
    [SerializeField] private Text waveStatusText;
    [SerializeField] private Text gameOverText;
    [SerializeField] private Button startWaveButton;
    [SerializeField] private Button pauseButton;
    [SerializeField] private Text pauseButtonLabel;
    [SerializeField] private Button restartButton;

    private void OnEnable()
    {
        if (gameManager != null)
        {
            gameManager.OnCurrencyChanged += HandleCurrencyChanged;
            gameManager.OnBaseHealthChanged += HandleBaseHealthChanged;
            gameManager.OnGameOver += HandleGameOver;
            gameManager.OnPauseChanged += HandlePauseChanged;
        }

        if (waveSpawner != null)
        {
            waveSpawner.OnWaveStarted += HandleWaveStarted;
            waveSpawner.OnWaveCompleted += HandleWaveCompleted;
        }

        if (startWaveButton != null)
        {
            startWaveButton.onClick.AddListener(HandleStartWaveClicked);
        }

        if (pauseButton != null)
        {
            pauseButton.onClick.AddListener(HandlePauseClicked);
        }

        if (restartButton != null)
        {
            restartButton.onClick.AddListener(HandleRestartClicked);
        }
    }

    private void OnDisable()
    {
        if (gameManager != null)
        {
            gameManager.OnCurrencyChanged -= HandleCurrencyChanged;
            gameManager.OnBaseHealthChanged -= HandleBaseHealthChanged;
            gameManager.OnGameOver -= HandleGameOver;
            gameManager.OnPauseChanged -= HandlePauseChanged;
        }

        if (waveSpawner != null)
        {
            waveSpawner.OnWaveStarted -= HandleWaveStarted;
            waveSpawner.OnWaveCompleted -= HandleWaveCompleted;
        }

        if (startWaveButton != null)
        {
            startWaveButton.onClick.RemoveListener(HandleStartWaveClicked);
        }

        if (pauseButton != null)
        {
            pauseButton.onClick.RemoveListener(HandlePauseClicked);
        }

        if (restartButton != null)
        {
            restartButton.onClick.RemoveListener(HandleRestartClicked);
        }
    }

    private void Start()
    {
        if (gameOverText != null)
        {
            gameOverText.gameObject.SetActive(false);
        }

        UpdateWaveStatusText("Press Start Wave");
    }

    private void HandleCurrencyChanged(int amount)
    {
        if (currencyText != null)
        {
            currencyText.text = $"Gold: {amount}";
        }
    }

    private void HandleBaseHealthChanged(int current, int max)
    {
        if (baseHealthText != null)
        {
            baseHealthText.text = $"Base HP: {current}/{max}";
        }
    }

    private void HandleWaveStarted(int waveNumber)
    {
        UpdateWaveStatusText($"Wave {waveNumber} incoming...");
    }

    private void HandleWaveCompleted(int waveNumber)
    {
        UpdateWaveStatusText($"Wave {waveNumber} cleared! Press Start Wave");
    }

    private void UpdateWaveStatusText(string message)
    {
        if (waveStatusText != null)
        {
            waveStatusText.text = message;
        }
    }

    private void HandleGameOver()
    {
        UpdateWaveStatusText("GAME OVER");

        if (gameOverText != null)
        {
            gameOverText.gameObject.SetActive(true);
        }

        if (startWaveButton != null)
        {
            startWaveButton.interactable = false;
        }

        if (pauseButton != null)
        {
            pauseButton.interactable = false;
        }
    }

    private void HandlePauseChanged(bool isPaused)
    {
        if (pauseButtonLabel != null)
        {
            pauseButtonLabel.text = isPaused ? "Resume" : "Pause";
        }
    }

    private void HandleStartWaveClicked()
    {
        if (waveSpawner != null)
        {
            waveSpawner.StartWave();
        }
    }

    private void HandlePauseClicked()
    {
        if (gameManager != null)
        {
            gameManager.TogglePause();
        }
    }

    private void HandleRestartClicked()
    {
        if (gameManager != null)
        {
            gameManager.RestartGame();
        }
    }
}
