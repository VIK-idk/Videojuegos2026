using UnityEngine;

public class MorsaAnimacion : MonoBehaviour
{
    [SerializeField] private Animator animator;

    private const string PARAM_REBOTAR = "Rebotar"; 

    private void Start()
    {
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
    }

    public void ReproducirRebote()
    {
        if (animator == null)
            return;

        animator.ResetTrigger(PARAM_REBOTAR);
        animator.SetTrigger(PARAM_REBOTAR);
    }
}