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

    [Header("Cursor Settings")]
    [SerializeField] private Texture2D cursorTexture;
    [SerializeField] private Vector2 hotspot = Vector2.zero;
    [SerializeField] private CursorMode cursorMode = CursorMode.Auto;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeManagers();
            Logger.Log("GameManager initialized.", this);
        }
        else if (Instance != this)
        {
            Logger.LogWarning("Duplicate GameManager found. Destroying extra instance.", this);
            Destroy(gameObject);
        }

        // Set custom cursor
        if (cursorTexture != null)
        {
            Cursor.SetCursor(cursorTexture, hotspot, cursorMode);
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    /// <summary>Initializes core managers (Options, Audio).</summary>
    private void InitializeManagers()
    {
        OptionsManager = LoadOrInstantiateManager<OptionsManager>("Prefabs/Managers/OptionsManager");
        AudioManager = LoadOrInstantiateManager<AudioManager>("Prefabs/Managers/AudioManager");
        // DeckManager = LoadOrInstantiateManager<DeckManager>("Prefabs/Managers/DeckManager");
    }

    /// <summary>
    /// Generic helper for loading or instantiating manager prefabs.
    /// </summary>
    private T LoadOrInstantiateManager<T>(string prefabPath) where T : Component
    {
        T manager = GetComponentInChildren<T>();
        if (manager != null)
            return manager;

        GameObject prefab = Resources.Load<GameObject>(prefabPath);
        if (prefab == null)
        {
            Logger.LogError($"{typeof(T).Name} prefab not found at: {prefabPath}", this);
            return null;
        }

        GameObject instance = Instantiate(prefab, transform.position, Quaternion.identity, transform);
        manager = instance.GetComponent<T>();

        if (manager == null)
        {
            Logger.LogError($"Instantiated prefab does not contain component: {typeof(T).Name}", this);
        }
        else
        {
            Logger.Log($"Instantiated {typeof(T).Name} from prefab.", this);
        }

        return manager;
    }
}
