using UnityEngine;
using UnityEngine.UI;

public class OptionUI : MonoBehaviour
{
    // optionButtonの参照
    [SerializeField] private Button optionButton;
    
    // optionPopupの参照
    [SerializeField] private GameObject optionPopup;
    
    // 現在表示されているポップアップ
    private GameObject currentPopup;

    void Start()
    {
        // Buttonが押されたとき
        optionButton.onClick.AddListener(ShowOptionScreen);
    }

    // ポップアップのインスタンス生成
    public void ShowOptionScreen()
    {
        currentPopup = Instantiate(optionPopup, transform);
    }
}
