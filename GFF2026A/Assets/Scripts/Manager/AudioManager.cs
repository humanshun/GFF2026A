using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem.Android;

public class AudioManager : MonoBehaviour
{
    // AudioManagerのシングルトン
    public static AudioManager Instance { get; private set; }

    // AudioClipのリスト
    [SerializeField] private List<AudioClipData> bgmClips;
    [SerializeField] private List<AudioClipData> seClips;

    [SerializeField] private AudioLibrarySO audioLibrary;

    // Soundjsonの参照
    [SerializeField] Soundjson soundjson;

    // AudioSourceコンポーネント
    private AudioSource bgmSource;
    private AudioSource seSource;

    // 音量
    private float bgmVolume = 1f;
    private float seVolume = 1f;

    // 音量の取得
    public float GetBGMVolume() => bgmVolume;
    public float GetSEVolume() => seVolume;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // AudioSourceコンポーネントを追加
        bgmSource = gameObject.AddComponent<AudioSource>();
        seSource = gameObject.AddComponent<AudioSource>();
        bgmSource.loop = true;

        // 音量をロード
        LoadVolumes();
    }

    void Start()
    {
        // 仮再生場所
        PlayBGM("BGM");
    }

    // BGMの再生
    public void PlayBGM(string key)
    {
        var data = audioLibrary.GetClipData(key);
        if (data == null || data.AudioType != AudioType.BGM) return;

        bgmSource.clip = data.Clip;
        bgmSource.volume = bgmVolume;
        bgmSource.loop = data.Loop;
        bgmSource.Play();
    }

    // SEの再生
    public void PlaySE(string key)
    {
        var data = audioLibrary.GetClipData(key);
        if (data == null || data.AudioType != AudioType.SE) return;

        seSource.PlayOneShot(data.Clip, seVolume);
    }

    // BGMの停止・再開
    public void ToggleBGM()
    {
        if (bgmSource.isPlaying)
        {
            // 一時停止
            bgmSource.Pause();
        }
        else if (bgmSource.clip != null)
        {
            // 再開
            bgmSource.UnPause();
        }
    }

    // SEの停止・再開
    public void ToggleSE()
    {
        if (bgmSource.isPlaying)
        {
            // 一時停止
            bgmSource.Pause();
        }
        else if (bgmSource.clip != null)
        {
            // 再開
            bgmSource.UnPause();
        }
    }

    // BGMのセット
    public void SetBGMVolume(float volume)
    {
        bgmVolume = volume;
        bgmSource.volume = bgmVolume;
        soundjson.UpdateVolume("BGM", bgmVolume);
    }

    // SEのセット
    public void SetSEVolume(float volume)
    {
        seVolume = volume;
        soundjson.UpdateVolume("SE", seVolume);
    }

    // 音量のロード
    private void LoadVolumes()
    {
        var volumes = soundjson.LoadVolumeData();
        bgmVolume = volumes.TryGetValue("BGM", out float by) ? by : 1f;
        seVolume = volumes.TryGetValue("SE", out float sv) ? sv : 1f;
    }
}
