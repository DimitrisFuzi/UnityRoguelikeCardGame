using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class VictoryUIManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text turnsText;
    [SerializeField] private TMP_Text dealtText;
    [SerializeField] private TMP_Text takenText;

    private void Start()
    {
        var s = GameSession.Instance;
        if (s != null)
        {
            if (turnsText) turnsText.text = s.turnsTaken.ToString();
            if (dealtText) dealtText.text = s.totalDamageDealt.ToString();
            if (takenText) takenText.text = s.totalDamageTaken.ToString();
        }
    }

    public void OnMainMenu()
    {
        Time.timeScale = 1f;

        var am = AudioManager.Instance;
        if (am != null) am.StopMusic();

        SceneFlowManager.Instance.LoadMainMenu();
    }

    public void OnQuit()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // ensure it works in the Editor
#endif
    }
}
