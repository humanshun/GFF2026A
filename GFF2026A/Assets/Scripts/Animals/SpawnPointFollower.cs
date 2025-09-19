using UnityEngine;

public class SpawnPointFollower : MonoBehaviour
{
    [SerializeField] private StackHeightTracker tracker;
    [SerializeField] private Transform spawnPoint;    // HoldControllerで使ってるやつ
    [SerializeField] private float topOffset = 3.5f;  // 最高点からどれだけ上に出すか
    [SerializeField] private float smoothTime = 0.15f;

    float _velY;

    void Reset()
    {
        spawnPoint = transform;
    }

    void LateUpdate()
    {
        if (!tracker || !spawnPoint) return;

        float targetY = tracker.TopY + topOffset;
        var pos = spawnPoint.position;
        pos.y = Mathf.SmoothDamp(pos.y, targetY, ref _velY, smoothTime);
        spawnPoint.position = pos;
    }
}
