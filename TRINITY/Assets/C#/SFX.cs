using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio; // Import for AudioMixerGroup

public class SFX : MonoBehaviour
{
    [Header("Pool Settings")]
    public int AudioPoolSize = 3; // Default size of the audio source pool

    [Header("Audio Mixer Settings")]
    public AudioMixerGroup MixerGroup; // The audio mixer group to route the audio through

    private AudioSource[] AudioSources; // Array of pre-allocated audio sources

    private void Awake()
    {
        // Initialize the pool with pre-allocated audio sources
        InitializePool();
    }

    private void InitializePool()
    {
        AudioSources = new AudioSource[AudioPoolSize];
        
        for (int i = 0; i < AudioPoolSize; i++)
        {
            AudioSource newSource = gameObject.AddComponent<AudioSource>();
            newSource.loop = false;
            newSource.playOnAwake = false; // Avoid auto-play on initialization
            if (MixerGroup != null)
            {
                newSource.outputAudioMixerGroup = MixerGroup; // Set the AudioMixerGroup
            }
            AudioSources[i] = newSource;
        }
    }

    /// <summary>
    /// Plays an audio clip with specified parameters.
    /// </summary>
    /// <param name="clip">The audio clip to play.</param>
    /// <param name="volume">The volume of the audio (default 1.0).</param>
    /// <param name="pitch">The pitch of the audio (default 1.0).</param>
    /// <param name="bLoop">Whether the sound should loop (default false).</param>
    public void Play(AudioClip clip = null, float volume = 1.0f, float pitch = 1.0f, bool bLoop = false)
    {
        if (!clip)
        {
            return;
        }

        // Check if there is an available audio source
        AudioSource source = GetAvailableAudioSource();

        if (source != null)
        {
            source.clip = clip;
            source.volume = volume;
            source.pitch = pitch;
            source.loop = bLoop;

            if (MixerGroup != null)
            {
                source.outputAudioMixerGroup = MixerGroup; // Ensure the AudioSource is routed to the mixer group
            }

            source.Play();
        }
        else
        {
            Debug.LogWarning("No available audio sources in the pool!");
        }
    }

    /// <summary>
    /// Retrieves the next available audio source from the pool.
    /// </summary>
    /// <returns>Returns the next available AudioSource, or null if all are in use.</returns>
    private AudioSource GetAvailableAudioSource()
    {
        for (int i = 0; i < AudioPoolSize; i++)
        {
            // Check if this source is currently not playing
            if (!AudioSources[i].isPlaying)
            {
                return AudioSources[i];
            }
        }
        return null; // Return null if all audio sources are in use
    }
}