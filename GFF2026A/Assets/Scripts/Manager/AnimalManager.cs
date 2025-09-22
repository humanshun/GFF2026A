using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimalManager : MonoBehaviour, IAnimalRegistry
{
    [Header("登録中（デバッグ可視用）")]
    [SerializeField] private List<GameObject> gameObjects = new();

    [Header("削除")]
    [SerializeField] private float destroyInterval = 0.02f;

    public static AnimalManager Instance { get; private set; }

    public IReadOnlyList<GameObject> Registered => gameObjects;
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
}
