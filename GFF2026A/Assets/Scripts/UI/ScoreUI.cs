using UnityEngine;
using TMPro;
using Unity.VisualScripting;

public class ScoreUI : MonoBehaviour
{
    public TextMeshProUGUI scoreText;

    void OnEnable()
    {
        ScoreManager.Instance.OnScoreChanged += UpdateScoreUI;
        UpdateScoreUI(ScoreManager.Instance.CurrentScore);
    }

    void OnDisable()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnScoreChanged -= UpdateScoreUI;
        }
    }

    void UpdateScoreUI(int newScore)
    {
        if (scoreText != null)
        {
            scoreText.text = newScore.ToString();
        }
    }
}
