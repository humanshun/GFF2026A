using UnityEngine;

public class HoldSlot : MonoBehaviour
{
    [Header("見た目上の配置先")]
    [SerializeField] private Transform anchor; //ここに格納表示する。未設定なら自身
    [SerializeField] private bool showWorldModelWhileStored = false;

    public GameObject Stored { get; private set; }
    public bool IsEmpty => Stored == null;

    void Reset()
    {
        if (!anchor) anchor = transform;
    }

    /// <summary>
    /// 現在のホールド内容を取り出す（保持状態で返す）
    /// </summary>
    public GameObject TakeOutForHolding(Vector3 holdingPos)
    {
        if (!Stored) return null;

        var go = Stored;
        Stored = null;

        // 親子解除して保持位置へ
        SetRenderersEnabled(go, true); 
        go.transform.SetParent(null, worldPositionStays: true);
        go.transform.position = holdingPos;

        // 保持状態（操作待機）：全てのRigidbody2DをKinematic+重力0に
        SetHeldPhysics(go, held: true);

        return go;
    }

    ///ゲームオブジェクトをホールドに格納する
    public void Store(GameObject go)
    {
        if (!go) return;

        //アンカーにぶら下げ＆見た目を整える
        var t = go.transform;
        t.SetParent(anchor ? anchor : transform, worldPositionStays: false);
        t.localPosition = Vector3.zero;
        t.localRotation = Quaternion.identity;

        //格納中は完全停止(kinematic/重力0/各速度0)
        SetHeldPhysics(go, held: true);

        SetHeldPhysics(go, held:true);
        if (!showWorldModelWhileStored)
            SetRenderersEnabled(go, false);   // 実体を非表示
        Stored = go;
    }

    private static void SetHeldPhysics(GameObject root, bool held)
    {
        //ルート配下のすべてのRigidbody2Dを対象にしておくとラグドールなどでも安全
        var rbs = root.GetComponentsInChildren<Rigidbody2D>(includeInactive: true);
        foreach (var rb in rbs)
        {
            if (held)
            {
                rb.bodyType = RigidbodyType2D.Kinematic;
                rb.gravityScale = 0f;
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }
        }
    }
    static void SetRenderersEnabled(GameObject root, bool enabled)
    {
        foreach (var r in root.GetComponentsInChildren<Renderer>(true))
            r.enabled = enabled;
        // 2D専用なら SpriteRenderer だけでもOK
        foreach (var c in root.GetComponentsInChildren<Collider2D>(true))
            c.enabled = enabled; // 完全に消したい時はコライダーも切る
    }

    public bool TryGetStored(out GameObject go)
    {
        go = Stored;
        return go != null;
    }
}
