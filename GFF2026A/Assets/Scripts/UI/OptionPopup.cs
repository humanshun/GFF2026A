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

    // 変更した音量のスライダーへの反映
    private void OnEnable()
    {
        bgmSlider.value = AudioManager.Instance.GetBGMVolume();
        seSlider.value = AudioManager.Instance.GetSEVolume();

        // スライダー操作時に音量を変更
        bgmSlider.onValueChanged.AddListener(AudioManager.Instance.SetBGMVolume);
        seSlider.onValueChanged.AddListener(AudioManager.Instance.SetSEVolume);
    }

    private void OnDisable()
    {
        //
        bgmSlider.onValueChanged.RemoveAllListeners();
        seSlider.onValueChanged.RemoveAllListeners();
    }

    // ポップアップを閉じる
    void ClosePopup()
    {
        Destroy(optionScreen);
    }

    // public void ChangBGMVolume()
    // {
    //     AudioManager.Instance.SetBGMVolume(bgmSlider.value);
    // }

    // public void ChangSEVolume()
    // {
    //     AudioManager.Instance.SetSEVolume(seSlider.value);
    // }
}
