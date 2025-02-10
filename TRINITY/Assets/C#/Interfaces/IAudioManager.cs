using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;

public class IAudioManager : MonoBehaviour
{
    private List<ATrinityAudioClip> AudioClips = new List<ATrinityAudioClip>();
    private Dictionary<string, ATrinityAudioClip> AudioClipLookup = new Dictionary<string, ATrinityAudioClip>();
    private Dictionary<string, AudioSource> ActiveLoopingSounds = new Dictionary<string, AudioSource>();

    private AudioSource[] AudioSourcePool;
    private const int POOL_SIZE = 4;

    public GameObject AudioClipPos;

    protected void Awake()
    {
        // Move initialization to Awake to ensure it happens before any audio plays
        AudioClips = (AudioClipPos != null ? AudioClipPos : gameObject).GetComponents<ATrinityAudioClip>().ToList();

        InitializeAudioClipDictionary();

    }

    public void OnEnable()
    {
        // Reinitialize pool when object is enabled (e.g., after scene load)
        InitializeAudioPool();
    }

    private void InitializeAudioClipDictionary()
    {
        AudioClipLookup.Clear(); // Clear existing entries
        for (int i = 0; i < AudioClips.Count; i++)
        {
            if (!string.IsNullOrEmpty(AudioClips[i].name) && AudioClips[i] != null)
            {
                AudioClipLookup[AudioClips[i].Name] = AudioClips[i];
            }
        }
    }

    void InitializeAudioPool()
    {
        
        // Clean up existing pool if it exists
        if (AudioSourcePool != null)
        {
            foreach (var source in AudioSourcePool)
            {
                if (source != null)
                {
                    Destroy(source);
                }
            }
        }

        AudioSourcePool = new AudioSource[POOL_SIZE];

        // Create audio sources
        for (int i = 0; i < POOL_SIZE; i++)
        {
            AudioSource source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            AudioSourcePool[i] = source;
        }
    }

    private AudioSource GetAvailableAudioSource()
    {
        if (AudioSourcePool == null)
        {
            InitializeAudioPool();
        }
        
        // First, validate all sources in the pool
        for (int i = 0; i < AudioSourcePool.Length; i++)
        {
            if (AudioSourcePool[i] == null)
            {
                AudioSourcePool[i] = gameObject.AddComponent<AudioSource>();
                AudioSourcePool[i].playOnAwake = false;
            }
        }

        // Look for non-playing source
        foreach (AudioSource source in AudioSourcePool)
        {
            if (!source.isPlaying)
            {
                return source;
            }
        }

        // Look for oldest non-looping source
        AudioSource oldestSource = null;
        float oldestStartTime = float.MaxValue;

        foreach (AudioSource source in AudioSourcePool)
        {
            if (!source.loop && source.time < oldestStartTime)
            {
                oldestStartTime = source.time;
                oldestSource = source;
            }
        }

        if (oldestSource != null)
        {
            return oldestSource;
        }

        // Create temporary source if needed
        AudioSource tempSource = gameObject.AddComponent<AudioSource>();
        tempSource.playOnAwake = false;
        return tempSource;
    }

    public void Play(string clipName)
    {
        if (!AudioClipLookup.ContainsKey(clipName))
        {
            Debug.LogWarning($"Audio key '{clipName}' not found in the audio dictionary!");
            return;
        }

        AudioSource source = GetAvailableAudioSource();
        ATrinityAudioClip audio = AudioClipLookup[clipName];
        ConfigureAudioSource(source, audio);
        source.loop = false;
        source.Play();

        // If this was a temporary source (created when pool was full of looping sounds)
        if (!AudioSourcePool.Contains(source))
        {
            Destroy(source);
        }
    }

    public void PlayWithVolume(string clipName, float clipVolume)
    {
        if (!AudioClipLookup.ContainsKey(clipName))
        {
            Debug.LogWarning($"Audio key '{clipName}' not found in the audio dictionary!");
            return;
        }

        AudioSource source = GetAvailableAudioSource();
        ATrinityAudioClip audio = AudioClipLookup[clipName];
        ConfigureAudioSource(source, audio);
        source.volume = Mathf.Clamp(clipVolume, 0f, 1f);
        source.loop = false;
        source.Play();

        // If this was a temporary source (created when pool was full of looping sounds)
        if (!AudioSourcePool.Contains(source))
        {
            Destroy(source);
        }
    }

    public void StartLoop(string clipName)
    {
        if (ActiveLoopingSounds.ContainsKey(clipName))
        {
            Debug.LogWarning($"'{clipName}' is already looping!");
            return;
        }

        if (!AudioClipLookup.ContainsKey(clipName))
        {
            Debug.LogWarning($"Audio key'{clipName}' not found in the audio dictionary!");
            return;
        }

        AudioSource source = GetAvailableAudioSource();
        ATrinityAudioClip audio = AudioClipLookup[clipName];
        ConfigureAudioSource(source, audio);
        source.loop = true;
        source.Play();

        ActiveLoopingSounds[clipName] = source;
    }

    public void StopLoop(string clipName)
    {
        if (!ActiveLoopingSounds.ContainsKey(clipName))
        {
            Debug.LogWarning($"No looping sound found for '{clipName}'!");
            return;
        }

        AudioSource source = ActiveLoopingSounds[clipName];
        source.Stop();
        source.loop = false;
        ActiveLoopingSounds.Remove(clipName);

        // If this was a temporary source (created when pool was full of looping sounds)
        if (!AudioSourcePool.Contains(source))
        {
            Destroy(source);
        }
    }

    private void ConfigureAudioSource(AudioSource source, ATrinityAudioClip audio)
    {
        source.clip = audio.Audio;
        source.volume = Mathf.Clamp(audio.Volume, 0f, 1f);
        source.pitch = Mathf.Clamp(audio.Pitch, 0f, 2f);
        source.outputAudioMixerGroup = ATrinityGameManager.GetAudioMixerGroup(audio.MixerGroup);

        switch (audio.MixerGroup)
        {
            case EAudioGroup.EAG_SFX:
                source.spatialBlend = 1f;
                break;
            case EAudioGroup.EAG_UI:
            case EAudioGroup.EAG_BGM:
            case EAudioGroup.EAG_AMBIENCE:
                source.spatialBlend = 0f;
                break;
        }
    }

    public void PlayAtPosition(string clipName, Transform transform)
    {
        if (!AudioClipLookup.ContainsKey(clipName))
        {
            Debug.LogWarning($"SFX key '{clipName}' not found in the audio dictionary!");
            return;
        }

        GameObject newAudioSourceObj = new GameObject();
        newAudioSourceObj.AddComponent<AudioSource>();
        newAudioSourceObj.transform.position = transform.position;
        newAudioSourceObj.transform.rotation = transform.rotation;
        newAudioSourceObj.transform.localScale = transform.localScale;

        AudioSource source = newAudioSourceObj.GetComponent<AudioSource>();

        ATrinityAudioClip audio = AudioClipLookup[clipName];

        ConfigureAudioSource(source, audio);

        source.Play();
        Destroy(newAudioSourceObj, source.clip.length + .1f);
    }
}