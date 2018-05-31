using UnityEngine;
using UnityEngine.UI;

public class UIEventFilter : MonoBehaviour {

    //触摸点数量
    private int m_nTouchNum = 0;
    //是否在监听的面板触摸
    private bool m_bIsOnTouchOver = false;

    void Start()
    {
        m_nTouchNum = 0;
        EventTriggerListener.Get(gameObject).onEnter = OnTouchEnter;
        EventTriggerListener.Get(gameObject).onExit = OnTouchExit;
    }
    /// <summary>
    /// 点击进入
    /// </summary>
    /// <param name="go"></param>
    public void OnTouchEnter(GameObject go)
    {
        m_nTouchNum++;
        m_bIsOnTouchOver = true;
    }
    /// <summary>
    /// 点击结束
    /// </summary>
    /// <param name="go"></param>
    public void OnTouchExit(GameObject go)
    {
        if (m_nTouchNum > 0)
            m_nTouchNum -= 1;
        if (m_nTouchNum == 0)
            m_bIsOnTouchOver = false;
    }

    /// <summary>
    /// 获取是否在UI上移动
    /// </summary>
    /// <returns></returns>
    public bool getIsOnTouchOver()
    {
        return m_bIsOnTouchOver;
    }
}
