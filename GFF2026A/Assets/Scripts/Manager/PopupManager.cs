using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopupManager : MonoBehaviour
{
    public static PopupManager Instance { get; private set; }
    [SerializeField] private Canvas canvas;
    [SerializeField] private bool addDim = true;
    [SerializeField] private int sortingOrder = 5000;

    readonly Stack<GameObject> stack = new();
    // ★ 追加：グループごとの現在ポップアップ
    readonly Dictionary<string, GameObject> groupCurrent = new();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (!canvas) canvas = FindFirstObjectByType<Canvas>();
        if (canvas) { canvas.overrideSorting = true; canvas.sortingOrder = sortingOrder; }
    }

    // 既存 Open（スタック運用）
    public GameObject Open(GameObject prefab, Action<GameObject> init = null)
    {
        if (!canvas || !prefab) { Debug.LogError("[PopupManager] canvas/prefab 未設定"); return null; }

        var go = Instantiate(prefab, canvas.transform, false);
        go.transform.SetAsLastSibling();

        if (addDim)
        {
            var dim = new GameObject("Dim", typeof(RectTransform), typeof(Image));
            var rt = (RectTransform)dim.transform;
            rt.SetParent(canvas.transform, false);
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            var img = dim.GetComponent<Image>();
            img.color = new Color(0, 0, 0, 0.5f);
            img.raycastTarget = true;

            dim.transform.SetSiblingIndex(go.transform.GetSiblingIndex());
            go.transform.SetParent(dim.transform, true);
        }

        init?.Invoke(go);

        if (go.TryGetComponent<IPopup>(out var p)) p.Open();
        else
        {
            var ricimi = go.GetComponent("Popup");
            ricimi?.GetType().GetMethod("Open")?.Invoke(ricimi, null);
        }

        stack.Push(go);
        return go;
    }

    public void CloseTop()
    {
        if (stack.Count == 0) return;
        CloseInternal(stack.Pop());
    }

    public void CloseAll()
    {
        while (stack.Count > 0) CloseTop();
    }

    void CloseInternal(GameObject go)
    {
        if (!go) return;
        if (go.TryGetComponent<IPopup>(out var p)) p.Close();
        else Destroy(go.transform.parent != null && go.transform.parent.name == "Dim"
            ? go.transform.parent.gameObject : go);
    }

    // ====== ここから ★排他グループ API ======

    // 同じ group に属する既存を閉じてから開く
    public GameObject OpenExclusive(string group, GameObject prefab, Action<GameObject> init = null)
    {
        if (!string.IsNullOrEmpty(group) && groupCurrent.TryGetValue(group, out var exist) && exist)
        {
            CloseInternal(exist);
        }

        var go = Open(prefab, init);
        if (!string.IsNullOrEmpty(group) && go)
        {
            groupCurrent[group] = go;

            // 破棄時に辞書をクリーンアップするためのトラッカーを付与
            var tracker = go.AddComponent<PopupGroupTracker>();
            tracker.Init(this, group, go);
        }
        return go;
    }

    public void CloseGroup(string group)
    {
        if (string.IsNullOrEmpty(group)) return;
        if (groupCurrent.TryGetValue(group, out var go) && go)
        {
            groupCurrent.Remove(group);
            CloseInternal(go);
        }
    }

    internal void OnPopupDestroyedInGroup(string group, GameObject go)
    {
        if (string.IsNullOrEmpty(group)) return;
        if (groupCurrent.TryGetValue(group, out var cur) && cur == go)
        {
            groupCurrent.Remove(group);
        }
    }
}

// 破棄フック
public class PopupGroupTracker : MonoBehaviour
{
    PopupManager manager;
    string group;
    GameObject go;

    public void Init(PopupManager m, string g, GameObject o)
    {
        manager = m; group = g; go = o;
    }

    void OnDestroy()
    {
        manager?.OnPopupDestroyedInGroup(group, go);
    }
}
