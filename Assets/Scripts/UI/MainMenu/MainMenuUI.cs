using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private Button optionsButton;

    private void Start()
    {
        // Dynamically bind the Options button to the OptionsUI.OpenOptions method
        var optionsUI = FindFirstObjectByType<OptionsUI>();
        if (optionsUI != null && optionsButton != null)
        {
            optionsButton.onClick.AddListener(() => optionsUI.OpenOptions());
        }
        else
        {
            Debug.LogError("❌ MainMenuUI: Failed to bind Options button. Check if OptionsUI or Button is missing.");
        }
    }

    public void StartGame()
    {
        SceneManager.LoadScene("Battle1");
    }

    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
