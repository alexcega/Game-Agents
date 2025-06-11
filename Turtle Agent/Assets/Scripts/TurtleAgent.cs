using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using System.Collections;

public class TurtleAgent : Agent
{
    [SerializeField] private Transform _goal;
    [SerializeField] private Renderer _groundRenderer;
    [SerializeField] private float _moveSpeed = 1.5f;
    [SerializeField] private float _rotationSpeed = 180f;

    private Renderer _renderer;

    [HideInInspector]public int CurrentEpisode = 0;
    [HideInInspector]public float CumulativeReward = 0;

    private Color _defaultGroundColor;
    private Coroutine _flashGroundColorCoroutine;

    public override void Initialize()
    {
        _renderer = GetComponent<Renderer>();
        CurrentEpisode=0;
        CumulativeReward = 0f;

        if (_groundRenderer != null)
        {
            _defaultGroundColor = _groundRenderer.material.color;
        }
    }

    public override void OnEpisodeBegin()
    {
        if(_groundRenderer != null && CumulativeReward != 0f)
        {
            Color flashColor = CumulativeReward > 0 ? Color.green : Color.red;
            if (_flashGroundColorCoroutine != null)
            {
                StopCoroutine(_flashGroundColorCoroutine);
            }
            _flashGroundColorCoroutine = StartCoroutine(FlashGroundColor(flashColor, 3.0f));
        }


        CurrentEpisode++;
        CumulativeReward = 0f;
        _renderer.material.color = Color.blue;
    
        SpawnObjects();
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

    public void SpawnObjects()
    {
        // Reset the agent's position
        transform.localRotation = Quaternion.identity;
        transform.localPosition = new Vector3(0f, 0.15f, 0f);

        // Randomly place the goal within a certain range
        float randomAngle  = Random.Range(0f, 360f);
        Vector3 randomDirection = Quaternion.Euler(0f, randomAngle, 0f) * Vector3.forward;

        // Randomize the distance within the range [1, 2.5]
        float randomDistance = Random.Range(1f, 2.5f);
        // Calculate the goal's position
        Vector3 goalPosition = transform.localPosition + randomDirection * randomDistance;
        // Apply the calculated position to the goal
        _goal.localPosition = new Vector3(goalPosition.x, 0.3f, goalPosition.z);
    }

    public override void CollectObservations(VectorSensor sensor)
    {   
        // The Goal's position
        float goalPosX_normalized = _goal.localPosition.x / 5f;
        float goalPosZ_normalized = _goal.localPosition.z / 5f;
        // The Turtle's position
        float turtlePosX_normalized = transform.localPosition.x / 5f;
        float turtlePosZ_normalized = transform.localPosition.z / 5f;
        // The Turtle's direction (on the Y Axis)
        float turtleRotation_normalized = (transform.localRotation.eulerAngles.y / 360f) * 2f - 1f;
        sensor.AddObservation(goalPosX_normalized);
        sensor.AddObservation(goalPosZ_normalized);
        sensor.AddObservation(turtlePosX_normalized);
        sensor.AddObservation(turtlePosZ_normalized);
        sensor.AddObservation(turtleRotation_normalized);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        // This method is used for manual control or testing.
        // Here we define the actions manually.
        var discreteActionsOut = actionsOut.DiscreteActions;
        discreteActionsOut[0] = 0; // Default action is to do nothing
        // 0: Do nothing, 1: Move forward, 2: Rotate left, 3: Rotate right
        if (Input.GetKey(KeyCode.W))
        {
            discreteActionsOut[0] = 1; // Move forward
        }
        else if (Input.GetKey(KeyCode.A))
        {
            discreteActionsOut[0] = 2; // Rotate left
        }
        else if (Input.GetKey(KeyCode.D))
        {
            discreteActionsOut[0] = 3; // Rotate right
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // Move the agent using the action.
        MoveAgent(actions.DiscreteActions);
        // Penalty given each step to encourage agent to finish task quickly.
        AddReward(-2f / MaxStep);
        // Update the cumulative reward after adding the step penalty.
        CumulativeReward = GetCumulativeReward();
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

    private void OnTriggerEnter(Collider other){
        if(other.gameObject.CompareTag ("goal")){
            GoalReached();
        }
    }

    private void GoalReached(){
        AddReward(1.0f); // Large reward for reaching the goal
        CumulativeReward = GetCumulativeReward();
        EndEpisode();
    }
    
    private void OnCollisionEnter(Collision collision){
        if (collision.gameObject.CompareTag("Wall")){
            // Apply a small negative reward when the collision starts
            AddReward(-0.05f);
             // Change the color of the TurtleAgent to red
            if (_renderer != null){
                _renderer.material.color = Color.yellow;
            }
        }
    }

    private void OnCollisionStay(Collision collision){
        if (collision.gameObject.CompareTag("Wall")){
            // Continually penalize the agent while it is in contact with the wall
            AddReward(-0.01f * Time.fixedDeltaTime);
        }
    }

    private void OnCollisionExit(Collision collision){
        if (collision.gameObject.CompareTag("Wall")){
            // Reset the color when the collision ends
            if (_renderer != null){
                // Assuming blue is the default color
                _renderer.material.color = Color.green;
            }
        }
    }



}
