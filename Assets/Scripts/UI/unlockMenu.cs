using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class unlockMenu : MonoBehaviour
{
    [Header("关卡解锁菜单预制体")]
    public GameObject unlockMenuPrefab;
    
    [Header("关卡锁数据")]
    public LevelLockData levelLockData;

    [Header("退出按钮")]
    public Button ExitBtn;
    // Start is called before the first frame update
    void Start()
    {
       
    }
    
    private List<GameObject> unlockMenus = new List<GameObject>();
    /// <summary>
    /// 创建解锁菜单
    /// </summary>
    public void CreateUnlockMenu()
    {
        unlockMenus.Clear();
        foreach (var item in levelLockData.AllLevels)
        {
            GameObject unlockMenu = Instantiate(unlockMenuPrefab,transform);
            unlockMenus.Add(unlockMenu);
            if (item.IsLock)
                unlockMenu.GetComponent<Image>().sprite = item.LevelSprite;
            else
            {
                unlockMenu.GetComponent<Image>().sprite = levelLockData.LockSprite;
            }
        }
    }

    /// <summary>
    /// 退出解锁菜单
    /// </summary>
    public void ExitUnlockMenu()
    {
        AudioController.Instance.PlayAudioClip(AudioType.CloseNotebook);
        foreach (var item in unlockMenus)
        {
            Destroy(item);
        }
        unlockMenus.Clear();
    }


    /// <summary>
    /// 进入解锁菜单
    /// </summary>
    public void EnterUnlockMenu(int index)
    {
        AudioController.Instance.PlayAudioClip(AudioType.OpenNotebook);
        ExitBtn.onClick.RemoveAllListeners();
        ExitBtn.onClick.AddListener(ExitUnlockMenu);
        ExitBtn.onClick.AddListener(() =>
        {
            MainController.Instance.LoadMenu(index);
        });

        CreateUnlockMenu();
    }
}


