using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;

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
        StartCoroutine(LoadSceneWithFade(scene.ToString()));
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
                Logger.LogWarning($"No next scene defined for '{currentScene}'.", this);
            }
        }
        else
        {
            Logger.LogError($"Current scene '{SceneManager.GetActiveScene().name}' is not mapped in SceneType enum.", this);
        }
    }

    private IEnumerator LoadSceneWithFade(string sceneName)
    {
        // Ensure fader exists
        if (ScreenFader.Instance == null)
            new GameObject("ScreenFader").AddComponent<ScreenFader>();

        // Fade out
        yield return ScreenFader.Instance.FadeOut();

        var am = AudioManager.Instance;
        if (am != null) am.StopMusic();

        // Async load without immediate activation
        var op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        op.allowSceneActivation = false;

        while (op.progress < 0.9f)
            yield return null;

        // Let one frame pass before activation
        yield return null;

        // Activate scene
        op.allowSceneActivation = true;
        while (!op.isDone) yield return null;

        // Bring overlay to front in the new scene
        ScreenFader.Instance?.BringToFront();

        // Give one frame for UI to settle
        yield return null;

        // Fade in
        yield return ScreenFader.Instance.FadeIn();
    }

    public void RetryCurrentScene()
    {
        Time.timeScale = 1f;
        var active = SceneManager.GetActiveScene();

        if (System.Enum.TryParse(active.name, out SceneType current))
        {
            LoadScene(current);
        }
        else
        {
            // Fallback: load by name
            SceneManager.LoadScene(active.name);
        }
    }

    // Optional helpers
    public void LoadRetry() => RetryCurrentScene();
    public void LoadMainMenu() => LoadScene(SceneType.MainMenu);
}
