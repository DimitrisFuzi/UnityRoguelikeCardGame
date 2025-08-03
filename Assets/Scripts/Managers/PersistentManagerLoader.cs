using UnityEngine;

public class PersistentManagerLoader : MonoBehaviour
{
    [SerializeField] private GameObject gameManagerPrefab;

    void Awake()
    {
        if (GameObject.FindWithTag("GameManager") == null)
        {
            GameObject gm = Instantiate(gameManagerPrefab);
            gm.tag = "GameManager"; // Set the tag to GameManager
        }
    }
}
