using UnityEngine;
using System.Collections;

public class Goalsensor : MonoBehaviour
{
    [Tooltip("Which player gets the point when this goal is scored?")]
    public int playerId;

    [Tooltip("Where the agent script lives to notify of the goal")]
    public hockeymovement playerAgent; 

    [Tooltip("Delay before awarding score and respawning puck")]
    public float respawnDelay = 0.5f;

    [Tooltip("Where the puck should reappear for the opponent")]
    public Transform respawnPoint;

    // Prevent double-triggering
    bool goalInProgress = false;

    void OnTriggerEnter(Collider other)
    {
        if (goalInProgress) return;
        
        // Assumes your puck GameObject is tagged "Puck"
        if (other.CompareTag("puck"))
        {
            goalInProgress = true;
            StartCoroutine(HandleGoal(other.attachedRigidbody));
        }
    }

    IEnumerator HandleGoal(Rigidbody puckRb)
    {
        // Optional: play goal sound/animation here

        // Wait the half-second
        yield return new WaitForSeconds(respawnDelay);

        // // 1) Update the score
        // ScoreManager.Instance.AddPointToPlayer(playerId);

        // 2) Teleport puck to respawn point and zero its velocity
        puckRb.position = respawnPoint.position;
        puckRb.linearVelocity = Vector3.zero;
        puckRb.angularVelocity = Vector3.zero;

        // Reset flag so next goal can be detected
        goalInProgress = false;
    }
}
