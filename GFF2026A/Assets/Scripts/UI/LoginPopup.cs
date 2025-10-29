using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoginPopup : BasePopup
{
    [Header("Inputs")]
    [SerializeField] private TMP_InputField emailInput;       // ログイン用メール
    [SerializeField] private TMP_InputField passwordInput;    // ログイン用パスワード

    [Header("Buttons")]
    [SerializeField] private Button loginButton;              // ログイン
    [SerializeField] private Button googleButton;             // Google
    [SerializeField] private Button twitterButton;            // X(Twitter)
    [SerializeField] private Button guestLoginButton;         // ゲスト
    [SerializeField] private Button forgotPassButton;         // パスワード忘れ

    [Header("UI")]
    [SerializeField] private GameObject loadingOverlay;       // 読み込み中のブロッカー（任意）
    [SerializeField] private TMP_Text messageText;            // エラ/案内表示（任意）

    [Header("OAuth")]
    [SerializeField] private string googleWebClientId =
        "487118386482-2krd3ge76vv8jf94n2ebgarudvrgmu0u.apps.googleusercontent.com";

    // --- BasePopup lifecycle ---
    protected override void OnAfterOpen()
    {
        if (loginButton)       loginButton.onClick.AddListener(OnLoginButton);
        if (guestLoginButton)  guestLoginButton.onClick.AddListener(OnGuestLoginButton);
        if (forgotPassButton)  forgotPassButton.onClick.AddListener(OnForgotPassButton);

        if (googleButton)  googleButton.onClick.AddListener(async () => await OnGoogleButtonAsync());
        if (twitterButton) twitterButton.onClick.AddListener(async () => await OnTwitterButtonAsync());

        SetBusy(false);
        SetMessage(""); // 初期表示クリア
    }

    protected override void OnBeforeClose()
    {
        if (loginButton)       loginButton.onClick.RemoveAllListeners();
        if (guestLoginButton)  guestLoginButton.onClick.RemoveAllListeners();
        if (forgotPassButton)  forgotPassButton.onClick.RemoveAllListeners();
        if (googleButton)      googleButton.onClick.RemoveAllListeners();
        if (twitterButton)     twitterButton.onClick.RemoveAllListeners();
    }

    // --- Handlers ---
    void OnLoginButton()
    {
        var email = emailInput ? emailInput.text.Trim() : "";
        var pass  = passwordInput ? passwordInput.text : "";

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(pass))
        {
            SetMessage("メールとパスワードを入力してください。");
            return;
        }

        SetBusy(true);
        Auth.instance.LoginFirebase(email, pass, success =>
        {
            SetBusy(false);
            if (success)
            {
                Close(); // BasePopupのCloseでOK（PopupManager経由なら自動破棄）
            }
            else
            {
                SetMessage("ログインに失敗しました。メール/パスワードを確認してください。");
            }
        });
    }

    void OnGuestLoginButton()
    {
        SetBusy(true);
        Auth.instance.GuestLogin(success =>
        {
            SetBusy(false);
            if (success) Close();
            else         SetMessage("ゲストログインに失敗しました。");
        });
    }

    void OnForgotPassButton()
    {
        var email = emailInput ? emailInput.text.Trim() : "";
        if (string.IsNullOrEmpty(email))
        {
            SetMessage("パスワード再設定にはメールを入力してください。");
            return;
        }

        Auth.instance.SendPasswordResetMail(email, ok =>
        {
            if (ok) SetMessage("再設定メールを送信しました。メールを確認してください。");
            else    SetMessage("送信に失敗しました。メールアドレスを確認してください。");
        });
    }

    async Task OnGoogleButtonAsync()
    {
        SetBusy(true);
#if UNITY_ANDROID || UNITY_IOS
        await Auth.instance.SignInWithGoogleAsync(googleWebClientId);

        // Googleサインイン後はAuth側で自動遷移/ユーザー名確認を行う設計ならここは閉じるだけ
        SetBusy(false);
        Close();
#else
        SetBusy(false);
        SetMessage("このプラットフォームではGoogleサインイン未対応です。");
#endif
    }

    async Task OnTwitterButtonAsync()
    {
        SetBusy(true);
#if UNITY_ANDROID || UNITY_IOS
        var oauth = FindFirstObjectByType<TwitterOAuth>();
        if (oauth != null)
        {
            await oauth.StartTwitterSignInAsync();
            SetBusy(false);
            Close();
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
    void SetBusy(bool busy)
    {
        if (loginButton)       loginButton.interactable      = !busy;
        if (googleButton)      googleButton.interactable      = !busy;
        if (twitterButton)     twitterButton.interactable     = !busy;
        if (guestLoginButton)  guestLoginButton.interactable  = !busy;
        if (forgotPassButton)  forgotPassButton.interactable  = !busy;
        if (emailInput)        emailInput.interactable        = !busy;
        if (passwordInput)     passwordInput.interactable     = !busy;
        if (loadingOverlay)    loadingOverlay.SetActive(busy);
    }

    void SetMessage(string msg)
    {
        if (messageText) messageText.text = msg ?? "";
        if (!string.IsNullOrEmpty(msg)) Debug.Log(msg);
    }
}
