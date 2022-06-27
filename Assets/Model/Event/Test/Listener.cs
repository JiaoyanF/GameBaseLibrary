using System;
using UnityEngine;
using UnityEngine.UI;
using Model.Event;

/// <summary>
/// 监听者
/// </summary>
public class Listener : MonoBehaviour
{
    // 刷新
    [SerializeField] private Text mTextShow;
    [SerializeField] private Button mBtnListenerShow; // 注册监听
    [SerializeField] private Button mBtnUnlistenerShow; // 注销监听
    // 计数
    [SerializeField] private Text mTextCount;
    [SerializeField] private Button mBtnListenerCount; // 注册监听
    [SerializeField] private Button mBtnUnlistenerCount; // 注销监听

    private int mCount = 0;

    private void Awake()
    {
        mBtnListenerShow.onClick.AddListener(OnClickShow);
        mBtnUnlistenerShow.onClick.AddListener(OnClickShow);
        mBtnListenerCount.onClick.AddListener(OnClickCount);
        mBtnUnlistenerCount.onClick.AddListener(OnClickCount);

        mTextCount.text = mCount.ToString();
    }

    private void OnEnable()
    {
        EventManager.Instance().SetListenStatus(this, true);
    }

    private void OnDisable()
    {
        EventManager.Instance().SetListenStatus(this, false);
    }

    private void OnClickShow()
    {
        if (EventManager.Instance().IsListened(this, EventKey.TestShow))
        {
            Debug.Log($"实例【{this.name}】移除刷新监听");
            EventManager.Instance().Unlisten(this, EventKey.TestShow);
        }
        else
        {
            Debug.Log($"实例【{this.name}】添加刷新监听");
            EventManager.Instance().Listen<EventTest>(EventKey.TestShow, RefreshShow);
        }
    }

    private void OnClickCount()
    {
        if (EventManager.Instance().IsListened(this, EventKey.TestCount))
        {
            Debug.Log($"实例【{this.name}】移除计数监听");
            mCount = 0;
            mTextCount.text = mCount.ToString();
            EventManager.Instance().Unlisten(this, EventKey.TestCount);
        }
        else
        {
            Debug.Log($"实例【{this.name}】添加计数监听");
            EventManager.Instance().Listen(EventKey.TestCount, RefreshCount);
        }
    }

    private void RefreshShow(EventTest param)
    {
        mTextShow.text = param.content;
    }

    private void RefreshCount()
    {
        mCount++;
        mTextCount.text = mCount.ToString();
    }
}