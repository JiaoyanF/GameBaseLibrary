using UnityEngine;

/// <summary>
/// 单例
/// </summary>
/// <typeparam name="T"></typeparam>
public class Singleton<T> where T : new()
{
    private static T _instance;
    protected static readonly object syslock = new object();

    public static T Instance()
    {
        if (_instance == null)
        {
            lock (syslock)
            {
                if (_instance == null)
                {
                    _instance = new T();
                }
            }
        }
        return _instance;
    }
}

/// <summary>
/// mono类单例
/// </summary>
/// <typeparam name="T"></typeparam>
public class SingletonMonoBehaviour<T> : MonoBehaviour where T : SingletonMonoBehaviour<T>
{
    private static T instance;

    public static T Instance { get => instance; }
    protected virtual void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
            throw new System.Exception("An instance of this singleton already exists.");
        }
        else
        {
            instance = (T)this;
        }
    }
}