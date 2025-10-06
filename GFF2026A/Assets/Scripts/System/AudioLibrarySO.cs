using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "AudioLibrary", menuName = "Audio/AudioLibrary")]
public class AudioLibrarySO : ScriptableObject
{
    [SerializeField] private List<AudioClipData> clipDataList = new List<AudioClipData>();

    private Dictionary<string, AudioClipData> clipDictionary;

    private void OnEnable()
    {
        InitializeDictionary();
    }

    private void InitializeDictionary()
    {
        clipDictionary = new Dictionary<string, AudioClipData>();

        foreach (var data in clipDataList)
        {
            if (data == null || string.IsNullOrEmpty(data.Key)) continue;

            if (!clipDictionary.ContainsKey(data.Key))
            {
                clipDictionary.Add(data.Key, data);
            }
            else
            {
                Debug.LogWarning($"キー {data.Key} が重複しています。");
            }
        }
    }

    public AudioClipData GetClipData(string key)
    {
        if (clipDictionary == null || clipDictionary.Count == 0)
        {
            InitializeDictionary();
        }

        if (clipDictionary.TryGetValue(key, out var data))
        {
            return data;
        }

        Debug.LogWarning($"キー {key} の AudioClipData が見つかりません。");
        return null;
    }

    public IEnumerable<AudioClipData> GetAllClipData() => clipDataList;
}
