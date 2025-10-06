using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using Firebase.Firestore;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using Google;
using System.Threading.Tasks;
using System;

public class Auth : MonoBehaviour
{
    public static Auth instance;
    private FirebaseAuth auth;
    private FirebaseFirestore firestore;
    public FirebaseUser user { get; private set; }
    public UserData userData { get; private set; }
    public event Action OnUserRegisterPanel;
    public event Action OnClosePanel;

    void Awake()
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
    }

    void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log("接続できた");
                FirebaseApp app = FirebaseApp.DefaultInstance;
                firestore = FirebaseFirestore.DefaultInstance;
                auth = FirebaseAuth.DefaultInstance;

                // 端末にセッションを保存（次回以降ノー入力）
                // TryAutoSignIn();
            }
            else
            {
                Debug.Log("接続できなかった");
            }
        });
    }

    public void Register(string email, string password, Action<bool> callback)
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

    public void LoginFirebase(string email, string password, Action<bool> callback)
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

    void CreateUser(string email, string password, Action<bool> callback)
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

    public void UserInfoRegister(string username, Action<bool> callback)
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
        int nowUnix = (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;

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
                    if (ok) OnClosePanel?.Invoke();
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
                    if (ok) OnClosePanel?.Invoke();
                });
            }
        });
    }

    public void UpdateBestScoreIfHigher(int newScore, Action<bool, int> callback = null)
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
                int nowUnix = (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
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

    private void TryAutoSignIn()
    {
        var cu = auth?.CurrentUser;
        if (cu == null)
        {
            Debug.Log("前回セッションなし → ログイン画面で待機");
            return;
        }

        user = cu; //既存セッションを採用
        user.ReloadAsync().ContinueWithOnMainThread(task =>
        {
            if (!task.IsFaulted && !task.IsCanceled)
            {
                Debug.Log($"AutoSignIn OK: {user.Email}");
                LoadUserInfoThenEnterGame();
            }
            else
            {
                Debug.Log("AutoSignIn失敗 → 再ログインが必要");
            }
        });
    }

    private void LoadUserInfoThenEnterGame()
    {
        if (user == null || firestore == null)
        {
            OnUserRegisterPanel?.Invoke();
            return;
        }

        var docRef = firestore.Collection("userInfo").Document(user.UserId);
        docRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && !task.IsFaulted && !task.IsCanceled && task.Result.Exists)
            {
                var snap = task.Result;
                if (userData == null) userData = new UserData();

                if (snap.ContainsField("username"))
                    userData.username = snap.GetValue<string>("username");

                if (snap.ContainsField("bestScore"))
                {
                    try { userData.bestScore = snap.GetValue<int>("bestScore"); }
                    catch { userData.bestScore = (int)snap.GetValue<long>("bestScore"); }
                }

                if (snap.ContainsField("timestamp"))
                {
                    try { userData.timestamp = snap.GetValue<int>("timestamp"); }
                    catch { userData.timestamp = (int)snap.GetValue<long>("timestamp"); }
                }
            }

            OnUserRegisterPanel?.Invoke();
        });
    }

    public async Task SignInWithGoogleAsync(string webClientId)
    {
#if UNITY_ANDROID || UNITY_IOS
    var config = new GoogleSignInConfiguration {
        WebClientId = webClientId,
        RequestIdToken = true,
        RequestEmail = true,
    };
    GoogleSignIn.Configuration = config;

    try
    {
        var gsUser = await GoogleSignIn.DefaultInstance.SignIn();
        var idToken = gsUser.IdToken;

        var cred = GoogleAuthProvider.GetCredential(idToken, null);
        var result = await FirebaseAuth.DefaultInstance.SignInWithCredentialAsync(cred);

        user = result;
        Debug.Log($"Google Sign-In OK: {user.Email}");

        // シーン遷移はメインスレッドで
        FirebaseHandler.RunOnMainThread(() => LoadUserInfoThenEnterGame());
    }
    catch (FirebaseException fe)
    {
        Debug.LogWarning($"Google Sign-In failed: {(AuthError)fe.ErrorCode} / {fe.Message}");
    }
    catch (System.Exception e)
    {
        Debug.LogWarning($"Google Sign-In failed: {e}");
    }
#else
        Debug.LogWarning("Googleサインインはこのプラットフォーム未対応の実装です");
        await Task.CompletedTask;  // ★ これでCS1998を回避
#endif
    }

    public async Task SignInWithTwitterAsync(string oauthToken, string oauthTokenSecret)
    {
#if UNITY_ANDROID || UNITY_IOS
    try {
        var cred = TwitterAuthProvider.GetCredential(oauthToken, oauthTokenSecret);
        var result = await FirebaseAuth.DefaultInstance.SignInWithCredentialAsync(cred);
        user = result;
        FirebaseHandler.RunOnMainThread(() => LoadUserInfoThenEnterGame());
    } catch (Exception e) {
        Debug.LogWarning($"Twitter Sign-In failed: {e}");
    }
#else
        Debug.LogWarning("Twitterサインインは実機でテストしてね");
        await Task.CompletedTask;
#endif
    }

    public void EnterGameOrAskUsername(Action<bool> onNeedUserName)
    {
        // 認証/DBがまだならユーザー名入力へ
        if (user == null || firestore == null) { onNeedUserName?.Invoke(true); return; }

        var docRef = firestore.Collection("userInfo").Document(user.UserId);
        docRef.GetSnapshotAsync().ContinueWithOnMainThread(t =>
        {
            bool need = true;

            if (t.IsCompleted && !t.IsFaulted && !t.IsCanceled && t.Result.Exists)
            {
                var snap = t.Result;

                // username の有無をチェック
                string name = null;
                if (snap.ContainsField("username"))
                {
                    try { name = snap.GetValue<string>("username"); }
                    catch { name = null; }
                }
                need = string.IsNullOrEmpty(name);

                // ついでにローカルキャッシュ更新（任意）
                if (!need)
                {
                    if (userData == null) userData = new UserData();
                    userData.username = name;

                    if (snap.ContainsField("bestScore"))
                    {
                        try { userData.bestScore = snap.GetValue<int>("bestScore"); }
                        catch { userData.bestScore = (int)snap.GetValue<long>("bestScore"); }
                    }
                    if (snap.ContainsField("timestamp"))
                    {
                        try { userData.timestamp = snap.GetValue<int>("timestamp"); }
                        catch { userData.timestamp = (int)snap.GetValue<long>("timestamp"); }
                    }
                }
            }

            if (need)
                onNeedUserName?.Invoke(true);     // ユーザー名未登録 → 入力パネルを出す
            else
                LoadUserInfoThenEnterGame();       // 既に登録あり → そのままゲームへ
        });
    }
}
