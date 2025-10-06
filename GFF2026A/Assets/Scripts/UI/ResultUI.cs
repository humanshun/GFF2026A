using UnityEngine;
using TMPro;
using Firebase.Firestore;
using Firebase.Extensions;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;

public class ResultUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI lastScoreText;
    [SerializeField] private TextMeshProUGUI bestScoreText;
    [SerializeField] private TextMeshProUGUI rankText;
    [SerializeField] private TextMeshProUGUI statusText; // 読み込み中/エラー表示用（任意）

    [Header("Options")]
    [SerializeField] private string inGameSceneName = "InGame";
    [SerializeField] private int rankTimeoutMs = 4000; // ランク取得のタイムアウト(ms)

    void Start()
    {
        LoadAndShow().Forget();
    }

    public void OnRetryButton()
    {
        SceneManager.LoadScene(inGameSceneName);
    }

    private async UniTaskVoid LoadAndShow()
    {
        // --- 今回スコア ---
        int lastScore = ScoreManager.Instance != null ? ScoreManager.Instance.LastRunScore : 0;
        if (lastScoreText) lastScoreText.text = lastScore.ToString();

        // --- 自己ベスト（ローカルがあれば即表示、なければFirestoreから取得） ---
        int bestScore = 0;
        bool hasAuth = Auth.instance != null && Auth.instance.user != null;

        if (hasAuth && Auth.instance.userData != null)
        {
            bestScore = Auth.instance.userData.bestScore;
        }
        else if (hasAuth)
        {
            // まだローカルに無ければFirestoreから拾う
            bestScore = await FetchBestFromFirestoreSafe();
        }
        else
        {
            // 未ログインの場合：best=last（ローカルのみ）
            bestScore = Mathf.Max(bestScore, lastScore);
        }
        if (bestScoreText) bestScoreText.text = bestScore.ToString();

        // --- 全体順位 ---
        if (!hasAuth)
        {
            if (rankText) rankText.text = "-";
            if (statusText) statusText.text = "未ログインのため順位は表示できません。";
            return;
        }

        if (statusText) statusText.text = "ランキング取得中…";
        int rank = await FetchGlobalRankSafe(bestScore, rankTimeoutMs);
        if (rank > 0)
        {
            if (rankText) rankText.text = $"#{rank}";
            if (statusText) statusText.text = "";
        }
        else
        {
            if (rankText) rankText.text = "-";
            if (statusText) statusText.text = "ランキング取得に失敗しました。";
        }
    }

    // Firestoreから自分のbestScoreを取得（なければ0）
    private async UniTask<int> FetchBestFromFirestoreSafe()
    {
        try
        {
            var fs = FirebaseFirestore.DefaultInstance;
            var uid = Auth.instance.user.UserId;
            var doc = await fs.Collection("userInfo").Document(uid).GetSnapshotAsync();
            if (doc.Exists && doc.ContainsField("bestScore"))
            {
                try { return doc.GetValue<int>("bestScore"); }
                catch { return (int)doc.GetValue<long>("bestScore"); }
            }
        }
        catch { /* ignore */ }
        return 0;
    }

    // bestScoreより大きい人数 + 1 を順位として返す（失敗時は0）
    private async UniTask<int> FetchGlobalRankSafe(int myBest, int timeoutMs)
    {
        try
        {
            var fs = FirebaseFirestore.DefaultInstance;
            // bestScore > myBest の人数を数える
            var q = fs.Collection("userInfo").WhereGreaterThan("bestScore", myBest);
            // タイムアウト付きで取得
            var snap = await q.GetSnapshotAsync().AsUniTask().Timeout(System.TimeSpan.FromMilliseconds(timeoutMs));
            int greaterCount = snap.Count;
            return greaterCount + 1;
        }
        catch
        {
            return 0;
        }
    }
}
