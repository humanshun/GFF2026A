using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using Firebase.Firestore;
using UnityEngine.SceneManagement;

public class Auth : MonoBehaviour
{
    public static Auth instance;
    private FirebaseAuth auth;
    private FirebaseFirestore firestore;
    public FirebaseUser user { get; private set; }
    public UserData userData { get; private set; }

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
    }

    public void LoginFirebase(string email, string password, System.Action<bool> callback)
    {
        auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && !task.IsFaulted && !task.IsCanceled)
            {
                user = task.Result.User;
                Debug.Log($"ログイン成功: {user.Email} ({user.UserId})");
                Debug.Log("ユーザーUID" + user.UserId);
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
                user = task.Result.User;
                callback(true);
            }
            else
            {
                Debug.Log("接続できなかった");
                callback(false);
            }
        });
    }

    public void UserInfoRegister(string username, System.Action<bool> callback)
    {
        if (string.IsNullOrEmpty(username))
        {
            Debug.Log("名前が入力されていません");
            return;
        }

        string uid = user.UserId;
        userData = new UserData
        {
            username = username,
            bestScore = 0,
            timestamp = (int)(System.DateTime.UtcNow - new System.DateTime(1970, 1, 1)).TotalSeconds
        };

        firestore.Collection("userInfo").Document(uid).SetAsync(userData).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log("ユーザーデータの登録に成功しました");
                SceneManager.LoadScene("InGame");
            }
            else
            {
                Debug.Log("ユーザーデータの登録に失敗しました");
            }
        });
    }
}
