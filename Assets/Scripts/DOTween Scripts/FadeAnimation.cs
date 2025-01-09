using UnityEngine;
using DG.Tweening;
using System;

[RequireComponent(typeof(CanvasGroup))]
public class FadeAnimation : MonoBehaviour
{
    [SerializeField] private float range = 1f;
    [SerializeField] public float duration = 1f;
    [SerializeField] private AnimationType animationType = AnimationType.FadeIn;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private bool isPlayOnStart = true;
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
        if (isFirstAnimation && isPlayOnStart)
        {
            isFirstAnimation = false;
            StartAnimation();
        }
    }

    public void StartAnimation()
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

    public void FadeIn()
    {
        canvasGroup.alpha = 0;

        Sequence sequence = DOTween.Sequence();
        sequence.Join(canvasGroup.DOFade(1f, duration));
    }

    public void FadeOut(Action onComplete = null)
    {
        canvasGroup.alpha = 1;

        Sequence sequence = DOTween.Sequence();
        sequence.Join(canvasGroup.DOFade(0f, duration)).OnComplete(() =>{ onComplete?.Invoke();});
    }

    internal enum AnimationType { FadeIn, FadeOut };
}
