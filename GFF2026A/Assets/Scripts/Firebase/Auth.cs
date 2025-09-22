using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using Firebase.Firestore;

public class Auth : MonoBehaviour
{
    public static Auth instance;
    private FirebaseAuth auth;
    private FirebaseFirestore firestore;
    void Start()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }

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

    public void Register(string email, string password, System.Action<bool> callback)
    {
        // ちゃんとメールアドレスになっているかとか、入力されているかとか
        // パスワードは何文字以上ですよとか、本当はもっと厳密にチェック
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            Debug.Log("メールアドレスかパスワードが入力されていません");
            return;
        }

        CreateUser(email, password, callback);
        // TODO: ホームシーンに飛ぶ
    }

    public void LoginFirebase(string email, string password, System.Action<bool> callback)
    {
        auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && !task.IsFaulted && !task.IsCanceled)
            {
                FirebaseUser user = task.Result.User;
                Debug.Log($"ログイン成功: {user.Email} ({user.UserId})");
                Debug.Log("ユーザーUID" + user.UserId);
                // TODO: ホームシーンに飛ぶ
                callback(true);
            }
            else
            {
                Debug.Log("接続できなかった");
                callback(false);
            }
        });
    }

    void CreateUser(string email, string password, System.Action<bool> callback)
    {
        auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                FirebaseUser newUser = task.Result.User;
                callback(true);
            }
            else
            {
                Debug.Log("接続できなかった");
                callback(false);
            }
        });
    }
}
