using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleHUDLoader : MonoBehaviour
{
    [SerializeField] private GameObject hudPrefab;  // <- σύρε εδώ το BattleHUD prefab
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
        bool isBattle = scene.name.StartsWith(battlePrefix);
        if (isBattle)
        {
            if (instance == null && hudPrefab != null)
                instance = Instantiate(hudPrefab);
        }
        else
        {
            if (instance != null) { Destroy(instance); instance = null; }
        }
    }
}
