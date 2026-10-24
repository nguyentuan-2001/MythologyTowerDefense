using UnityEngine;

public class WaypointPath : MonoBehaviour
{
    public static WaypointPath Instance { get; private set; }

    [SerializeField] private Transform[] waypoints;

    public int WaypointCount => waypoints != null ? waypoints.Length : 0;

    private void Awake()
    {
        Instance = this;
    }

    public Vector3 GetPoint(int index)
    {
        return waypoints[index].position;
    }
}
