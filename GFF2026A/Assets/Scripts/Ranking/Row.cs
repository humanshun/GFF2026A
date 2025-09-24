using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardRow : MonoBehaviour
{
    [SerializeField] private TMP_Text rankText;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private Image highlight;

    public void Bind(int rank, string name, int score, string uid, string myUid = null)
    {
        rankText.text = rank.ToString();

        string tag = PublicTag.Make(uid);
        nameText.text = string.IsNullOrEmpty(name) ? $"NoName#{tag}" : $"{name}#{tag}";

        scoreText.text = score.ToString();

        if (highlight)
            highlight.enabled = !string.IsNullOrEmpty(myUid) && myUid == uid;
    }
}
