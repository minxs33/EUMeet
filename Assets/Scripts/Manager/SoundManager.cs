using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

[RequireComponent(typeof(AudioSource))]
public class SoundManager : MonoBehaviour
{
    [SerializeField] private SoundsSO SO;
    private static SoundManager instance = null;
    private AudioSource audioSource;
    private Coroutine fadeCoroutine;

    private void Awake()
    {
        if(!instance)
        {
            instance = this;
            audioSource = GetComponent<AudioSource>();
            DontDestroyOnLoad(instance);
        }
    }

    public static void PlaySound(SoundType sound, AudioSource source = null, float volume = 1)
    {
        SoundList soundList = instance.SO.sounds[(int)sound];
        AudioClip[] clips = soundList.sounds;
        AudioClip randomClip = clips[UnityEngine.Random.Range(0, clips.Length)];

        if(source)
        {
            source.outputAudioMixerGroup = soundList.mixer;
            source.clip = randomClip;
            source.volume = volume * soundList.volume;
            source.Play();
        }
        else
        {
            instance.audioSource.outputAudioMixerGroup = soundList.mixer;
            instance.audioSource.PlayOneShot(randomClip, volume * soundList.volume);
        }
    }

    public static void PlaySoundWithTransition(SoundType sound, float fadeDuration = 1.0f, float volume = 1)
    {
        if (instance.fadeCoroutine != null)
        {
            instance.StopCoroutine(instance.fadeCoroutine);
        }

        instance.fadeCoroutine = instance.StartCoroutine(instance.FadeOutAndPlay(sound, fadeDuration, volume));
    }

    private IEnumerator FadeOutAndPlay(SoundType sound, float fadeDuration, float targetVolume)
    {
        // Fade out current sound
        if (audioSource.isPlaying)
        {
            float startVolume = audioSource.volume;

            for (float t = 0; t < fadeDuration; t += Time.deltaTime)
            {
                audioSource.volume = Mathf.Lerp(startVolume, 0, t / fadeDuration);
                yield return null;
            }

            audioSource.Stop();
        }

        SoundList soundList = SO.sounds[(int)sound];
        AudioClip[] clips = soundList.sounds;
        AudioClip randomClip = clips[UnityEngine.Random.Range(0, clips.Length)];

        audioSource.outputAudioMixerGroup = soundList.mixer;
        audioSource.clip = randomClip;
        audioSource.volume = 0;
        audioSource.Play();

        float endVolume = targetVolume * soundList.volume;
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            audioSource.volume = Mathf.Lerp(0, endVolume, t / fadeDuration);
            yield return null;
        }

        audioSource.volume = endVolume;
    }
}

[Serializable]
public struct SoundList
{
    [HideInInspector] public string name;
    [Range(0, 1)] public float volume;
    public AudioMixerGroup mixer;
    public AudioClip[] sounds;
}