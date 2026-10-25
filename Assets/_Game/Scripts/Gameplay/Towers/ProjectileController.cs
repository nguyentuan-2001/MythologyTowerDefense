using System.Collections.Generic;
using UnityEngine;
using MythologyTowerDefense.Gameplay.Enemies;
using MythologyTowerDefense.Gameplay.Waves;

namespace MythologyTowerDefense.Gameplay.Towers
{
    public class ProjectileController : MonoBehaviour
    {
        private const float HitDistance = 0.3f;

        private EnemyController target;
        private int damage;
        private float speed;
        private float splashRadius;
        private WaveManager waveManager;
        private Vector3 lastKnownPosition;

        /// <summary>
        /// splashRadius 0 (default) damages only the locked target, matching single-target towers.
        /// A positive splashRadius damages every enemy within that radius of the impact point
        /// (the target included), for area-damage towers like the Pangu Titan.
        /// </summary>
        public void Launch(EnemyController targetEnemy, int damageAmount, float moveSpeed, float splash = 0f, WaveManager wave = null)
        {
            target = targetEnemy;
            damage = damageAmount;
            speed = moveSpeed;
            splashRadius = splash;
            waveManager = wave;
            lastKnownPosition = target != null ? target.transform.position : transform.position;
        }

        private void Update()
        {
            if (target != null)
            {
                lastKnownPosition = target.transform.position;
            }

            transform.position = Vector3.MoveTowards(transform.position, lastKnownPosition, speed * Time.deltaTime);

            if (Vector3.Distance(transform.position, lastKnownPosition) <= HitDistance)
            {
                Impact();
                Destroy(gameObject);
            }
        }

        private void Impact()
        {
            if (splashRadius <= 0f || waveManager == null)
            {
                if (target != null)
                {
                    target.TakeDamage(damage);
                }

                return;
            }

            float splashSq = splashRadius * splashRadius;

            // Snapshot first: TakeDamage can trigger WaveManager's defeat handler, which mutates
            // the live ActiveEnemies list — iterating that list directly while damaging would skip entries.
            List<EnemyController> enemiesInRange = new List<EnemyController>(waveManager.ActiveEnemies);

            foreach (EnemyController enemy in enemiesInRange)
            {
                if (enemy == null)
                {
                    continue;
                }

                if ((enemy.transform.position - lastKnownPosition).sqrMagnitude <= splashSq)
                {
                    enemy.TakeDamage(damage);
                }
            }
        }
    }
}
