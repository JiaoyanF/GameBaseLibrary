using System.Collections.Generic;

namespace Model.Event
{
    public delegate void EventDelegate();
    public delegate void EventDelegateObj(object param);
    public delegate void EventDelegate<T>(T param) where T : IEventParams;

    public interface IEventParams
    {
    }

    #region 基础通用参数类型

    public class EventInt : IEventParams
    {
        public int Value;
    }

    public class EventBool : IEventParams
    {
        public bool Value;
    }

    public class EventFloat : IEventParams
    {
        public float Value;
    }

    public class EventString : IEventParams
    {
        public string Value;
    }

    #endregion

    #region 拓展参数类型

    // 测试事件参数
    public class EventTest : IEventParams
    {
        public string content;
        public int index;
    }

    #endregion
}