using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class LoginUI : MonoBehaviour
{
    public GameObject UserRegisterPanel;
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;

    public TMP_InputField emailLoginInput;
    public TMP_InputField passwordLoginInput;

    public TMP_InputField userNameInput;

    public GameObject loginErrorText;

    public Button registarButton;
    public Button loginButton;
    public Button submitButton;

    void Start()
    {
        registarButton.onClick.AddListener(OnRegisterButton);
        loginButton.onClick.AddListener(OnLoginButton);
        submitButton.onClick.AddListener(OnSubmitUserData);
    }

    public void OnRegisterButton()
    {
        Auth.instance.Register(emailInput.text, passwordInput.text, success =>
        {
            if (success)
            {
                UserRegisterPanel.SetActive(true);
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
                UserRegisterPanel.SetActive(true);
            }
            else
            {
                loginErrorText.SetActive(true);
            }
        });
    }

    public void OnSubmitUserData()
    {
        Auth.instance.UserInfoRegister(userNameInput.text, success =>{});
    }
}
