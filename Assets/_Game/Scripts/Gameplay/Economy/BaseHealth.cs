using System;
using UnityEngine;

namespace MythologyTowerDefense.Gameplay.Economy
{
    public class BaseHealth : MonoBehaviour
    {
        [SerializeField] private int startingHealth = 10;

        public int Health { get; private set; }
        public int MaxHealth { get; private set; }

        public event Action<int, int> OnHealthChanged;

        /// <summary>Raised exactly once when health first reaches zero.</summary>
        public event Action OnDepleted;

        private bool isDepleted;

        private void Awake()
        {
            MaxHealth = startingHealth;
            Health = startingHealth;
        }

        private void Start()
        {
            OnHealthChanged?.Invoke(Health, MaxHealth);
        }

        public void TakeDamage(int amount)
        {
            if (isDepleted || amount <= 0)
            {
                return;
            }

            Health = Mathf.Max(0, Health - amount);
            OnHealthChanged?.Invoke(Health, MaxHealth);

            if (Health <= 0)
            {
                isDepleted = true;
                OnDepleted?.Invoke();
            }
        }
    }
}
