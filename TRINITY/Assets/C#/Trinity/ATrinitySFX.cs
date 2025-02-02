using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ATrinitySFX : MonoBehaviour
{
    [SerializeField]
    private List<string> AudioClipKeys;
    [SerializeField]
    private List<AudioClip> AudioClips;

    private Dictionary<string, AudioClip> KeyToAudioMap;
    private AudioSource[] AudioSourcePool;
    private const int PoolSize = 4;
    
    void Start()
    {
        ATrinityGameManager.SetSFX(this);
        InitializeSFXDictionary();
        InitializeAudioPool();
    }

    private void InitializeSFXDictionary()
    {
        KeyToAudioMap = new Dictionary<string, AudioClip>();
        
        // Ensure the lists have the same length
        if (AudioClipKeys.Count != AudioClips.Count)
        {
            Debug.LogError("cannot make dictionary: AudioClipKeys and AudioClips must have the same number of elements!");
            return;
        }

        for (int i = 0; i < AudioClipKeys.Count; i++)
        {
            if (!string.IsNullOrEmpty(AudioClipKeys[i]) && AudioClips[i] != null)
            {
                KeyToAudioMap[AudioClipKeys[i]] = AudioClips[i];
            }
        }
    }

    private void InitializeAudioPool()
    {
        AudioSourcePool = new AudioSource[PoolSize];
        
        // Create audio sources
        for (int i = 0; i < PoolSize; i++)
        {
            AudioSource source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            AudioSourcePool[i] = source;
        }
    }

    private AudioSource GetAvailableAudioSource()
    {
        foreach (AudioSource source in AudioSourcePool)
        {
            if (!source.isPlaying)
            {
                return source;
            }
        }
        
        AudioSource oldestSource = AudioSourcePool[0];
        float oldestStartTime = float.MaxValue;
        
        foreach (AudioSource source in AudioSourcePool)
        {
            if (source.time < oldestStartTime)
            {
                oldestStartTime = source.time;
                oldestSource = source;
            }
        }
        
        return oldestSource;
    }

    public void Play(string sfxKey, float volume = 1.0f, float pitch = 1.0f)
    {
        if (!KeyToAudioMap.ContainsKey(sfxKey))
        {
            Debug.LogWarning($"SFX key '{sfxKey}' not found in the audio dictionary!");
            return;
        }

        AudioSource source = GetAvailableAudioSource();
        source.clip = KeyToAudioMap[sfxKey];
        source.volume = Mathf.Clamp(volume, 0f, 1f);
        source.pitch = Mathf.Clamp(pitch, 0f, 2f);
        source.Play();
    }
}