using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PlacementInputController : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;
    [SerializeField] private LayerMask raycastMask = ~0;

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
            TowerPlacementPad pad = hit.collider.GetComponentInParent<TowerPlacementPad>();
            if (pad != null)
            {
                pad.TryPlaceTower();
            }
        }
    }
}
