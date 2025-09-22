using UnityEngine;
using UnityEngine.UI;

public class BGMSEUI : MonoBehaviour
{

    // BGMのスライダー
    private Slider bgmSlider;

    // SEのスライダー
    private Slider seSlider;

    private Soundjson soundjson;

    void Start()
    {
        soundjson = FindAnyObjectByType<Soundjson>();
        // bgmSliderコンポーネントの取得
        bgmSlider = GetComponent<Slider>();

        // seSliderコンポーネントの取得
        seSlider = GetComponent<Slider>();
    }

    public void ChangBGMVolume()
    {
        soundjson.UpdateVolume("BGM", bgmSlider.value);
    }

    public void ChangSEVolume()
    {
        soundjson.UpdateVolume("SE", seSlider.value);
    }
}
