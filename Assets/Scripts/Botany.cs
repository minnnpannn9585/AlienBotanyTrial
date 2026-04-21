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
    
    public Button _finishBtn;
    private Button _button;
    private List<GameObject> labelList;
    
    // Start is called before the first frame update
    void Start()
    {
        labelList = new List<GameObject>();
        labelList = GetFirstLevelChildren(transform);
        _button = GetComponent<Button>();
      
        
        // _finishBtn.onClick.AddListener(() =>
        // {
        //     // Left_Page.SetActive(false);
        //     // Right_Page.SetActive(false);
        //     Left_Page.GetComponent<CanvasGroup>().DOFade(0f, 1f);
        //     Left_Page.GetComponent<RectTransform>().DOAnchorPos(new Vector2(-280,40f), 1f);
        //     Right_Page.GetComponent<CanvasGroup>().DOFade(0f, 1f);
        //      Right_Page.GetComponent<RectTransform>().DOAnchorPos(new Vector2(260,16f), 1f);
        //     Color targetColor = new Color(1f, 1f, 1f, 1f);
        //     GameUICanvas.GetComponent<Image>().DOColor(targetColor, 1f);
        //     foreach (var item in GetFirstLevelChildren(transform))
        //     {
        //         Destroy(item);
        //     }
        // });
        
        _imageSpriteAnimation.OnAnimationCompleted.AddListener(() =>
        {
            _button.onClick.AddListener(() =>
            {
                ClickFinishBtn();
            });
        });
    }


    public void FinishBtnEvent()
    {
        Left_Page.GetComponent<CanvasGroup>().DOFade(0f, 1f);
        Left_Page.GetComponent<RectTransform>().DOAnchorPos(new Vector2(-280,40f), 1f);
        Right_Page.GetComponent<CanvasGroup>().DOFade(0f, 1f);
        Right_Page.GetComponent<RectTransform>().DOAnchorPos(new Vector2(260,16f), 1f);
        Color targetColor = new Color(1f, 1f, 1f, 1f);
        GameUICanvas.GetComponent<Image>().DOColor(targetColor, 1f);
        foreach (var item in GetFirstLevelChildren(transform))
        {
            Destroy(item);
        }
    }
    
    
    void ClickFinishBtn()
    {
        // 目标颜色：红色 (R=1, G=0, B=0, A=0.5)
        Color targetColor = new Color(0.3f, 0.3f, 0.3f, 1f);
        GameUICanvas.GetComponent<Image>().DOColor(targetColor, 1f);
        _button.onClick.RemoveAllListeners();
        transform.DOScale(1f, 1f).SetEase(Ease.InCubic).OnComplete(() =>
        {
            Left_Page.GetComponent<CanvasGroup>().DOFade(1f, 1f);
            Left_Page.GetComponent<RectTransform>().DOAnchorPos(new Vector2(264f,40f), 1f);
            Right_Page.GetComponent<CanvasGroup>().DOFade(1f, 1f);
            Right_Page.GetComponent<RectTransform>().DOAnchorPos(new Vector2(-260,16f), 1f);
            SetLabelActive(true);
            RandomGeneration();
        });
    }
    
    /// <summary>
    /// 获取当前物体的 所有一级子物体（只第一层，不递归）
    /// </summary>
    /// <param name="parent">父物体</param>
    /// <returns>一级子物体 GameObject 列表</returns>
    public static List<GameObject> GetFirstLevelChildren(Transform parent)
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
    
    public GameObject objectPrefab;   // 要生成的物体预制体（例如一个圆形 Sprite）
    public Image boundaryImage;       // 用于限定范围的 Image（作为背景或区域指示）
    public Transform spawnParent;     // 生成物体的父物体（可选）
    public float objectRadius = 0.5f; // 物体半径（根据实际物体大小调整）
    
    public LevelDataItem currentLevelDataItem;
    void RandomGeneration()
    {
        currentLevelDataItem = _gameController.currentLevelDataItem;
        
        // 在 Image 范围内生成 10 个不重叠的物体
        List<GameObject> spawned = GenerateNonOverlapObjectsInImage(
            prefab: objectPrefab,
            parent: spawnParent,
            image: boundaryImage,
            count: currentLevelDataItem.botanyTags.Count,
            radius: objectRadius,
            maxAttempts: 5000
        );

        Debug.Log($"成功生成 {spawned.Count} 个物体");
    }
    
    /// <summary>
    /// 在 Image 组件所占据的世界坐标矩形范围内生成指定数量的不重叠物体
    /// </summary>
    /// <param name="prefab">要生成的物体预制体</param>
    /// <param name="parent">生成的物体的父物体（可为 null）</param>
    /// <param name="image">用于界定生成区域的 Image 组件</param>
    /// <param name="count">需要生成的物体数量</param>
    /// <param name="radius">每个物体的半径（假设为圆形，用于碰撞检测和边界约束）</param>
    /// <param name="maxAttempts">单个物体的最大随机尝试次数，防止死循环</param>
    /// <returns>成功生成的物体列表（数量可能小于 count，当区域无法容纳时）</returns>
    public  List<GameObject> GenerateNonOverlapObjectsInImage(GameObject prefab, 
        Transform parent, Image image, int count, float radius, int maxAttempts = 1000)
    {
        // 获取 Image 的世界坐标矩形区域
        Rect worldRect = GetWorldRectFromImage(image);
        // 调用核心生成函数
        return GenerateNonOverlapObjectsInWorldRect(prefab, parent, worldRect, count, radius, maxAttempts);
    }

    /// <summary>
    /// 在指定的世界坐标矩形区域内生成不重叠物体（核心实现）
    /// </summary>
    /// <param name="prefab">预制体</param>
    /// <param name="parent">父物体</param>
    /// <param name="worldRect">世界坐标下的矩形区域 (xMin, yMin, xMax, yMax)</param>
    /// <param name="count">目标数量</param>
    /// <param name="radius">物体半径</param>
    /// <param name="maxAttempts">单个物体最大尝试次数</param>
    /// <returns>实际生成的物体列表</returns>
    public List<GameObject> GenerateNonOverlapObjectsInWorldRect(GameObject prefab, 
        Transform parent, Rect worldRect, int count, float radius, int maxAttempts = 1000)
    {
        List<GameObject> generatedObjects = new List<GameObject>();
        List<Vector2> usedPositions = new List<Vector2>();  // 记录已生成的物体中心点

        float minDistance = radius * 2f;  // 两个物体中心点之间的最小距离

        // 边界约束：物体中心点可活动的范围（需保证整个物体在矩形内）
        float xMin = worldRect.xMin + radius;
        float xMax = worldRect.xMax - radius;
        float yMin = worldRect.yMin + radius;
        float yMax = worldRect.yMax - radius;

        // 检查区域是否足够大（即使一个物体也无法放置）
        if (xMin >= xMax || yMin >= yMax)
        {
            Debug.LogError($"生成区域过小，无法容纳半径为 {radius} 的物体！区域范围：{worldRect}");
            return generatedObjects;
        }

        // 尝试生成指定数量的物体
        for (int i = 0; i < count; i++)
        {
            bool placed = false;
            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                // 随机生成一个在边界内的中心点
                float x = Random.Range(xMin, xMax);
                float y = Random.Range(yMin, yMax);
                Vector2 candidate = new Vector2(x, y);

                // 检查是否与已有物体重叠
                bool overlap = false;
                foreach (Vector2 pos in usedPositions)
                {
                    if (Vector2.Distance(candidate, pos) < minDistance)
                    {
                        overlap = true;
                        break;
                    }
                }

                if (!overlap)
                {
                    // 不重叠，生成物体
                    // 保持预制体原有的 Z 坐标（可自行调整，例如设为 0 或 parent 的 Z）
                    float originalZ = prefab.transform.position.z;
                    Vector3 worldPosition = new Vector3(candidate.x, candidate.y, originalZ);
                    GameObject obj = Instantiate(prefab, worldPosition, Quaternion.identity, parent);
                    BotanyTagData dataItem = currentLevelDataItem.botanyTags[i];
                    LabelInformation labelInfo = obj.GetComponent<LabelInformation>();
                    labelInfo.SetIcon((BotanyPoisonousType)dataItem.botanyPoison,
                        (SensoryType)dataItem.sensoryType,dataItem.chineseDescribe);
                    generatedObjects.Add(obj);
                    usedPositions.Add(candidate);
                    placed = true;
                    break;
                }
            }

            if (!placed)
            {
                Debug.LogWarning($"仅成功生成 {generatedObjects.Count} / {count} 个物体。可能区域已满或尝试次数不足。");
                break;
            }
        }

        return generatedObjects;
    }

    /// <summary>
    /// 获取 Image 组件在世界坐标系下的轴对齐矩形范围（假设 Image 无旋转）
    /// </summary>
    private  Rect GetWorldRectFromImage(Image image)
    {
        if (image == null)
        {
            Debug.LogError("Image 组件为空！");
            return new Rect(0, 0, 0, 0);
        }

        RectTransform rectTransform = image.rectTransform;
        Vector3[] corners = new Vector3[4];
        rectTransform.GetWorldCorners(corners);  // 顺序：左下、左上、右上、右下

        // 计算最小最大值（形成轴对齐包围盒）
        float minX = Mathf.Min(corners[0].x, corners[1].x, corners[2].x, corners[3].x);
        float maxX = Mathf.Max(corners[0].x, corners[1].x, corners[2].x, corners[3].x);
        float minY = Mathf.Min(corners[0].y, corners[1].y, corners[2].y, corners[3].y);
        float maxY = Mathf.Max(corners[0].y, corners[1].y, corners[2].y, corners[3].y);

        return Rect.MinMaxRect(minX, minY, maxX, maxY);
    }
}
