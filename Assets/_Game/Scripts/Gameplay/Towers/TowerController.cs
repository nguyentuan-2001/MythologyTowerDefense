using UnityEngine;
using MythologyTowerDefense.Gameplay.Enemies;
using MythologyTowerDefense.Gameplay.Waves;

namespace MythologyTowerDefense.Gameplay.Towers
{
    public class TowerController : MonoBehaviour
    {
        [Header("Identity")]
        [SerializeField] private string displayName = "Celestial Archer";

        [Header("Combat")]
        [SerializeField] private float range = 4.5f;
        [SerializeField] private float attackInterval = 0.8f;
        [SerializeField] private int damage = 12;
        [SerializeField] private float projectileSpeed = 8f;
        [SerializeField] private float splashRadius = 0f;

        [Header("References")]
        [SerializeField] private Transform firePoint;
        [SerializeField] private Transform rotatingHead;
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private WaveManager waveManager;
        [SerializeField] private GameObject rangeIndicator;

        public string DisplayName => displayName;
        public float Range => range;
        public int Damage => damage;
        public float AttackInterval => attackInterval;
        public float SplashRadius => splashRadius;

        private float cooldown;

        /// <summary>
        /// Towers are instantiated at runtime from a prefab, so the scene-only WaveManager
        /// reference can't be baked into the prefab asset. The spawning TowerPad calls this
        /// right after Instantiate, mirroring how WaveManager.SpawnEnemy initializes enemies.
        /// </summary>
        public void Initialize(WaveManager wave)
        {
            waveManager = wave;
        }

        private void Update()
        {
            cooldown -= Time.deltaTime;

            EnemyController target = FindTarget();
            if (target == null)
            {
                return;
            }

            AimAt(target.transform.position);

            if (cooldown <= 0f)
            {
                Shoot(target);
                cooldown = attackInterval;
            }
        }

        /// <summary>
        /// Target priority: the enemy furthest along the route (closest to the base) within range.
        /// Deterministic — ties resolve to whichever enemy is first in the active-enemy list.
        /// Range is measured from the tower's own base (transform.position), matching the visible
        /// range indicator disc — NOT from firePoint, which moves as the head rotates to aim and
        /// would otherwise make "in range" depend on which way the turret currently happens to face.
        /// </summary>
        private EnemyController FindTarget()
        {
            if (waveManager == null || firePoint == null)
            {
                return null;
            }

            EnemyController best = null;
            float bestProgress = -1f;
            float rangeSq = range * range;

            foreach (EnemyController enemy in waveManager.ActiveEnemies)
            {
                if (enemy == null)
                {
                    continue;
                }

                float distSq = (enemy.transform.position - transform.position).sqrMagnitude;
                if (distSq > rangeSq)
                {
                    continue;
                }

                if (enemy.DistanceTraveled > bestProgress)
                {
                    bestProgress = enemy.DistanceTraveled;
                    best = enemy;
                }
            }

            return best;
        }

        private void AimAt(Vector3 worldPosition)
        {
            if (rotatingHead == null)
            {
                return;
            }

            Vector3 dir = worldPosition - rotatingHead.position;
            dir.y = 0f;

            if (dir.sqrMagnitude < 0.0001f)
            {
                return;
            }

            rotatingHead.rotation = Quaternion.LookRotation(dir);
        }

        private void Shoot(EnemyController target)
        {
            if (projectilePrefab == null || firePoint == null)
            {
                return;
            }

            GameObject projectileGO = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
            ProjectileController projectile = projectileGO.GetComponent<ProjectileController>();
            if (projectile != null)
            {
                projectile.Launch(target, damage, projectileSpeed, splashRadius, waveManager);
            }
        }

        public void SetSelected(bool selected)
        {
            if (rangeIndicator != null)
            {
                rangeIndicator.SetActive(selected);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, range);
        }
    }
}
