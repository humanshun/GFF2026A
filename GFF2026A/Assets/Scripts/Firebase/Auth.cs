using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using Firebase.Firestore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using UnityEngine.SceneManagement;

public class Auth : MonoBehaviour
{
    public static Auth instance;
    public const string DefaultUsername = "No Name";
    private FirebaseAuth auth;
    private FirebaseFirestore firestore;
    public FirebaseUser user { get; private set; }
    public UserData userData { get; private set; }
    public event Action OnUserRegisterPanel;
    public event Action OnClosePanel;
    public event Action OnUserDataUpdated;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
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
                TryAutoSignIn();
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
                LoadUserInfoThenEnterGame();
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
                EnterGameOrAskUsername(needName =>
                {
                    if (needName) OnUserRegisterPanel?.Invoke();
                    else OnClosePanel?.Invoke();
                });
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
        if (string.IsNullOrWhiteSpace(username))
        {
            username = DefaultUsername;
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
                        OnUserDataUpdated?.Invoke();
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
                    username = DefaultUsername,
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

    public void GuestLogin(Action<bool> callback)
    {
        if (auth == null)
        {
            Debug.LogError("Firebase Auth 未初期化");
            callback?.Invoke(false);
            return;
        }

        auth.SignInAnonymouslyAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogWarning($"ゲストログイン失敗");
                callback?.Invoke(false);
                return;
            }

            user = task.Result.User;
            Debug.Log($"ゲストログイン成功: {user.UserId}, isAnonymous={user.IsAnonymous}");

            //Firestoreにユーザードキュメントがなければ作る(usernameは空のまま)
            EnsureUserDocExistsForGuest(
                onSuccess: () =>
                {
                    EnterGameOrAskUsername(needName =>
                    {
                        if (needName) OnUserRegisterPanel?.Invoke();
                        else OnClosePanel?.Invoke();
                    });
                    callback?.Invoke(true);
                },

                onFail: () =>
                {
                    // Firestore作成に失敗しても「ログイン自体」は成功しているのでUIだけ出す
                    OnUserRegisterPanel?.Invoke();
                    callback?.Invoke(true);
                });
        });
    }
    
    private void EnsureUserDocExistsForGuest(Action onSuccess, Action onFail)
    {
        if (user == null || firestore == null)
        {
            Debug.LogWarning("EnsureUserDocExistsForGuest: 未初期化");
            onFail?.Invoke();
            return;
        }

        var docRef = firestore.Collection("userInfo").Document(user.UserId);
        docRef.GetSnapshotAsync().ContinueWithOnMainThread(getTask =>
        {
            if (getTask.IsFaulted || getTask.IsCanceled)
            {
                Debug.LogWarning("ユーザードキュメントの取得に失敗");
                onFail?.Invoke();
                return;
            }

            var snap = getTask.Result;
            if (snap.Exists)
            {
                // 既存あり → timestampだけ更新（任意）
                int now = (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
                var updates = new Dictionary<string, object> { { "timestamp", now } };
                docRef.SetAsync(updates, SetOptions.MergeAll).ContinueWithOnMainThread(_ => onSuccess?.Invoke());
                return;
            }

            // 既存なし → 空のusernameで新規作成
            int nowUnix = (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
            var newData = new UserData
            {
                username = DefaultUsername,     // ← まだ未登録
                bestScore = 0,
                timestamp = nowUnix
            };

            docRef.SetAsync(newData).ContinueWithOnMainThread(setTask =>
            {
                if (setTask.IsFaulted || setTask.IsCanceled)
                {
                    Debug.LogWarning("ユーザードキュメントの作成に失敗");
                    onFail?.Invoke();
                    return;
                }

                userData = newData; // ローカルにも反映（任意）
                onSuccess?.Invoke();
            });
        });
    }

    private void LoadUserInfoThenEnterGame()
    {
        if (user == null || firestore == null) { OnClosePanel?.Invoke(); return; }

        var docRef = firestore.Collection("userInfo").Document(user.UserId);
        docRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && !task.IsFaulted && !task.IsCanceled && task.Result.Exists)
            {
                var snap = task.Result;
                if (userData == null) userData = new UserData();

                string name = null;
                if (snap.ContainsField("username"))
                    name = snap.GetValue<string>("username");

                // ここが変更点：空なら NoName を補完書き込み
                if (string.IsNullOrWhiteSpace(name))
                {
                    name = DefaultUsername;
                    int now = (int)(DateTime.UtcNow - new DateTime(1970,1,1)).TotalSeconds;
                    var updates = new Dictionary<string, object> { { "username", name }, { "timestamp", now } };
                    docRef.SetAsync(updates, SetOptions.MergeAll);
                }
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

                OnUserDataUpdated?.Invoke();
                OnClosePanel?.Invoke();
            }
            else
            {
                // 初回でドキュメントも無い場合は、NoNameで作ってしまってから閉じる
                int nowUnix = (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
                var newData = new UserData { username = DefaultUsername, bestScore = 0, timestamp = nowUnix };
                docRef.SetAsync(newData).ContinueWithOnMainThread(_ =>
                {
                    userData = newData;
                    OnUserDataUpdated?.Invoke();
                    OnClosePanel?.Invoke();
                });
            }
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
        if (user == null || firestore == null) { onNeedUserName?.Invoke(true); return; }

        var docRef = firestore.Collection("userInfo").Document(user.UserId);
        docRef.GetSnapshotAsync().ContinueWithOnMainThread(t =>
        {
            bool need = true;
            if (t.IsCompleted && !t.IsFaulted && !t.IsCanceled && t.Result.Exists)
            {
                var snap = t.Result;
                string name = null;
                if (snap.ContainsField("username"))
                {
                    try { name = snap.GetValue<string>("username"); } catch { name = null; }
                }

                // ここが変更点：空や未設定なら即 NoName を書き込んで続行
                if (string.IsNullOrWhiteSpace(name))
                {
                    int now = (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
                    var updates = new Dictionary<string, object>
                    {
                        { "username", DefaultUsername },
                        { "timestamp", now }
                    };
                    docRef.SetAsync(updates, SetOptions.MergeAll).ContinueWithOnMainThread(_ =>
                    {
                        if (userData == null) userData = new UserData();
                        userData.username = DefaultUsername;

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

                        // 名前入力不要として、そのままゲームへ
                        LoadUserInfoThenEnterGame();
                        onNeedUserName?.Invoke(false);
                    });
                    return; // ここで終了（以降の分岐は走らせない）
                }

                // 既に名前あり
                need = false;
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

            if (need) onNeedUserName?.Invoke(true);
            else      LoadUserInfoThenEnterGame();
        });
    }

    /// <summary>
    /// ログアウト（セッション破棄→ローカルクリア→UI通知→任意でタイトルに戻る）
    /// </summary>
    public void Logout(bool goToTitleScene = true, string titleSceneName = "Title")
    {
        // 1) 連携SDKのサインアウト（使っている場合のみ）
#if UNITY_ANDROID || UNITY_IOS
        try
        {
            // Google 連携を使っている場合のみ有効
            GoogleSignIn.DefaultInstance?.SignOut();
        }
        catch (Exception e)
        {
            Debug.LogWarning($"GoogleSignIn.SignOut 失敗: {e.Message}");
        }
#endif
        // Twitter は Firebase 側の SignOut でOK（SDK側の保持はなし）

        // 2) Firebase サインアウト
        try
        {
            auth?.SignOut(); // これで CurrentUser は null になる
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Firebase SignOut 失敗: {e.Message}");
        }

        // 3) ローカルキャッシュをクリア
        user = null;
        userData = null;

        // 4) UI へ通知
        //   - TitleUIなどが「No Name」に更新できるように
        OnUserDataUpdated?.Invoke();
        //   - ログイン/ユーザー名入力パネルを出したい場合
        OnUserRegisterPanel?.Invoke();

        // 5) 必要ならタイトルへ戻す
        if (goToTitleScene)
        {
            try
            {
                SceneManager.LoadScene(titleSceneName, LoadSceneMode.Single);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"シーン遷移失敗: {e.Message}");
            }
        }
    }
}
