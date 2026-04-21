using System;
using System.Collections;
using System.Collections.Generic;
using com.guanayao.Data;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
public class GameController : MonoBehaviour
{
    private ImageSpriteAnimation _imageSpriteAnimation;
    [Header("关卡数据配置")]
    public LevelConfigData levelConfigData;
    [Header("当前关卡")] 
    public LevelDataItem currentLevelDataItem;
    [Header("边界")]
    public Image botanyImage;
    [Header("结算弹窗")]
    public GameObject paymentPage;
    [Header("中毒阈值")]
    [Range(1,8)]
    public int Poisonousthreshold;
    [Header("中毒次数")]
    [Range(0,8)]
    public int poisonousCount = 0;
    [Header("轮次次数")]
    [Range(0,8)]
    public int currentTaskIndex = 0;
    [Header("完成按钮")]
    public Button FinishBtn;
    [Header("轻微中毒闪送")]
    public Image EnterPoisonous;
    [Header("无毒卡槽")]
    public GameObject Left_Page;
    [Header("有毒卡槽")]
    public GameObject Right_Page;
    [Header("闪烁时间")]
    [Range(0,8)]
    public float poisonousTime = 0.5f;
    
    // Start is called before the first frame update
    void Start()
    {
        currentLevelDataItem = levelConfigData.LevelDataItems[currentTaskIndex];
        _imageSpriteAnimation = GetComponent<ImageSpriteAnimation>();
        FinishBtn.onClick.AddListener(() =>
        {
            Debug.Log($"当前关卡：{GetFullyUnlock()}");
            Debug.Log($"levelConfigData 当前关卡：{levelConfigData.LevelDataItems[currentTaskIndex].botanyTags.Count}");
            if (GetFullyUnlock() == levelConfigData.LevelDataItems[currentTaskIndex].botanyTags.Count)
            {
                Botany.Instance.FinishBtnEvent();
                NextTask();
            }
        });
        
    }
    
    /// <summary>
    /// 下一个关卡
    /// </summary>
    public void NextTask()
    {
        PoisonousCalculate();
        LocalSave();
        currentTaskIndex++;
        if (currentTaskIndex >= levelConfigData.LevelDataItems.Count)
        {
            // paymentPage.SetActive(true);
            MainController.Instance.LoadMenu(4);
            AudioController.Instance.PlayAudioClip(AudioType.Succeed);
            return;
        }
        // 获取当前关卡数据
        currentLevelDataItem = levelConfigData.LevelDataItems[currentTaskIndex];
        botanyImage.sprite = currentLevelDataItem.BotanyIcon;
        botanyImage.SetNativeSize();
        Vector2 newSize = botanyImage.GetComponent<RectTransform>().sizeDelta;
        botanyImage.GetComponent<RectTransform>().sizeDelta = new Vector2(newSize.x/2, newSize.y/2);
        _imageSpriteAnimation.Restart();
    }
    
    /// <summary>
    /// 获取当前物体的 所有一级子物体（只第一层，不递归）
    /// </summary>
    /// <param name="parent">父物体</param>
    /// <returns>一级子物体 GameObject 列表</returns>
    public List<GameObject> GetFirstLevelChildren(Transform parent)
    {
        // 创建空列表
        List<GameObject> childList = new List<GameObject>();

        if (parent == null)
            return childList;

        // 遍历 所有一级子物体
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform childTrans = parent.GetChild(i);
            childList.Add(childTrans.gameObject);
        }

        return childList;
    }
    /// <summary>
    /// 完成所有任务结算功能
    /// </summary>
    void FinishAllTask()
    {
        if (Poisonousthreshold > poisonousCount)
        {
            MainController.Instance.LoadMenu(4);
        }
    }
    
    /// <summary>
    /// 一轮中毒结算
    /// </summary>
    void PoisonousCalculate()
    {
        foreach (var item in GetFirstLevelChildren(Left_Page.transform.GetChild(0)))
        {
            LabelInformation temp = item.GetComponentInChildren<LabelInformation>();
            if (temp != null)
            {
                if (item.GetComponentInChildren<LabelInformation>().IsPoisonous)
                    poisonousCount++;
                Destroy(temp.gameObject);
            }
        }
        foreach (var item in GetFirstLevelChildren(Right_Page.transform.GetChild(0)))
        {
            LabelInformation temp = item.GetComponentInChildren<LabelInformation>();
            if (temp != null)
            {
                if (item.GetComponentInChildren<LabelInformation>().IsPoisonous)
                    poisonousCount++;
                Destroy(temp.gameObject);
            }
        }

        // 检查是否轻微中毒
        if (poisonousCount == 1)
        {
            EnterPoisonous.DOFade(1f, 1f);
            EnterPoisonous.DOColor(Color.red, poisonousTime)
                .SetEase(Ease.InOutSine)      // 建议使用平滑的缓动曲线，OutBack 回弹感较强
                .SetLoops(-1, LoopType.Yoyo);  // -1 表示无限循环，Yoyo 表示来回往返
            AudioController.Instance.PlayAudioClip(AudioType.Poisoning);
        }
        
        // 检查是否中毒
        if (poisonousCount >= Poisonousthreshold)
        {
            // 中毒
            Debug.Log("中毒了");
            AudioController.Instance.PlayAudioClip(AudioType.Lose);
            MainController.Instance.LoadMenu(5);
        }

        if (Poisonousthreshold > poisonousCount)
        {
            // 未中毒
            Debug.Log("未中毒");
        }

        Debug.Log("poisonousCount:" + poisonousCount);
    }
    
    [Header("关卡解锁数据")]
    public LevelLockData levelLockData;
    
    /// <summary>
    /// 解锁当前关卡的解锁菜单
    /// </summary>
    void LocalSave()
    {
        bool isAllNonPoisonous = false;
        foreach (var item in GetFirstLevelChildren(Left_Page.transform.GetChild(0)))
        {
            LabelInformation temp = item.GetComponentInChildren<LabelInformation>();
            if (temp != null)
            {
                if (item.GetComponentInChildren<LabelInformation>().botanyType == BotanyPoisonousType.NonPoisonous)
                    isAllNonPoisonous = true;
                else
                {
                    levelLockData.AllLevels[currentTaskIndex].IsLock = false;
                    return;
                }
                   
            }
        }
        foreach (var item in GetFirstLevelChildren(Right_Page.transform.GetChild(0)))
        {
            LabelInformation temp = item.GetComponentInChildren<LabelInformation>();
            if (temp != null)
            {
                if (item.GetComponentInChildren<LabelInformation>().botanyType == BotanyPoisonousType.Poisonous)
                    isAllNonPoisonous = true;
                else
                {
                    levelLockData.AllLevels[currentTaskIndex].IsLock = false;
                    return;
                }
            }
        }
        if (isAllNonPoisonous)
        {
            levelLockData.AllLevels[currentTaskIndex].IsLock = true;
        }
    }


    /// <summary>
    /// 获取卡槽已解锁数量
    /// </summary>
    /// <returns></returns>
    int GetFullyUnlock()
    {
        int count = 0;
        foreach (var item in GetFirstLevelChildren(Left_Page.transform.GetChild(0)))
        {
            LabelInformation temp = item.GetComponentInChildren<LabelInformation>();
            if (temp != null)
            {
                count++;
            }
        }
        foreach (var item in GetFirstLevelChildren(Right_Page.transform.GetChild(0)))
        {
            LabelInformation temp = item.GetComponentInChildren<LabelInformation>();
            if (temp != null)
            {
                count++;
            }
        }
        return count;
    }
}
