using UnityEngine;
using MythologyTowerDefense.Gameplay.Economy;
using MythologyTowerDefense.Gameplay.Waves;

namespace MythologyTowerDefense.Gameplay.Towers
{
    /// <summary>
    /// A buildable slot beside the road. Which tower gets built here is chosen by the player
    /// (via TowerBuildMenuController) at click time, not fixed per pad — any pad can host any
    /// tower in the catalog.
    /// </summary>
    public class TowerPad : MonoBehaviour
    {
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private Renderer padRenderer;
        [SerializeField] private CurrencyManager currencyManager;
        [SerializeField] private WaveManager waveManager;
        [SerializeField] private Color availableColor = new Color(0.83f, 0.69f, 0.22f);
        [SerializeField] private Color occupiedColor = new Color(0.17f, 0.35f, 0.32f);

        public bool IsOccupied { get; private set; }
        public TowerController PlacedTower { get; private set; }

        private void Awake()
        {
            ApplyColor();
        }

        public bool TryPlaceTower(GameObject towerPrefab, int cost)
        {
            if (IsOccupied || towerPrefab == null || currencyManager == null)
            {
                return false;
            }

            if (!currencyManager.TrySpend(cost))
            {
                return false;
            }

            Vector3 position = spawnPoint != null ? spawnPoint.position : transform.position;
            GameObject instance = Instantiate(towerPrefab, position, Quaternion.identity);
            PlacedTower = instance.GetComponent<TowerController>();
            PlacedTower?.Initialize(waveManager);
            IsOccupied = true;
            ApplyColor();
            return true;
        }

        private void ApplyColor()
        {
            if (padRenderer == null)
            {
                return;
            }

            padRenderer.material.color = IsOccupied ? occupiedColor : availableColor;
        }
    }
}
