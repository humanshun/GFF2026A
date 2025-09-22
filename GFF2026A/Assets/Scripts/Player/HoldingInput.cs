using UnityEngine;
using UnityEngine.EventSystems; // UI上クリック無視に使う（任意）

[RequireComponent(typeof(HoldController))]
public class HoldingInput : MonoBehaviour
{
    [Header("Mouse Follow")]
    [SerializeField] bool followMouseX = true;
    [SerializeField] float mouseLerp = 15f;
    [SerializeField] float maxX = 4f;
    [SerializeField] bool ignoreWhenPointerOverUI = true;

    [Header("Rotation (Mouse Wheel)")]
    [SerializeField] float rotateStep = 15f;

    HoldController _holder;
    Camera _cam;

    void Awake()
    {
        _holder = GetComponent<HoldController>();
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
    }

    bool IsPointerOverUI()
    {
        // スタンドアロン/エディタ向けの簡易判定
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }
}
