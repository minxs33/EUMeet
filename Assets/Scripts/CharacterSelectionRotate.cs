using UnityEngine;
using DG.Tweening; // Import DoTween namespace

public class CharacterSelectionRotate : MonoBehaviour
{
    public float rotationDuration = 2f;

    void Start()
    {
        transform.DORotate(new Vector3(0, 360, 0), rotationDuration, RotateMode.FastBeyond360)
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Restart);
    }
}
