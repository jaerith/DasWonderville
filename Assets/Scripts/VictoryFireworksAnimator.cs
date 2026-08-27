using UnityEngine;

public class VictoryFireworksAnimator : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private string stateName;
    [SerializeField] private int layerIndex = 0;

    private void OnEnable()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        PlayFromStart();
    }

    private void Update()
    {
        if (animator == null)
            return;

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(layerIndex);

        if (stateInfo.normalizedTime >= 1f)
        {
            PlayFromStart();
        }
    }

    private void PlayFromStart()
    {
        if (animator == null)
            return;

        if (string.IsNullOrEmpty(stateName))
            animator.Play(0, layerIndex, 0f);
        else
            animator.Play(stateName, layerIndex, 0f);
    }
}
