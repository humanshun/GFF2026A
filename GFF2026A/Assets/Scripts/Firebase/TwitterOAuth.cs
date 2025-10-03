using System;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class TwitterOAuth : MonoBehaviour
{
    // X Developer Portal で発行した Consumer Keys
    [SerializeField] string consumerKey = "3JaVQlrZYVuZBX5b9e7XW3OaY";
    [SerializeField] string consumerSecret = "73nnDXqpkRV9FNnyVZEmjl0WmoJAnIjfFBLLiIWqmqkrF2YmMd";

    // ディープリンクのコールバック（ManifestとXのCallback URLsに登録）
    const string CALLBACK = "myapp://twitter-callback";

    string _tempOAuthToken = null;
    string _tempOAuthTokenSecret = null;

    void OnEnable()
    {
        Application.deepLinkActivated += OnDeepLink;
        // アプリがURLから起動された直後のケース
        if (!string.IsNullOrEmpty(Application.absoluteURL)) OnDeepLink(Application.absoluteURL);
    }

    void OnDisable() => Application.deepLinkActivated -= OnDeepLink;

    // 1) リクエストトークンを取得 → ブラウザで認可画面へ
    public async Task StartTwitterSignInAsync()
    {
        // request_token
        var req = await OAuthRequestAsync(
            method: "POST",
            url: "https://api.twitter.com/oauth/request_token",
            extraParams: new Dictionary<string, string> { { "oauth_callback", CALLBACK } },
            tokenSecret: null
        );
        if (req == null) { Debug.LogWarning("request_token failed"); return; }

        var dict = ParseQuery(req);
        if (!dict.TryGetValue("oauth_token", out _tempOAuthToken) ||
            !dict.TryGetValue("oauth_token_secret", out _tempOAuthTokenSecret))
        {
            Debug.LogWarning("request_token parse failed");
            return;
        }

        // ユーザー承認へ（外部ブラウザ）
        Application.OpenURL("https://api.twitter.com/oauth/authenticate?oauth_token=" + Uri.EscapeDataString(_tempOAuthToken));
    }

    // 2) ディープリンクで戻ってくる（oauth_verifier 付き）
    private async void OnDeepLink(string url)
    {
        // 例: myapp://twitter-callback?oauth_token=...&oauth_verifier=...
        if (string.IsNullOrEmpty(url) || !url.StartsWith(CALLBACK)) return;

        var q = ParseQuery(new Uri(url).Query.TrimStart('?'));
        if (!q.TryGetValue("oauth_verifier", out var verifier))
        {
            Debug.LogWarning("no oauth_verifier");
            return;
        }

        // access_token
        var acc = await OAuthRequestAsync(
            method: "POST",
            url: "https://api.twitter.com/oauth/access_token",
            extraParams: new Dictionary<string, string> { { "oauth_verifier", verifier } },
            tokenSecret: _tempOAuthTokenSecret,
            token: _tempOAuthToken
        );
        if (acc == null) { Debug.LogWarning("access_token failed"); return; }

        var accDict = ParseQuery(acc);
        if (!accDict.TryGetValue("oauth_token", out var finalToken) ||
            !accDict.TryGetValue("oauth_token_secret", out var finalSecret))
        {
            Debug.LogWarning("access_token parse failed");
            return;
        }

        Debug.Log("Twitter OAuth OK");
        // 3) Firebase へサインイン
        await Auth.instance.SignInWithTwitterAsync(finalToken, finalSecret);
    }

    // ---- OAuth 1.0a 署名付きリクエスト ----
    async Task<string> OAuthRequestAsync(string method, string url,
                                         Dictionary<string,string> extraParams,
                                         string tokenSecret, string token = null)
    {
        var nonce = Guid.NewGuid().ToString("N");
        var timestamp = ((long)(DateTime.UtcNow - new DateTime(1970,1,1)).TotalSeconds).ToString();

        var oauthParams = new SortedDictionary<string, string>
        {
            { "oauth_consumer_key", consumerKey },
            { "oauth_nonce", nonce },
            { "oauth_signature_method", "HMAC-SHA1" },
            { "oauth_timestamp", timestamp },
            { "oauth_version", "1.0" },
        };
        if (!string.IsNullOrEmpty(token)) oauthParams["oauth_token"] = token;
        if (extraParams != null) foreach (var kv in extraParams) oauthParams[kv.Key] = kv.Value;

        // 署名ベース文字列
        string paramString = EncodeParams(oauthParams);
        string baseString = $"{method.ToUpper()}&{Uri.EscapeDataString(url)}&{Uri.EscapeDataString(paramString)}";

        // 署名キー
        string key = $"{Uri.EscapeDataString(consumerSecret)}&{Uri.EscapeDataString(tokenSecret ?? "")}";
        string signature;
        using (var hasher = new HMACSHA1(Encoding.ASCII.GetBytes(key)))
            signature = Convert.ToBase64String(hasher.ComputeHash(Encoding.ASCII.GetBytes(baseString)));

        // Authorizationヘッダ（extraのoauth_callback等はヘッダではなくボディ/クエリにも入れる）
        var authHeader = new StringBuilder("OAuth ");
        void A(string k, string v) => authHeader.AppendFormat("{0}=\"{1}\", ", k, Uri.EscapeDataString(v));
        A("oauth_consumer_key", consumerKey);
        A("oauth_nonce", nonce);
        if (!string.IsNullOrEmpty(token)) A("oauth_token", token);
        A("oauth_signature_method", "HMAC-SHA1");
        A("oauth_timestamp", timestamp);
        A("oauth_version", "1.0");
        A("oauth_signature", signature);
        var header = authHeader.ToString().TrimEnd(' ', ',');

        // POST（bodyは空 or extraParamsをx-www-form-urlencodedで送る）
        var form = new List<IMultipartFormSection>();
        WWWForm www = new WWWForm();
        if (extraParams != null)
            foreach (var kv in extraParams) www.AddField(kv.Key, kv.Value);

        using (var req = UnityWebRequest.Post(url, www))
        {
            req.SetRequestHeader("Authorization", header);
            req.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");
            await req.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
            if (req.result != UnityWebRequest.Result.Success)
#else
            if (req.isNetworkError || req.isHttpError)
#endif
            {
                Debug.LogWarning($"OAuthRequest {url} failed: {req.error} {req.downloadHandler.text}");
                return null;
            }
            return req.downloadHandler.text;
        }
    }

    static string EncodeParams(SortedDictionary<string,string> p)
    {
        var sb = new StringBuilder();
        foreach (var kv in p)
        {
            if (sb.Length > 0) sb.Append('&');
            sb.Append(Uri.EscapeDataString(kv.Key)).Append('=').Append(Uri.EscapeDataString(kv.Value));
        }
        return sb.ToString();
    }

    static Dictionary<string,string> ParseQuery(string q)
    {
        var dict = new Dictionary<string, string>();
        var parts = q.Split('&');
        foreach (var part in parts)
        {
            var kv = part.Split('=');
            if (kv.Length == 2) dict[Uri.UnescapeDataString(kv[0])] = Uri.UnescapeDataString(kv[1]);
        }
        return dict;
    }
}
