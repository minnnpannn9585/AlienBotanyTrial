using System.Collections;
using System.Collections.Generic;
using GuanYao.Tool.Singleton;
using UnityEngine;

public class AudioController : SingletonMono<AudioController>
{
    public AudioConfigData audioConfigData;
    
    private AudioSource mainCameraAudioSource;
    private AudioSource audioSource;
    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        mainCameraAudioSource = Camera.main.GetComponent<AudioSource>();
        PlayMainBackgroundAudio();
    }
    
    /// <summary>
    /// 播放主背景音乐
    /// </summary>
    public void PlayMainBackgroundAudio()
    {
        audioSource.clip = GetAudioSource(AudioType.MainBackground);
        audioSource.volume = audioConfigData.BGvolume;
        audioSource.Play();
    }
    
    // /// <summary>
    // /// 播放剧情背景音乐
    // /// </summary>
    // public void PlayPlotBackgroundAudio()
    // {
    //     PlayAudioClip(AudioType.PlotBackground);
    // }
    
    /// <summary>
    /// 播放走的音效
    /// </summary>
    public void PlayPlotBackgroundAudio()
    {
        mainCameraAudioSource.clip = GetAudioSource(AudioType.PlotBackground);
        mainCameraAudioSource.volume = audioConfigData.BGvolume;
        mainCameraAudioSource.Play();
    }
    
    /// <summary>
    /// 播放走的音效
    /// </summary>
    public void StopPlotBackgroundAudio()
    {
        mainCameraAudioSource.Stop();
    }
    
    
    /// <summary>
    /// 获取音频文件 AudioClip
    /// </summary>
    /// <param name="audioType"></param>
    /// <returns></returns>
    public AudioClip GetAudioSource(AudioType audioType)
    {
        for (int i = 0; i < audioConfigData.audioDataList.Count; i++)
        {
            if (audioConfigData.audioDataList[i].audioType == audioType)
                return audioConfigData.audioDataList[i].audioClip;
        }
        return null;
    }
    
    /// <summary>
    /// 播放音效
    /// </summary>
    /// <param name="audioType"></param>
    public void PlayAudioClip(AudioType audioType)
    {
        if (audioType == AudioType.MainBackground || audioType == AudioType.PlotBackground)
            AudioSource.PlayClipAtPoint(GetAudioSource(audioType), Camera.main.transform.position,
                audioConfigData.BGvolume);
        else
           AudioSource.PlayClipAtPoint(GetAudioSource(audioType), Camera.main.transform.position,
                        audioConfigData.EffectVolume);
    }

 
    
    /// <summary>
    /// 播放走的音效
    /// </summary>
    public void PlayGameRunAudio()
    {
        mainCameraAudioSource.clip = GetAudioSource(AudioType.Run);
        mainCameraAudioSource.volume = audioConfigData.BGvolume;
        mainCameraAudioSource.Play();
        
        Debug.Log("播放走的音效");
    }

    /// <summary>
    /// 停止走的音效
    /// </summary>
    public void StopGameRunAudio()
    {
        mainCameraAudioSource.Stop();
        Debug.Log("停止走的音效");
    }

}
