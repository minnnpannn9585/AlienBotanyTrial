using System.Collections;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class TypewriterEffect : MonoBehaviour
{
    [Header("目标文本组件")]
    [SerializeField] private TextMeshProUGUI targetText;

    [Header("打字速度设置")]
    [SerializeField] private float timePerCharacter = 0.05f;

    [Header("完成事件")]
    [SerializeField] private UnityEvent onTypingComplete;

    [Header("打字内容")]
    public string content;

    private Coroutine typeCoroutine;
    private string fullText = "";
    private float typingStartTime;
    
    public bool IsTypingComplete { get; private set; }
    public float TotalTypingDuration { get; private set; }

    private void Awake()
    {
        if (targetText == null)
            targetText = GetComponent<TextMeshProUGUI>();
    }

    private void Start()
    {
        StartTypewriting(content);
    }

    public float timer;
    
    // ========== 计算预计耗时的函数 ==========
    /// <summary>使用当前 Content 和当前打字速度计算预计总耗时</summary>
    [Button]
    public float CalculateTypingDuration()
    {
        timer =CalculateTypingDuration(content, timePerCharacter);
        return timer;
    }

    /// <summary>根据指定文本和速度（可选）计算预计总耗时</summary>
    public float CalculateTypingDuration(string text, float? speed = null)
    {
        if (string.IsNullOrEmpty(text))
            return 0f;
        float effectiveSpeed = (speed.HasValue && speed.Value > 0) ? speed.Value : timePerCharacter;
        return text.Length * effectiveSpeed;
    }
    // ======================================

    public void SetTimePerCharacter(float seconds)
    {
        timePerCharacter = Mathf.Max(0.001f, seconds);
    }

    public float GetTimePerCharacter() => timePerCharacter;

    public void StartTypewriting(string newText)
    {
        if (typeCoroutine != null)
            StopCoroutine(typeCoroutine);

        fullText = newText;
        IsTypingComplete = false;
        TotalTypingDuration = 0f;
        typingStartTime = Time.time;

        if (string.IsNullOrEmpty(fullText))
        {
            targetText.text = "";
            IsTypingComplete = true;
            TotalTypingDuration = 0f;
            onTypingComplete?.Invoke();
            typeCoroutine = null;
            return;
        }

        typeCoroutine = StartCoroutine(TypewriteCoroutine());
    }

    public void ShowFullTextImmediately()
    {
        if (IsTypingComplete) return;

        if (typeCoroutine != null)
        {
            StopCoroutine(typeCoroutine);
            typeCoroutine = null;
            TotalTypingDuration = typingStartTime > 0 ? Time.time - typingStartTime : 0f;
        }
        else
        {
            TotalTypingDuration = 0f;
        }

        targetText.text = fullText;
        IsTypingComplete = true;
        onTypingComplete?.Invoke();
    }

    public void StopAndClear()
    {
        if (typeCoroutine != null)
            StopCoroutine(typeCoroutine);
        targetText.text = "";
        fullText = "";
        IsTypingComplete = false;
        TotalTypingDuration = 0f;
        typingStartTime = 0f;
        typeCoroutine = null;
    }

    private IEnumerator TypewriteCoroutine()
    {
        targetText.text = "";
        foreach (char c in fullText)
        {
            targetText.text += c;
            yield return new WaitForSeconds(timePerCharacter);
        }

        TotalTypingDuration = typingStartTime > 0 ? Time.time - typingStartTime : 0f;
        IsTypingComplete = true;
        onTypingComplete?.Invoke();
        typeCoroutine = null;
    }

    [ContextMenu("测试打字效果")]
    private void TestTypewrite()
    {
        StartTypewriting("Hello, Unity! 这是逐字显示的效果。");
    }
}