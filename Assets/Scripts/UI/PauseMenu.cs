using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] GameObject root;   // βάλε εδώ το ίδιο το panel (root GO)

    void Awake() { if (root) root.SetActive(false); }

    public void Show() { if (root) root.SetActive(true); Time.timeScale = 0f; }
    public void Hide() { if (root) root.SetActive(false); Time.timeScale = 1f; }
    public void Toggle() { if (!root) return; if (root.activeSelf) Hide(); else Show(); }

    public void OnClickMainMenu()
    {
        Time.timeScale = 1f;

        var am = AudioManager.Instance;
        if (am != null) am.StopMusic();   // κόψε άμεσα το battle BGM

        SceneFlowManager.Instance.LoadMainMenu();
    }

    public void OnClickRetry()
    {
        var am = AudioManager.Instance;
        if (am != null) am.StopMusic();

        SceneFlowManager.Instance.LoadRetry();
    }
}
