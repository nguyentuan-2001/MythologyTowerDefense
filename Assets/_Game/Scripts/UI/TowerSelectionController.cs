using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using MythologyTowerDefense.Gameplay.Towers;

namespace MythologyTowerDefense.UI
{
    /// <summary>
    /// Handles pad clicks and tower selection/deselection via the new Input System. Clicking an
    /// empty pad doesn't build directly — it marks that pad "pending" so TowerBuildMenuController
    /// can show a build-choice popup; clicking an occupied pad selects the tower built there.
    /// </summary>
    public class TowerSelectionController : MonoBehaviour
    {
        [SerializeField] private Camera targetCamera;
        [SerializeField] private LayerMask raycastMask = ~0;

        private readonly List<RaycastResult> uiRaycastResults = new List<RaycastResult>();

        public TowerController SelectedTower { get; private set; }
        public TowerPad PendingPad { get; private set; }

        public event Action<TowerController> OnSelectionChanged;
        public event Action<TowerPad> OnPendingPadChanged;

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

            Vector2 screenPos = Mouse.current.position.ReadValue();

            if (IsPointerOverUI(screenPos))
            {
                return;
            }

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

            ClearPendingPad();
            Deselect();
        }

        /// <summary>
        /// Deliberately does NOT use EventSystem.current.IsPointerOverGameObject() — that call is
        /// keyed to a specific pointer ID and is known to misreport with InputSystemUIInputModule
        /// in some builds (observed here: WebGL), which let this script's own world-raycast fallback
        /// fire on the same click a UI button was meant to receive, deactivating the panel (and thus
        /// its buttons) before the click could reach them. A direct UI raycast at the exact screen
        /// position has no pointer-ID bookkeeping to get out of sync.
        /// </summary>
        private bool IsPointerOverUI(Vector2 screenPosition)
        {
            if (EventSystem.current == null)
            {
                return false;
            }

            PointerEventData pointerData = new PointerEventData(EventSystem.current) { position = screenPosition };
            uiRaycastResults.Clear();
            EventSystem.current.RaycastAll(pointerData, uiRaycastResults);
            return uiRaycastResults.Count > 0;
        }

        private void HandlePadClick(TowerPad pad)
        {
            if (pad.IsOccupied)
            {
                ClearPendingPad();
                Select(pad.PlacedTower);
                return;
            }

            Deselect();
            SetPendingPad(pad);
        }

        /// <summary>Called by TowerBuildMenuController after it successfully builds on PendingPad.</summary>
        public void ConfirmPendingPadBuilt()
        {
            if (PendingPad == null)
            {
                return;
            }

            TowerController built = PendingPad.PlacedTower;
            ClearPendingPad();
            Select(built);
        }

        private void SetPendingPad(TowerPad pad)
        {
            PendingPad = pad;
            OnPendingPadChanged?.Invoke(PendingPad);
        }

        private void ClearPendingPad()
        {
            if (PendingPad == null)
            {
                return;
            }

            PendingPad = null;
            OnPendingPadChanged?.Invoke(null);
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
