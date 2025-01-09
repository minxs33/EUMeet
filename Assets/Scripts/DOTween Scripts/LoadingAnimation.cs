using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class LoadingAnimation : MonoBehaviour
{
    //Loading Bar Image, Transform
    private Image loadingBar;
    private Transform loadingBarTransform;

    //FillAmount, Rotate Speed
    private float LoadingDuration = 3f;
    private float rotateDuration = 1.5f;

    private void OnEnable(){
        GameEventsManager.instance.UIEvents.onLocalPlayerJoined += StopLoading;
    }

    private void OnDisable(){
        GameEventsManager.instance.UIEvents.onLocalPlayerJoined -= StopLoading;
    }

    private void Awake()
    {
        loadingBar = GetComponent<Image>();
        loadingBarTransform = GetComponent<Transform>();
    }

    private void Start()
    {
        StartLoading();
        RotateLoading();
    }

    //FillAmount 0 -> 1, 1 -> 0 YOYO Type Lasts 3 Seconds
    private void StartLoading()
    {
        loadingBar.fillAmount = 0f;
        loadingBar.DOFillAmount(1f, LoadingDuration)
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Yoyo);
    }


    //Image Rotation repeat right rotation based on Z axis
    //RotateMode FastBeyond360 useful for creating animations that take several turns to reach a target angle
    private void RotateLoading()
    {
        loadingBarTransform.DORotate(new Vector3(0f, 0f, -360f), rotateDuration, RotateMode.FastBeyond360)
            .SetEase(Ease.Linear)
            .SetLoops(-1);
    }

    public void StopLoading()
    {
        // Stop the fill amount animation with a smooth transition to 0
        loadingBar.DOKill(); // Stop the current animation on loadingBar
        loadingBar.DOFillAmount(0f, 0.5f) // Smoothly transition to 0 fill amount over 0.5 seconds
            .SetEase(Ease.OutQuad);

        // Stop the rotation animation with a smooth deceleration
        loadingBarTransform.DOKill(); // Stop the current animation on loadingBarTransform
        loadingBarTransform.DORotate(Vector3.zero, 0.5f, RotateMode.FastBeyond360) // Smoothly reset to original rotation over 0.5 seconds
            .SetEase(Ease.OutQuad);
        loadingBar.DOFade(0f, 0.5f).SetEase(Ease.OutQuad);
    }
}