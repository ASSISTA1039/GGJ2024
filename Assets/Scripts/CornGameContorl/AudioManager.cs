using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

/// <summary>
/// 声音的控制以及管理
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    private AudioSource bgmSource;
    private AudioSource effectSource;

    private bool isBGMPlaying = false;

    public void Awake()
    {
        Instance = this;
        // DontDestroyOnLoad(gameObject);  // 可根据需要取消注释，避免场景切换时销毁
    }
    public void Start()
    {
        
    }
    // 初始化音频源
    public void Init()
    {
        if (bgmSource == null)
        {
            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.loop = true;  // 默认 BGM 循环播放
        }

        if (effectSource == null)
        {
            effectSource = gameObject.AddComponent<AudioSource>();
        }
    }

    // 播放背景音乐
    public void PlayBGM(string name, bool isLoop = true)
    {
        AudioClip clip = Resources.Load<AudioClip>("Sounds/BGM/" + name);
        if (clip != null)
        {
            bgmSource.clip = clip;
            bgmSource.loop = isLoop;
            bgmSource.volume = 1.0f;
            bgmSource.Play();
            isBGMPlaying = true;
        }
        else
        {
            Debug.LogWarning("BGM 文件未找到: " + name);
        }
    }

    // 停止背景音乐
    public void StopBGM()
    {
        bgmSource.Stop();
        isBGMPlaying = false;
    }

    // 播放音效
    public void PlayEffect(string name)
    {
        AudioClip clip = Resources.Load<AudioClip>("Sounds/Effect/" + name);
        if (clip != null)
        {
            AudioSource.PlayClipAtPoint(clip, transform.position);
        }
        else
        {
            Debug.LogWarning("音效文件未找到: " + name);
        }
    }

    // 增加背景音乐音量
    public void IncreaseBGMVolume()
    {
        bgmSource.volume = Mathf.Min(bgmSource.volume + 0.1f, 1f);  // 最大音量为1
    }

    // 减少背景音乐音量
    public void DecreaseBGMVolume()
    {
        bgmSource.volume = Mathf.Max(bgmSource.volume - 0.1f, 0f);  // 最小音量为0
    }

    // 暂停背景音乐
    public void PauseBGM()
    {
        if (isBGMPlaying)
        {
            bgmSource.Pause();
        }
    }

    // 恢复背景音乐
    public void ResumeBGM()
    {
        if (!isBGMPlaying)
        {
            bgmSource.Play();
        }
    }

    // 暂停所有音效
    public void PauseEffects()
    {
        effectSource.Pause();
    }

    // 恢复所有音效
    public void ResumeEffects()
    {
        effectSource.UnPause();
    }
}