using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleHUDLoader : MonoBehaviour
{
    [SerializeField] private GameObject hudPrefab;   // assign the BattleHUD prefab
    [SerializeField] private string battlePrefix = "Battle";

    private GameObject instance;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (hudPrefab == null)
        {
            Logger.LogError("BattleHUDLoader: hudPrefab is not assigned.", this);
            return;
        }

        bool isBattle = scene.name.StartsWith(battlePrefix);

        if (isBattle)
        {
            if (instance == null)
                instance = Instantiate(hudPrefab);
        }
        else
        {
            if (instance != null)
            {
                Destroy(instance);
                instance = null;
            }
        }
    }
}
