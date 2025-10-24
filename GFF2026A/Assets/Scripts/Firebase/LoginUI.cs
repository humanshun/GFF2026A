using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Threading.Tasks;

public class LoginUI : MonoBehaviour
{
    public GameObject userRegisterPanel;
    public GameObject loginPanel;
    public GameObject signUpPanel;
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;

    public TMP_InputField emailLoginInput;
    public TMP_InputField passwordLoginInput;

    public TMP_InputField userNameInput;

    public GameObject loginErrorText;

    public Button registarButton;
    public Button loginButton;
    public Button guestLoginButton;
    public Button submitButton;
    public Button googleButton;
    public Button twitterButton;

    // 追加：読み込み中のオーバーレイ/スピナー（任意）
    public GameObject loadingOverlay;
    private TitleUI titleUI;

    [SerializeField] string googleWebClientId = "487118386482-2krd3ge76vv8jf94n2ebgarudvrgmu0u.apps.googleusercontent.com";

    void OnEnable()
    {
        Auth.instance.OnUserRegisterPanel += DisplayUserRegisterPanel;
        Auth.instance.OnClosePanel += ClosePanel;
        titleUI = FindAnyObjectByType<TitleUI>();
        if (titleUI != null) titleUI.OnChangeNameButton += DisplayUserRegisterPanel;
    }
    void OnDisable()
    {
        Auth.instance.OnUserRegisterPanel -= DisplayUserRegisterPanel;
        Auth.instance.OnClosePanel -= ClosePanel;
        if (titleUI != null) titleUI.OnChangeNameButton -= DisplayUserRegisterPanel;
    }
    void Start()
    {
        ClosePanel();
        InitUI();
        registarButton.onClick.AddListener(OnRegisterButton);
        loginButton.onClick.AddListener(OnLoginButton);
        guestLoginButton.onClick.AddListener(OnGuestLoginButton);
        submitButton.onClick.AddListener(OnSubmitUserData);

        if (googleButton) googleButton.onClick.AddListener(async () => await OnGoogleButtonAsync());
        if (twitterButton) twitterButton.onClick.AddListener(async () => await OnTwitterButtonAsync());
    }

    void SetBusy(bool busy)
    {
        registarButton.interactable = !busy;
        loginButton.interactable = !busy;
        submitButton.interactable = !busy;
        if (googleButton) googleButton.interactable = !busy;
        if (twitterButton) twitterButton.interactable = !busy;
        if (loadingOverlay) loadingOverlay.SetActive(busy);
    }

    void InitUI()
    {
        ClosePanel();
        var auth = Auth.instance;
        bool loggedIn = auth != null && auth.user != null;

        // ユーザー名（ニックネーム）が登録されているか
        bool hasUserName = false;
        if (auth != null && auth.userData != null)
        {
            hasUserName = !string.IsNullOrEmpty(auth.userData.username);
        }

        if (!loggedIn)
        {
            //未ログイン　=> サインアップ/ログイン選択を出す
            DisplayLoginPanel();
        }
        else if (!hasUserName)
        {
            //ログイン済み、ユーザー名未登録 => ユーザー名登録を出す
            DisplayUserRegisterPanel();
        }
        else
        {
            //ログイン済み、ユーザー名登録済み => 何も出さない
            ClosePanel();
        }

        if (loginErrorText) loginErrorText.SetActive(false);
        SetBusy(false);
    }

    public void OnRegisterButton()
    {
        Auth.instance.Register(emailInput.text, passwordInput.text, success =>
        {
            if (success)
            {
                ClosePanel();
            }
            else
            {
                //失敗したときの処理
                Debug.Log("登録に失敗しました");
            }
        });
    }

    public void OnLoginButton()
    {
        Auth.instance.LoginFirebase(emailLoginInput.text, passwordLoginInput.text, success =>
        {
            if (success)
            {
                ClosePanel();
            }
            else
            {
                loginErrorText.SetActive(true);
            }
        });
    }

    public void OnGuestLoginButton()
    {
        Auth.instance.GuestLogin(success =>
        {
            if (success)
            {
                ClosePanel();
            }
            else
            {
                loginErrorText.SetActive(true);
            }
        });
    }

    public void OnSubmitUserData()
    {
        Auth.instance.UserInfoRegister(userNameInput.text, success => { });
    }

    async Task OnGoogleButtonAsync()
    {
        SetBusy(true);
#if UNITY_ANDROID || UNITY_IOS
        await Auth.instance.SignInWithGoogleAsync(googleWebClientId);
        // サインインが通ったら、ユーザー名の有無で分岐
        Auth.instance.EnterGameOrAskUsername(needName =>
        {
            SetBusy(false);
            if (needName) UserRegisterPanel.SetActive(true);
        });
#else
        SetBusy(false);
        loginErrorText.SetActive(true);
        Debug.LogWarning("このプラットフォームではGoogleサインイン未対応です");
#endif
    }

    async Task OnTwitterButtonAsync()
    {
#if UNITY_ANDROID || UNITY_IOS
    SetBusy(true);
    await FindObjectOfType<TwitterOAuth>().StartTwitterSignInAsync();
    // 成功したら Auth 側で InGame 遷移まで行く
    SetBusy(false);
#else
        Debug.LogWarning("Twitterサインインは実機でテストしてね");
#endif
    }

    public void DisplayLoginPanel()
    {
        userRegisterPanel.SetActive(false);
        loginPanel.SetActive(true);
        signUpPanel.SetActive(false);
    }

    public void DisplaySignUpPanel()
    {
        userRegisterPanel.SetActive(false);
        loginPanel.SetActive(false);
        signUpPanel.SetActive(true);
    }

    public void DisplayUserRegisterPanel()
    {
        userRegisterPanel.SetActive(true);
        loginPanel.SetActive(false);
        signUpPanel.SetActive(false);
    }

    public void ClosePanel()
    {
        userRegisterPanel.SetActive(false);
        loginPanel.SetActive(false);
        signUpPanel.SetActive(false);
    }
}
