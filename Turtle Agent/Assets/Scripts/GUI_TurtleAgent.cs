using UnityEngine;
using TMPro;

public class TurtleGUI : MonoBehaviour
{
    [SerializeField] private TurtleAgent _turtleAgent;
    [SerializeField] private TextMeshProUGUI episodeText;   
    [SerializeField] private TextMeshProUGUI rewardText;

    private GUIStyle _defaultStyle = new GUIStyle();
    private GUIStyle _positiveStyle = new GUIStyle();
    private GUIStyle _negativeStyle = new GUIStyle();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start(){
        episodeText.fontSize = 36;
        episodeText.color = Color.yellow;

        // _positiveStyle and _negativeStyle will be set dynamically based on reward
        rewardText.fontSize = 36; // Same size for both positive/negative
        rewardText.color = Color.green; // Start as green (default)
    }

    // Update is called once per frame
    void Update()
    {
         // Text content
        episodeText.text = $"Episode: {_turtleAgent.CurrentEpisode} - Step: {_turtleAgent.StepCount}";
        rewardText.text = $"Reward: {_turtleAgent.CumulativeReward:F2}";

        // Dynamically change reward text color
        rewardText.color = _turtleAgent.CumulativeReward < 0 ? Color.red : Color.green;
    }
}
