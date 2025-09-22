using UnityEngine;

public class HoldController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private AnimalSpawner spawner;
    [SerializeField] private Transform spawnPoint;

    [Header("Options")]
    [SerializeField] private bool autoSpawnNextOnSecondInput = true;

    public GameObject CurrentHolding { get; private set; }
    public bool HasHolding => _holdingRb != null;

    Rigidbody2D _holdingRb;
    float _originalGravity = 1f;

    void Start() => PrepareNextHold();

    public void PrepareNextHold()
    {
        if (_holdingRb) return;
        var pos = spawnPoint ? spawnPoint.position : Vector3.zero;

        var go = spawner.Spawn(pos, Quaternion.identity);
        if (!go) return;

        CurrentHolding = go;
        _holdingRb = go.GetComponentInChildren<Rigidbody2D>();
        if (_holdingRb)
        {
            _originalGravity = _holdingRb.gravityScale;
            // 保持中：操作できるようKinematic+重力0
            _holdingRb.bodyType = RigidbodyType2D.Kinematic;
            _holdingRb.gravityScale = 0f;
            _holdingRb.linearVelocity = Vector2.zero;
            _holdingRb.angularVelocity = 0f;
        }
    }

    public void ReleaseHold()
    {
        if (!_holdingRb)
        {
            CurrentHolding = null;
            return;
        }

        // ★ 着地一発目で次を用意：ここで購読
        if (CurrentHolding.TryGetComponent(out AnimalPiece piece))
        {
            void Handler()
            {
                piece.OnFirstLand -= Handler;

                // ★ 全体のMaxYから次のスポーン高さを決める
                float topY = TopHeightUtility.ComputeTopY(AnimalManager.Instance?.Registered, 0f);
                if (spawnPoint) {
                    float offset = 3.5f;                // 好きな余白
                    var p = spawnPoint.position;
                    p.y = topY + offset;
                    spawnPoint.position = p;
                }

                // その後で次を保持生成
                PrepareNextHold();
            }
            piece.OnFirstLand -= Handler; // 念のため
            piece.OnFirstLand += Handler;
        }

        // 落下開始（Dynamicに戻す）
        _holdingRb.bodyType = RigidbodyType2D.Dynamic;
        _holdingRb.gravityScale = _originalGravity;

        _holdingRb = null;
        CurrentHolding = null;
    }

    public void HandleDropOrPrepare()
    {
        if (HasHolding) ReleaseHold();
        else if (autoSpawnNextOnSecondInput) PrepareNextHold();
    }

    public bool TryGetHoldingTransform(out Transform t)
    {
        t = CurrentHolding ? CurrentHolding.transform : null;
        return t != null;
    }
}
