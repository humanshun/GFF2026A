using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using Firebase.Firestore;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

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
            callback?.Invoke(false);
            return;
        }
        if (user == null || firestore == null)
        {
            Debug.LogWarning("Auth 未初期化または未ログインです");
            callback?.Invoke(false);
            return;
        }

        string uid = user.UserId;
        var docRef = firestore.Collection("userInfo").Document(uid);
        int nowUnix = (int)(System.DateTime.UtcNow - new System.DateTime(1970, 1, 1)).TotalSeconds;

        // まず存在確認
        docRef.GetSnapshotAsync().ContinueWithOnMainThread(getTask =>
        {
            if (!getTask.IsCompleted || getTask.IsFaulted || getTask.IsCanceled)
            {
                Debug.LogWarning("ユーザーデータの取得に失敗");
                callback?.Invoke(false);
                return;
            }

            var snap = getTask.Result;

            if (snap.Exists)
            {
                // 既存ユーザー → username/timestamp だけ差分更新（bestScoreは維持）
                var updates = new Dictionary<string, object>
                {
                { "username",  username },
                { "timestamp", nowUnix }
                };

                docRef.SetAsync(updates, SetOptions.MergeAll).ContinueWithOnMainThread(setTask =>
                {
                    bool ok = setTask.IsCompleted && !setTask.IsFaulted && !setTask.IsCanceled;
                    if (ok)
                    {
                        // ローカルキャッシュを更新（bestScore は既存値を保持）
                        if (userData == null) userData = new UserData();
                        userData.username = username;
                        userData.timestamp = nowUnix;

                        if (snap.ContainsField("bestScore"))
                        {
                            try { userData.bestScore = snap.GetValue<int>("bestScore"); }
                            catch { userData.bestScore = (int)snap.GetValue<long>("bestScore"); }
                        }
                        else
                        {
                            userData.bestScore = 0;
                        }
                    }

                    callback?.Invoke(ok);
                    if (ok) SceneManager.LoadScene("InGame");
                });
            }
            else
            {
                // 新規ユーザー → 初期化して作成
                var newData = new UserData
                {
                    username = username,
                    bestScore = 0,
                    timestamp = nowUnix
                };

                docRef.SetAsync(newData).ContinueWithOnMainThread(setTask =>
                {
                    bool ok = setTask.IsCompleted && !setTask.IsFaulted && !setTask.IsCanceled;
                    if (ok) userData = newData;

                    callback?.Invoke(ok);
                    if (ok) SceneManager.LoadScene("InGame");
                });
            }
        });
    }

    public void UpdateBestScoreIfHigher(int newScore, System.Action<bool, int> callback = null)
    {
        if (user == null || firestore == null)
        {
            Debug.LogWarning("Auth 未初期化または未ログインです");
            callback?.Invoke(false, -1);
            return;
        }

        string uid = user.UserId;
        var docRef = firestore.Collection("userInfo").Document(uid);

        docRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (!task.IsCompleted || task.IsFaulted || task.IsCanceled)
            {
                Debug.LogWarning("ユーザーデータの取得に失敗");
                callback?.Invoke(false, -1);
                return;
            }

            var snap = task.Result;
            int currentBest = 0;

            if (snap.Exists && snap.ContainsField("bestScore"))
            {
                //Firestoreのintはlongにマップされることもあるので安全に取り出す
                try { currentBest = snap.GetValue<int>("bestScore"); }
                catch { currentBest = (int)snap.GetValue<long>("bestScore"); }
            }

            if (newScore > currentBest)
            {
                int nowUnix = (int)(System.DateTime.UtcNow - new System.DateTime(1970, 1, 1)).TotalSeconds;
                var updates = new Dictionary<string, object>
                {
                    { "bestScore", newScore },
                    { "timestamp", nowUnix }
                };

                //ついでにローカルキャッシュも更新
                if (userData != null)
                {
                    userData.bestScore = newScore;
                    userData.timestamp = nowUnix;
                }

                docRef.SetAsync(updates, SetOptions.MergeAll).ContinueWithOnMainThread(taskt =>
                {
                    if (!taskt.IsFaulted && !taskt.IsCanceled)
                    {
                        // 成功
                        callback?.Invoke(true, newScore);
                    }
                    else
                    {
                        // 失敗
                        if (taskt.Exception != null) Debug.LogException(taskt.Exception);
                        callback?.Invoke(false, currentBest);
                    }
                });
            }
            else
            {
                //更新なし
                Debug.Log($"bestScore 変化なし(現状 {currentBest}, 今回 {newScore})");
                callback?.Invoke(false, currentBest);
            }

        });
    }
}
