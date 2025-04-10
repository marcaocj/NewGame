using UnityEngine;

public class IsometricCamera : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Position Settings")]
    public Vector3 offset = new Vector3(0, 10, -10);
    public float smoothSpeed = 0.125f;

    [Header("Rotation Settings")]
    public float rotationSpeed = 100f;
    private float currentAngle = 0f;

    [Header("Zoom Settings")]
    public float zoomSpeed = 4f;
    public float minZoom = 5f;
    public float maxZoom = 15f;
    public float startingZoom = 10f;

    private float currentZoom;
    private Vector3 initialOffset;

    private void Start()
    {
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                target = player.transform;
        }

        currentZoom = startingZoom;
        initialOffset = offset;
    }

    private void LateUpdate()
    {
        if (target == null)
            return;

        HandleZoom();
        HandleRotation();

        // Calcula a posição com rotação aplicada
        Quaternion rotation = Quaternion.Euler(0f, currentAngle, 0f);
        Vector3 rotatedOffset = rotation * initialOffset.normalized * currentZoom;

        Vector3 desiredPosition = target.position + rotatedOffset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        transform.position = smoothedPosition;
        transform.rotation = Quaternion.Euler(45f, currentAngle, 0f); // visão isométrica fixa em X, rotacionando em Y
    }

    private void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0f)
        {
            currentZoom -= scroll * zoomSpeed;
            currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);
        }
    }

    private void HandleRotation()
    {
        if (Input.GetMouseButton(1)) // botão direito do mouse
        {
            float mouseX = Input.GetAxis("Mouse X");
            currentAngle += mouseX * rotationSpeed * Time.deltaTime;
        }
    }
}
