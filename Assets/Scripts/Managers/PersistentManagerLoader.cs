using UnityEngine;

/// <summary>
/// Ensures core persistent managers exist; instantiates prefabs if missing.
/// </summary>
public class PersistentManagerLoader : MonoBehaviour
{
    [SerializeField] private GameObject gameManagerPrefab;
    [SerializeField] private GameObject sceneFlowManagerPrefab;
    [SerializeField] private GameObject gameOverUIManager;

    private void Awake()
    {
        if (GameObject.FindWithTag("GameManager") == null && gameManagerPrefab != null)
        {
            GameObject gm = Instantiate(gameManagerPrefab);
            gm.tag = "GameManager";
            Logger.Log("Instantiated GameManager prefab.");
        }

        if (SceneFlowManager.Instance == null && sceneFlowManagerPrefab != null)
        {
            var go = Instantiate(sceneFlowManagerPrefab);
            DontDestroyOnLoad(go);
            Logger.Log("Instantiated SceneFlowManager prefab.");
        }

        if (GameOverUIManager.Instance == null && gameOverUIManager != null)
        {
            GameObject go = Instantiate(gameOverUIManager);
            DontDestroyOnLoad(go);
            Logger.Log("Instantiated GameOverUIManager prefab.");
        }
    }
}
