using UnityEngine;
using UnityEngine.UI;
public class ArenaManager : MonoBehaviour
{

    [Header("Agents & Puck")]
    public hockeymovement[] players;    // size == 2
    public Rigidbody    puckRb;
    public Transform    puckTransform;

    [Header("Scoring")]
    [Tooltip("Number of goals required to win the match")]
    public int maxGoals = 10;

    // UI (optional) to display scores
    public UnityEngine.UI.Text[] scoreTexts;  // in Inspector: two Text components

    // internal state
    private int[] playerScores;


    [Header("Spawn Points")]
    public Transform[]  playerSpawnPoints;  // size == 2
    public Transform    puckSpawnPoint;

    // cached poses
    private Vector3[]   _playerStartPos;
    private Quaternion[] _playerStartRot;
    private Vector3     _puckStartPos;
    private Quaternion  _puckStartRot;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        CacheStartPoses();
        playerScores = new int[players.Length];
        for (int i = 0; i < playerScores.Length; i++){
            playerScores[i] = 0;
        }

    UpdateScoreUI();
    }
    void CacheStartPoses()
    {
        _playerStartPos = new Vector3[players.Length];
        _playerStartRot = new Quaternion[players.Length];
        for (int i = 0; i < players.Length; i++)
        {
            _playerStartPos[i] = players[i].transform.position;
            _playerStartRot[i] = players[i].transform.rotation;
        }
        _puckStartPos = puckTransform.position;
        _puckStartRot = puckTransform.rotation;
    }


    public void OnGoalScored(int scoringPlayerId){

        // Increment score
        playerScores[scoringPlayerId]++;

        // call the ML Agents method to notify the player agent
        players[scoringPlayerId].GoalReached(scoringPlayerId);
        
        // Check if reached max goals
        if (playerScores[scoringPlayerId] >= maxGoals){
            // Console: announce winner
            Debug.Log($"Player {scoringPlayerId} wins the match!");

            // Reset scores for a brand‐new match
            for (int i = 0; i < playerScores.Length; i++)
                playerScores[i] = 0;
            UpdateScoreUI();
            // Reset the scene
            ResetScene();
        }
    }
    public void ResetScene()
    {
        // reset players
        for (int i = 0; i < players.Length; i++)
        {
            var agent = players[i];
            agent.transform.SetPositionAndRotation(_playerStartPos[i], _playerStartRot[i]);
            var rb = agent.GetComponent<Rigidbody>();
            rb.linearVelocity        = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            agent.OnEpisodeBegin();
        }

        // reset puck
        puckTransform.SetPositionAndRotation(_puckStartPos, _puckStartRot);
        puckRb.linearVelocity        = Vector3.zero;
        puckRb.angularVelocity = Vector3.zero;
    }

    private void UpdateScoreUI(){
        if (scoreTexts == null || scoreTexts.Length != players.Length) return;
        for (int i = 0; i < players.Length; i++)
        {
            scoreTexts[i].text = playerScores[i].ToString();
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
