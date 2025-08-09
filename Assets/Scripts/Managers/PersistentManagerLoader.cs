using UnityEngine;

public class PersistentManagerLoader : MonoBehaviour
{
    [SerializeField] private GameObject gameManagerPrefab;
    [SerializeField] private GameObject sceneFlowManagerPrefab;
    [SerializeField] private GameObject gameOverUIManager;


    void Awake()
    {
        if (GameObject.FindWithTag("GameManager") == null)
        {
            GameObject gm = Instantiate(gameManagerPrefab);
            gm.tag = "GameManager"; // Set the tag to GameManager
        }

        if (SceneFlowManager.Instance == null && sceneFlowManagerPrefab != null)
        {
            Instantiate(sceneFlowManagerPrefab);
        }

        if (GameOverUIManager.Instance == null && gameOverUIManager != null)
        {
            GameObject go = Instantiate(gameOverUIManager);
            DontDestroyOnLoad(go);
        }
    }
}
