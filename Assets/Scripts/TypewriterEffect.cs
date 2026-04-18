using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;   // 引入 UnityEvent

/// <summary>
/// 打字机效果：将字符串逐个字符显示到文本组件上，并可设置每个字符的显示时间间隔。
/// 完成时可触发 UnityEvent 事件。
/// </summary>
public class TypewriterEffect : MonoBehaviour
{
    [Header("目标文本组件")]
    [SerializeField] private TextMeshProUGUI targetText;

    [Header("打字速度设置")]
    [Tooltip("每个字符显示的时间间隔（秒）")]
    [SerializeField] private float timePerCharacter = 0.05f;

    [Header("完成事件")]
    [SerializeField] private UnityEvent onTypingComplete;   // 打字完全结束时触发

    public Button NextButton;
    
    private Coroutine typeCoroutine;
    private string fullText = "";

    /// <summary>
    /// 打字是否已经完整结束（未开始/进行中为 false，完整显示完成为 true）
    /// </summary>
    public bool IsTypingComplete { get; private set; }

    public string content;
    private void Awake()
    {
        if (targetText == null)
            targetText = GetComponent<TextMeshProUGUI>();
    }

    private void Start()
    {
        onTypingComplete.AddListener(() =>
        {
            NextButton.onClick.AddListener(() =>
            {
                MainController.Instance.LoadMenu(2);
            });
        });
        // 玩家将成为一位生物学家，需要在神秘海岛中完成所有的生物采样，要保证自己的安全不要吃毒！  空点即可开始游戏
        StartTypewriting(content);
    }

    /// <summary>
    /// 设置每个字符的显示间隔时间（秒）
    /// </summary>
    public void SetTimePerCharacter(float seconds)
    {
        timePerCharacter = Mathf.Max(0.001f, seconds);
    }

    public float GetTimePerCharacter() => timePerCharacter;

    /// <summary>
    /// 开始逐字显示文本（如果已有进行中的动画，会先停止）
    /// </summary>
    /// <param name="newText">要显示的完整字符串</param>
    public void StartTypewriting(string newText)
    {
        if (typeCoroutine != null)
            StopCoroutine(typeCoroutine);

        fullText = newText;
        IsTypingComplete = false;
        typeCoroutine = StartCoroutine(TypewriteCoroutine());
    }

    /// <summary>
    /// 立即显示完整文本（跳过逐字动画），并触发完成事件
    /// </summary>
    public void ShowFullTextImmediately()
    {
        if (typeCoroutine != null)
            StopCoroutine(typeCoroutine);

        targetText.text = fullText;
        IsTypingComplete = true;
        onTypingComplete?.Invoke();   // 触发事件
    }

    /// <summary>
    /// 停止打字动画，并清空文本（不会触发完成事件）
    /// </summary>
    public void StopAndClear()
    {
        if (typeCoroutine != null)
            StopCoroutine(typeCoroutine);

        targetText.text = "";
        fullText = "";
        IsTypingComplete = false;
        // 注意：清空文本不算“完成”，因此不触发 onTypingComplete
    }

    private IEnumerator TypewriteCoroutine()
    {
        targetText.text = "";
        foreach (char c in fullText)
        {
            targetText.text += c;
            yield return new WaitForSeconds(timePerCharacter);
        }

        IsTypingComplete = true;
        onTypingComplete?.Invoke();   // 正常打字结束，触发事件
        typeCoroutine = null;
    }

    [ContextMenu("测试打字效果")]
    private void TestTypewrite()
    {
        StartTypewriting("Hello, Unity! 这是逐字显示的效果。");
    }
}