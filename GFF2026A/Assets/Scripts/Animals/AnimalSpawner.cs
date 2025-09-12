using UnityEngine;

public class AnimalSpawner : MonoBehaviour
{
    [SerializeField] private AnimalDatabase database;
    [SerializeField] private SpawnDecider decider;
    public SpawnDecider Decider => decider;

    public GameObject Spawn(Vector3 pos, Quaternion rot)
    {
        if (!database || !decider) { Debug.LogWarning("Spawner未設定"); return null; }

        var data = decider.RollNext(database);
        if (!data || !data.Prefab) return null;

        var go = Instantiate(data.Prefab, pos, rot);
        return go;
    }

    private void Start()
    {
        // ネクストを事前に満たす
        if (decider is NextDecider nd && database)
        {
            nd.Prime(database);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            Spawn(Vector3.zero, Quaternion.identity);

        // デバッグで中身を見たいなら必要な時だけ
        // if (decider is NextDecider nd)
        // {
        //     foreach (var a in nd.NextAnimalsList)
        //         Debug.Log($"Next: {a?.DisplayName}");
        // }
    }
}
