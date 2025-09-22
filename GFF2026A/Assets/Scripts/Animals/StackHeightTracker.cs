using System;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-50)]
public class StackHeightTracker : MonoBehaviour
{
    [SerializeField] private AnimalManager registry;        // 省略時は自動取得
    [SerializeField] private float baseGroundY = 0f;        // 地面のY（最低値）
    [SerializeField] private float pollInterval = 0.1f;     // 何秒毎に再計算するか
    [SerializeField] private float gizmoWidth = 20f;        // デバッグ線の横幅

    public float TopY { get; private set; }
    public event Action<float> OnTopHeightChanged;

    float _lastTopY;
    float _timer;

    void Awake()
    {
        if (!registry) registry = AnimalManager.Instance;
        TopY = _lastTopY = baseGroundY;
    }

    void Update()
    {
        _timer += Time.deltaTime;
        if (_timer < pollInterval) return;
        _timer = 0f;

        float top = baseGroundY;
        if (registry != null)
        {
            // AnimalManagerの登録GOから最大Y（Collider/Rendererのbounds）を探す
            var list = GetPrivateList(registry); // 友好的に中身を覗くヘルパ
            if (list != null)
            {
                foreach (var go in list)
                {
                    if (!go) continue;

                    // 1) Collider2D優先
                    var col = go.GetComponentInChildren<Collider2D>();
                    if (col)
                    {
                        top = Mathf.Max(top, col.bounds.max.y);
                        continue;
                    }
                    // 2) SpriteRendererなどのboundsでも可
                    var ren = go.GetComponentInChildren<Renderer>();
                    if (ren)
                    {
                        top = Mathf.Max(top, ren.bounds.max.y);
                    }
                }
            }
        }

        TopY = top;

        if (!Mathf.Approximately(TopY, _lastTopY))
        {
            _lastTopY = TopY;
            OnTopHeightChanged?.Invoke(TopY);
        }
    }

    // AnimalManagerの登録リストを取得する簡易ヘルパ（SerializeFieldがprivateでもOKにするならPropertyにしてもらってもOK）
    List<GameObject> GetPrivateList(AnimalManager m)
    {
        // 直接publicにしてない場合に備えて、SerializeFieldのフィールドを反射で取る（安全性重視ならAnimalManagerに公開Getterを追加してね）
        var fi = typeof(AnimalManager).GetField("gameObjects", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return fi?.GetValue(m) as List<GameObject>;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(new Vector3(-gizmoWidth * 0.5f, TopY, 0), new Vector3(gizmoWidth * 0.5f, TopY, 0));
    }
}
