using System;
using UnityEngine;

namespace MythologyTowerDefense.Gameplay.Economy
{
    public class CurrencyManager : MonoBehaviour
    {
        [SerializeField] private int startingCurrency = 60;

        public int Currency { get; private set; }

        public event Action<int> OnCurrencyChanged;

        private void Awake()
        {
            Currency = startingCurrency;
        }

        private void Start()
        {
            OnCurrencyChanged?.Invoke(Currency);
        }

        public void Add(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            Currency += amount;
            OnCurrencyChanged?.Invoke(Currency);
        }

        public bool TrySpend(int amount)
        {
            if (amount < 0 || Currency < amount)
            {
                return false;
            }

            Currency -= amount;
            OnCurrencyChanged?.Invoke(Currency);
            return true;
        }
    }
}
