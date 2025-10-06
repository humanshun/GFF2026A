using UnityEngine;


[CreateAssetMenu(fileName = "AudioClipData", menuName = "Audio/ClipData")]
public class AudioClipData : ScriptableObject
{
    [SerializeField] private string key;

    [SerializeField] private AudioType audioType;

    [SerializeField] private bool loop;

    // 音源
    [SerializeField] private AudioClip clip;

    // デフォルト音量
    [Range(0f, 1f)][SerializeField] private float defoultVolume = 1f;

    public string Key => key;
    public AudioType AudioType => audioType;
    public bool Loop => loop;
    public AudioClip Clip => clip;
    public float DefoultVolume => defoultVolume;
}
