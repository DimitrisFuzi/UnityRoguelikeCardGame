using UnityEngine;

public class EnergyImageAnimation : MonoBehaviour
{
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
        animator.Play("EnergyOrb_Animation");
    }
}