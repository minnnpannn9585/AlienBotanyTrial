using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

/// <summary>
/// 自定义带 GameObject 参数的 UnityEvent，以便在 Inspector 中显示
/// </summary>
[Serializable]
public class DropEvent : UnityEvent<GameObject> { }

/// <summary>
/// UGUI 拖拽组件：支持拖拽到指定 Tag 的 UI 上时触发事件，并传递目标对象
/// </summary>
public class DraggableUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("拖拽设置")]
    public bool enableDrag = true;
    public bool bringToFrontOnDrag = true;
    public bool clampToParent = false;

    [Header("视觉效果")]
    public bool changeAlphaOnDrag = false;
    [Range(0.2f, 1f)]
    public float dragAlpha = 0.6f;

    [Header("拖拽目标检测（按Tag）")]
    [Tooltip("目标 Tag：拖拽到此 Tag 的 UI 上时触发事件（如 \"DropTarget\"）")]
    public string targetTag = "DropTarget";
    [Tooltip("是否检测父物体：如果为 true，会向上查找父物体是否带有目标 Tag")]
    public bool checkParent = true;
    [Tooltip("拖拽到目标上时触发的事件，参数为目标 GameObject")]
    public DropEvent OnDropOnTarget;   // 改为带参数的事件

    private RectTransform rectTransform;
    private Canvas parentCanvas;
    private Vector2 offset;
    private CanvasGroup canvasGroup;
    private float originalAlpha = 1f;
    private Transform originalParent;
    private int originalSiblingIndex;

    private GameObject ParentGameObject;
    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        if (rectTransform == null)
        {
            Debug.LogError("DraggableUI 需要挂载在带有 RectTransform 的 UI 物体上");
            enabled = false;
            return;
        }

        parentCanvas = GetComponentInParent<Canvas>();
        if (parentCanvas == null)
        {
            Debug.LogError("DraggableUI 的父级中必须包含 Canvas");
            enabled = false;
            return;
        }

        if (changeAlphaOnDrag)
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            originalAlpha = canvasGroup.alpha;
        }
    }

    private void Start()
    {
        ParentGameObject = transform.parent.gameObject;
        // 示例：监听事件，打印目标名称（可在 Inspector 中绑定，也可代码添加）
        OnDropOnTarget.AddListener((target) =>
        {
            transform.SetParent(target.transform);
            Debug.Log($"拖拽到了目标上：{target.name}");
        });
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!enableDrag) return;
        
        transform.SetParent(ParentGameObject.transform);

        if (bringToFrontOnDrag)
        {
            originalParent = transform.parent;
            originalSiblingIndex = transform.GetSiblingIndex();
            transform.SetAsLastSibling();
        }

        RectTransform parentRect = rectTransform.parent as RectTransform;
        if (parentRect != null)
        {
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentRect, eventData.position, eventData.pressEventCamera, out localPoint);
            offset = rectTransform.anchoredPosition - localPoint;
        }

        if (changeAlphaOnDrag && canvasGroup != null)
            canvasGroup.alpha = dragAlpha;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!enableDrag) return;

        RectTransform parentRect = rectTransform.parent as RectTransform;
        if (parentRect == null) return;

        Vector2 localPoint;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentRect, eventData.position, eventData.pressEventCamera, out localPoint))
        {
            Vector2 newPosition = localPoint + offset;
            if (clampToParent)
                newPosition = ClampToParentRect(newPosition, parentRect);
            rectTransform.anchoredPosition = newPosition;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!enableDrag) return;

        // 恢复透明度
        if (changeAlphaOnDrag && canvasGroup != null)
            canvasGroup.alpha = originalAlpha;

        // 检测是否拖拽到了带有目标 Tag 的 UI 上，并获取目标对象
        if (!string.IsNullOrEmpty(targetTag))
        {
            GameObject target = GetTargetUnderPointer(eventData);
            if (target != null)
            {
                OnDropOnTarget?.Invoke(target);
            }
        }

        // 可选：恢复原始层级
        // if (bringToFrontOnDrag && originalParent != null && transform.parent == originalParent)
        //     transform.SetSiblingIndex(originalSiblingIndex);
    }

    /// <summary>
    /// 获取鼠标下方的第一个带有目标 Tag 的 GameObject（支持父物体检测）
    /// </summary>
    private GameObject GetTargetUnderPointer(PointerEventData eventData)
    {
        var results = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (var result in results)
        {
            GameObject go = result.gameObject;
            if (go == null) continue;

            // 直接检查当前物体
            if (go.CompareTag(targetTag))
                return go;

            // 检查父物体链
            if (checkParent)
            {
                Transform parent = go.transform.parent;
                while (parent != null)
                {
                    if (parent.CompareTag(targetTag))
                        return parent.gameObject;
                    parent = parent.parent;
                }
            }
        }
        return null;
    }

    private Vector2 ClampToParentRect(Vector2 position, RectTransform parentRect)
    {
        Vector2 min = parentRect.rect.min;
        Vector2 max = parentRect.rect.max;
        Vector2 size = rectTransform.rect.size * 0.5f;

        float clampedX = Mathf.Clamp(position.x, min.x + size.x, max.x - size.x);
        float clampedY = Mathf.Clamp(position.y, min.y + size.y, max.y - size.y);
        return new Vector2(clampedX, clampedY);
    }

    public void SetDragEnabled(bool enabled)
    {
        enableDrag = enabled;
    }
}