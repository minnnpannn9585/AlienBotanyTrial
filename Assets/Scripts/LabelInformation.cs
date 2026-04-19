using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class LabelInformation : MonoBehaviour
{
    [Header("是否有毒")]
    public BotanyPoisonousType botanyType;
    [Header("感官类型")]
    public SensoryType sensoryType;
    [Header("内容描述")]
    public string describe;
    [Header("是否中毒")]
    public bool IsPoisonous;
    [Header("IconList")]
    public List<Sprite> IconSprites;
    
    public void SetIcon(BotanyPoisonousType _botanyType, SensoryType _sensoryType,string _describe)
    {
        botanyType = _botanyType;
        sensoryType = _sensoryType;
        describe = _describe;
        transform.GetComponentInChildren<TextMeshProUGUI>().text = _describe;
        transform.GetComponent<Image>().sprite = IconSprites[(int)_sensoryType];
    }
}
