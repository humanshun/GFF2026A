using UnityEngine;
using UnityEngine.Android;

public enum AudioType{ BGM, SE }

[CreateAssetMenu(fileName = "AudioClipData", menuName = "Audio/ClipData")]
public class AudioClipData : ScriptableObject
{
    // BGMかSEか
    public AudioType audioType;

    // 音源
    public AudioClip clip;

    // デフォルト音量
    [Range(0f, 1f)] public float defoultVolume = 1f;
}
