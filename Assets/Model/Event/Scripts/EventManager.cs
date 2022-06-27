using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;
using Model.Event;

namespace Model.Event
{
    public class EventManager : Singleton<EventManager>, IEvent
    {
        // 事件集合 嵌套字典
        //  {event_key1,
        //      {
        //          {listener1,  delegate_func},
        //          {listener2, delegate_func},
        //          {...}
        //      }
        //  },
        //  {event_key2, table2}
        private Dictionary<EventKey, Hashtable> Events = new Dictionary<EventKey, Hashtable>();
        private List<object> UnlistenList = new List<object>();// 暂时不监听事件的对象集合

        /// <summary>
        /// 注册监听事件（无参回调）
        /// </summary>
        /// <param name="key"></param>
        /// <param name="dele"></param>
        public void Listen(EventKey key, EventDelegate dele)
        {
            // 判断事件key
            Hashtable listeners = null;
            if (!Events.ContainsKey(key))
            {
                listeners = new Hashtable();
                Events.Add(key, listeners);
            }
            else
            {
                listeners = Events[key] as Hashtable;
            }

            // 判断实例对象
            object target = dele.Target;
            if (!listeners.ContainsKey(target))
            {
                listeners.Add(target, dele);
            }

            SetListenStatus(target, true);// 默认开启监听状态
        }

        /// <summary>
        /// 注册监听事件（object参数回调）
        /// </summary>
        /// <param name="key"></param>
        /// <param name="dele"></param>
        public void Listen(EventKey key, EventDelegateObj dele)
        {
            // 判断事件key
            Hashtable listeners = null;
            if (!Events.ContainsKey(key))
            {
                listeners = new Hashtable();
                Events.Add(key, listeners);
            }
            else
            {
                listeners = Events[key] as Hashtable;
            }

            // 判断实例对象
            object target = dele.Target;
            if (!listeners.ContainsKey(target))
            {
                listeners.Add(target, dele);
            }

            SetListenStatus(target, true);// 默认开启监听状态
        }

        /// <summary>
        /// 注册监听事件（泛型参数回调）
        /// </summary>
        /// <param name="key"></param>
        /// <param name="dele"></param>
        /// <typeparam name="T"></typeparam>
        public void Listen<T>(EventKey key, EventDelegate<T> dele) where T : IEventParams
        {
            // 判断事件key
            Hashtable listeners = null;
            if (!Events.ContainsKey(key))
            {
                listeners = new Hashtable();
                Events.Add(key, listeners);
            }
            else
            {
                listeners = Events[key] as Hashtable;
            }

            // 判断实例对象
            object target = dele.Target;
            if (!listeners.ContainsKey(target))
            {
                listeners.Add(target, dele);
            }

            SetListenStatus(target, true);// 默认开启监听状态
        }

        /// <summary>
        /// 移除所有事件key的监听
        /// </summary>
        /// <param name="key"></param>
        public void UnlistenAll(EventKey key)
        {
            if (Events.ContainsKey(key))
            {
                Events.Remove(key);
            }
        }

        /// <summary>
        /// 移除当前监听器的事件key监听
        /// </summary>
        /// <param name="target"></param>
        /// <param name="key"></param>
        public void Unlisten(object target, EventKey key)
        {
            if (!Events.ContainsKey(key)) return;

            Hashtable listeners = Events[key] as Hashtable;
            if (listeners.ContainsKey(target))
            {
                listeners.Remove(target);
            }
        }

        /// <summary>
        /// 触发监听（不带参）
        /// </summary>
        /// <param name="key"></param>
        public void Trigger(EventKey key)
        {
            if (!Events.ContainsKey(key)) return;

            Hashtable listeners = Events[key] as Hashtable;
            if (listeners.Count == 0) return;

            List<object> triggers = listeners.Keys.Cast<object>().ToList();
            for (int i = 0; i < triggers.Count; i++)
            {
                object trigger = triggers[i];
                // 对象不监听
                if (UnlistenList.Contains(trigger)) continue;
                EventDelegate dele = listeners[trigger] as EventDelegate;
                dele();
            }
        }

        /// <summary>
        /// 触发监听（object参数）
        /// </summary>
        /// <param name="key"></param>
        /// <param name="param"></param>
        public void Trigger(EventKey key, object param)
        {
            if (!Events.ContainsKey(key)) return;

            Hashtable listeners = Events[key] as Hashtable;
            if (listeners.Count == 0) return;

            List<object> triggers = listeners.Keys.Cast<object>().ToList();
            for (int i = 0; i < triggers.Count; i++)
            {
                object trigger = triggers[i];
                // 对象不监听
                if (UnlistenList.Contains(trigger)) continue;
                EventDelegateObj dele = listeners[trigger] as EventDelegateObj;
                dele(param);
            }
        }

        /// <summary>
        /// 触发监听（泛型参数）
        /// </summary>
        /// <param name="key"></param>
        /// <param name="param"></param>
        /// <typeparam name="T"></typeparam>
        public void Trigger<T>(EventKey key, T param) where T : IEventParams
        {
            if (!Events.ContainsKey(key)) return;

            Hashtable listeners = Events[key] as Hashtable;
            if (listeners.Count == 0) return;

            List<object> triggers = listeners.Keys.Cast<object>().ToList();
            for (int i = 0; i < triggers.Count; i++)
            {
                object trigger = triggers[i];
                // 对象不监听
                if (UnlistenList.Contains(trigger)) continue;
                EventDelegate<T> dele = listeners[trigger] as EventDelegate<T>;
                dele(param);
            }
        }

        /// <summary>
        /// 是否已监听事件
        /// </summary>
        /// <param name="target"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool IsListened(object target, EventKey key)
        {
            if (!Events.ContainsKey(key)) return false;

            Hashtable listeners = Events[key] as Hashtable;
            if (listeners.Count == 0) return false;
            // 对象注册了监听事件，且是开启了监听状态
            return listeners.ContainsKey(target) && !UnlistenList.Contains(target);
        }

        /// <summary>
        /// 设置监听状态
        /// </summary>
        /// <param name="target"></param>
        /// <param name="listen">true开启监听状态</param>
        public void SetListenStatus(object target, bool listen)
        {
            if (UnlistenList.Contains(target) && listen)
            {
                // 在屏蔽列表，且设置为监听
                UnlistenList.Remove(target);
            }
            else if (!UnlistenList.Contains(target) && !listen)
            {
                // 不在屏蔽列表，要设置为不监听
                UnlistenList.Add(target);
            }
        }
    }
}