namespace Model.Event
{
    public interface IEvent
    {
        // 注册监听
        void Listen(EventKey key, EventDelegate func);
        void Listen(EventKey key, EventDelegateObj func);
        void Listen<T>(EventKey key, EventDelegate<T> func) where T : IEventParams;
        // 移除监听
        void UnlistenAll(EventKey key);
        void Unlisten(object target, EventKey key);
        // 触发监听
        void Trigger(EventKey key);
        void Trigger(EventKey key, object param);
        void Trigger<T>(EventKey key, T param) where T : IEventParams;
        // 对象是否监听事件
        bool IsListened(object target, EventKey key);
        // 设置监听状态
        void SetListenStatus(object target, bool listen);
    }
}