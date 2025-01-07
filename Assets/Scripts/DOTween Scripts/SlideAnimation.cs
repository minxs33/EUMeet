using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(CanvasGroup))]
public class SlideAnimation : MonoBehaviour
{
    [SerializeField] private float range = 1f;         // Distance to offset from the original position
    [SerializeField] private float duration = 1f;       // Duration of the animation
    [SerializeField] private Direction startDirection = Direction.Up; // Starting direction
    [SerializeField] private AnimationType animationType = AnimationType.SlideIn;
    [SerializeField] private CanvasGroup canvasGroup;   // For fade effect
    private bool isFirstAnimation = true;

    private Vector3 originalPosition;
    private Vector3 offsetPosition;

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

        offsetPosition = GetOffsetPosition(originalPosition);

        if (animationType == AnimationType.SlideIn)
        {
            SlideIn();
        }
        else if (animationType == AnimationType.SlideOut)
        {
            SlideOut();
        }
    }

    private void SlideIn()
    {
        canvasGroup.alpha = 0;
        transform.position = offsetPosition;

        Sequence sequence = DOTween.Sequence();
        sequence
            .Append(transform.DOMove(originalPosition, duration).SetEase(Ease.OutSine))
            .Join(canvasGroup.DOFade(1f, duration));
    }

    private void SlideOut()
    {
        canvasGroup.alpha = 1;

        Sequence sequence = DOTween.Sequence();
        sequence
            .Append(transform.DOMove(offsetPosition, duration).SetEase(Ease.InSine))
            .Join(canvasGroup.DOFade(0f, duration));
    }

    private Vector3 GetOffsetPosition(Vector3 basePosition)
    {
        Vector3 calculatedOffset = basePosition;
        switch (startDirection)
        {
            case Direction.Up:
                calculatedOffset.y += range;
                break;
            case Direction.Down:
                calculatedOffset.y -= range;
                break;
            case Direction.Left:
                calculatedOffset.x -= range;
                break;
            case Direction.Right:
                calculatedOffset.x += range;
                break;
        }
        return calculatedOffset;
    }

    internal enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }

    internal enum AnimationType
    {
        SlideIn,
        SlideOut
    }
}


