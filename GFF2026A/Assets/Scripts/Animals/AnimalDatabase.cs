using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AnimalDatabase", menuName = "Scriptable Objects/Animal Database")]
public class AnimalDatabase : ScriptableObject
{
    //インスペクターで登録する動物データの一覧
    [SerializeField] private List<AnimalData> animals = new();
    //外部からは読み取り専用で参照できる
    public IReadOnlyList<AnimalData> Animals => animals;
    //高速検索用のインデックス（ID => AnimalData）
    private Dictionary<string, AnimalData> byId;

    // ---  ライフサイクル ---
    //スクリプタブルオブジェクトが有効化されたときに呼ばれる
    //ゲーム開始時やインスペクターで触られたときなど
    private void OnEnable() => BuildIndex();

    // animalsの内容から辞書を作りなおす
    public void BuildIndex()
    {
        byId = new Dictionary<string, AnimalData>(animals.Count);
        foreach (var a in animals)
        {
            //nullやID未設定のデータはスキップ
            if (a == null || string.IsNullOrEmpty(a.Id)) continue;

            if (!byId.TryAdd(a.Id, a))
                Debug.LogWarning($"[AnimalDatabase] Duplicate Id: {a.Id}", this);
        }
    }

    // --- 検索 ---
    // IDからAnimalDataを取得する（失敗時はfalseを返す）
    public bool TryGetById(string id, out AnimalData data)
    {
        //まだ辞書が作られていなければ構築する
        if (byId == null) BuildIndex();

        //IDが空なら失敗
        if (string.IsNullOrEmpty(id)) { data = null; return false; }

        return byId.TryGetValue(id, out data);
    }

    // IDから直接取得する簡易版（見つからなければnullを返す）
    public AnimalData GetById(string id)
    {
        TryGetById(id, out var d);
        return d;
    }

    public IEnumerable<AnimalData> Enumerate(Func<AnimalData, bool> filter = null)
    {
        foreach (var a in animals)
        {
            if (a == null) continue;
            if (filter == null || filter(a)) yield return a;
        }
    }

    // --- 重み付きランダム ---
    // spawnWaightに基づいてランダムにAnimalDataを選ぶ
    // filterで条件を付けられる（例：アイコンがあるやつだけ）
    //　rngを渡せばシード固定の乱数を使える（オンライン同期向け）
    public AnimalData GetRandomByWeight(Func<AnimalData, bool> filter = null, System.Random rng = null)
    {
        var candidates = new List<AnimalData>();
        int total = 0;

        //合計重みを計算しながら候補を収集
        foreach (var a in animals)
        {
            if (a == null) continue;
            if (filter != null && !filter(a)) continue;

            int w = Mathf.Max(0, a.SpawnWeight);
            if (w <= 0) continue;

            total += w;
            candidates.Add(a);
        }
        if (total <= 0) return null;

        //Unityの乱数 or System.Randomを使って重み抽選
        int roll = rng == null ? UnityEngine.Random.Range(0, total) : rng.Next(0, total);

        //抽選に当たった動物を渡す
        foreach (var a in candidates)
        {
            int w = Mathf.Max(0, a.SpawnWeight);
            if (roll < w) return a;
            roll -= w;
        }
        return null;
    }

#if UNITY_EDITOR
    //インスペクターで編集したときに呼ばれる
    private void OnValidate()
    {
        //データの簡易チェック
        var set = new HashSet<string>();
        foreach (var a in animals)
        {
            if (a == null) continue;

            //ID未設定なら警告
            if (string.IsNullOrWhiteSpace(a.Id))
                Debug.LogWarning($"[AnimalDatabase] Animal has empty Id: {a?.name}", this);

            //重複IDなら警告
            if (!set.Add(a.Id))
                Debug.LogWarning($"[AnimalDatabase] Duplicate Id in list: {a.Id}", this);

            //重みが負数なら警告
            if (a.SpawnWeight < 0)
                Debug.LogWarning($"[AnimalDatavase] Nagative weight on: {a.Id}", this);
        }
    }
#endif
}
