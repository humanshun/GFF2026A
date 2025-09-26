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

    // Soundjsonの参照
    [SerializeField] Soundjson soundjson;

    // AudioSourceコンポーネント
    private AudioSource bgmSource;
    private AudioSource seSource;

    // 音量
    private float bgmVolume = 1f;
    private float seVolume = 1f;

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
        PlayBGM(0);
    }

    // BGMの再生
    public void PlayBGM(int index)
    {
        if (index < 0 || index >= bgmClips.Count) return;
        bgmSource.clip = bgmClips[index].clip;
        bgmSource.volume = bgmVolume;
        bgmSource.Play();
    }

    // SEの再生
    public void PlaySE(int index)
    {
        if (index < 0 || index >= seClips.Count) return;
        seSource.PlayOneShot(seClips[index].clip, seVolume);
    }

    // BGMの停止
    public void StopBGM() => bgmSource.Stop();

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
