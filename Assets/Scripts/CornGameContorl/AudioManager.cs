using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;
/// <summary>
/// 声音的控制以及管理
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    private AudioSource bgmSource;
    //private AudioSource effectSource;
    public void Awake()
    {
        Instance = this;
        //DontDestroyOnLoad(gameObject);
    }
    //初始化
    public void Init()
    {
        if (bgmSource == null)
        {
            bgmSource = gameObject.AddComponent<AudioSource>();
        }
    }
    //BGM播放
    public void PlayMusicBGM(string name, bool isLoop = true)
    {
        AudioClip clip = Resources.Load<AudioClip>("Sounds/BGM/" + name);
        bgmSource.clip = clip;
        bgmSource.loop = isLoop;
        bgmSource.Play();
    }

    public void SoundUp()
    {
        bgmSource.volume += 0.1f;
    }

    public void SoundDown()
    {
        bgmSource.volume -= 0.1f;
    }
    public void StopMusic()
    {
        bgmSource.Stop();
    }

    //效果音播放
    public void PlayEffectMusic(string name)
    {
        AudioClip clip = Resources.Load<AudioClip>("Sounds/Effect" + name); //注意这里少一层路径
        AudioSource.PlayClipAtPoint(clip, transform.position);//在某位置播放声音
    }
}