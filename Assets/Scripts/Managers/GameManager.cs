using UnityEngine;

/// <summary>
/// Central controller for core game systems and persistent data.
/// Implements Singleton pattern to persist across scenes.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public OptionsManager OptionsManager { get; private set; }
    public AudioManager AudioManager { get; private set; }
    //public DeckManager DeckManager { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            //DontDestroyOnLoad(gameObject);
            InitializeManagers();
            Logger.Log("✅ GameManager initialized.", this);
        }
        else if (Instance != this)
        {
            Logger.LogWarning("⚠️ Duplicate GameManager found. Destroying extra instance.", this);
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Initializes all core managers (Options, Audio, Deck).
    /// Tries to find them as children, or instantiate prefabs if missing.
    /// </summary>
    private void InitializeManagers()
    {
        OptionsManager = LoadOrInstantiateManager<OptionsManager>("Prefabs/Managers/OptionsManager");
        AudioManager = LoadOrInstantiateManager<AudioManager>("Prefabs/Managers/AudioManager");
        //DeckManager = LoadOrInstantiateManager<DeckManager>("Prefabs/Managers/DeckManager");
    }


    /// <summary>
    /// Generic helper for loading or instantiating manager prefabs.
    /// </summary>
    /// <typeparam name="T">Type of the manager (must be a Component).</typeparam>
    /// <param name="prefabPath">Resources path to the manager prefab.</param>
    /// <returns>The found or newly instantiated manager component.</returns>
    private T LoadOrInstantiateManager<T>(string prefabPath) where T : Component
    {
        T manager = GetComponentInChildren<T>();
        if (manager != null)
        {
            return manager;
        }

        GameObject prefab = Resources.Load<GameObject>(prefabPath);
        if (prefab == null)
        {
            Logger.LogError($"❌ {typeof(T).Name} prefab not found at: {prefabPath}", this);
            return null;
        }

        GameObject instance = Instantiate(prefab, transform.position, Quaternion.identity, transform);
        manager = instance.GetComponent<T>();

        if (manager == null)
        {
            Logger.LogError($"❌ Instantiated prefab does not contain component: {typeof(T).Name}", this);
        }
        else
        {
            Logger.Log($"✅ Instantiated {typeof(T).Name} from prefab.", this);
        }

        return manager;
    }
}
