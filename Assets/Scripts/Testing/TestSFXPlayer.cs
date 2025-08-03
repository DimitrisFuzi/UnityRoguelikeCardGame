using UnityEngine;

public class TestSFXPlayer : MonoBehaviour
{
    public string sfxName = "Enemy_Hit";

    [ContextMenu("Play Test SFX")]
    public void PlayTestSFX()
    {
        AudioManager.Instance.PlaySFX(sfxName);
    }
}
