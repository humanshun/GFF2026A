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

    public event Action OnRegistryChanged; // 変更検知用イベント（登録/解除/全消し/何か変わった）

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

        OnRegistryChanged?.Invoke();
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

        OnRegistryChanged?.Invoke();
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
        OnRegistryChanged?.Invoke();
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
        OnRegistryChanged?.Invoke(); // ★完了後に通知
    }
}
