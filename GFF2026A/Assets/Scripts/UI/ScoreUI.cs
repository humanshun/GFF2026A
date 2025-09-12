using UnityEngine;
using TMPro;

public class ScoreUI : MonoBehaviour
{
    public TextMeshProUGUI scoreText;

    void OnEnable()
    {
        ScoreManager.Instance.OnScoreChanged += UpdateScoreUI;
        scoreText.text = ScoreManager.Instance.currentScore.ToString();
    }

    void OnDisable()
    {
        ScoreManager.Instance.OnScoreChanged -= UpdateScoreUI;
    }

    void UpdateScoreUI(int newScore)
    {
        scoreText.text = newScore.ToString();
    }
}
