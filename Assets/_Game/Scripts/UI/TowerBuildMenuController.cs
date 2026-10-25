using UnityEngine;
using UnityEngine.UI;
using MythologyTowerDefense.Gameplay.Towers;

namespace MythologyTowerDefense.UI
{
    [System.Serializable]
    public struct TowerCatalogEntry
    {
        public string displayName;
        public GameObject prefab;
        public int cost;
    }

    /// <summary>
    /// Shows a small build-choice popup when a pad is pending (see TowerSelectionController) and
    /// builds whichever catalog entry the player picks. One fixed on-screen panel is reused for
    /// whichever pad is currently pending, since only one pad can be pending at a time.
    /// </summary>
    public class TowerBuildMenuController : MonoBehaviour
    {
        [SerializeField] private TowerSelectionController selectionController;
        [SerializeField] private GameObject panel;
        [SerializeField] private Button[] optionButtons;
        [SerializeField] private Text[] optionLabels;
        [SerializeField] private TowerCatalogEntry[] catalog;

        // Cached the instant the panel is shown, NOT re-read from selectionController at click
        // time, so a stale/cleared PendingPad can never silently no-op a correctly-clicked button.
        private TowerPad cachedPendingPad;

        private void OnEnable()
        {
            if (selectionController != null)
            {
                selectionController.OnPendingPadChanged += HandlePendingPadChanged;
            }

            for (int i = 0; i < optionButtons.Length; i++)
            {
                int index = i;
                if (optionButtons[i] != null)
                {
                    optionButtons[i].onClick.AddListener(() => HandleOptionClicked(index));
                }
            }
        }

        private void OnDisable()
        {
            if (selectionController != null)
            {
                selectionController.OnPendingPadChanged -= HandlePendingPadChanged;
            }

            foreach (Button button in optionButtons)
            {
                if (button != null)
                {
                    button.onClick.RemoveAllListeners();
                }
            }
        }

        private void Start()
        {
            for (int i = 0; i < optionLabels.Length && i < catalog.Length; i++)
            {
                if (optionLabels[i] != null)
                {
                    optionLabels[i].text = $"{catalog[i].displayName}\n{catalog[i].cost}g";
                }
            }

            if (panel != null)
            {
                panel.SetActive(false);
            }
        }

        private void HandlePendingPadChanged(TowerPad pad)
        {
            cachedPendingPad = pad;

            if (panel != null)
            {
                panel.SetActive(pad != null);
                Canvas.ForceUpdateCanvases();
            }
        }

        private void HandleOptionClicked(int index)
        {
            if (cachedPendingPad == null)
            {
                return;
            }

            if (index < 0 || index >= catalog.Length)
            {
                return;
            }

            TowerCatalogEntry entry = catalog[index];

            if (cachedPendingPad.TryPlaceTower(entry.prefab, entry.cost))
            {
                selectionController.ConfirmPendingPadBuilt();
            }

            cachedPendingPad = null;
        }
    }
}
