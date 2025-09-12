using System.Collections.Generic;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;
    public int currentScore = 0; // 実際のスコア

    // 仮のスコアリスト
    public List<AnimalData> tempScores;

    private int index = 0; // どの仮スコアを使うか管理
    public event System.Action<int> OnScoreChanged;

    private void Awake()
    {
        // 既に存在している場合は削除、なければ自分を代入
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // シーンをまたいでも破棄されない
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            AddTempScore();
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetScoreAndIndex();
        }
    }



    void AddTempScore()
    {
        if (tempScores != null && index < tempScores.Count)
        {
            int addScore = tempScores[index].ScoreValue;
            currentScore += addScore;
            OnScoreChanged?.Invoke(currentScore); // イベント発火
            index++;
        }
    }
    void ResetScoreAndIndex()
    {
        currentScore = 0;
        index = 0;
        OnScoreChanged?.Invoke(currentScore); // イベント発火
    }
}
