using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class ATrinityAudio : IAudioManager
{
    public AudioMixer Mixer;
    // Start is called before the first frame update

    private void Awake()
    {
        ATrinityGameManager.SetAudio(this);
    }

    void Start()
    {        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
