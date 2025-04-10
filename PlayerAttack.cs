using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public float attackRange = 2f;
    public float attackCooldown = 1f;
    public float baseDamage = 10f;

    private PlayerStats stats;
    private float lastAttackTime;
    private Camera cam;

    private void Start()
    {
        cam = Camera.main;
        stats = GetComponent<PlayerStats>();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && Time.time >= lastAttackTime + attackCooldown)
        {
            TryAttack();
        }
    }

    private void TryAttack()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            EnemyStats enemy = hit.collider.GetComponent<EnemyStats>();

            if (enemy != null && Vector3.Distance(transform.position, enemy.transform.position) <= attackRange)
            {
                float finalDamage = baseDamage + (stats.strength * 2); // dano baseado na força
                enemy.TakeDamage(finalDamage);
                lastAttackTime = Time.time;

                Debug.Log($"Player atacou causando {finalDamage} de dano!");
            }
        }
    }
}
