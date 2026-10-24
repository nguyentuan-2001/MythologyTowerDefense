using UnityEngine;

public class TowerPlacementPad : MonoBehaviour
{
    [SerializeField] private GameObject towerPrefab;
    [SerializeField] private int cost = 50;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Renderer padRenderer;
    [SerializeField] private Color availableColor = new Color(0.85f, 0.7f, 0.3f);
    [SerializeField] private Color occupiedColor = new Color(0.4f, 0.4f, 0.4f);

    public bool IsOccupied { get; private set; }

    private void Awake()
    {
        ApplyColor();
    }

    public bool TryPlaceTower()
    {
        if (IsOccupied || towerPrefab == null || GameManager.Instance == null)
        {
            return false;
        }

        if (!GameManager.Instance.SpendCurrency(cost))
        {
            return false;
        }

        Vector3 pos = spawnPoint != null ? spawnPoint.position : transform.position;
        Instantiate(towerPrefab, pos, Quaternion.identity);
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
