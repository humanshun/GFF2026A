using UnityEngine;

public class AnimalSpawner : MonoBehaviour
{
    [SerializeField] private MonoBehaviour registryProvider; // IAnimalRegistryを提供するコンポーネント
    private IAnimalRegistry _registry;
    
    [Header("Refs")]
    [SerializeField] private AnimalDatabase database;
    [SerializeField] private SpawnDecider decider;

    [Header("Spawn")]
    [SerializeField] private Transform spawnPoint;    // 生成位置。未設定なら(0,0,0)

    public SpawnDecider Decider => decider;

    public GameObject Spawn(Vector3 pos, Quaternion rot)
    {
        if (!database || !decider) { Debug.LogWarning("Spawner未設定"); return null; }

        var data = decider.RollNext(database);
        if (!data || !data.Prefab) return null;

        var go = Instantiate(data.Prefab, pos, rot);

        if (go.TryGetComponent(out AnimalPiece piece))
        {
            piece.Initialize(_registry); // DI注入
        }
        return go;
    }

    private void Awake()
    {
        _registry = registryProvider as IAnimalRegistry;
        if (_registry == null)
        {
            Debug.LogWarning("AnimalSpawner: IAnimalRegistryを提供するコンポーネントが設定されていません。");

            // 最低限の保険（無ければシングルトン参照）
            _registry = AnimalManager.Instance; // シングルトンにフォールバック
        }

        if (decider is NextDecider nd && database)
        {
            nd.Prime(database); // NextDeciderはDBを必要とする
        }
    }
}
