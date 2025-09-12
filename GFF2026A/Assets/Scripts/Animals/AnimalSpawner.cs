using UnityEngine;

public class AnimalSpawner : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private AnimalDatabase database;
    [SerializeField] private SpawnDecider decider;

    [Header("Spawn")]
    [SerializeField] private Transform spawnPoint;    // 生成位置。未設定なら(0,0,0)
    [SerializeField] private bool autoSpawnNextOnSecondSpace = true;
    // ↑ 2回目以降のスペースで「次を保持生成」まで回したい場合にON

    public SpawnDecider Decider => decider;

    // 保持中（停止中）の個体を覚えておく
    private GameObject holdingGO;
    private Rigidbody2D holdingRb2d;
    private float originalGravityScale = 1f;

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

        // 開始時に1体スポーンして停止状態で保持
        PrepareNextHold();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (holdingRb2d) // 何か保持している → 落とす
            {
                ReleaseHold();
            }
            else if (autoSpawnNextOnSecondSpace) // 何も保持していない → 次を保持生成
            {
                PrepareNextHold();
            }
        }
    }

    /// <summary>
    /// 次の動物を生成し、物理を止めた「保持状態」で待機させる
    /// </summary>
    private void PrepareNextHold()
    {
        // 既に保持中なら何もしない（安全策）
        if (holdingRb2d) return;

        // 生成位置
        var pos = spawnPoint ? spawnPoint.position : Vector3.zero;

        var go = Spawn(pos, Quaternion.identity); // RollNextもここで消費
        if (!go)
        {
            Debug.LogWarning("保持生成に失敗（Prefabなし or DB未設定）");
            return;
        }

        // Rigidbody2D を見つける（子に付いている場合も考慮）
        var rb = go.GetComponentInChildren<Rigidbody2D>();
        if (!rb)
        {
            Debug.LogWarning("生成したPrefabにRigidbody2Dが見つかりません。落下制御できません。");
            // 物理が無い場合でも保持構造だけ覚えておく（すぐ落ちない想定ならDestroyしても可）
            holdingGO = go;
            holdingRb2d = null;
            return;
        }

        // 現在値を保存して停止
        originalGravityScale = rb.gravityScale;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.simulated = false;     // ← 物理シミュレーションを止める（重力も当たらない）

        holdingGO = go;
        holdingRb2d = rb;
    }

    /// <summary>
    /// 保持している個体の物理を再開して「落とす」
    /// </summary>
    private void ReleaseHold()
    {
        if (!holdingRb2d)
        {
            // Rigidbodyが無いケース（2D物理でないPrefabなど）
            holdingGO = null;
            return;
        }

        // 物理再開
        holdingRb2d.simulated = true;
        holdingRb2d.gravityScale = originalGravityScale;

        // 参照解放（以降は保持なし状態）
        holdingGO = null;
        holdingRb2d = null;
    }
}
