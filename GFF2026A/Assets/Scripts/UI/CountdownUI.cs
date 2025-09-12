using TMPro;
using UnityEngine;

public class CountdownUI : MonoBehaviour
{
    // 現在の時間
    private float currentTime = 120;

    // テキストの参照
    private TextMeshProUGUI timeText;

    void Start()
    {
        // TMPUGUIコンポーネントの取得
        timeText = GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        // 時間を減らす
        if (currentTime > 0)
        {
            currentTime -= Time.deltaTime;
            if (currentTime < 0) currentTime = 0;
        }
        // 分と秒に変換して表示
        int totalSeconds = Mathf.FloorToInt(currentTime);
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;
        timeText.text = $"Time: {minutes:D2}:{seconds:D2}";
    }
}
