using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Rigidbody))]
public class PlayerClickMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;
    public float stoppingDistance = 0.1f;
    
    [Header("References")]
    public LayerMask groundLayer;
    public Transform playerModel;
    
    [Header("Animation")]
    public string idleAnimationName = "Idle";
    public string runAnimationName = "Run";
    public float animationBlendSpeed = 0.2f;
    
    // Components
    private Rigidbody rb;
    private Animator animator;
    
    // Movement variables
    private Vector3 targetPosition;
    private bool isMoving = false;
    private bool isMouseHeld = false;
    
    // Animation variables
    private int speedParameterHash;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponentInChildren<Animator>();
        
        // Cache animation parameter hash for improved performance
        speedParameterHash = Animator.StringToHash("Speed");
        
        // Configurações do Rigidbody
        rb.freezeRotation = true; // Impede que a física rotacione o personagem
        
        // Inicializa a posição alvo como a posição atual
        targetPosition = transform.position;
    }
    private void Update()
{
    if (Input.GetMouseButtonDown(0))
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;

        if (EventSystem.current.IsPointerOverGameObject()) return;
        isMouseHeld = true;
    }

    if (Input.GetMouseButtonUp(0))
    {
        isMouseHeld = false;
    }

    if (isMouseHeld)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, groundLayer))
        {
            targetPosition = hit.point;
            isMoving = true;

            CreateClickEffect(hit.point); // opcional: marque visualmente o destino
        }
    }

    UpdateAnimation();
}
 /**   private void Update()
    {
        // Verifica se o jogador clicou com o botão esquerdo do mouse
        if (Input.GetMouseButtonDown(0))
        {
            if (Input.GetMouseButtonDown(0))
            {
                // Ignora o clique se estiver sobre um elemento de UI
            if (EventSystem.current.IsPointerOverGameObject())
        return;

    // ... restante do código de raycast e movimentação
}

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            
            // Realiza o raycast para detectar onde o jogador clicou
            if (Physics.Raycast(ray, out hit, 100f, groundLayer))
            {
                // Define o novo destino
                targetPosition = hit.point;
                isMoving = true;
                
                // Efeito visual opcional (feedback do clique)
                CreateClickEffect(hit.point);
            }
        }
        
        // Anima o personagem
        UpdateAnimation();
    }**/
    
    private void FixedUpdate()
    {
        // Movimento do personagem usando física
        MoveCharacter();
    }
    
    private void MoveCharacter()
    {
        if (!isMoving)
            return;
            
        // Calcula a distância até o alvo
        Vector3 direction = targetPosition - transform.position;
        direction.y = 0; // Mantém o movimento no plano horizontal
        float distance = direction.magnitude;
        
        // Se chegou perto o suficiente do destino, para de mover
        if (distance <= stoppingDistance)
        {
            rb.linearVelocity = Vector3.zero;
            isMoving = false;
            return;
        }
        
        // Normaliza a direção e aplica a velocidade
        direction.Normalize();
        Vector3 targetVelocity = direction * moveSpeed;
        
        // Aplica a velocidade ao Rigidbody
        rb.linearVelocity = new Vector3(targetVelocity.x, rb.linearVelocity.y, targetVelocity.z);
        
        // Rotaciona o modelo para a direção do movimento
        if (playerModel != null && direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            playerModel.rotation = Quaternion.Slerp(playerModel.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }
    }
    
    private void UpdateAnimation()
    {
        if (animator != null)
        {
            // Calcula a velocidade normalizada para a animação
            float currentSpeed = rb.linearVelocity.magnitude / moveSpeed;
            
            // Aplica a velocidade ao parâmetro de animação (com transição suave)
            animator.SetFloat(speedParameterHash, currentSpeed, animationBlendSpeed, Time.deltaTime);
        }
    }
    
    private void CreateClickEffect(Vector3 position)
    {
        // Você pode adicionar um efeito visual quando o jogador clica
        // Por exemplo, um círculo, partículas, etc.
        Debug.Log("Clicked at: " + position);
        
        // Exemplo de efeito simples (você pode substituir por um prefab de partículas)
        GameObject clickMarker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        clickMarker.transform.position = new Vector3(position.x, position.y + 0.1f, position.z);
        clickMarker.transform.localScale = new Vector3(0.3f, 0.1f, 0.3f);
        
        // Remove o collider para não interferir com cliques futuros
        Destroy(clickMarker.GetComponent<Collider>());
        
        // Opcional: adiciona um material semi-transparente
        Renderer renderer = clickMarker.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = new Color(1f, 1f, 0f, 0.5f); // Amarelo semi-transparente
        }
        
        // Destrói o marcador após alguns segundos
        Destroy(clickMarker, 1f);
    }
}
