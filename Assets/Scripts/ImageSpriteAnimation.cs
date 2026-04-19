using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;  // 新增：支持 UnityEvent

/// <summary>
/// 为 UGUI Image 组件提供帧动画控制：播放、暂停、重新播放、速率调节。
/// 附加功能：播放时可对另一个单独的 Image 进行缩放动画（开始大小 -> 结束大小），与帧动画同步。
/// 新增功能：动画播放完成时（非循环模式到达最后一帧并停止）触发 UnityEvent。
/// </summary>
[RequireComponent(typeof(Image))]
public class ImageSpriteAnimation : MonoBehaviour
{
    [Header("动画帧集合")]
    [Tooltip("按顺序播放的 Sprite 数组")]
    public Sprite[] sprites;

    [Header("播放设置")]
    [Tooltip("每秒播放的帧数（基础帧率）")]
    public float framesPerSecond = 12f;
    [Tooltip("是否循环播放")]
    public bool loop = true;
    [Tooltip("场景启动时自动开始播放")]
    public bool playOnStart = true;

    [Header("缩放动画（单独 Image）")]
    [Tooltip("是否启用播放时的缩放动画")]
    public bool enableScaleAnimation = false;
    [Tooltip("要进行缩放动画的独立 Image 组件（不能是当前物体上的 Image）")]
    public Image scaleTargetImage;
    [Tooltip("缩放动画的起始大小（播放开始时的Scale）")]
    public Vector3 startScale = Vector3.one;
    [Tooltip("缩放动画的结束大小（播放完成时的Scale）")]
    public Vector3 endScale = Vector3.one * 1.2f;
    [Tooltip("缩放动画曲线（X轴：播放进度0~1，Y轴：插值系数）")]
    public AnimationCurve scaleCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
    [Tooltip("循环播放时是否重新执行缩放动画（每次循环都从startScale到endScale）")]
    public bool resetScaleOnLoop = false;

    [Header("事件")]
    [Tooltip("动画播放完成时触发（仅非循环模式，到达最后一帧并停止时）")]
    public UnityEvent OnAnimationCompleted;

    private Image targetImage;           // 用于显示帧动画的 Image（当前物体）
    private bool isPlaying;
    private float currentFrameTime;
    private int currentFrameIndex;
    private float speedFactor = 1f;

    // 缩放动画相关
    private bool scaleAnimActive = true;
    private float lastProgress = 0f;

    // 完成事件触发标志（防止重复触发）
    private bool hasInvokedCompleted = false;

    /// <summary> 当前是否正在播放 </summary>
    public bool IsPlaying => isPlaying;

    /// <summary> 当前播放速率（1 = 正常速度）</summary>
    public float SpeedFactor
    {
        get => speedFactor;
        set => speedFactor = Mathf.Max(0f, value);
    }

    /// <summary> 当前显示到第几帧（只读）</summary>
    public int CurrentFrame => currentFrameIndex;

    private void Awake()
    {
        // 获取当前物体的 Image 组件用于帧动画显示
        targetImage = GetComponent<Image>();
        if (targetImage == null)
        {
            Debug.LogError("ImageSpriteAnimation 需要 Image 组件！");
            enabled = false;
            return;
        }
    }

    private void Start()
    {
        // 初始化帧动画
        if (sprites != null && sprites.Length > 0)
        {
            currentFrameIndex = 0;
            targetImage.sprite = sprites[0];
        }
        isPlaying = playOnStart;
        currentFrameTime = 0f;
        hasInvokedCompleted = false;   // 重置完成标志

        // 初始化缩放动画（仅当启用且有独立目标时）
        InitializeScaleAnimation();
    }

    private void Update()
    {
        // 1. 帧动画更新
        if (!isPlaying) goto ScaleUpdate;
        if (sprites == null || sprites.Length == 0) goto ScaleUpdate;
        if (speedFactor <= 0f) goto ScaleUpdate;

        float effectiveFrameRate = framesPerSecond * speedFactor;
        float frameInterval = 1f / effectiveFrameRate;

        currentFrameTime += Time.deltaTime;

        if (currentFrameTime >= frameInterval)
        {
            int framesToAdvance = Mathf.FloorToInt(currentFrameTime / frameInterval);
            currentFrameTime -= framesToAdvance * frameInterval;

            for (int i = 0; i < framesToAdvance; i++)
            {
                if (loop)
                {
                    bool willWrapAround = (currentFrameIndex == sprites.Length - 1);
                    currentFrameIndex = (currentFrameIndex + 1) % sprites.Length;

                    if (resetScaleOnLoop && willWrapAround && scaleAnimActive == false)
                    {
                        // 新一轮开始，重新激活缩放动画
                        scaleAnimActive = true;
                        if (scaleTargetImage != null)
                            scaleTargetImage.transform.localScale = startScale;
                    }
                }
                else
                {
                    if (currentFrameIndex + 1 < sprites.Length)
                    {
                        currentFrameIndex++;
                    }
                    else
                    {
                        // 播放结束（非循环到达最后一帧后）
                        isPlaying = false;
                        break;
                    }
                }
            }

            if (targetImage != null && currentFrameIndex >= 0 && currentFrameIndex < sprites.Length)
            {
                targetImage.sprite = sprites[currentFrameIndex];
            }
        }

        // 非循环模式下，检测播放刚刚结束，触发完成事件（确保只触发一次）
        if (!loop && !isPlaying && !hasInvokedCompleted && (sprites == null || currentFrameIndex >= sprites.Length - 1))
        {
            hasInvokedCompleted = true;
            OnAnimationCompleted?.Invoke();
        }

        ScaleUpdate:
        // 2. 缩放动画更新（基于帧进度，作用于单独的 scaleTargetImage）
        if (enableScaleAnimation && scaleTargetImage != null && sprites != null && sprites.Length > 0)
        {
            UpdateScaleAnimation();
        }
    }

    /// <summary>
    /// 初始化缩放动画（重置状态、设置起始缩放到独立 Image）
    /// </summary>
    private void InitializeScaleAnimation()
    {
        if (!enableScaleAnimation) return;

        if (scaleTargetImage == null)
        {
            Debug.LogWarning("ImageSpriteAnimation: 启用了缩放动画但未指定 scaleTargetImage，缩放动画将被忽略。");
            enableScaleAnimation = false;
            return;
        }

        scaleAnimActive = true;
        lastProgress = 0f;
        scaleTargetImage.transform.localScale = startScale;
    }

    /// <summary>
    /// 根据当前帧索引更新独立 Image 的缩放值
    /// </summary>
    private void UpdateScaleAnimation()
    {
        if (!scaleAnimActive) return;

        float progress = GetNormalizedProgress();

        if (progress >= 1f || (!loop && !isPlaying && currentFrameIndex == sprites.Length - 1))
        {
            scaleTargetImage.transform.localScale = endScale;
            scaleAnimActive = false;
            return;
        }

        float curveValue = scaleCurve.Evaluate(progress);
        Vector3 newScale = Vector3.Lerp(startScale, endScale, curveValue);
        scaleTargetImage.transform.localScale = newScale;
        lastProgress = progress;
    }

    /// <summary>
    /// 获取当前动画标准化进度 [0,1]
    /// </summary>
    private float GetNormalizedProgress()
    {
        if (sprites == null || sprites.Length <= 1)
            return 1f;

        if (!loop)
        {
            return (float)currentFrameIndex / (sprites.Length - 1);
        }
        else
        {
            return (float)currentFrameIndex / (sprites.Length - 1);
        }
    }

    /// <summary>
    /// 播放动画（若动画已播放完毕且非循环，则自动从头开始）
    /// </summary>
    public void Play()
    {
        if (!loop && currentFrameIndex >= sprites.Length - 1)
        {
            Restart();
        }
        else
        {
            isPlaying = true;
        }
    }

    /// <summary>
    /// 暂停动画
    /// </summary>
    public void Pause()
    {
        isPlaying = false;
    }

    /// <summary>
    /// 重新播放（重置到第一帧并开始播放，同时重置缩放动画和完成事件标志）
    /// </summary>
    public void Restart()
    {
        if (sprites == null || sprites.Length == 0) return;

        currentFrameIndex = 0;
        currentFrameTime = 0f;
        isPlaying = true;
        hasInvokedCompleted = false;   // 重置完成标志
        if (targetImage != null)
            targetImage.sprite = sprites[0];

        // 重置独立 Image 的缩放动画
        if (enableScaleAnimation && scaleTargetImage != null)
        {
            scaleAnimActive = true;
            lastProgress = 0f;
            scaleTargetImage.transform.localScale = startScale;
        }
    }

    /// <summary>
    /// 设置播放速率（倍数）
    /// </summary>
    /// <param name="speed">速率倍数，必须 >= 0。0 时动画冻结，1 为正常速度</param>
    public void SetSpeed(float speed)
    {
        SpeedFactor = speed;
    }

    /// <summary>
    /// 动态更换动画帧集合（同时重置动画状态）
    /// </summary>
    /// <param name="newSprites">新的 Sprite 数组</param>
    public void SetSprites(Sprite[] newSprites)
    {
        sprites = newSprites;
        Restart();
    }

    /// <summary>
    /// 手动重置缩放动画（例如外部需要重新触发缩放效果）
    /// </summary>
    public void ResetScaleAnimation()
    {
        if (!enableScaleAnimation || scaleTargetImage == null) return;
        scaleAnimActive = true;
        lastProgress = 0f;
        scaleTargetImage.transform.localScale = startScale;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (framesPerSecond <= 0f) framesPerSecond = 0.01f;
        if (speedFactor < 0f) speedFactor = 0f;
        if (targetImage == null) targetImage = GetComponent<Image>();

        if (scaleCurve == null || scaleCurve.keys.Length == 0)
            scaleCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        // 警告：启用了缩放动画但没有指定目标 Image
        if (enableScaleAnimation && scaleTargetImage == null)
        {
            Debug.LogWarning("ImageSpriteAnimation: 启用了缩放动画但未指定 scaleTargetImage，请在 Inspector 中赋值。");
        }
    }
#endif
}