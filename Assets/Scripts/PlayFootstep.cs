using UnityEngine;
using static SoundManager;

[RequireComponent(typeof(AudioSource))]
public class PlayFootstep : MonoBehaviour {
    private AudioSource audioSource;

    private void Awake(){
        audioSource = GetComponent<AudioSource>();
    }
    public void PlaySound(){
        SoundManager.PlaySound(SoundType.FOOTSTEP, audioSource, 0.25f);
    }
}