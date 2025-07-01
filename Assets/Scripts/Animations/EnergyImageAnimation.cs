using UnityEngine;

public class EnergyImageAnimation : MonoBehaviour
{
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>(); // Αφαίρεση του "Animator" πριν από τη μεταβλητή
        if (animator != null)
        {
            Debug.Log("Animator found and active.");
            //animator.Play("EnergySpriteAnimation1");
        }
        else
        {
            Debug.LogError("Animator not found on this object!");
        }
    }
}