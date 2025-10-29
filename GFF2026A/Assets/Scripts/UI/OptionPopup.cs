using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class OptionPopup : BasePopup
{
    [Header("UI")]
    [SerializeField] private Button closeButton;         // 閉じる
    [SerializeField] private Slider bgmSlider;           // BGM
    [SerializeField] private Slider seSlider;            // SE
    [SerializeField] private Button logoutButton;        // ログアウト
    [SerializeField] private Button verifyEmailButton;   // メール確認

    // --- BasePopup のフック：開いた直後にUI配線を行う ---
    protected override void OnAfterOpen()
    {
        // 閉じる
        if (closeButton) closeButton.onClick.AddListener(Close);

        // ログアウト
        if (logoutButton)
        {
            logoutButton.onClick.RemoveAllListeners();
            logoutButton.onClick.AddListener(() =>
            {
                Auth.instance?.Logout(goToTitleScene: true, titleSceneName: "Title");
            });
        }

        // メール確認
        if (verifyEmailButton)
        {
            verifyEmailButton.onClick.RemoveAllListeners();
            verifyEmailButton.onClick.AddListener(OnVerifyEmailButton);
            UpdateVerifyButtonState(); // 表示/有効状態を反映
        }

        // スライダーは AudioManager の準備ができてから配線
        StartCoroutine(SetupAudioSliders());
    }

    // --- BasePopup のフック：閉じる直前に後片付け ---
    protected override void OnBeforeClose()
    {
        // リスナー解除（多重登録防止）
        if (closeButton)         closeButton.onClick.RemoveListener(Close);
        if (logoutButton)        logoutButton.onClick.RemoveAllListeners();
        if (verifyEmailButton)   verifyEmailButton.onClick.RemoveAllListeners();

        if (bgmSlider) bgmSlider.onValueChanged.RemoveAllListeners();
        if (seSlider)  seSlider.onValueChanged.RemoveAllListeners();
    }

    // AudioManager が使えるまで待ってから、値反映とリスナー登録
    private IEnumerator SetupAudioSliders()
    {
        if (bgmSlider == null || seSlider == null)
        {
            Debug.LogError("[OptionPopup] Slider が未アサインです（PrefabのInspectorで割当ててください）");
            yield break;
        }

        int safety = 180; // 約3秒
        while (AudioManager.Instance == null && safety-- > 0) yield return null;

        if (AudioManager.Instance == null)
        {
            Debug.LogError("[OptionPopup] AudioManager.Instance が見つかりません。初期化順を確認してください。");
            yield break;
        }

        var am = AudioManager.Instance;

        // 現在値反映（イベント発火なし）
        bgmSlider.SetValueWithoutNotify(am.GetBGMVolume());
        seSlider .SetValueWithoutNotify(am.GetSEVolume());

        // リスナー配線（念のため既存解除）
        bgmSlider.onValueChanged.RemoveAllListeners();
        seSlider .onValueChanged.RemoveAllListeners();

        bgmSlider.onValueChanged.AddListener(am.SetBGMVolume);
        seSlider .onValueChanged.AddListener(am.SetSEVolume);
    }

    // メール確認ボタン押下
    private void OnVerifyEmailButton()
    {
        if (Auth.instance == null || Auth.instance.user == null)
        {
            Debug.LogWarning("未ログインです");
            return;
        }

        Auth.instance.SendVerificationMail(success =>
        {
            if (success)
            {
                Debug.Log("確認メールを送信しました。メール内リンクを開いてから、この画面は閉じてもOKです。");
            }
            else
            {
                Debug.LogWarning("確認メール送信に失敗しました。しばらくしてから再度お試しください。");
            }
        });
    }

    // Verifyボタンの表示/有効切替（任意でUIに合わせて調整）
    private void UpdateVerifyButtonState()
    {
        if (verifyEmailButton == null) return;

        var u = Auth.instance?.user;
        bool canShow = (u != null && !u.IsAnonymous && !u.IsEmailVerified);

        verifyEmailButton.gameObject.SetActive(canShow);
        verifyEmailButton.interactable = canShow;
    }
}
