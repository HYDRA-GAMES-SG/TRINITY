using UnityEngine;
using UnityEngine.Audio;

public class ATrinityAudio : IAudioManager
{
    private ATrinityInput Input;
    public AudioMixer Mixer;
    
    private void Awake()
    {
        ATrinityGameManager.SetAudio(this);
        Input = ATrinityGameManager.GetInput();
        ATrinityGameManager.OnGameFlowStateChanged += BindPlayerAudio;
    }

    private void BindPlayerAudio(EGameFlowState newGameFlowState)
    {
        if (newGameFlowState == EGameFlowState.PLAY)
        {
            Bind();
        }
        else
        {
            Unbind();
        }
    }

    
    private void OnEnable()
    {
        // Call base class OnEnable
        base.OnEnable();
        
        // Rebind if needed based on current game state
        if (ATrinityGameManager.GetGameFlowState() == EGameFlowState.PLAY)
        {
            Bind();
        }
    }

    private void OnDisable()
    {
        Unbind();
    }
    
    void Update()
    {
        
    }

    void OnDestroy()
    {
        ATrinityGameManager.OnGameFlowStateChanged -= BindPlayerAudio;
        Unbind();
    }

    public void Unbind()
    {
        
        ATrinityController.OnJump -= PlayJump;
        ATrinityController.OnLand -= PlayLand;
        ATrinityController.OnGlideEnd -= EndGlideLoop;
        ATrinityController.OnGlideStart -= PlayGlideLoop;
        ATrinityController.OnTerrainCollision -= PlayTerrainCollision;
        ATrinityController.OnDeath -= PlayDeath;
        ATrinityController.OnJump -= PlayJumpGrunt;
        ATrinityController.OnDeath -= PlayGameOver;
        ATrinityController.OnBeginFalling -= PlayBeginFalling;
    }

    public void Bind()
    {
        
        ATrinityController.OnJump += PlayJump;
        ATrinityController.OnLand += PlayLand;
        ATrinityController.OnGlideEnd += EndGlideLoop;
        ATrinityController.OnGlideStart += PlayGlideLoop;
        ATrinityController.OnTerrainCollision += PlayTerrainCollision;
        ATrinityController.OnDeath += PlayDeath;
        ATrinityController.OnJump += PlayJumpGrunt;
        ATrinityController.OnDeath += PlayGameOver;
        ATrinityController.OnBeginFalling += PlayBeginFalling;
    }

    // ui
    public void PlayOptionsMenuSlider() => Play("OptionsSlider");
    public void PlayOptionsMenuToggle() => Play("OptionsToggle");
    public void PlayOptionsMenuButton() => Play("OptionsButton");
    public void PlayOptionsMenuNavigate() => Play("OptionsNavigate");
    public void PlayMainMenuNavigate() => Play("MainMenuNavigate");
    public void PlayMainMenuSelect() => Play("MainMenuSelect");

    // sfx
    void PlayJump() => Play("Jump");
    void PlayLand(float verticalVelocity) => PlayWithVolume("Land", Mathf.Clamp01(verticalVelocity / 14f));
    void PlayGlideLoop() => StartLoop("GlideLoop");
    void EndGlideLoop() => StopLoop("GlideLoop");
    void PlayTerrainCollision() => Play("TerrainCollision");
    void PlayDeath() => Play("Death");
    void PlayJumpGrunt() => Play("JumpGrunt");
    void PlayGameOver() => Play("GameOver");
    void PlayBeginFalling() => Play("BeginFalling");
}