using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 植物有毒无毒类型
/// </summary>
public enum BotanyPoisonousType
{
    Poisonous,
    NonPoisonous
}

/// <summary>
/// 感官类型
/// </summary>
public enum SensoryType
{
    Vision,
    Smell,
    Touch
}

[CreateAssetMenu(fileName = "DataConfig", menuName = "自定义配置/数据配置")]
public class DataConfig : ScriptableObject
{
    // 加一行 [SerializeField] 保证显示
    [SerializeField]
    public List<BotanyDataList> dataList;
}

/// <summary>
/// 每轮数据
/// </summary>
[Serializable]
public class BotanyDataList
{
    // 关键！必须加这个才能在面板显示！
    [SerializeField]
    public List<BotanyDataItem> dataItemList;
}

/// <summary>
/// 单独数据项
/// </summary>
[Serializable]
public class BotanyDataItem
{
    public BotanyPoisonousType botanyType;
    public SensoryType sensoryType;
    public string describe;
}