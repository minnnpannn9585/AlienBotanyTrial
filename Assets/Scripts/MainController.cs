using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using GuanYao.Tool.Singleton;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainController : SingletonMono<MainController>
{
    [Header("设置")]
    [Tooltip("需要触发鼠标显示的 UI 元素的 Tag")]
    public string targetTag = "CursorTarget";

    [Header("剧情按钮Btn")]
    public Button PlotNext;
    
    [Header("菜单列表")]
    public List<GameObject> MenuList;
    
    [Header("鼠标对象")]
    public GameObject cursor;

    [Header("关卡解锁数据")]
    public LevelLockData levelLockData;
    
    public List<Sprite> cursorState;
    
    [DllImport("__Internal")]
    private static extern void CloseWindow();
    private bool isOverTarget = false;   // 当前是否在目标 UI 上
    public bool isCursorState = true;

    /// <summary>
    /// 是否打开 我们设计的光标手
    /// </summary>
    private bool MouseIsRun = false;
    
    // Update is called once per frame
    void Update()
    {
        // 鼠标是否激活
        if (MouseIsRun)
        {
            Cursor.visible = false;
            cursor.transform.position = Input.mousePosition;
        }
        //是否开启切换状态
        if (isCursorState)
        {
            // 每帧检测鼠标下的 UI 并切换鼠标显示状态
            CheckAndSwitchCursor();
        }
    }

    /// <summary>
    /// 设置鼠标状态的
    /// </summary>
    /// <param name="mouseIsRun">是否开启鼠标运行状态</param>
    /// <param name="IsisCursorState">是否激鼠标和放大镜状态切换</param>
    public void SetCursorState(bool mouseIsRun, bool IsisCursorState)
    {
        cursor.gameObject.SetActive(mouseIsRun);
        if (!IsisCursorState)
        {
            cursor.GetComponent<Image>().sprite = cursorState[0];
            cursor.GetComponent<Image>().SetNativeSize();
            cursor.GetComponent<RectTransform>().pivot = new Vector2(0.1404321f, 0.9798688f);
        }
        Cursor.visible = !mouseIsRun;
        MouseIsRun = mouseIsRun;
        isCursorState = IsisCursorState;
    }
    
    /// <summary>
    /// 关闭所有UI
    /// </summary>
    void CloseMenuUi()
    {
        foreach (var menu in MenuList)
        {
            menu.SetActive(false);
        }
    }
    
    /// <summary>
    /// 加载UI页面
    /// </summary>
    /// <param name="index"></param>
    public void LoadMenu(int index)
    {
        CloseMenuUi();
        MenuList[index].SetActive(true);
    }
    
    /// <summary>
    /// 剧情跳转功能绑定
    /// </summary>
    public void PlotBingdingEvent()
    {
        PlotNext.onClick.AddListener(() =>
        {
            LoadMenu(2);
        });
    }

    /// <summary>
    /// 退出游戏
    /// </summary>
    public void ExitGame()
    {
        // 可选：先调用 Application.Quit() 来停止 Unity 内部的运行逻辑
        Application.Quit();
        // 再调用 JS 方法关闭浏览器标签页
        CloseWindow();
    }
    
    /// <summary>
    /// 检测鼠标下的 UI 是否具有指定 Tag，并切换鼠标显示/隐藏
    /// </summary>
    private void CheckAndSwitchCursor()
    {
        // 获取当前鼠标位置下的所有 UI 元素
        PointerEventData pointerData = new PointerEventData(EventSystem.current);
        pointerData.position = Input.mousePosition;

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        bool found = false;
        foreach (RaycastResult result in results)
        {
            if (result.gameObject.CompareTag(targetTag))
            {
                found = true;
                break;
            }
        }

        // 状态变化时切换鼠标可见性
        if (found && !isOverTarget)
        {
            // 进入目标 UI → 显示鼠标
            // Cursor.visible = true;
            cursor.GetComponent<Image>().sprite = cursorState[1];
            cursor.GetComponent<Image>().SetNativeSize();
            cursor.GetComponent<RectTransform>().pivot = new Vector2(0.2824724f, 0.791186f);
            isOverTarget = true;
        }
        else if (!found && isOverTarget)
        {
            // 离开目标 UI → 隐藏鼠标
            // Cursor.visible = false;
            cursor.GetComponent<Image>().sprite = cursorState[0];
            cursor.GetComponent<Image>().SetNativeSize();
            cursor.GetComponent<RectTransform>().pivot = new Vector2(0.1404321f, 0.9798688f);
            isOverTarget = false;
        }
    }
    
    /// <summary>
    /// 重新开始游戏
    /// </summary>
    public void RestartGame()
    {
        // 获取当前场景的名字，然后重新加载它
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    /// <summary>
    /// 防止退出后鼠标仍然隐藏（影响其他程序）
    /// </summary>
    private void OnDestroy()
    {
        Cursor.visible = true;
    }
    
    /// <summary>
    /// 退出游戏时，解锁所有关卡
    /// </summary>
    private void OnApplicationQuit()
    {
        Cursor.visible = true;
        foreach (LevelLock item in levelLockData.AllLevels)
        {
            item.IsLock = false;
        }
    }
}
