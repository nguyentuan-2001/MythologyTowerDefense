using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private float moveSpeed = 3.5f;
    [SerializeField] private int maxHealth = 10;
    [SerializeField] private int currencyReward = 15;
    [SerializeField] private int damageToBase = 1;

    [Header("Health Bar (optional)")]
    [SerializeField] private Transform healthBarFill;

    private int currentHealth;
    private int waypointIndex;
    private WaypointPath path;
    private bool isDead;

    private void Start()
    {
        currentHealth = maxHealth;
        path = WaypointPath.Instance;
        waypointIndex = 0;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterEnemy(this);
        }
    }

    private void Update()
    {
        if (isDead || path == null)
        {
            return;
        }

        if (waypointIndex >= path.WaypointCount)
        {
            ReachedEnd();
            return;
        }

        Vector3 targetPos = path.GetPoint(waypointIndex);
        transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPos) < 0.1f)
        {
            waypointIndex++;
        }
    }

    public void TakeDamage(int amount)
    {
        if (isDead)
        {
            return;
        }

        currentHealth -= amount;
        UpdateHealthBar();

        if (currentHealth <= 0)
        {
            Die();
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

    private void Die()
    {
        isDead = true;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddCurrency(currencyReward);
            GameManager.Instance.UnregisterEnemy(this);
        }

        Destroy(gameObject);
    }

    private void ReachedEnd()
    {
        isDead = true;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.DamageBase(damageToBase);
            GameManager.Instance.UnregisterEnemy(this);
        }

        Destroy(gameObject);
    }
}
