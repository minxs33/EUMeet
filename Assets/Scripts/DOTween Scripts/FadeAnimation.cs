using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(CanvasGroup))]
public class FadeAnimation : MonoBehaviour
{
    [SerializeField] private float range = 1f;
    [SerializeField] private float duration = 1f;
    [SerializeField] private AnimationType animationType = AnimationType.FadeIn;
    [SerializeField] private CanvasGroup canvasGroup;
    private bool isFirstAnimation = true;

    private Vector3 originalPosition;

    private void OnEnable()
    {
        if (!isFirstAnimation)
        {
            StartAnimation();
        }
    }

    private void Start()
    {
        originalPosition = transform.position;
        if (isFirstAnimation)
        {
            isFirstAnimation = false;
            StartAnimation();
        }
    }

    private void StartAnimation()
    {
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null) return;
        }

        if (animationType == AnimationType.FadeIn)
        {
            FadeIn();
        }
        else if (animationType == AnimationType.FadeOut)
        {
            FadeOut();
        }
    }

    private void FadeIn()
    {
        canvasGroup.alpha = 0;

        Sequence sequence = DOTween.Sequence();
        sequence.Join(canvasGroup.DOFade(1f, duration));
    }

    private void FadeOut()
    {
        canvasGroup.alpha = 1;

        Sequence sequence = DOTween.Sequence();
        sequence.Join(canvasGroup.DOFade(0f, duration));
    }

    internal enum AnimationType { FadeIn, FadeOut };
}
