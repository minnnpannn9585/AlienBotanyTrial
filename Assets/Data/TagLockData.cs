using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "TagLockData", menuName = "Param/TagLockData")]
public class TagLockData : ScriptableObject
{
    [Header("标签库")]
    public List<TagData> AllTags;
}

[Serializable]
public class TagData
{
    [Header("感觉类型")]
    public SensoryType sensoryType;
    
    [Header("是否解锁")]
    public bool IsLock;
    
    [Header("标签中文描述")]
    public string chinesedescribe;
    
    [Header("标签英文描述")]
    public string englishdescribe;
}
