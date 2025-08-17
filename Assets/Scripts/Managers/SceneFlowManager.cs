using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

/// <summary>
/// Enum for all known scenes used in scene flow.
/// </summary>
public enum SceneType
{
    MainMenu,
    Battle1,
    Reward1,
    BattleBoss1,
    Victory
}

public class SceneFlowManager : MonoBehaviour
{
    public static SceneFlowManager Instance { get; private set; }

    private Dictionary<SceneType, SceneType> sceneFlowMap;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeSceneFlow();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeSceneFlow()
    {
        sceneFlowMap = new Dictionary<SceneType, SceneType>
        {
            { SceneType.Battle1, SceneType.Reward1 },
            { SceneType.Reward1, SceneType.BattleBoss1 },
            { SceneType.BattleBoss1, SceneType.Victory }
        };
    }

    public void LoadScene(SceneType scene)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(scene.ToString());
    }

    public void LoadNextAfterBattle()
    {
        if (System.Enum.TryParse(SceneManager.GetActiveScene().name, out SceneType currentScene))
        {
            if (sceneFlowMap.TryGetValue(currentScene, out SceneType nextScene))
            {
                LoadScene(nextScene);
            }
            else
            {
                Debug.LogWarning($"⚠️ No next scene defined for '{currentScene}'.");
            }
        }
        else
        {
            Debug.LogError($"❌ Current scene '{SceneManager.GetActiveScene().name}' is not mapped in SceneType enum.");
        }
    }

    public void RetryCurrentScene()
    {
        Time.timeScale = 1f;
        var active = SceneManager.GetActiveScene();
        // Αν το όνομα της σκηνής αντιστοιχεί στο enum σου, μπορείς να μείνεις συνεπής:
        if (System.Enum.TryParse(active.name, out SceneType current))
        {
            LoadScene(current); // διατηρεί το flow API σου
        }
        else
        {
            // fallback: φόρτωσε με όνομα
            SceneManager.LoadScene(active.name);
        }
    }

    // Optional helpers
    public void LoadRetry() => RetryCurrentScene();
    public void LoadMainMenu() => LoadScene(SceneType.MainMenu);
}
