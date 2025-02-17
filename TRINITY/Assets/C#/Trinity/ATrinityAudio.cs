using System.Linq;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Windows;
using KWS;
public class ATrinityAudio : IAudioManager
{
    public KWS_InteractWithWater LeftFoot, RightFoot;
    private ATrinityInput Input;
    public AudioMixer Mixer;
    private AudioSource TrinitySource;

    private AudioClip[] GrassFootsteps;
    private AudioClip[] RockFootsteps;
    private AudioClip[] WaterFootsteps;
    private AudioClip[] SnowFootsteps;

    const string GrassResourcePath = "GrassFootsteps";
    const string RockResourcePath = "RockFootsteps";
    const string WaterResourcePath = "WaterFootsteps";
    const string SnowResourcePath = "SnowFootsteps";

    const float SPLASHVOLUME = 0.05f;
    void Awake()
    {
        TrinitySource = GetComponent<AudioSource>();
        ATrinityGameManager.SetAudio(this);
        base.Awake();

        GrassFootsteps = new AudioClip[ArrayLength(GrassResourcePath)];
        RockFootsteps = new AudioClip[ArrayLength(RockResourcePath)];
        SnowFootsteps = new AudioClip[ArrayLength(SnowResourcePath)];
        WaterFootsteps = new AudioClip[ArrayLength(WaterResourcePath)];

        InitalizeFootsteps(GrassFootsteps, GrassResourcePath);
        InitalizeFootsteps(RockFootsteps, RockResourcePath);
        InitalizeFootsteps(WaterFootsteps, WaterResourcePath);
        InitalizeFootsteps(SnowFootsteps, SnowResourcePath);
    }

    // ui
    public void PlayOptionsMenuSlider() => Play("OptionsSlider");
    public void PlayOptionsMenuToggle() => Play("OptionsToggle");
    public void PlayOptionsMenuButton() => Play("OptionsButton");
    public void PlayOptionsMenuNavigate() => Play("OptionsNavigate");
    public void PlayMainMenuNavigate() => Play("MainMenuNavigate");
    public void PlayMainMenuSelect() => Play("MainMenuSelect");

    // sfx
    public void PlayJump()
    {
        if (ATrinityGameManager.GetGameFlowState() != EGameFlowState.PLAY)
        {
            return;
        }
        int rng = Random.Range(1, 6);
        string jumpAudio = "Jump" + rng.ToString();
        print("jump");
        Play(jumpAudio);
    }
    public void PlayLand(float verticalVelocity)
    {
        if (ATrinityGameManager.GetGameFlowState() != EGameFlowState.PLAY)
        {
            return;
        }
        int rng = Random.Range(1, 3);
        string landAudio = "Land" + rng.ToString();
        PlayWithVolume(landAudio, Mathf.Clamp01(verticalVelocity / 14f));
    }
    public void PlayGlideLoop()
    {
        if (ATrinityGameManager.GetGameFlowState() != EGameFlowState.PLAY)
        {
            return;
        }
        StartLoop("GlideLoop");
    }
    public void EndGlideLoop()
    {
        if (ATrinityGameManager.GetGameFlowState() != EGameFlowState.PLAY)
        {
            return;
        }
        print("glide end");
        StopLoop("GlideLoop");
    }
    public void PlayTerrainCollision()
    {
        if (ATrinityGameManager.GetGameFlowState() != EGameFlowState.PLAY)
        {
            return;
        }

        Play("TerrainCollision");
    }
    public void PlayDeath()
    {
        if (ATrinityGameManager.GetGameFlowState() != EGameFlowState.PLAY)
        {
            return;
        }

        Play("Death");
    }
    public void PlayJumpGrunt()
    {
        if (ATrinityGameManager.GetGameFlowState() != EGameFlowState.PLAY)
        {
            return;
        }

        Play("JumpGrunt");
    }
    public void PlayGameOver()
    {
        if (ATrinityGameManager.GetGameFlowState() != EGameFlowState.PLAY)
        {
            return;
        }

        Play("GameOver");
    }
    public void PlayBeginFalling()
    {
        if (ATrinityGameManager.GetGameFlowState() != EGameFlowState.PLAY)
        {
            return;
        }

        Play("BeginFalling");
    }
    public int ArrayLength(string typeOfFootstep)
    {
        AudioClip[] footstepClips = Resources.LoadAll<AudioClip>(typeOfFootstep);
        int length = footstepClips.Length;
        return length;
    }
    public void InitalizeFootsteps(AudioClip[] stepsArray, string typeOfFootstep)
    {
        AudioClip[] footstepClips = Resources.LoadAll<AudioClip>(typeOfFootstep);
        for (int i = 0; i < footstepClips.Length; i++)
        {
            stepsArray[i] = footstepClips[i];
        }
    }
    public AudioClip RandomClip(AudioClip[] clipArray)
    {
        int rng = Random.Range(0, clipArray.Length);
        AudioClip randomClip = clipArray[rng];
        return randomClip;
    }
    public void PlayFootstep()
    {
        if (Physics.Raycast(ATrinityGameManager.GetPlayerController().transform.position, Vector3.down, out RaycastHit hitInfo))
        {
            string tagName = hitInfo.collider.gameObject.tag;
            switch (tagName)
            {
                case "Ground":
                    TrinitySource.PlayOneShot(RandomClip(GrassFootsteps));
                    break;
                case "Rock":
                    TrinitySource.volume = 1;
                    TrinitySource.PlayOneShot(RandomClip(RockFootsteps));
                    break;
                case "Snow":
                    TrinitySource.volume = 0.6f;
                    TrinitySource.PlayOneShot(RandomClip(SnowFootsteps));
                    break;
            }
            //int i = Random.Range(0, clipstoplay.Length);
            //FootstepSource.PlayOneShot(clipstoplay[i]);
        }
        if (LeftFoot.IsIntersect() || RightFoot.IsIntersect()) 
        {
            print("intersect water");
            TrinitySource.PlayOneShot(RandomClip(WaterFootsteps), SPLASHVOLUME);
        }
    }
}