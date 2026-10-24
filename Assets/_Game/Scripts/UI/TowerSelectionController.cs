using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using MythologyTowerDefense.Gameplay.Towers;

namespace MythologyTowerDefense.UI
{
    /// <summary>
    /// Handles pad clicks (build or select) and tower selection/deselection via the new Input System.
    /// </summary>
    public class TowerSelectionController : MonoBehaviour
    {
        [SerializeField] private Camera targetCamera;
        [SerializeField] private LayerMask raycastMask = ~0;

        public TowerController SelectedTower { get; private set; }

        public event Action<TowerController> OnSelectionChanged;

        private void Awake()
        {
            if (targetCamera == null)
            {
                targetCamera = Camera.main;
            }
        }

        private void Update()
        {
            if (Mouse.current == null || targetCamera == null)
            {
                return;
            }

            if (!Mouse.current.leftButton.wasPressedThisFrame)
            {
                return;
            }

            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            Vector2 screenPos = Mouse.current.position.ReadValue();
            Ray ray = targetCamera.ScreenPointToRay(screenPos);

            if (Physics.Raycast(ray, out RaycastHit hit, 500f, raycastMask))
            {
                TowerPad pad = hit.collider.GetComponentInParent<TowerPad>();
                if (pad != null)
                {
                    HandlePadClick(pad);
                    return;
                }
            }

            Deselect();
        }

        private void HandlePadClick(TowerPad pad)
        {
            if (pad.IsOccupied)
            {
                Select(pad.PlacedTower);
                return;
            }

            if (pad.TryPlaceTower())
            {
                Select(pad.PlacedTower);
            }
        }

        private void Select(TowerController tower)
        {
            if (SelectedTower == tower)
            {
                return;
            }

            if (SelectedTower != null)
            {
                SelectedTower.SetSelected(false);
            }

            SelectedTower = tower;

            if (SelectedTower != null)
            {
                SelectedTower.SetSelected(true);
            }

            OnSelectionChanged?.Invoke(SelectedTower);
        }

        private void Deselect()
        {
            if (SelectedTower == null)
            {
                return;
            }

            SelectedTower.SetSelected(false);
            SelectedTower = null;
            OnSelectionChanged?.Invoke(null);
        }
    }
}
