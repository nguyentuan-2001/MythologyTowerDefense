using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Starting Values")]
    [SerializeField] private int startingCurrency = 150;
    [SerializeField] private int startingBaseHealth = 20;

    public int Currency { get; private set; }
    public int BaseHealth { get; private set; }
    public int MaxBaseHealth { get; private set; }
    public bool IsPaused { get; private set; }
    public bool IsGameOver { get; private set; }

    public event Action<int> OnCurrencyChanged;
    public event Action<int, int> OnBaseHealthChanged;
    public event Action OnGameOver;
    public event Action<bool> OnPauseChanged;

    public List<Enemy> ActiveEnemies { get; } = new List<Enemy>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        Currency = startingCurrency;
        MaxBaseHealth = startingBaseHealth;
        BaseHealth = startingBaseHealth;
        Time.timeScale = 1f;
    }

    private void Start()
    {
        OnCurrencyChanged?.Invoke(Currency);
        OnBaseHealthChanged?.Invoke(BaseHealth, MaxBaseHealth);
    }

    public void RegisterEnemy(Enemy enemy)
    {
        if (!ActiveEnemies.Contains(enemy))
        {
            ActiveEnemies.Add(enemy);
        }
    }

    public void UnregisterEnemy(Enemy enemy)
    {
        ActiveEnemies.Remove(enemy);
    }

    public void AddCurrency(int amount)
    {
        Currency += amount;
        OnCurrencyChanged?.Invoke(Currency);
    }

    public bool SpendCurrency(int amount)
    {
        if (Currency < amount)
        {
            return false;
        }

        Currency -= amount;
        OnCurrencyChanged?.Invoke(Currency);
        return true;
    }

    public void DamageBase(int amount)
    {
        if (IsGameOver)
        {
            return;
        }

        BaseHealth = Mathf.Max(0, BaseHealth - amount);
        OnBaseHealthChanged?.Invoke(BaseHealth, MaxBaseHealth);

        if (BaseHealth <= 0)
        {
            TriggerGameOver();
        }
    }

    private void TriggerGameOver()
    {
        IsGameOver = true;
        Time.timeScale = 0f;
        OnGameOver?.Invoke();
    }

    public void TogglePause()
    {
        if (IsGameOver)
        {
            return;
        }

        IsPaused = !IsPaused;
        Time.timeScale = IsPaused ? 0f : 1f;
        OnPauseChanged?.Invoke(IsPaused);
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
