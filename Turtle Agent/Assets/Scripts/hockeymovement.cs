using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System.Collections;
using System.Collections.Generic;

public class hockeymovement : Agent
{
    [Header("Drag Settings")]
    [SerializeField] LayerMask draggableLayer;

    [Header("Movement Area")]
    [Tooltip("BoxCollider (IsTrigger) defining allowed play zone")]
    [SerializeField] BoxCollider movementArea;
    [Header("Ground Flash Settings")]
    [SerializeField] private Renderer _groundRenderer;
    [SerializeField] private float _flashFadeDuration = 0.8f;
    [Header("Player settings")]
    [SerializeField] private float _moveSpeed = 1.5f;
    [SerializeField] private float _rotationSpeed = 180f;

    [SerializeField] private float _forceMagnitude = 2f;

    private Renderer _renderer;
    private float fixedY;
    private Rigidbody rb;
    private Vector3 offset;
    private Camera mainCamera;
    private bool isDragging = false;
    private Color _defaultGroundColor;
    private Coroutine _flashGroundColorCoroutine;
    [HideInInspector] public int CurrentEpisode = 0;
    [HideInInspector] public float CumulativeReward = 0f;

    public override void Initialize()
    {
        _renderer = GetComponent<Renderer>();
        CurrentEpisode = 0;
        CumulativeReward = 0f;

        if (_groundRenderer != null)
        {
            _defaultGroundColor = _groundRenderer.material.color;
        }
    }

    public override void OnEpisodeBegin()
    {
        // Flash the ground green or red based on last episode's reward
        if (_groundRenderer != null && CumulativeReward != 0f)
        {
            // Choose flash color
            Color target = CumulativeReward > 0 ? Color.green : Color.red;

            // Stop any still‐running flash
            if (_flashGroundColorCoroutine != null)
            {
                StopCoroutine(_flashGroundColorCoroutine);
            }
            // Start our lerp flash
            _flashGroundColorCoroutine = StartCoroutine(
                FlashGroundColor(target, _flashFadeDuration)
            );
        }

        // Prepare for next episode
        CurrentEpisode++;
        CumulativeReward = 0f;
        // _renderer.material.color = Color.blue;  // reset agent color, if you like


    }
    private IEnumerator FlashGroundColor(Color flashColor, float duration)
    {
        float elapsedTime = 0f;
        _groundRenderer.material.color = flashColor;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            _groundRenderer.material.color = Color.Lerp(flashColor, _defaultGroundColor, elapsedTime / duration);
            yield return null; // Wait for the next frame
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // Example movement (replace with your own logic)
        float move = actions.DiscreteActions[0] == 1 ? 1f : 0f;
        float turn = actions.DiscreteActions[1] == 1 ? -1f
                    : actions.DiscreteActions[1] == 2 ? +1f : 0f;

        transform.Translate(Vector3.forward * move * _moveSpeed * Time.deltaTime, Space.Self);
        transform.Rotate(Vector3.up * turn * _rotationSpeed * Time.deltaTime, Space.Self);

        // Reward shaping, termination, etc...
    }

    public void GoalReached(int scoringPlayerId)
    {
        // scoringPlayerId is the same as playerId on the GoalSensor.
        // If you need to distinguish self vs opponent, compare against your own ID:
        if (scoringPlayerId == 1)
            AddReward(+1f);
        else
            AddReward(-1f);

        EndEpisode();
    }

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

 


    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            // Apply a small negative reward when the collision starts
            AddReward(-0.05f);
        }
        if (collision.gameObject.CompareTag("puck"))
        {
            // Apply a small positive reward when hitting the puck
            AddReward(0.1f);
        }
    }



    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            // Continually penalize the agent while it is in contact with the wall
            AddReward(-0.01f * Time.fixedDeltaTime);
        }
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
    
        public void MoveAgent(ActionSegment<int> act){
        var action = act[0];
        switch(action){
            case 1: // Move forward
                transform.position += transform.forward * _moveSpeed * Time.deltaTime;
                break;
            case 2: // Rotate Left
                transform.Rotate(0f, -_rotationSpeed * Time.deltaTime, 0f);
                break;
            case 3: // Rotate right
                transform.Rotate(0f, _rotationSpeed * Time.deltaTime, 0f);
                break;
        }
    }
}
