using UnityEngine;
using UnityEngine.EventSystems; // UI上クリック無視に使う（任意）

[RequireComponent(typeof(PlayerController))]
public class PlayerInput : MonoBehaviour
{
    [Header("Mouse Follow")]
    [SerializeField] bool followMouseX = true;
    [SerializeField] float mouseLerp = 15f;
    [SerializeField] float maxX = 4f;
    [SerializeField] bool ignoreWhenPointerOverUI = true;

    [Header("Rotation (Mouse Wheel)")]
    [SerializeField] float rotateStep = 15f;

    [Header("Hold / Swap Input")]
    [SerializeField] bool useRightClickForHold = true;        //右クリックでもホールド・交換
    [SerializeField] KeyCode holdKey = KeyCode.LeftShift;     //キーでもホールド・交換
    [SerializeField] bool ignoreHoldWhenPointerOverUI = true; //UI上クリックは無視
    PlayerController _holder;
    Camera _cam;

    void Awake()
    {
        _holder = GetComponent<PlayerController>();
        _cam = Camera.main;
    }

    void Update()
    {
        // if (!_holder.HasHolding) {
        //     // 未保持時でもクリックで次を保持生成したい場合はここで受けてもOK
        //     if (Input.GetMouseButtonDown(0))
        //         _holder.HandleDropOrPrepare();
        //     return;
        // }

        if (!_holder.TryGetHoldingTransform(out var t)) return;

        // === 1) マウスX追従 ===
        if (followMouseX && _cam)
        {
            var mouseWorld = _cam.ScreenToWorldPoint(Input.mousePosition);
            float x = Mathf.Lerp(t.position.x, mouseWorld.x, mouseLerp * Time.deltaTime);
            x = Mathf.Clamp(x, -maxX, maxX);
            t.position = new Vector3(x, t.position.y, t.position.z);
        }

        // === 2) ホイールで回転 ===
        float wheel = Input.mouseScrollDelta.y;
        if (Mathf.Abs(wheel) > 0f)
        {
            t.Rotate(0f, 0f, -Mathf.Sign(wheel) * rotateStep);
        }

        // === 3) 左クリックでドロップ（UI上を除外するオプションあり）===
        if (Input.GetMouseButtonDown(0))
        {
            if (!ignoreWhenPointerOverUI || !IsPointerOverUI())
            {
                _holder.HandleDropOrPrepare();
            }
        }
                // === 4) 右クリック or キーでホールド/交換 ===
        if (useRightClickForHold && Input.GetMouseButtonDown(1))
        {
            if (!ignoreHoldWhenPointerOverUI || !IsPointerOverUI())
                _holder.HandleHoldOrSwap();
        }
        if (Input.GetKeyDown(holdKey))
        {
            _holder.HandleHoldOrSwap();
        }
    }

    bool IsPointerOverUI()
    {
        // スタンドアロン/エディタ向けの簡易判定
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }
}
