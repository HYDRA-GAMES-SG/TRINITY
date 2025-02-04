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
        ATrinityMainMenu.OnMainMenuNavigate += PlayMainMenuNavigate;
        ATrinityMainMenu.OnMainMenuSelection += PlayMainMenuSelect;
        
        ATrinityOptions.OnOptionsMenuSlider += PlayOptionsMenuSlider;
        ATrinityOptions.OnOptionsMenuToggle += PlayOptionsMenuToggle;
        ATrinityOptions.OnOptionsMenuButton += PlayOptionsMenuButton;
        ATrinityOptions.OnOptionsMenuNavigate += PlayOptionsMenuNavigate;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void PlayOptionsMenuSlider()
    {
        Play("OptionsSlider");
    }
    
    void PlayOptionsMenuToggle()
    {
        Play("OptionsToggle");
    }
    
    void PlayOptionsMenuButton()
    {
        Play("OptionsButton");
    }

    void PlayOptionsMenuNavigate()
    {
        Play("OptionsNavigate");
    }
    
    void PlayMainMenuNavigate()
    {
        Play("MainMenuNavigate");
    }

    void PlayMainMenuSelect()
    {
        Play("MainMenuSelect");
    }

}
