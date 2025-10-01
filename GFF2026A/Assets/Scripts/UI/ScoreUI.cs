using UnityEngine;
using TMPro;
using Cysharp.Threading.Tasks;

public class ScoreUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;

    void Awake()
    {
        // Inspector 未割り当ての保険
        if (!scoreText)
            scoreText = GetComponentInChildren<TextMeshProUGUI>(true);
    }

    async void OnEnable()
    {
        // ScoreManager の生成を待つ（1フレームで来ない場合も考慮）
        await UniTask.WaitUntil(() => ScoreManager.Instance != null, 
                                cancellationToken: this.GetCancellationTokenOnDestroy());

        // ここに来た時点で Instance は非 null
        ScoreManager.Instance.OnScoreChanged += UpdateScoreUI;

        // 初期表示も更新
        UpdateScoreUI(ScoreManager.Instance.CurrentScore);
    }

    void OnDisable()
    {
        if (ScoreManager.Instance != null)
            ScoreManager.Instance.OnScoreChanged -= UpdateScoreUI;
    }

    void UpdateScoreUI(int newScore)
    {
        if (scoreText) scoreText.text = newScore.ToString();
    }
}
