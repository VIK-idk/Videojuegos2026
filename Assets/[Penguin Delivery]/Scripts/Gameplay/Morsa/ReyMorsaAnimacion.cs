using UnityEngine;

public class ReyMorsaAnimacion : MonoBehaviour
{
    [Header("Animator")]
    [SerializeField] private Animator animator;

    private static readonly int TriggerAplaudir = Animator.StringToHash("Aplaudir");
    private static readonly int TriggerEnojar = Animator.StringToHash("Enojar");

    private void Awake()
    {
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
    }

    public void Aplaudir()
    {
        if (animator == null)
            return;

        animator.ResetTrigger(TriggerEnojar);
        animator.ResetTrigger(TriggerAplaudir);

        animator.SetTrigger(TriggerAplaudir);
    }

    public void Enojar()
    {
        if (animator == null)
            return;

        animator.ResetTrigger(TriggerAplaudir);
        animator.ResetTrigger(TriggerEnojar);

        animator.SetTrigger(TriggerEnojar);
    }
}