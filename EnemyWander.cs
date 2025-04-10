using UnityEngine;
[RequireComponent(typeof(Rigidbody))]
public class EnemyWander : MonoBehaviour
{
    [Header("Wander Settings")]
    public float moveSpeed = 2f;
    public float rotationSpeed = 5f;
    public float moveDuration = 2f;
    public float waitDuration = 2f;
    [Header("Detection")]
    public LayerMask groundLayer;
    
    private Rigidbody rb;
    private Animator animator;
    private Vector3 moveDirection;
    private float timer;
    private bool isMoving;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponentInChildren<Animator>();
        rb.freezeRotation = true;
        PickNewDirection();
    }
    
    private void Update()
    {
        timer -= Time.deltaTime;
        if (isMoving)
        {
            // Movimento contínuo
            rb.linearVelocity = moveDirection * moveSpeed + new Vector3(0, rb.linearVelocity.y, 0);
            // Rotaciona suavemente para a direção atual
            if (moveDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
            if (timer <= 0f)
            {
                isMoving = false;
                timer = waitDuration;
                rb.linearVelocity = Vector3.zero;
                
                // Ativa animação idle
                if (animator) animator.SetFloat("Speed", 0);
            }
        }
        else
        {
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0); // mantém a gravidade
            if (timer <= 0f)
            {
                PickNewDirection();
                isMoving = true;
                timer = moveDuration;
                
                // Ativa animação de movimento
                if (animator) animator.SetFloat("Speed", 1);
            }
        }
    }
    
    private void PickNewDirection()
    {
        // Gira aleatoriamente no plano horizontal
        float angle = Random.Range(0f, 360f);
        Vector3 newDir = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)).normalized;
        moveDirection = newDir;
    }
}