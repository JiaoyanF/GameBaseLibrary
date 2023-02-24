using UnityEngine;
using UnityEngine.UI;
using Model.Event;

/// <summary>
/// 被监听者
/// </summary>
public class Trigger : MonoBehaviour
{
    [SerializeField] private InputField mInputContent;
    [SerializeField] private Button mBtnTrigger; // 触发按钮

    private void Awake()
    {
        mBtnTrigger.onClick.AddListener(OnClickTrigger);
    }

    private void OnClickTrigger()
    {
        Debug.Log($"触发监听");
        EventManager.Instance().Trigger<EventTest>(EventKey.TestShow, new EventTest
        {
            content = mInputContent.text
        });
        EventManager.Instance().Trigger(EventKey.TestCount);
    }
}