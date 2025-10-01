using UnityEngine;

public class AnimalScoreProvider : MonoBehaviour
{
    [SerializeField] private AnimalData data;

    // 着地するまではカウントしない（生成直後は保持中想定）
    [SerializeField] private bool countOnlyAfterLanded = true;
    public AnimalData Data => data;

    //スコア集計に含めるか
    public bool IncludeInScore { get; private set; } = false;

    void Awake()
    {
        if (!countOnlyAfterLanded)
        {
            IncludeInScore = true;
            return;
        }

        if (TryGetComponent(out AnimalPiece piece))
        {
            // すでに着地済みなら即カウント
            IncludeInScore = piece.HasLanded;

            piece.OnFirstLand -= OnFirstLandHandler;
            piece.OnFirstLand += OnFirstLandHandler;
        }
        else
        {
            IncludeInScore = true; // AnimalPieceが無いなら常にカウント
        }
    }

    void OnDestroy()
    {
        if (TryGetComponent(out AnimalPiece piece))
        {
            piece.OnFirstLand -= OnFirstLandHandler;
        }
    }

    void OnFirstLandHandler()
    {
        IncludeInScore = true;
        ScoreManager.Instance?.RecalculateAndNotify();
    }

    public void SetIncludeInScore(bool include)
    {
        if (IncludeInScore == include) return;
        IncludeInScore = include;
        ScoreManager.Instance?.RecalculateAndNotify();
    }
    public int GetScore() => data ? data.ScoreValue : 0;
}
