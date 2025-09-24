using System.Collections.Generic;
using UnityEngine;
using Firebase.Firestore;
using Firebase.Extensions;

public class Ranking : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Transform contentParent;      // ScrollView の Content
    [SerializeField] private LeaderboardRow rowPrefab;     // ランキング行のプレハブ
    [SerializeField] private int fetchCount = 10;          // 上位何件表示するか

    private FirebaseFirestore firestore;

    private void Start()
    {
        // Auth.instance を使っている前提
        firestore = FirebaseFirestore.DefaultInstance;
        FetchTopRanking();
    }

    public void FetchTopRanking()
    {
        // 既存の行をクリア
        foreach (Transform child in contentParent)
            Destroy(child.gameObject);

        firestore.Collection("userInfo")
                 .OrderByDescending("bestScore")
                 .Limit(fetchCount)
                 .GetSnapshotAsync()
                 .ContinueWithOnMainThread(task =>
        {
            if (!task.IsCompleted || task.IsFaulted)
            {
                Debug.LogError("ランキング取得に失敗");
                return;
            }

            var snap = task.Result;
            int rank = 1;
            foreach (var doc in snap.Documents)
            {
                string name = doc.ContainsField("username") ? doc.GetValue<string>("username") : "NoName";
                int best = doc.ContainsField("bestScore") ? doc.GetValue<int>("bestScore") : 0;

                var row = Instantiate(rowPrefab, contentParent);
                row.Bind(rank, name, best);
                rank++;
            }
        });
    }
}
