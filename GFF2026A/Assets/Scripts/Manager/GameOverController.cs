using UnityEngine;

public class GameOverController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private AnimalManager animalManager;
    [SerializeField] private HoldController holder;
    [SerializeField] private AnimalSpawner spawner;         // decider/db取得用
    [SerializeField] private Transform spawnPoint;          // リセット時の初期Y決定に使う
    [SerializeField] private Camera mainCamera;             // 位置/ズームを戻したい場合（任意）

    [Header("Fail Condition")]
    [SerializeField] private float failY = -5f;             // ここより下に落ちたらゲームオーバー
    [SerializeField] private float checkInterval = 0.1f;    // 判定周期
    [SerializeField] private float graceTime = 0.5f;        // 生成直後の猶予（秒）

    [Header("Reset Params")]
    [SerializeField] private float baseGroundY = 0f;        // 地面のY
    [SerializeField] private float spawnOffset = 3.5f;      // TopY からの余白
    [SerializeField] private Vector3 cameraStartPos = new Vector3(0, 0, -10); // カメラ初期位置
    [SerializeField] private float cameraStartSize = 5f;    // 直交カメラの初期サイズ

    float _timer;
    bool _isGameOver;
    float _sinceLastSpawn; // 直近の生成からの経過時間

    void Awake()
    {
        if (!animalManager) animalManager = AnimalManager.Instance;
        if (!mainCamera) mainCamera = Camera.main;
    }

    void OnEnable()
    {
        // 直近の生成時間を取るため、保持生成のたびにタイマーをリセット
        if (holder != null)
        {
            // HoldControllerにイベントが無い場合は、PrepareNextHold後にここを手動呼びでもOK
            // ここでは簡易にStart時に1回初期化
            _sinceLastSpawn = 999f;
        }
    }

    void Update()
    {
        if (_isGameOver) return;

        _timer += Time.deltaTime;
        _sinceLastSpawn += Time.deltaTime;

        if (_timer < checkInterval) return;
        _timer = 0f;

        // 生成直後の一瞬で落下判定しないための猶予
        if (_sinceLastSpawn < graceTime) return;

        var list = animalManager ? animalManager.Registered : null;
        if (list == null || list.Count == 0) return;

        // どれか一つでも failY 未満ならゲームオーバー
        for (int i = 0; i < list.Count; i++)
        {
            var go = list[i];
            if (!go) continue;
            if (go.transform.position.y < failY)
            {
                TriggerGameOver();
                break;
            }
        }
    }

    /// <summary>外部から「いま保持生成したよ」と教えてもらう（任意）</summary>
    public void NotifySpawned()
    {
        _sinceLastSpawn = 0f;
    }

    public void TriggerGameOver()
    {
        if (_isGameOver) return;
        _isGameOver = true;

        // ここで演出（SFX/画面フラッシュ/文字）を出してもOK
        ResetGame();
    }

    void ResetGame()
    {
        // 全削除
        if (animalManager) animalManager.ClearAll();

        // Nextリスト再構築
        if (spawner && spawner.Decider is NextDecider nd)
        {
            var dbField = typeof(AnimalSpawner).GetField("database", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var db = dbField?.GetValue(spawner) as AnimalDatabase; // 内部参照を拝借
            if (db) nd.Prime(db);
        }

        // スポーン位置リセット（地面 + 余白）
        if (spawnPoint)
        {
            var p = spawnPoint.position;
            p.y = baseGroundY + spawnOffset;
            spawnPoint.position = p;
        }

        // カメラ初期化（任意）
        if (mainCamera && mainCamera.orthographic)
        {
            mainCamera.transform.position = cameraStartPos;
            mainCamera.orthographicSize = cameraStartSize;
        }

        // 次を保持生成
        if (holder)
        {
            holder.HandleDropOrPrepare(); // 保持が無ければPrepareNextHold()が走る設定
            _sinceLastSpawn = 0f;
        }

        _isGameOver = false;
    }
}
