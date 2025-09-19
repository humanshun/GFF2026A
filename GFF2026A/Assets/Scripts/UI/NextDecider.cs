using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NextDecider", menuName = "Scriptable Objects/Next Decider")]
public class NextDecider : SpawnDecider
{
    [SerializeField] private int queueSize = 3;   // ネクストに表示する数

    private Queue<AnimalData> _queue;
    private readonly List<AnimalData> _cache = new(); // ★ UI用キャッシュ

    public IReadOnlyList<AnimalData> NextAnimalsList => _cache;
    public event Action QueueChanged;

    private void OnEnable()
    {
        // Domain Reloadで毎回初期化される想定
        _queue = new Queue<AnimalData>(Mathf.Max(1, queueSize));
        RebuildCacheAndNotify();
    }

    public void Prime(AnimalDatabase db)
    {
        if (_queue == null) _queue = new Queue<AnimalData>(Mathf.Max(1, queueSize));
        EnsureFill(db);
        RebuildCacheAndNotify();
    }

    public override AnimalData RollNext(AnimalDatabase db)
    {
        if (_queue == null) _queue = new Queue<AnimalData>(Mathf.Max(1, queueSize));

        EnsureFill(db);

        // 取り出し（空なら null）
        var result = _queue.Count > 0 ? _queue.Dequeue() : null;

        // ★ db があるときだけ補充する（条件修正）
        if (db != null)
        {
            var next = db.GetRandomByWeight();
            if (next != null) _queue.Enqueue(next);
        }

        RebuildCacheAndNotify();
        return result;
    }

    private void EnsureFill(AnimalDatabase db)
    {
        if (db == null) return;

        while (_queue.Count < Mathf.Max(1, queueSize))
        {
            var a = db.GetRandomByWeight();
            if (a != null) _queue.Enqueue(a);
            else break; // DBが空など
        }
    }

    private void RebuildCacheAndNotify()
    {
        _cache.Clear();
        if (_queue != null) _cache.AddRange(_queue); // ★ FIFO順でそのまま
        QueueChanged?.Invoke();
    }

    // 便利: ランタイムでサイズ変更したいとき
    public void SetQueueSize(int newSize, AnimalDatabase db)
    {
        queueSize = Mathf.Max(1, newSize);
        if (_queue == null) _queue = new Queue<AnimalData>(queueSize);

        // サイズに合わせて補充/削減
        while (_queue.Count > queueSize) _queue.Dequeue();
        EnsureFill(db);
        RebuildCacheAndNotify();
    }
}
