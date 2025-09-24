using TMPro;
using UnityEngine;

public class LeaderboardRow : MonoBehaviour
{
    [SerializeField] private TMP_Text rankText;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text scoreText;

    public void Bind(int rank, string name, int score)
    {
        rankText.text = rank.ToString();
        nameText.text = string.IsNullOrEmpty(name) ? "NoName" : name;
        scoreText.text = score.ToString();
    }
}
