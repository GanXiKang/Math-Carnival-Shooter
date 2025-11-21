using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Audio Clips")]
    [SerializeField] private AudioClip[] bgmClips;
    [SerializeField] private AudioClip[] sfxClips;

    private Dictionary<string, AudioClip> bgmDict;
    private Dictionary<string, AudioClip> sfxDict;

    private void Awake()
    {
        // Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // 建立字典
        bgmDict = new Dictionary<string, AudioClip>();
        sfxDict = new Dictionary<string, AudioClip>();

        foreach (var clip in bgmClips)
            bgmDict[clip.name] = clip;

        foreach (var clip in sfxClips)
            sfxDict[clip.name] = clip;
    }

    public void PlayBGM(string name)
    {
        if (!bgmDict.ContainsKey(name))
        {
            Debug.LogWarning($"[AudioManager] 找不到 BGM: {name}");
            return;
        }

        if (bgmSource.clip == bgmDict[name])
            return; // 已經是同一首

        bgmSource.clip = bgmDict[name];
        bgmSource.loop = true;
        bgmSource.Play();
    }

    public void StopBGM()
    {
        bgmSource.Stop();
    }

    public void PlaySFX(string name)
    {
        if (!sfxDict.ContainsKey(name))
        {
            Debug.LogWarning($"[AudioManager] 找不到音效: {name}");
            return;
        }

        sfxSource.PlayOneShot(sfxDict[name]);
    }
}
