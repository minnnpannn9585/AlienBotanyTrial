using System.Collections;
using System.Collections.Generic;
using com.guanayao.Data;
using DG.Tweening;
using GuanYao.Tool.Singleton;
using UnityEngine;
using UnityEngine.UI;

public class Botany : SingletonMono<Botany>
{
    [Header("游戏核心控制器")] 
    public GameController _gameController;

    /// <summary>
    /// 处理动画脚本
    /// </summary>
    public ImageSpriteAnimation _imageSpriteAnimation;

    /// <summary>
    /// 无毒卡槽位子
    /// </summary>
    public GameObject Left_Page;

    /// <summary>
    /// 有毒卡槽位子
    /// </summary>
    public GameObject Right_Page;

    /// <summary>
    /// 游戏UI父物体
    /// </summary>
    public GameObject GameUICanvas;

    public GameObject objectPrefab; // 要生成的物体预制体（例如一个圆形 Sprite）
    public Image boundaryImage; // 用于限定范围的 Image（作为背景或区域指示）
    public Transform spawnParent; // 生成物体的父物体（可选）
    
    public Button _finishBtn;
    private Button _button;
    private List<GameObject> labelList;

    // Start is called before the first frame update
    void Start()
    {
        labelList = new List<GameObject>();
        labelList = GetFirstLevelChildren(transform);
        _button = GetComponent<Button>();
        _imageSpriteAnimation.OnAnimationCompleted.AddListener(() =>
        {
            _button.onClick.AddListener(() => { ClickFinishBtn(); });
        });
    }


    /// <summary>
    /// 点击完成事件
    /// </summary>
    public void FinishBtnEvent()
    {
        Left_Page.GetComponent<CanvasGroup>().DOFade(0f, 1f);
        Left_Page.GetComponent<RectTransform>().DOAnchorPos(new Vector2(-280, 40f), 1f);
        Right_Page.GetComponent<CanvasGroup>().DOFade(0f, 1f);
        Right_Page.GetComponent<RectTransform>().DOAnchorPos(new Vector2(260, 16f), 1f);
        Color targetColor = new Color(1f, 1f, 1f, 1f);
        GameUICanvas.GetComponent<Image>().DOColor(targetColor, 1f);
        foreach (var item in GetFirstLevelChildren(transform))
        {
            Destroy(item);
        }
    }


    void ClickFinishBtn()
    {
        MainController.Instance.SetCursorState(true, false);
        // 目标颜色：红色 (R=1, G=0, B=0, A=0.5)
        Color targetColor = new Color(0.3f, 0.3f, 0.3f, 1f);
        GameUICanvas.GetComponent<Image>().DOColor(targetColor, 1f);
        _button.onClick.RemoveAllListeners();
        transform.DOScale(1f, 1f).SetEase(Ease.InCubic).OnComplete(() =>
        {
            Left_Page.GetComponent<CanvasGroup>().DOFade(1f, 1f);
            Left_Page.GetComponent<RectTransform>().DOAnchorPos(new Vector2(264f, 40f), 1f);
            Right_Page.GetComponent<CanvasGroup>().DOFade(1f, 1f);
            Right_Page.GetComponent<RectTransform>().DOAnchorPos(new Vector2(-260, 16f), 1f);
            SetLabelActive(true);
            RandomGeneration();
        });
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

    void SetLabelActive(bool active)
    {
        foreach (GameObject label in labelList)
        {
            label.SetActive(active);
        }
    }

  
    /// <summary>
    /// 解锁关卡数据
    /// </summary>
    public LevelDataItem currentLevelDataItem;

    /// <summary>
    /// 随机函数
    /// </summary>
    void RandomGeneration()
    {
        currentLevelDataItem = _gameController.currentLevelDataItem;
        List<GameObject> spawned = GenerateNonOverlapUI(
            objectPrefab,
            spawnParent,
            GetWorldRectFromImage(boundaryImage),
            currentLevelDataItem.botanyTags.Count);

        Debug.Log($"成功生成 {spawned.Count} 个物体");
    }


    #region  随机函数计算封装

       /// <summary>
    /// 在指定矩形区域内生成不重叠的 UI 物体（自动根据物体实际尺寸避免重叠）
    /// </summary>
    /// <param name="prefab">预制体（必须带有 RectTransform 或 Renderer）</param>
    /// <param name="parent">父物体（Canvas 或其它）</param>
    /// <param name="worldRect">世界坐标系下的矩形区域 (xMin, yMin, xMax, yMax)</param>
    /// <param name="count">要生成的数量</param>
    /// <param name="maxAttempts">最大尝试次数（默认 500）</param>
    /// <returns>生成的物体列表（数量不足时返回实际生成的列表，并输出错误）</returns>
    public List<GameObject> GenerateNonOverlapUI(GameObject prefab, Transform parent,
        Rect worldRect, int count, int maxAttempts = 500)
    {
        List<GameObject> results = new List<GameObject>();

        // 1. 获取物体的实际尺寸（宽/高）
        Vector2 objectSize = GetObjectSize(prefab);
        float width = objectSize.x;
        float height = objectSize.y;

        // 2. 计算中心点的活动范围（保证物体完全在矩形内）
        float xMin = worldRect.xMin + width / 2f;
        float xMax = worldRect.xMax - width / 2f;
        float yMin = worldRect.yMin + height / 2f;
        float yMax = worldRect.yMax - height / 2f;

        if (xMin >= xMax || yMin >= yMax)
        {
            Debug.LogError($"区域太小，无法放下尺寸为 {width}×{height} 的物体！");
            return results;
        }

        // 3. 预先生成所有不重叠的位置（不实例化）
        List<Vector2> positions = TryGeneratePositions(xMin, xMax, yMin, yMax,
            width, height, count, maxAttempts);

        if (positions == null || positions.Count < count)
        {
            Debug.LogError($"尝试 {maxAttempts} 次后，只能生成 {positions?.Count ?? 0} 个不重叠物体，目标数量 {count}");
            return results;
        }

        // 4. 位置已确定，批量实例化
        float originalZ = prefab.transform.position.z;
        for (int i = 0; i < positions.Count; i++)
        {
            Vector3 worldPos = new Vector3(positions[i].x, positions[i].y, originalZ);
            GameObject obj = Instantiate(prefab, worldPos, Quaternion.identity, parent);
            BotanyTagData dataItem = currentLevelDataItem.botanyTags[i];
            LabelInformation labelInfo = obj.GetComponent<LabelInformation>();
            labelInfo.SetIcon((BotanyPoisonousType)dataItem.botanyPoison,
                (SensoryType)dataItem.sensoryType, dataItem.chineseDescribe);
            results.Add(obj);
        }
        return results;
    }

    /// <summary>
    /// 获取物体在世界空间中的尺寸（支持 RectTransform 或普通 Renderer）
    /// </summary>
    private Vector2 GetObjectSize(GameObject obj)
    {
        RectTransform rect = obj.GetComponent<RectTransform>();
        if (rect != null)
        {
            // UI 元素：宽高取自 sizeDelta（需确保 Canvas 缩放模式正确）
            return rect.sizeDelta;
        }

        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
        {
            // 3D 物体：取包围盒的宽高
            Bounds bounds = renderer.bounds;
            return new Vector2(bounds.size.x, bounds.size.y);
        }

        // 保底默认值（可自行调整）
        Debug.LogWarning($"预制体 {obj.name} 没有 RectTransform 或 Renderer，默认尺寸 20×20");
        return new Vector2(20f, 20f);
    }

    /// <summary>
    /// 尝试生成一组不重叠的中心点坐标（预生成，不实例化）
    /// </summary>
    private List<Vector2> TryGeneratePositions(float xMin, float xMax, float yMin, float yMax,
        float width, float height, int count, int maxAttempts)
    {
        List<Vector2> positions = new List<Vector2>();
        float minDistanceX = width; // 中心点最小水平间距
        float minDistanceY = height; // 中心点最小垂直间距

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            positions.Clear();
            bool allPlaced = true;

            for (int i = 0; i < count; i++)
            {
                bool placed = false;
                for (int sub = 0; sub < 100; sub++) // 每个点单独尝试100次
                {
                    float x = Random.Range(xMin, xMax);
                    float y = Random.Range(yMin, yMax);
                    Vector2 candidate = new Vector2(x, y);

                    bool overlap = false;
                    foreach (Vector2 p in positions)
                    {
                        if (Mathf.Abs(candidate.x - p.x) < minDistanceX &&
                            Mathf.Abs(candidate.y - p.y) < minDistanceY)
                        {
                            overlap = true;
                            break;
                        }
                    }

                    if (!overlap)
                    {
                        positions.Add(candidate);
                        placed = true;
                        break;
                    }
                }

                if (!placed)
                {
                    allPlaced = false;
                    break;
                }
            }

            if (allPlaced && positions.Count == count)
                return positions;
        }

        return null;
    }
    

    /// <summary>
    /// 获取 Image 组件在世界坐标系下的轴对齐矩形范围（假设 Image 无旋转）
    /// </summary>
    private Rect GetWorldRectFromImage(Image image)
    {
        if (image == null)
        {
            Debug.LogError("Image 组件为空！");
            return new Rect(0, 0, 0, 0);
        }

        RectTransform rectTransform = image.rectTransform;
        Vector3[] corners = new Vector3[4];
        rectTransform.GetWorldCorners(corners); // 顺序：左下、左上、右上、右下

        // 计算最小最大值（形成轴对齐包围盒）
        float minX = Mathf.Min(corners[0].x, corners[1].x, corners[2].x, corners[3].x);
        float maxX = Mathf.Max(corners[0].x, corners[1].x, corners[2].x, corners[3].x);
        float minY = Mathf.Min(corners[0].y, corners[1].y, corners[2].y, corners[3].y);
        float maxY = Mathf.Max(corners[0].y, corners[1].y, corners[2].y, corners[3].y);

        return Rect.MinMaxRect(minX, minY, maxX, maxY);
    }
    #endregion
}
