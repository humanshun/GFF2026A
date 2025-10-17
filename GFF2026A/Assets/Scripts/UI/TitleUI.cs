using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class TitleUI : MonoBehaviour
{
    [SerializeField] private Button startButton;
    [SerializeField] private Button changeNameButton;
    [SerializeField] private Button logoutButton;
    [SerializeField] private TextMeshProUGUI userNameText;
    public event Action OnChangeNameButton;

    void OnEnable()
    {
        if (Auth.instance != null)
            Auth.instance.OnUserDataUpdated += UpdateUserName;
    }
    void OnDisable()
    {
        if (Auth.instance != null)
            Auth.instance.OnUserDataUpdated -= UpdateUserName;
    }

    void Start()
    {
        startButton.onClick.AddListener(SwitchScene);
        changeNameButton.onClick.AddListener(() => OnChangeNameButton?.Invoke());
        if (logoutButton) logoutButton.onClick.AddListener(() =>
        {
            Auth.instance?.Logout(goToTitleScene: true, titleSceneName: "Title");
        });

        UpdateUserName();
    }
    public void SwitchScene()
    {
        SceneManager.LoadScene("InGame", LoadSceneMode.Single);
    }

    private void UpdateUserName()
    {
        var auth = Auth.instance;
        if (auth != null && auth.userData != null && !string.IsNullOrEmpty(auth.userData.username))
        {
            Debug.Log("ユーザー名を更新しました: " + auth.userData.username);
            userNameText.text = Auth.instance.userData.username;
        }
        else
        {
            Debug.Log("名前が設定されていません。");
            userNameText.text = "No Name";
        }
    }
}
