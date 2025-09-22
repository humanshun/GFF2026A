using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using Firebase.Firestore;
using TMPro;
using UnityEngine.UI;
using Unity.VisualScripting;

public class Login : MonoBehaviour
{
    private FirebaseAuth auth;
    private FirebaseFirestore firestore;

    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;

    public TMP_InputField emailLoginInput;
    public TMP_InputField passwordLoginInput;

    public GameObject loginErrorText;

    public Button registarButton;
    public Button loginButton;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        registarButton.onClick.AddListener(OnRegisterButton);
        loginButton.onClick.AddListener(OnLoginButton);
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log("接続できた");
                FirebaseApp app = FirebaseApp.DefaultInstance;
                firestore = FirebaseFirestore.DefaultInstance;
                auth = FirebaseAuth.DefaultInstance;

                // CreateUser("test@gmail.com", "testtest");
            }
            else
            {
                Debug.Log("接続できなかった");
            }
        });

    }

    public void OnRegisterButton()
    {
        string email = emailInput.text;
        string password = passwordInput.text;

        // ちゃんとメールアドレスになっているかとか、入力されているかとか
        // パスワードは何文字以上ですよとか、本当はもっと厳密にチェック
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            Debug.Log("メールアドレスかパスワードが入力されていません");
            return;
        }

        CreateUser(email, password);
        // TODO: ホームシーンに飛ぶ
    }

    public void OnLoginButton()
    {
        string email = emailLoginInput.text;
        string password = passwordLoginInput.text;

        auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && !task.IsFaulted && !task.IsCanceled)
            {
                FirebaseUser user = task.Result.User;
                Debug.Log($"ログイン成功: {user.Email} ({user.UserId})");
                Debug.Log("ユーザーUID" + user.UserId);
                // TODO: ホームシーンに飛ぶ
            }
            else
            {
                Debug.Log("接続できなかった");
                loginErrorText.SetActive(true);
            }
        });
    }

    void CreateUser(string email, string password)
    {
        auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                FirebaseUser newUser = task.Result.User;
            }
            else
            {
                Debug.Log("接続できなかった");
            }
        });
    }
}
