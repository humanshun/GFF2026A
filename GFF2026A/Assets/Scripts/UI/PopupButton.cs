using UnityEngine;
using UnityEngine.UI;

public class PopupButton : MonoBehaviour
{
    // 開きたいポップアップのPrefab
    [SerializeField] private GameObject popupPrefab;

    // ボタンにクリックイベントを追加
    void Start()
    {
        GetComponent<Button>().onClick.AddListener(OpenPopup);
    }

    // ポップアップを開く
    void OpenPopup()
    {
        PopupManager.Instance.Open(popupPrefab);
    }
}
