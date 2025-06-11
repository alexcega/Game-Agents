using UnityEngine;

public class hockeymovement : MonoBehaviour
{
    [Header("Drag Settings")]
    [SerializeField] LayerMask draggableLayer;

    [Header("Movement Area")]
    [Tooltip("BoxCollider (IsTrigger) defining allowed play zone")]
    [SerializeField] BoxCollider movementArea;

    private float fixedY;
    private Rigidbody  rb;
    private Vector3 offset;
    private Camera mainCamera;
    private bool isDragging = false;

    void Start()
    {
        mainCamera = Camera.main;
        rb = GetComponent<Rigidbody>();

        rb.constraints = 
            RigidbodyConstraints.FreezePositionY |
            RigidbodyConstraints.FreezeRotationX |
            RigidbodyConstraints.FreezeRotationZ;

        fixedY = transform.position.y; // Save original Y position
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, draggableLayer))
            {
                if (hit.transform == transform)
                {
                    isDragging = true;
                    offset = transform.position - GetMouseWorldPosition();
                }
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }
    }

    void FixedUpdate()
    {
        if (isDragging)
        {
            Vector3 targetPosition = GetMouseWorldPosition() + offset;
            targetPosition.y = fixedY;

            // limit position to movement area
            Vector3 clamped = movementArea.ClosestPoint(targetPosition);
            targetPosition.x = clamped.x;
            targetPosition.z = clamped.z;

            rb.MovePosition(targetPosition);
        }
    }

    // Get mouse position in world on the same Y plane as object
    Vector3 GetMouseWorldPosition()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);


        Plane plane = new Plane(Vector3.up, new Vector3(0, fixedY, 0));

        if (plane.Raycast(ray, out float distance))
        {
            return ray.GetPoint(distance);
        }

        return Vector3.zero;
    }
}
