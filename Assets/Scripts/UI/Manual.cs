using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class Manual : MonoBehaviour
{
    [Header("退出按钮")]
    public Button ExitBtn;
    [Header("滑动区域")]
    public ScrollRect ManualScrollRect;
    
    private bool isAtBottom = false;
    // Start is called before the first frame update
    void Start()
    {
        ManualScrollRect.onValueChanged.AddListener((val) =>
        {
            if (val.x >= 1f && !isAtBottom)
            {
                ExitBtn.GetComponent<RectTransform>().DOAnchorPos(new Vector2(-270f, 100f), 0.2f);
                ExitBtn.GetComponent<CanvasGroup>().DOFade(1f, 0.2f);
                isAtBottom = true;
            }

            if (val.x < 0.9f && isAtBottom)
            {
                ExitBtn.GetComponent<RectTransform>().DOAnchorPos(new Vector2(270f, 100f), 0.2f);
                ExitBtn.GetComponent<CanvasGroup>().DOFade(0f, 0.2f);
                isAtBottom = false;
            }
            // Debug.Log($"当前滚动位置: {val}");
        });
    }
}
