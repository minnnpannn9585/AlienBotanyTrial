using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

public enum ButtonEffectType
{
    Scale,
    Color,
    Fade,
    Rotate
}
public class ButtonEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public ButtonEffectType effectType;

    private void Start()
    {
        GetComponent<Button>().onClick.AddListener(() =>
        {
            switch (effectType)
            {
                case ButtonEffectType.Scale:
                    transform.DOScale(2f, 0.2f).SetEase(Ease.OutBack);
                    break;
                case ButtonEffectType.Color:
                    transform.GetComponent<Image>().DOColor(Color.gray, 0.2f);
                    break;
                case ButtonEffectType.Fade:
                    transform.GetComponent<CanvasGroup>().DOFade(1f, 0.2f);
                    break;
                case ButtonEffectType.Rotate:
                    transform.DORotate(Vector3.zero, 0.2f);
                    break;
            }
        });
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        AudioController.Instance.PlayAudioClip(AudioType.UIHover);
        switch (effectType)
        {
            case ButtonEffectType.Scale:
                transform.DOScale(2.1f, 0.2f).SetEase(Ease.OutBack);
                break;
            case ButtonEffectType.Color:
                transform.GetComponent<Image>().DOColor(Color.white, 0.2f);
                break;
            case ButtonEffectType.Fade:
                transform.GetComponent<CanvasGroup>().DOFade(0.9f, 0.2f);
                break;
            case ButtonEffectType.Rotate:
                transform.DORotate(new Vector3(0,0,5), 0.2f);
                break;
        }
      
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        switch (effectType)
        {
            case ButtonEffectType.Scale:
                transform.DOScale(2f, 0.2f).SetEase(Ease.OutBack);
                break;
            case ButtonEffectType.Color:
                transform.GetComponent<Image>().DOColor(Color.gray, 0.2f);
                break;
            case ButtonEffectType.Fade:
                transform.GetComponent<CanvasGroup>().DOFade(1f, 0.2f);
                break;
            case ButtonEffectType.Rotate:
                transform.DORotate(Vector3.zero, 0.2f);
                break;
        }
    }
}