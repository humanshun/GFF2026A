using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Data;

public class Soundjson : MonoBehaviour
{
    // jsonのファイルパス
    private string filePath;

    // SoundDaataの構造体
    [System.Serializable]
    public class SoundData
    {
        public string soundName;
        public float soundValue;
    }


    // SoundListの構造体
    [System.Serializable]
    public class SoundList
    {
        public List<SoundData> datas;
    }

    private void Awake()
    {
        // ファイルパスの設定
        filePath = Path.Combine(Application.persistentDataPath, "soundData.json");
    }

    void Start()
    {
        LoadData();
    }

    // データをセーブ
    public void SaveData(SoundList soundList)
    {
        string json = JsonUtility.ToJson(soundList, true);
        File.WriteAllText(filePath, json);
        Debug.Log("セーブしました" + json);
    }

    public void LoadData()
    {
        // ファイルが存在するか確認
        if (File.Exists(filePath))
        {
            // ファイルからJSONデータを読み込む
            string jsonData = File.ReadAllText(filePath);

            // JSONをC#のオブジェクトに変換
            SoundList loadedList = JsonUtility.FromJson<SoundList>(jsonData);

            // 読み込んだデータを表示
            foreach (var data in loadedList.datas)
            {
                Debug.Log($"ロードしたデータ: 名前: {data.soundName}, 音量: {data.soundValue}");
            }
        }
        else
        {
            Debug.LogWarning("セーブデータが見つかりません");

            SoundList soundList = new();
            soundList.datas = new List<SoundData>
            {
                new SoundData { soundName = "BGM", soundValue = 1.0f },
                new SoundData { soundName = "SE", soundValue = 1.0f }
            };

            SaveData(soundList);
        }
    }

    public void UpdateVolume(string soundName, float newVolume)
    {
        SoundList datas;

        if (File.Exists(filePath))
        {
            string jsonData = File.ReadAllText(filePath);
            datas = JsonUtility.FromJson<SoundList>(jsonData);
        }
        else
        {
            datas = new SoundList { datas = new List<SoundData>() };
        }

        var soundData = datas.datas.Find(d => d.soundName == soundName);
        if (soundData != null)
        {
            soundData.soundValue = newVolume;
        }
        else
        {
            datas.datas.Add(new SoundData { soundName = soundName, soundValue = newVolume });
        }

        SaveData(datas);
    }

    public Dictionary<string, float> LoadVolumeData()
    {
        Dictionary<string, float> result = new();

        if (File.Exists(filePath))
        {
            string jsonData = File.ReadAllText(filePath);
            SoundList loadedList = JsonUtility.FromJson<SoundList>(jsonData);

            foreach (var data in loadedList.datas)
            {
                result[data.soundName] = data.soundValue;
            }
        }
        else
        {
            result["BGM"] = 1.0f;
            result["SE"] = 1.0f;

            SoundList soundList = new()
            {
                datas = new List<SoundData>
                {
                    new SoundData { soundName = "BGM", soundValue = 1.0f },
                    new SoundData { soundName = "SE", soundValue = 1.0f }
                }
            };
            SaveData(soundList);
        }
        return result;
    }
}
