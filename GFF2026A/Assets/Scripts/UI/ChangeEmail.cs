using System;
using System.Collections;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChangeEmail : BasePopup
{
    [Header("UI")]
    [SerializeField] private Button closeButton;
    [SerializeField] private TMP_InputField newEmailInput;
    [SerializeField] private Button applyEmailChangeButton;
    [SerializeField] private Button refreshButton;         // 任意：Reload用
    [SerializeField] private TMP_Text currentEmailText;    // 任意：現在メール表示
    [SerializeField] private TMP_Text messageText;         // 任意：トースト/メッセージ表示

    protected override void OnAfterOpen()
    {
        if (closeButton) closeButton.onClick.AddListener(Close);

        if (applyEmailChangeButton) {
            applyEmailChangeButton.onClick.RemoveAllListeners();
            applyEmailChangeButton.onClick.AddListener(OnApplyEmailChangeButton);
        }
        if (refreshButton) {
            refreshButton.onClick.RemoveAllListeners();
            refreshButton.onClick.AddListener(OnClickRefresh);
        }
        if (newEmailInput) newEmailInput.onValueChanged.AddListener(_ => ValidateForm());
        RefreshUI();
        ValidateForm();
    }

    protected override void OnBeforeClose()
    {
        if (applyEmailChangeButton) applyEmailChangeButton.onClick.RemoveAllListeners();
        if (refreshButton)          refreshButton.onClick.RemoveAllListeners();
        if (newEmailInput)          newEmailInput.onValueChanged.RemoveAllListeners();
    }

    void RefreshUI()
    {
        var u = Auth.instance?.user;
        bool loggedIn = (u != null);
        bool canChange = loggedIn && !u.IsAnonymous;

        if (currentEmailText) currentEmailText.text =
            loggedIn ? (u.Email ?? "（未設定）") : "（未ログイン）";
        if (applyEmailChangeButton) applyEmailChangeButton.interactable = canChange;

        // 外部連携のみのユーザーの場合、注意文を出す（任意）
        if (messageText)
        {
            if (!loggedIn)            messageText.text = "ログインしてください。";
            else if (u.IsAnonymous)   messageText.text = "匿名ユーザーはメール変更できません。";
            else                      messageText.text = "";
        }
    }

    void ValidateForm()
    {
        var u = Auth.instance?.user;
        bool loggedIn = (u != null);
        bool canChange = loggedIn && !u.IsAnonymous;

        string input = newEmailInput ? newEmailInput.text.Trim() : "";
        bool looksEmail = LooksLikeEmail(input);
        bool sameAsCurrent = loggedIn && string.Equals(input, u.Email, StringComparison.OrdinalIgnoreCase);

        bool enable = canChange && looksEmail && !sameAsCurrent;
        if (applyEmailChangeButton) applyEmailChangeButton.interactable = enable;

        if (messageText)
        {
            if (canChange)
            {
                if (string.IsNullOrEmpty(input))       messageText.text = "";
                else if (!looksEmail)                   messageText.text = "メール形式が正しくありません。";
                else if (sameAsCurrent)                 messageText.text = "現在と同じメールです。";
                else                                    messageText.text = "";
            }
            // canChange==false の場合は RefreshUI() 側の文言を維持
        }
    }

    async void OnClickRefresh()
    {
        var u = Auth.instance?.user;
        if (u == null) { Toast("未ログインです"); return; }

        await u.ReloadAsync();
        RefreshUI();
        Toast("最新状態に更新しました。");
    }

    void OnApplyEmailChangeButton()
    {
        var a = Auth.instance;
        var u = a?.user;
        if (u == null) { Toast("未ログインです"); return; }

        string newEmail = newEmailInput ? newEmailInput.text.Trim() : "";
        if (!LooksLikeEmail(newEmail)) { Toast("メール形式が正しくありません"); return; }
        if (string.Equals(newEmail, u.Email, StringComparison.OrdinalIgnoreCase)) { Toast("現在と同じメールです"); return; }

        SetBusy(true);

        // あなたが実装済みの Auth.ChangeEmail(newEmail, (ok, reason)=>{})
        a.ChangeEmail(newEmail, async (ok, reason) =>
        {
            if (ok)
            {
                Toast("確認リンクを新しいメールに送信しました。開くと変更が確定します。");
                // 必要ならここで入力欄クリア
                if (newEmailInput) newEmailInput.text = "";
            }
            else if (reason == "RECENT_LOGIN_REQUIRED")
            {
                // 直近ログインが古いので再認証が必要
                // 例：Email/Password の場合（パス入力UIは各自実装）
                var pass = await PromptPasswordAsync(u.Email);
                if (!string.IsNullOrEmpty(pass))
                {
                    bool reauthed = await ReauthenticateEmailPasswordAsync(u.Email, pass);
                    if (reauthed)
                    {
                        // 再認証後にリトライ
                        a.ChangeEmail(newEmail, (ok2, _) =>
                        {
                            Toast(ok2
                                ? "確認リンクを再送しました。メール内リンクを開いてください。"
                                : "変更開始に失敗しました。");
                        });
                    }
                    else Toast("再認証に失敗しました。");
                }
                else
                {
                    Toast("再認証をキャンセルしました。");
                }
            }
            else if (reason == "WAITING_VERIFICATION")
            {
                Toast("メールのリンクを開いた後、この画面の“更新”で反映してください。");
            }
            else
            {
                Toast("変更開始に失敗しました。別のメールを試すか、時間をおいて再試行してください。");
            }

            SetBusy(false);
            ValidateForm();
        });
    }

    // ---- helpers ----
    void Toast(string msg)
    {
        if (messageText) messageText.text = msg;
        Debug.Log(msg);
    }
    void SetBusy(bool b)
    {
        if (applyEmailChangeButton) applyEmailChangeButton.interactable = !b;
        if (newEmailInput)          newEmailInput.interactable = !b;
    }
    bool LooksLikeEmail(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return false;
        // シンプルなメール判定（必要なら強化）
        return Regex.IsMatch(s, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
    }

    // ★再認証ユーティリティ（Email/Password の例。UIは各自実装に合わせて）
    System.Threading.Tasks.Task<string> PromptPasswordAsync(string email)
    {
        // TODO: パス入力ポップアップを出して、入力文字列を返す実装に置き換え
        Debug.Log($"再認証のためパスワードを入力してください: {email}");
        return System.Threading.Tasks.Task.FromResult<string>(null);
    }
    async System.Threading.Tasks.Task<bool> ReauthenticateEmailPasswordAsync(string email, string password)
    {
        try
        {
            var cred = Firebase.Auth.EmailAuthProvider.GetCredential(email, password);
            await Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser.ReauthenticateAsync(cred);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Reauthenticate 失敗: {e.Message}");
            return false;
        }
    }
}
