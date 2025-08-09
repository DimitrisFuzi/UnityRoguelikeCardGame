using UnityEngine;

public abstract class SceneSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    public static T Instance { get; private set; }

    protected virtual void Awake()
    {
        if (Instance != null && Instance != (T)(object)this)
        {
            Debug.LogWarning($"Duplicate {typeof(T).Name} in scene. Destroying duplicate.", this);
            Destroy(gameObject);
            return;
        }
        Instance = (T)(object)this;
    }

    protected virtual void OnDestroy()
    {
        if (Instance == (T)(object)this) Instance = null;
    }
}
