using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Threading.Tasks;

public class LoginUI : MonoBehaviour
{
    private TitleUI titleUI;

    [Header("Auth Popups (Prefabs)")]
    [SerializeField] private GameObject loginPopupPrefab;        // ← loginPanel のPrefab版（BasePopup）
    [SerializeField] private GameObject signUpPopupPrefab;       // ← signUpPanel のPrefab版
    [SerializeField] private GameObject registerNamePopupPrefab; // ← userRegisterPanel のPrefab版

    // 既存の入力やボタンは、各Popup側で持つ設計に寄せるのがベスト。
    // ここでは表示制御に専念する。

    [SerializeField] string googleWebClientId = "YOUR_WEB_CLIENT_ID";

    void OnEnable()
    {
        titleUI = FindFirstObjectByType<TitleUI>();
        if (titleUI != null) titleUI.OnChangeNameButton += DisplayUserRegisterPanel;
        Auth.instance.OnUserRegisterPanel += DisplayUserRegisterPanel;
        Auth.instance.OnClosePanel        += ClosePanel;
    }
    void OnDisable()
    {
        if (titleUI != null) titleUI.OnChangeNameButton -= DisplayUserRegisterPanel;
        Auth.instance.OnUserRegisterPanel -= DisplayUserRegisterPanel;
        Auth.instance.OnClosePanel        -= ClosePanel;
    }

    void Start()
    {
        InitUI();
    }

    void InitUI()
    {
        var auth = Auth.instance;
        bool loggedIn = auth != null && auth.user != null;
        bool hasUserName = auth?.userData != null && !string.IsNullOrEmpty(auth.userData.username);

        if (!loggedIn)
        {
            DisplayLoginPanel();
        }
        else if (!hasUserName)
        {
            DisplayUserRegisterPanel();
        }
        else
        {
            ClosePanel();
        }
    }

    // ===== ここからは「表示切替」だけ =====

    public void DisplayLoginPanel()
        => PopupManager.Instance.OpenExclusive("auth", loginPopupPrefab);

    public void DisplaySignUpPanel()
        => PopupManager.Instance.OpenExclusive("auth", signUpPopupPrefab);

    public void DisplayUserRegisterPanel()
        => PopupManager.Instance.OpenExclusive("auth", registerNamePopupPrefab);

    public void ClosePanel()
        => PopupManager.Instance.CloseGroup("auth");
}
