using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "AudioConfigData", menuName = "Param/AudioConfigData")]
public class AudioConfigData : ScriptableObject
{
    [Header("背景音量")][Range(0,1f)]
    public float BGvolume = 0.5f;
    
    [Header("效果音量")][Range(0,1f)]
    public float EffectVolume = 0.8f;

    [Header("音效集合")]
    public List<AudioData> audioDataList;
}

[Serializable]
public enum AudioType
{
    MainBackground,
    PlotBackground,
    UIHover,
    OpenNotebook,
    CloseNotebook,
    DragStartLabel,
    DragEndLabel,
    Poisoning,
    Succeed,
    Lose,
    Run
}

[Serializable]
public class AudioData
{
    
    /// <summary>
    /// 音效类型
    /// </summary>
    [Header("音效类型")]
    public AudioType audioType;
    /// <summary>
    /// 音效文件
    /// </summary>
    [Header("音频文件")]
    public AudioClip audioClip;
}