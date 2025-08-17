using UnityEngine;
using UnityEngine.UI;
public class GameOverUIManager : MonoBehaviour
{
    public static GameOverUIManager Instance { get; private set; }

    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button mainMenuButton;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (retryButton != null)
            retryButton.onClick.AddListener(OnRetryClicked);

        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(OnMainMenuClicked);
    }

    public void ShowGameOver()
    {
        Time.timeScale = 0f;
        gameOverPanel.SetActive(true);
    }

    public void HideGameOver()
    {
        Time.timeScale = 1f;
        gameOverPanel.SetActive(false);
    }

    public void OnRetryClicked()
    {
        HideGameOver();
        SceneFlowManager.Instance.RetryCurrentScene();
    }

    public void OnMainMenuClicked()
    {
        HideGameOver();
        SceneFlowManager.Instance.LoadMainMenu();
    }
}
