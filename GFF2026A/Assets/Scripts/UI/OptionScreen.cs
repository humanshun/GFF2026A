using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class OptionScreen : MonoBehaviour
{
    // 閉じるボタン
    [SerializeField] private Button closeButton;

    // オプション画面のオブジェクト
    [SerializeField] private GameObject optionScreen;

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
}
