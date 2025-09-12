using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NextDecider", menuName = "Scriptable Objects/Next Decider")]

public class NextDecider : SpawnDecider
{
    [SerializeField] private int queueSize = 3; //ネクストに表示する数
    private Queue<AnimalData> queue;
    public IReadOnlyCollection<AnimalData> NextAnimals => queue;

    private void OnEnable()
    {
        queue = new Queue<AnimalData>(queueSize);
    }

    public override AnimalData RollNext(AnimalDatabase db)
    {
        // 初期化されてなければ再構築
        if (queue == null) queue = new Queue<AnimalData>(queueSize);

        // キューが空なら補完
        while (queue.Count < queueSize)
        {
            var a = db.GetRandomByWeight();
            if (a != null) queue.Enqueue(a);
        }

        // 先頭を取り出して、新しい候補を補充
        var result = queue.Dequeue();
        var next = db.GetRandomByWeight();
        if (next != null) queue.Enqueue(next);

        return result;
    }
}
