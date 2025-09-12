using UnityEngine;
using UnityEngine.UI;

public class BGMSEUI : MonoBehaviour
{
    // BGMのスライダー
    private Slider bgmSlder;

    // SEのスライダー
    private Slider seSlider;

    void Start()
    {
        // Sliderコンポーネントの取得
        bgmSlder = GetComponent<Slider>();

        // Sliderコンポーネントの取得
        seSlider = GetComponent<Slider>();

        // valueを初期化
        bgmSlder.value = 1.0f;

        // valueを初期化
        seSlider.value = 1.0f;

        // Sliderの値が変わった時のイベント登録
        bgmSlder.onValueChanged.AddListener(OnBGMVolumeChanged);

        // Sliderの値が変わった時のイベント登録
        seSlider.onValueChanged.AddListener(OnSEVolumeChanged);
    }

    private void OnBGMVolumeChanged(float value)
    {
        // ここに処理を追加
    }

    private void OnSEVolumeChanged(float value)
    {
        // ここに処理を追加
    }
}
