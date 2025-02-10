using UnityEngine;
using UnityEngine.Audio;

public class ATrinityAudio : IAudioManager
{
    private ATrinityInput Input;
    public AudioMixer Mixer;

    void Awake()
    {
        ATrinityGameManager.SetAudio(this);
        base.Awake();
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
        
        Play("Jump");
    }
    public void PlayLand(float verticalVelocity)
    {
        if (ATrinityGameManager.GetGameFlowState() != EGameFlowState.PLAY)
        {
            return;
        }
        
        PlayWithVolume("Land", Mathf.Clamp01(verticalVelocity / 14f));
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
}