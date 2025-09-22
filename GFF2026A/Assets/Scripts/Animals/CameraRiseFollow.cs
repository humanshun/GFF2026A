using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraRiseFollow : MonoBehaviour
{
    [SerializeField] private StackHeightTracker tracker;
    [SerializeField] private float topMargin = 4f;    // 画面上端からの余白
    [SerializeField] private float smoothTime = 0.25f;
    [SerializeField] private bool onlyRise = true;    // trueならカメラは上にしか動かない

    float _velY;

    void LateUpdate()
    {
        if (!tracker) return;

        var cam = GetComponent<Camera>();
        float halfH = cam.orthographicSize;
        float desiredCenterY = tracker.TopY + topMargin - halfH;

        var p = transform.position;
        float targetY = onlyRise ? Mathf.Max(p.y, desiredCenterY) : desiredCenterY;
        p.y = Mathf.SmoothDamp(p.y, targetY, ref _velY, smoothTime);
        transform.position = p;
    }
}
