using UnityEngine;
using System.Collections.Generic;
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

        // 必要ならここで Rigidbody2D や Collider2D に data の値を適用する
        return go;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            Spawn(Vector3.zero, Quaternion.identity);

        // ネクストをUIに表示
        if (decider is NextDecider nd)
        {
            foreach (var a in nd.NextAnimals)
                Debug.Log($"Next: {a.DisplayName}");
        }
    }
}
