using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    public event Action<int> OnScoreChanged;
    public int CurrentScore { get; private set; }

    public int LastRunScore { get; private set; }
    public void SetLastRunScore(int score) => LastRunScore = score;

    [SerializeField] private AnimalManager animalManager; // Inspector参照でもOK。毎回再解決する

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        // 起動直後の初期シーンでも一度バインド
        RebindAnimalManager();
        RecalculateAndNotify();
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        UnsubscribeAnimalManager();
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 新しいシーンの AnimalManager に差し替え
        RebindAnimalManager();

        // InGame に戻ったらスコア初期化（シーン名はあなたの実名に合わせて）
        if (scene.name == "InGame")
            ResetScore();
    }

    void RebindAnimalManager()
    {
        // 既存購読を外す
        UnsubscribeAnimalManager();

        // 新しいインスタンスを拾う（Inspector未設定/破棄済みの保険）
        if (animalManager == null || animalManager != AnimalManager.Instance)
            animalManager = AnimalManager.Instance;

        // 購読し直し
        if (animalManager != null)
            animalManager.OnRegistryChanged += RecalculateAndNotify;
    }

    void UnsubscribeAnimalManager()
    {
        if (animalManager != null)
            animalManager.OnRegistryChanged -= RecalculateAndNotify;
    }

    public void ResetScore()
    {
        if (CurrentScore != 0)
        {
            CurrentScore = 0;
            OnScoreChanged?.Invoke(CurrentScore);
        }
    }

    public void RecalculateAndNotify()
    {
        if (animalManager == null) return;
        int total = 0;
        if (animalManager != null)
        {
            foreach (var go in animalManager.Registered)
            {
                if (!go) continue;
                var providers = go.GetComponentsInChildren<AnimalScoreProvider>();
                foreach (var p in providers)
                {
                    // ★保持中は除外（初着地で IncludeInScore=true に）
                    if (p.IncludeInScore)
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
