using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private AnimalSpawner spawner;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private HoldSlot holdSlot;

    [Header("Options")]
    [SerializeField] private bool autoSpawnNextOnSecondInput = true;
    [SerializeField] private float releaseGravityScale = 1f;

    public GameObject CurrentHolding { get; private set; }
    public bool HasHolding => _holdingRb != null;

    Rigidbody2D _holdingRb;

    void Start() => PrepareNextHold();

    public void PrepareNextHold()
    {
        if (_holdingRb) return;
        var pos = spawnPoint ? spawnPoint.position : Vector3.zero;

        var go = spawner.Spawn(pos, Quaternion.identity);
        if (!go) return;

        AttachAsHolding(go);
    }
    
    private void AttachAsHolding(GameObject go)
    {
        CurrentHolding = go;

        _holdingRb = go.GetComponentInChildren<Rigidbody2D>();
        if (_holdingRb)
        {
            // 保持中（停止）
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

        // ★ 着地一発目で次を用意
        if (CurrentHolding.TryGetComponent(out AnimalPiece piece))
        {
            void Handler()
            {
                piece.OnFirstLand -= Handler;

                float topY = TopHeightUtility.ComputeTopY(AnimalManager.Instance?.Registered, 0f);
                if (spawnPoint) {
                    float offset = 3.5f;
                    var p = spawnPoint.position;
                    p.y = topY + offset;
                    spawnPoint.position = p;
                }
                PrepareNextHold();
            }
            piece.OnFirstLand -= Handler;
            piece.OnFirstLand += Handler;
        }

        // 落下開始：配下の全Rigidbody2Dを Dynamic + 既定重力に
        var rbs = CurrentHolding.GetComponentsInChildren<Rigidbody2D>(includeInactive:false);
        foreach (var rb in rbs)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = releaseGravityScale;
        }

        _holdingRb = null;
        CurrentHolding = null;
    }

    /// <summary>
    /// スペース等：落下 or 次を準備（従来）
    /// </summary>
    public void HandleDropOrPrepare()
    {
        if (HasHolding) ReleaseHold();
        else if (autoSpawnNextOnSecondInput) PrepareNextHold();
    }

    public void HandleHoldOrSwap()
    {
        if (!HasHolding) return;
        if (!holdSlot) { Debug.LogWarning("HoldSlotが未設定です"); return; }

        var holdingGO = CurrentHolding;

        if (holdSlot.IsEmpty)
        {
            // 今のピースをホールドに入れて、ネクストを保持
            holdSlot.Store(holdingGO);
            _holdingRb = null;
            CurrentHolding = null;
            PrepareNextHold();
        }
        else
        {
            // 交換
            var spawnPos = spawnPoint ? spawnPoint.position : Vector3.zero;
            var fromHold = holdSlot.TakeOutForHolding(spawnPos);

            holdSlot.Store(holdingGO);

            _holdingRb = null;
            CurrentHolding = null;
            AttachAsHolding(fromHold);
        }
    }

    public bool TryGetHoldingTransform(out Transform t)
    {
        t = CurrentHolding ? CurrentHolding.transform : null;
        return t != null;
    }
}
