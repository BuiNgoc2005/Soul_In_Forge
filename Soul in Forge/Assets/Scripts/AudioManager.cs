using System.Collections;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Volume")]
    [Range(0f, 1f)] public float masterVolume = 1f;

    [Header("Crossfade")]
    public float defaultCrossfade = 0.6f;

    AudioSource srcA;
    AudioSource srcB;
    bool usingA = true;
    Coroutine fadeRoutine;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SetupSources();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void SetupSources()
    {
        // create 2 audio source to crossfade
        srcA = gameObject.AddComponent<AudioSource>();
        srcB = gameObject.AddComponent<AudioSource>();

        srcA.playOnAwake = srcB.playOnAwake = false;
        srcA.loop = srcB.loop = true;
        srcA.volume = srcB.volume = 0f;
    }

    AudioSource ActiveSource => usingA ? srcA : srcB;
    AudioSource InactiveSource => usingA ? srcB : srcA;

    // play 1 clip (no fade)
    public void PlayClipImmediate(AudioClip clip, float volume = 1f)
    {
        if (clip == null) return;
        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        ActiveSource.clip = clip;
        ActiveSource.volume = volume * masterVolume;
        ActiveSource.Play();

        InactiveSource.Stop();
    }

    // Crossfade to new clip  (if clip == null => fade out current)
    public void CrossfadeToClip(AudioClip newClip, float duration = -1f)
    {
        if (duration <= 0f) duration = defaultCrossfade;
        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(CrossfadeRoutine(newClip, duration));
    }

    IEnumerator CrossfadeRoutine(AudioClip newClip, float duration)
    {
        AudioSource from = ActiveSource;
        AudioSource to = InactiveSource;

        if (newClip != null && from.clip == newClip && from.isPlaying)
        {
            from.volume = 1f * masterVolume;
            yield break;
        }

        // set to source
        if (newClip != null)
        {
            to.clip = newClip;
            to.volume = 0f;
            to.Play();
        }
        else
        {
            // fade to silence
            to.clip = null;
            to.volume = 0f;
            // we won't play 'to'
        }

        float t = 0f;
        float startVolFrom = from.isPlaying ? from.volume : 0f;
        float startVolTo = to.isPlaying ? to.volume : 0f;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float a = Mathf.Clamp01(t / duration);
            if (from.isPlaying) from.volume = Mathf.Lerp(startVolFrom, 0f, a) * masterVolume;
            if (to.isPlaying) to.volume = Mathf.Lerp(startVolTo, 1f, a) * masterVolume;
            yield return null;
        }

        // finalize
        if (from.isPlaying) from.volume = 0f;
        if (to.isPlaying) to.volume = 1f * masterVolume;

        // stop the old source
        if (from.isPlaying) from.Stop();

        // swap active flag
        usingA = !usingA;
        fadeRoutine = null;
    }
}
