using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject root; // the panel root GO

    private void Awake()
    {
        if (root) root.SetActive(false);
    }

    public void Show()
    {
        if (root) root.SetActive(true);
        Time.timeScale = 0f;
    }

    public void Hide()
    {
        if (root) root.SetActive(false);
        Time.timeScale = 1f;
    }

    public void Toggle()
    {
        if (!root) return;
        if (root.activeSelf) Hide(); else Show();
    }

    public void OnClickMainMenu()
    {
        Time.timeScale = 1f;
        AudioManager.Instance?.StopMusic();
        SceneFlowManager.Instance?.LoadMainMenu();
    }

    public void OnClickRetry()
    {
        AudioManager.Instance?.StopMusic();
        SceneFlowManager.Instance?.LoadRetry();
    }
}
