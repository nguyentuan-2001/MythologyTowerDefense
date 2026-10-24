using UnityEngine;

public class Tower : MonoBehaviour
{
    [Header("Combat")]
    [SerializeField] private float range = 6f;
    [SerializeField] private float fireRate = 1f;
    [SerializeField] private int damage = 4;
    [SerializeField] private float projectileSpeed = 14f;

    [Header("References")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private Transform rotatingHead;
    [SerializeField] private GameObject projectilePrefab;

    private float cooldown;

    private void Update()
    {
        cooldown -= Time.deltaTime;

        Enemy target = FindNearestEnemyInRange();
        if (target == null)
        {
            return;
        }

        AimAt(target.transform.position);

        if (cooldown <= 0f)
        {
            Shoot(target);
            cooldown = 1f / Mathf.Max(0.01f, fireRate);
        }
    }

    private Enemy FindNearestEnemyInRange()
    {
        if (GameManager.Instance == null)
        {
            return null;
        }

        Enemy nearest = null;
        float nearestDistSq = range * range;

        foreach (Enemy enemy in GameManager.Instance.ActiveEnemies)
        {
            if (enemy == null)
            {
                continue;
            }

            float distSq = (enemy.transform.position - firePoint.position).sqrMagnitude;
            if (distSq <= nearestDistSq)
            {
                nearestDistSq = distSq;
                nearest = enemy;
            }
        }

        return nearest;
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

    private void Shoot(Enemy target)
    {
        if (projectilePrefab == null || firePoint == null)
        {
            return;
        }

        GameObject projGO = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        Projectile projectile = projGO.GetComponent<Projectile>();
        if (projectile != null)
        {
            projectile.Init(target, damage, projectileSpeed);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}
