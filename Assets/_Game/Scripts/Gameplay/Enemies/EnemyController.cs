using System;
using UnityEngine;
using MythologyTowerDefense.Gameplay.Path;

namespace MythologyTowerDefense.Gameplay.Enemies
{
    public class EnemyController : MonoBehaviour
    {
        [Header("Stats")]
        [SerializeField] private float moveSpeed = 2.0f;
        [SerializeField] private int maxHealth = 40;
        [SerializeField] private int currencyReward = 8;
        [SerializeField] private int baseDamage = 1;

        [Header("Optional Health Bar")]
        [SerializeField] private Transform healthBarFill;

        /// <summary>Raised exactly once when health reaches zero. Passes the currency reward.</summary>
        public event Action<EnemyController, int> OnDefeated;

        /// <summary>Raised exactly once when the enemy reaches the final waypoint. Passes base damage.</summary>
        public event Action<EnemyController, int> OnReachedBase;

        /// <summary>Cumulative distance walked along the route. Used by towers to prioritize the enemy closest to the base.</summary>
        public float DistanceTraveled { get; private set; }

        private PathDefinition path;
        private int waypointIndex;
        private int currentHealth;
        private bool isResolved;

        public void Initialize(PathDefinition pathDefinition)
        {
            path = pathDefinition;
            currentHealth = maxHealth;
            waypointIndex = 0;
            DistanceTraveled = 0f;
            isResolved = false;

            if (path != null && path.WaypointCount > 0)
            {
                transform.position = path.GetWaypoint(0);
            }

            UpdateHealthBar();
        }

        private void Update()
        {
            if (isResolved || path == null)
            {
                return;
            }

            if (waypointIndex >= path.WaypointCount)
            {
                ResolveReachedBase();
                return;
            }

            Vector3 targetPos = path.GetWaypoint(waypointIndex);
            Vector3 toTarget = targetPos - transform.position;
            float step = moveSpeed * Time.deltaTime;

            if (toTarget.sqrMagnitude <= step * step)
            {
                DistanceTraveled += toTarget.magnitude;
                transform.position = targetPos;
                waypointIndex++;
            }
            else
            {
                Vector3 direction = toTarget.normalized;
                transform.position += direction * step;
                DistanceTraveled += step;

                if (direction.sqrMagnitude > 0.0001f)
                {
                    transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
                }
            }
        }

        public void TakeDamage(int amount)
        {
            if (isResolved || amount <= 0)
            {
                return;
            }

            currentHealth -= amount;
            UpdateHealthBar();

            if (currentHealth <= 0)
            {
                ResolveDefeated();
            }
        }

        private void UpdateHealthBar()
        {
            if (healthBarFill == null)
            {
                return;
            }

            float ratio = Mathf.Clamp01((float)currentHealth / maxHealth);
            Vector3 scale = healthBarFill.localScale;
            scale.x = ratio;
            healthBarFill.localScale = scale;
        }

        private void ResolveDefeated()
        {
            isResolved = true;
            OnDefeated?.Invoke(this, currencyReward);
            Destroy(gameObject);
        }

        private void ResolveReachedBase()
        {
            isResolved = true;
            OnReachedBase?.Invoke(this, baseDamage);
            Destroy(gameObject);
        }
    }
}
