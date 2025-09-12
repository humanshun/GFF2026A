using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NextDecider", menuName = "Scriptable Objects/Next Decider")]

public class NextDecider : SpawnDecider
{
    [SerializeField] private int queueSize = 3; //ネクストに表示する数
    private Queue<AnimalData> queue;
    public IReadOnlyList<AnimalData> NextAnimalsList => new List<AnimalData>(queue);
    public event System.Action QueueChanged;

    private void OnEnable()
    {
        queue = new Queue<AnimalData>(queueSize);
    }

    public void Prime(AnimalDatabase db)
    {
        if (queue == null) queue = new Queue<AnimalData>(queueSize);
        EnsureFill(db);
        QueueChanged?.Invoke();
    }

    public override AnimalData RollNext(AnimalDatabase db)
    {
        // 初期化されてなければ再構築
        if (queue == null) queue = new Queue<AnimalData>(queueSize);
        EnsureFill(db);
        var result = queue.Count > 0 ? queue.Dequeue() : null;

        if (db == null)
        {
            var next = db.GetRandomByWeight();
            if (next != null) queue.Enqueue(next);
        }
        QueueChanged?.Invoke();
        return result;
    }
    private void EnsureFill(AnimalDatabase db)
    {
        if (db == null) return;
        while (queue.Count < queueSize)
        {
            var a = db.GetRandomByWeight();
            if (a != null) queue.Enqueue(a);
            else break; // DBが空などの場合は抜ける
        }
    }
}
