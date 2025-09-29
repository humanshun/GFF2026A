using UnityEngine;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using System.Threading; // CancellationToken

public class PlayerController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private AnimalSpawner spawner;   // ピースを生成するためのSpawner参照
    [SerializeField] private Transform spawnPoint;    // ピースを出す位置
    [SerializeField] private HoldSlot holdSlot;       // ホールドスロット（入れ替え機能用）

    [Header("Options")]
    [SerializeField] private bool autoSpawnNextOnSecondInput = true; // 落下後の入力で自動的に次を生成するか
    [SerializeField] private float releaseGravityScale = 1f;         // 落下時の重力倍率

    [Header("Camera Lift On Land (DoTween)")]
    [SerializeField] private Transform playerRigForCamera;  // カメラを子にしているオブジェクト
    [SerializeField] private float playerTopMargin = 2f;    // 最高点 + 余白
    [SerializeField] private bool onlyRisePlayer = true;    // 上方向にしか動かさない
    [SerializeField] private float playerRiseDuration = 0.3f; // 上昇にかける時間
    [SerializeField] private Ease playerRiseEase = Ease.OutQuad; // イージング種類

    public GameObject CurrentHolding { get; private set; } // 現在保持しているピース
    public bool HasHolding => _holdingRb != null;          // 何か保持中かどうか

    Rigidbody2D _holdingRb;       // 保持中のRigidbodyキャッシュ
    Collider2D[] _holdingCols;    // 保持中のColliderキャッシュ（無効化・有効化用）

    void Awake()
    {
        // playerRigForCamera が未設定ならこのコンポーネントが付いたオブジェクトを使う
        if (!playerRigForCamera) playerRigForCamera = transform;
    }

    void Start() => PrepareNextHold(); // ゲーム開始時に最初のピースを準備

    /// <summary>次のピースを保持生成</summary>
    public void PrepareNextHold()
    {
        if (_holdingRb) return; // すでに保持中なら何もしない

        var pos = spawnPoint ? spawnPoint.position : Vector3.zero;

        // Spawnerから新しいピースを生成
        var go = spawner.Spawn(pos, Quaternion.identity);
        if (!go) return;

        // 生成したピースを保持状態に設定
        AttachAsHolding(go);
    }

    /// <summary>指定したGameObjectを保持状態にする</summary>
    private void AttachAsHolding(GameObject go)
    {
        CurrentHolding = go;

        // Rigidbody2D をキャッシュして保持モードに切り替え
        _holdingRb = go.GetComponentInChildren<Rigidbody2D>();
        if (_holdingRb)
        {
            _holdingRb.bodyType = RigidbodyType2D.Kinematic; // 動かない物体にする
            _holdingRb.gravityScale = 0f;                    // 重力を無効
            _holdingRb.linearVelocity = Vector2.zero;        // 移動速度をリセット
            _holdingRb.angularVelocity = 0f;                 // 回転速度をリセット
        }

        // 保持開始時に全てのColliderを無効化（他オブジェクトとぶつからないようにする）
        _holdingCols = go.GetComponentsInChildren<Collider2D>(includeInactive: true);
        SetCollidersEnabled(_holdingCols, false);
    }

    /// <summary>保持中のピースを落下開始させる</summary>
    public void ReleaseHold()
    {
        if (!_holdingRb)
        {
            // 何も保持していない場合
            CurrentHolding = null;
            _holdingCols = null;
            return;
        }

        // ★ 物理の復帰は次のFixedUpdateでまとめて実行する（抜け防止）
        var cols = _holdingCols; // ローカル退避（後でフィールドをnullにするため）
        var rbs  = CurrentHolding.GetComponentsInChildren<Rigidbody2D>(includeInactive: false);
        ActivatePhysicsNextFixedAsync(cols, rbs, releaseGravityScale, this.GetCancellationTokenOnDestroy()).Forget();

        // 着地一発目で「プレイヤー(カメラ親)を上げる + 次を用意」
        if (CurrentHolding.TryGetComponent(out AnimalPiece piece))
        {
            void Handler()
            {
                piece.OnFirstLand -= Handler; // イベント解除（1回だけ実行）

                // 現在のタワーの最高点を取得（未着地は除外している前提）
                float topY = TopHeightUtility.ComputeTopY(AnimalManager.Instance?.Registered, 0f);

                // === スポーンポイントを上げる ===
                if (spawnPoint)
                {
                    float offset = 2f; // 上にどれくらい余白を取るか
                    var p = spawnPoint.position;
                    p.y = topY + offset;
                    spawnPoint.position = p;
                }

                // --- プレイヤーだけを上昇させる ---
                if (playerRigForCamera)
                {
                    float targetY = topY + playerTopMargin;
                    if (onlyRisePlayer)
                        targetY = Mathf.Max(playerRigForCamera.position.y, targetY); // 現在位置より下には下げない

                    playerRigForCamera.DOKill(); // もし動いていたら一旦停止（多重Tween防止）

                    playerRigForCamera
                        .DOMoveY(targetY, playerRiseDuration)
                        .SetEase(playerRiseEase)
                        .SetLink(playerRigForCamera.gameObject, LinkBehaviour.KillOnDestroy); // 破棄時にTweenも停止
                }

                // 次のピースを保持生成
                PrepareNextHold();
            }
            piece.OnFirstLand -= Handler; // 二重登録防止
            piece.OnFirstLand += Handler;
        }

        // このフレームでは Rigidbody を Dynamic にしない（次のFixedでやる）

        // キャッシュをクリア（非同期にローカル渡し済みなのでOK）
        _holdingRb = null;
        CurrentHolding = null;
        _holdingCols = null;
    }

    /// <summary>
    /// 次のFixedUpdateタイミングで「Collider ON → Rigidbody Dynamic化」を行う
    /// </summary>
    private async UniTaskVoid ActivatePhysicsNextFixedAsync(
        Collider2D[] cols, Rigidbody2D[] rbs, float gravityScale, CancellationToken ct)
    {
        // 物理更新直前まで待つ（破棄されたらキャンセル）
        await UniTask.Yield(PlayerLoopTiming.FixedUpdate, ct);

        if (cols != null) SetCollidersEnabled(cols, true);    // 1) まず当たり判定を戻す

        if (rbs != null)                                     // 2) 直後にDynamic化
        {
            for (int i = 0; i < rbs.Length; i++)
            {
                var rb = rbs[i];
                if (!rb) continue;
                rb.bodyType = RigidbodyType2D.Dynamic;
                rb.gravityScale = gravityScale;
                // 必要ならCCD: rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
                // 必要なら微小浮かせ: rb.position += Vector2.up * 0.001f;
            }
        }
    }

    /// <summary>入力による落下 or 次の生成</summary>
    public void HandleDropOrPrepare()
    {
        if (HasHolding) ReleaseHold();          // もし保持していたら落下させる
        else if (autoSpawnNextOnSecondInput)    // そうでなければ次を生成
            PrepareNextHold();
    }

    /// <summary>ホールド機能：現在のピースをホールド or 交換</summary>
    public void HandleHoldOrSwap()
    {
        if (!HasHolding) return;
        if (!holdSlot) { Debug.LogWarning("HoldSlotが未設定です"); return; }

        var holdingGO = CurrentHolding;

        if (holdSlot.IsEmpty)
        {
            // ピースをホールドに格納 → 新しいピースを準備
            holdSlot.Store(holdingGO);
            _holdingRb = null;
            CurrentHolding = null;
            _holdingCols = null;
            PrepareNextHold();
        }
        else
        {
            // ホールド内のピースと現在のピースを入れ替える
            var spawnPos = spawnPoint ? spawnPoint.position : Vector3.zero;
            var fromHold = holdSlot.TakeOutForHolding(spawnPos);

            holdSlot.Store(holdingGO);

            _holdingRb = null;
            CurrentHolding = null;
            _holdingCols = null;

            AttachAsHolding(fromHold);
        }
    }

    /// <summary>現在保持中のTransformを取得</summary>
    public bool TryGetHoldingTransform(out Transform t)
    {
        t = CurrentHolding ? CurrentHolding.transform : null;
        return t != null;
    }

    // ===== helper =====
    /// <summary>Collider群の有効/無効を切り替える</summary>
    static void SetCollidersEnabled(Collider2D[] cols, bool enabled)
    {
        if (cols == null) return;
        foreach (var c in cols)
        {
            if (!c) continue;
            c.enabled = enabled;
        }
    }
}
