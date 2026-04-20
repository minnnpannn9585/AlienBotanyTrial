using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.guanayao.Data
{
    /// <summary>
    /// 是否有毒
    /// </summary>
    public enum BotanyPoison
    {
        Poisonous,
        Nontoxic
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

    [CreateAssetMenu(fileName = "LevelConfigData", menuName = "Param/LevelConfigData")]
    public class LevelConfigData : ScriptableObject
    {
        /// <summary>
        /// 关卡数据
        /// </summary>
        public List<LevelDataItem> LevelDataItems;
    }

    [Serializable]
    public class LevelDataItem
    {
        /// <summary>
        /// 关卡索引 第几关
        /// </summary>
        public int LevelIndex;

        /// <summary>
        /// 关卡名称
        /// </summary>
        public string name;

        /// <summary>
        /// 植物Icon
        /// </summary>
        public Sprite BotanyIcon;

        /// <summary>
        /// 植物标签集合
        /// </summary>
        /// <returns></returns>
        public List<BotanyTagData> botanyTags;
    }

    /// <summary>
    /// 植物标签类
    /// </summary>
    [Serializable]
    public class BotanyTagData
    {
        /// <summary>
        /// 植物是否有毒
        /// </summary>
        public BotanyPoison botanyPoison;

        /// <summary>
        /// 感觉类型
        /// </summary>
        public SensoryType sensoryType;

        /// <summary>
        /// 植物的特征描述
        /// </summary>
        public string chineseDescribe;
        
        /// <summary>
        /// 植物的特征描述
        /// </summary>
        public string englishDescribe;
    }
}