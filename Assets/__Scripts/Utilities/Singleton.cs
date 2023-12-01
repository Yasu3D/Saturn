using UnityEngine;

/// <summary>
/// A static instance is similar to a singleton, but instead of destroying any new
/// instances, it overrides the current instance. This is handy for resetting the state
/// and saves you doing it manually.
/// </summary>
public abstract class StaticInstance<T> : MonoBehaviour where T : MonoBehaviour
{
    public static T Instance {get; private set; }
    protected virtual void Awake() => Instance = this as T;

    protected virtual void OnApplicationQuit() 
    {
        Instance = null;
        Destroy(gameObject);
    }

    protected virtual void OnDestroy()
    {
        Instance = null;
    }
}

/// <summary>
/// Basic singleton. This will destroy any new
/// versions created, leaving the original instance intact.
/// </summary>
public abstract class Singleton<T> : StaticInstance<T> where T : MonoBehaviour
{
    protected override void Awake()
    {
        if (Instance != null) Destroy(gameObject);
        base.Awake();
    }
}

/// <summary>
/// Persistent singleton. This will survive through scene loads.
/// </summary>
public abstract class PersistentSingleton<T> : Singleton<T> where T : MonoBehaviour
{
    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
    }
}