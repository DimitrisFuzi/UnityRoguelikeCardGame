using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class BattleHUDController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Button deckButton;
    [SerializeField] private Button menuButton;
    [SerializeField] private DeckViewerUI deckViewer;
    [SerializeField] private GameObject pausePanel; // panel with Retry/Main Menu

    private void Awake()
    {
        if (deckButton) deckButton.onClick.AddListener(() => deckViewer.Toggle());
        if (menuButton) menuButton.onClick.AddListener(TogglePause);
        if (pausePanel) pausePanel.SetActive(false);
    }

    private void TogglePause()
    {
        if (!pausePanel) return;
        pausePanel.SetActive(!pausePanel.activeSelf);
    }

    public void OnClickRetry()
    {
        var sf = SceneFlowManager.Instance ?? FindAnyObjectByType<SceneFlowManager>();
        if (sf != null) sf.LoadRetry();
        else SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void OnClickMainMenu()
    {
        var sf = SceneFlowManager.Instance ?? FindAnyObjectByType<SceneFlowManager>();
        if (sf != null) sf.LoadMainMenu();
        else SceneManager.LoadScene("MainMenu");
    }
}
