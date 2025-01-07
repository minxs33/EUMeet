using UnityEngine;
using UnityEngine.EventSystems;

public class PlaySoundButton : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    [SerializeField] private SoundType hoverSound;
    [SerializeField] private SoundType clickSound;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (hoverSound != SoundType.NONE)
        {
            SoundManager.PlaySound(hoverSound, null, 1f);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (clickSound != SoundType.NONE)
        {
            SoundManager.PlaySound(clickSound, null, 1f);
        }
    }
}
