using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;

public class SignUpPopup : BasePopup
{
    [Header("Inputs")]
    [SerializeField] private TMP_InputField emailInput;
    [SerializeField] private TMP_InputField passwordInput;

    [Header("Buttons")]
    [SerializeField] private Button signUpButton;     // 登録
    [SerializeField] private Button toLoginButton;    // 「ログインへ」
    [SerializeField] private Button googleButton;     // Googleで登録/ログイン
    [SerializeField] private Button twitterButton;    // X(Twitter)で登録/ログイン

    [Header("UI")]
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private GameObject loadingOverlay;

    [Header("Prefabs for Navigation")]
    [SerializeField] private GameObject loginPopupPrefab;          // ログインへ戻る
    [SerializeField] private GameObject userRegisterPopupPrefab;   // 任意：ユーザー名登録へ

    [Header("OAuth")]
    [SerializeField] private string googleWebClientId =
        "487118386482-2krd3ge76vv8jf94n2ebgarudvrgmu0u.apps.googleusercontent.com";

    protected override void OnAfterOpen()
    {
        if (signUpButton)  signUpButton.onClick.AddListener(OnSignUp);
        if (toLoginButton) toLoginButton.onClick.AddListener(ToLogin);

        if (googleButton)  googleButton.onClick.AddListener(async () => await OnGoogleButtonAsync());
        if (twitterButton) twitterButton.onClick.AddListener(async () => await OnTwitterButtonAsync());

        SetBusy(false);
        SetMessage("");
    }

    protected override void OnBeforeClose()
    {
        if (signUpButton)  signUpButton.onClick.RemoveAllListeners();
        if (toLoginButton) toLoginButton.onClick.RemoveAllListeners();
        if (googleButton)  googleButton.onClick.RemoveAllListeners();
        if (twitterButton) twitterButton.onClick.RemoveAllListeners();
    }

    // --- Email/Password で登録（成功時点で既にログイン済み） ---
    void OnSignUp()
    {
        var email = emailInput ? emailInput.text.Trim() : "";
        var pass  = passwordInput ? passwordInput.text   : "";

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(pass))
        {
            SetMessage("メールとパスワードを入力してください。");
            return;
        }

        SetBusy(true);
        Auth.instance.Register(email, pass, success =>
        {
            SetBusy(false);
            if (success)
            {
                // Auth.CreateUser内で確認メール送信済み想定
                // ここでユーザー名未登録ならユーザー名ポップへ遷移しても良い
                if (userRegisterPopupPrefab)
                {
                    PopupManager.Instance.OpenExclusive("auth", userRegisterPopupPrefab);
                }
                Close();
            }
            else
            {
                SetMessage("登録に失敗しました。時間をおいて再試行してください。");
            }
        });
    }

    // --- 既にアカウントを持っている人向けの導線 ---
    void ToLogin()
    {
        if (loginPopupPrefab)
            PopupManager.Instance.OpenExclusive("auth", loginPopupPrefab);
        Close();
    }

    // --- Google で登録/ログイン（どちらも同じボタンでOK） ---
    async Task OnGoogleButtonAsync()
    {
        SetBusy(true);
#if UNITY_ANDROID || UNITY_IOS
        await Auth.instance.SignInWithGoogleAsync(googleWebClientId);
        SetBusy(false);

        // Google はサインイン時点で「登録orログイン」が完了している
        // ユーザー名の有無で分岐するなら Auth.EnterGameOrAskUsername を使ってもOK
        if (registerNamePopupPrefab)
        {
            // 必要ならここでユーザー名登録へ誘導
            // PopupManager.Instance.OpenExclusive("auth", registerNamePopupPrefab);
        }
        Close();
#else
        SetBusy(false);
        SetMessage("このプラットフォームではGoogleサインイン未対応です。");
#endif
    }

    // --- X(Twitter) で登録/ログイン ---
    async Task OnTwitterButtonAsync()
    {
        SetBusy(true);
#if UNITY_ANDROID || UNITY_IOS
        var oauth = FindFirstObjectByType<TwitterOAuth>();
        if (oauth != null)
        {
            await oauth.StartTwitterSignInAsync();
            SetBusy(false);
            Close(); // サインイン完了想定
        }
        else
        {
            SetBusy(false);
            SetMessage("TwitterOAuthコンポーネントが見つかりません。");
        }
#else
        SetBusy(false);
        SetMessage("Twitterサインインは実機でテストしてください。");
#endif
    }

    // --- UI helpers ---
    void SetBusy(bool b)
    {
        if (signUpButton)  signUpButton.interactable = !b;
        if (toLoginButton) toLoginButton.interactable = !b;
        if (googleButton)  googleButton.interactable  = !b;
        if (twitterButton) twitterButton.interactable = !b;

        if (emailInput)    emailInput.interactable    = !b;
        if (passwordInput) passwordInput.interactable = !b;

        if (loadingOverlay) loadingOverlay.SetActive(b);
    }

    void SetMessage(string msg)
    {
        if (messageText) messageText.text = msg ?? "";
        if (!string.IsNullOrEmpty(msg)) Debug.Log(msg);
    }
}
