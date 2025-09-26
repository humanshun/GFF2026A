using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class OptionPopup : MonoBehaviour
{
    [SerializeField] private Button closeButton; // 閉じるボタン
    [SerializeField] private GameObject optionScreen; // オプション画面のオブジェクト
    [SerializeField] private Slider bgmSlider; // BGMのスライダー
    [SerializeField] private Slider seSlider; // SEのスライダー
    [SerializeField] Soundjson soundjson;

    // 閉じるボタンにクリックイベントを追加
    void Start()
    {
        closeButton.onClick.AddListener(ClosePopup);
    }

    // ポップアップを閉じる
    void ClosePopup()
    {
        Destroy(optionScreen);
    }

    public void ChangBGMVolume()
    {
        AudioManager.Instance.SetBGMVolume(bgmSlider.value);
    }

    public void ChangSEVolume()
    {
        AudioManager.Instance.SetSEVolume(seSlider.value);
    }
}
