using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class AnimalManager : MonoBehaviour, IAnimalRegistry
{
    [Header("登録中（デバッグ可視用）")]
    [SerializeField] private List<GameObject> gameObjects = new();

    [Header("削除")]
    [SerializeField] private float destroyInterval = 0.02f;

    public static AnimalManager Instance { get; private set; }

    public IReadOnlyList<GameObject> Registered => gameObjects;
    public event Action<int> OnScoreRecalculated; // スコア集計完了時に通知
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }
    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    // ===== 登録 =====
    public void Register(GameObject go)
    {
        if (!go)
        {
            Debug.LogWarning("AnimalManager.Register: null は登録できません");
            return;
        }

        if (!gameObjects.Contains(go))
        {
            gameObjects.Add(go);
        }
    }

    // ===== 解除 =====
    public void Unregister(GameObject go)
    {
        if (!go)
        {
            Debug.LogWarning("AnimalManager.Unregister: null は解除できません");
            return;
        }

        if (gameObjects.Contains(go))
        {
            gameObjects.Remove(go);
        }
    }

    // ===== 全削除 =====
    public void ClearAll()
    {
        StopAllCoroutines();
        StartCoroutine(CoClearAll());
    }

    // AnimalManager に追加（例）
    public void ClearAllExcept(IEnumerable<GameObject> keep)
    {
        StopAllCoroutines();
        StartCoroutine(CoClearAllExcept(keep));
    }
    private IEnumerator CoClearAllExcept(IEnumerable<GameObject> keep)
    {
        var keepSet = new HashSet<GameObject>(keep ?? Array.Empty<GameObject>());

        // 登録リストのコピーを使ってDestroy
        var copy = new List<GameObject>(gameObjects);
        foreach (var go in copy)
        {
            if (!go) continue;

            if (keepSet.Contains(go))
            {
                // 残すもの：Destroyしない・リストからも消さない
                continue;
            }

            // 破棄対象
            Destroy(go);
            gameObjects.Remove(go);
            yield return new WaitForSeconds(destroyInterval);
        }
    }

    private IEnumerator CoClearAll()
    {
        var copy = new List<GameObject>(gameObjects);
        foreach (var go in copy)
        {
            if (go) Destroy(go);
            yield return new WaitForSeconds(destroyInterval);
        }
        gameObjects.Clear();
    }

    // ===== 全動物のスコア集計 =====
    public int CalculateTotalScore()
    {
        int total = 0;
        foreach (var go in gameObjects)
        {
            if (!go) continue;
            var providers = go.GetComponentsInChildren<AnimalScoreProvider>();
            foreach (var p in providers)
            {
                total += p.GetScore();
            }
        }
        return total;
    }

    public int RecalculateAndNotify()
    {
        int total = CalculateTotalScore();
        OnScoreRecalculated?.Invoke(total);
        return total;
    }
}
