using UnityEngine;

/// <summary>
/// Simple per-scene singleton base. Clears Instance on destroy to allow scene reloads.
/// </summary>
public abstract class SceneSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    public static T Instance { get; private set; }

    protected virtual void Awake()
    {
        var self = this as T;
        if (Instance != null && Instance != self)
        {
            Logger.LogWarning($"Duplicate {typeof(T).Name} found. Destroying duplicate.", this);
            Destroy(gameObject);
            return;
        }
        Instance = self;
    }

    protected virtual void OnDestroy()
    {
        if (Instance == (this as T))
            Instance = null;
    }
}
