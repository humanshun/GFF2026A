using System;
using System.Collections.Generic;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    public event Action<int> OnScoreChanged; // スコア変更時に通知
    public int CurrentScore { get; private set; }

    [SerializeField] private AnimalManager animalManager;

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

    void OnEnable()
    {
        if (animalManager != null)
        {
            animalManager.OnRegistryChanged += RecalculateAndNotify;
        }
        RecalculateAndNotify(); // ★初期反映
    }

    void OnDisable()
    {
        if (animalManager != null)
        {
            animalManager.OnRegistryChanged -= RecalculateAndNotify;
        }
    }

    // 集計ロジックはここに集約
    public void RecalculateAndNotify()
    {
        int total = 0;
        if (animalManager != null)
        {
            foreach (var go in animalManager.Registered)
            {
                if (!go) continue;
                var providers = go.GetComponentsInChildren<AnimalScoreProvider>();
                foreach (var p in providers)
                {
                    if (p.IncludeInScore) // ★保持中は false、初着地で true
                        total += p.GetScore();
                }
            }
        }

        if (total != CurrentScore)
        {
            CurrentScore = total;
            OnScoreChanged?.Invoke(CurrentScore);
        }
    }
}
