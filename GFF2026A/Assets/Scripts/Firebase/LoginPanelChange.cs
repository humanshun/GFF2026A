using System;
using UnityEngine;
using TMPro;
using NUnit.Framework;
using UnityEngine.UI;

public class LoginPanelChange : MonoBehaviour
{
    [SerializeField] private GameObject loginPanel;
    [SerializeField] private GameObject signupPanel;
    [SerializeField] private TextMeshProUGUI changePanelText;
    [SerializeField] private Button changePanelButton;
    private bool isLogin = true;
    void Start()
    {
        changePanelButton.onClick.AddListener(OnChangePanelButton);
        loginPanel.SetActive(true);
        signupPanel.SetActive(false);
        changePanelText.text = "サインアップ";
    }

    public void OnChangePanelButton()
    {
        if (isLogin)
        {
            loginPanel.SetActive(false);
            signupPanel.SetActive(true);
            changePanelText.text = "ログイン";
        }
        else
        {
            loginPanel.SetActive(true);
            signupPanel.SetActive(false);
            changePanelText.text = "サインアップ";
        }

        isLogin = !isLogin;
    }    
}
