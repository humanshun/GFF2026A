using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UserRegisterPopup : BasePopup
{
    [Header("Inputs")]
    [SerializeField] private TMP_InputField userNameInput;

    [Header("Buttons")]
    [SerializeField] private Button submitButton;
    [SerializeField] private Button cancelButton;

    [Header("UI")]
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private GameObject loadingOverlay;

    // 任意：外部から初期名や完了後のコールバックを注入したい場合
    private Action onCompleted;

    protected override void OnAfterOpen()
    {
        if (submitButton) submitButton.onClick.AddListener(OnSubmit);
        if (cancelButton) cancelButton.onClick.AddListener(OnCancel);

        // 初期値（Authのキャッシュを表示）
        if (userNameInput)
        {
            var initial = Auth.instance?.userData?.username;
            userNameInput.text = string.IsNullOrWhiteSpace(initial) ? "" : initial;
        }
        SetBusy(false);
        SetMessage("");
    }

    protected override void OnBeforeClose()
    {
        if (submitButton) submitButton.onClick.RemoveAllListeners();
        if (cancelButton) cancelButton.onClick.RemoveAllListeners();
    }

    public void Setup(string initialName = null, Action onCompleted = null)
    {
        this.onCompleted = onCompleted;
        if (userNameInput && !string.IsNullOrEmpty(initialName))
            userNameInput.text = initialName;
    }

    void OnSubmit()
    {
        var name = userNameInput ? userNameInput.text.Trim() : "";
        if (string.IsNullOrEmpty(name))
        {
            SetMessage("ユーザー名を入力してください。");
            return;
        }

        SetBusy(true);
        Auth.instance.UserInfoRegister(name, ok =>
        {
            SetBusy(false);
            if (ok)
            {
                onCompleted?.Invoke(); // 画面更新など呼び出し元で続きの処理
                Close();
            }
            else
            {
                SetMessage("登録に失敗しました。時間をおいて再試行してください。");
            }
        });
    }

    void OnCancel()
    {
        // 取消し時は閉じるだけ（必要ならタイトルに残す等は呼び出し側で）
        Close();
    }

    void SetBusy(bool b)
    {
        if (submitButton) submitButton.interactable = !b;
        if (cancelButton) cancelButton.interactable = !b;
        if (userNameInput) userNameInput.interactable = !b;
        if (loadingOverlay) loadingOverlay.SetActive(b);
    }

    void SetMessage(string msg)
    {
        if (messageText) messageText.text = msg ?? "";
        if (!string.IsNullOrEmpty(msg)) Debug.Log(msg);
    }
}
