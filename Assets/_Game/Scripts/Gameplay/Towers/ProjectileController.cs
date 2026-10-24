using UnityEngine;
using MythologyTowerDefense.Gameplay.Enemies;

namespace MythologyTowerDefense.Gameplay.Towers
{
    public class ProjectileController : MonoBehaviour
    {
        private const float HitDistance = 0.3f;

        private EnemyController target;
        private int damage;
        private float speed;
        private Vector3 lastKnownPosition;

        public void Launch(EnemyController targetEnemy, int damageAmount, float moveSpeed)
        {
            target = targetEnemy;
            damage = damageAmount;
            speed = moveSpeed;
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
                if (target != null)
                {
                    target.TakeDamage(damage);
                }

                Destroy(gameObject);
            }
        }
    }
}
