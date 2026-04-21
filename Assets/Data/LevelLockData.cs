using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelLockData", menuName = "Param/LevelLockData")]
public class LevelLockData : ScriptableObject
{
    [Header("锁图标")]
    public Sprite LockSprite;
    [Header("所有关卡")]
    public List<LevelLock> AllLevels;
}

[Serializable]
public class LevelLock
{
    [Header("关卡图标")] public Sprite LevelSprite;
    [Header("是否解锁")] public bool IsLock;
}