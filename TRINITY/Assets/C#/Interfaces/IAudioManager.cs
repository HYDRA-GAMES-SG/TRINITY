using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;

public class IAudioManager : MonoBehaviour
{
    private List<ATrinityAudioClip> AudioClips;
    private Dictionary<string, ATrinityAudioClip> AudioClipLookup;
    
    //audio pool
    private AudioSource[] AudioSourcePool;
    private const int POOL_SIZE = 4;
    
    void Start()
    {
        AudioClips = GetComponents<ATrinityAudioClip>().ToList();
        InitializeAudioClipDictionary();
        InitializeAudioPool();
    }

    private void InitializeAudioClipDictionary()
    {
        AudioClipLookup = new Dictionary<string, ATrinityAudioClip>();

        for (int i = 0; i < AudioClips.Count; i++)
        {
            if (!string.IsNullOrEmpty(AudioClips[i].name) && AudioClips[i] != null)
            {
                AudioClipLookup[AudioClips[i].name] = AudioClips[i];
            }
        }
    }

    private void InitializeAudioPool()
    {
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
    
    public void Play(string clipName)
    {
        if (!AudioClipLookup.ContainsKey(clipName))
        {
            Debug.LogWarning($"SFX key '{clipName}' not found in the audio dictionary!");
            return;
        }

        AudioSource source = GetAvailableAudioSource();
        ATrinityAudioClip audio = AudioClipLookup[clipName];
        source.clip = audio.Audio;
        source.volume = Mathf.Clamp(audio.Volume, 0f, 1f);
        source.pitch = Mathf.Clamp(audio.Pitch, 0f, 2f);
        source.outputAudioMixerGroup = ATrinityGameManager.GetAudioMixerGroup(audio.MixerGroup);

        switch(audio.MixerGroup)
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

        source.Play();
    }
}