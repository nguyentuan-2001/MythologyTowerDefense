using UnityEngine;

namespace MythologyTowerDefense.Gameplay.Path
{
    public class PathDefinition : MonoBehaviour
    {
        [SerializeField] private Transform[] waypoints;
        [SerializeField] private Color gizmoColor = new Color(0.83f, 0.69f, 0.22f);

        public int WaypointCount => waypoints != null ? waypoints.Length : 0;

        public Vector3 StartPosition => WaypointCount > 0 ? waypoints[0].position : transform.position;

        public Vector3 GetWaypoint(int index)
        {
            return waypoints[index].position;
        }

        private void OnDrawGizmos()
        {
            if (waypoints == null || waypoints.Length < 2)
            {
                return;
            }

            Gizmos.color = gizmoColor;

            for (int i = 0; i < waypoints.Length; i++)
            {
                if (waypoints[i] == null)
                {
                    continue;
                }

                Gizmos.DrawSphere(waypoints[i].position, 0.3f);

                if (i < waypoints.Length - 1 && waypoints[i + 1] != null)
                {
                    Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
                }
            }
        }
    }
}
