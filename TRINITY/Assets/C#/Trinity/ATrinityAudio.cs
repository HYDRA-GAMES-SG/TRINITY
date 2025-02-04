using UnityEngine;
using UnityEngine.Audio;

public class ATrinityAudio : IAudioManager
{
    public AudioMixer Mixer;

    private void Awake()
    {
        ATrinityGameManager.SetAudio(this);
        BindToEvents(true);

    }

    void OnDestroy()
    {
        BindToEvents(false);
    }

    void BindToEvents(bool bBind)
    {
        if (bBind)
        {
            ATrinityMainMenu.OnMainMenuNavigate += PlayMainMenuNavigate;
            ATrinityMainMenu.OnMainMenuSelection += PlayMainMenuSelect;
        
            ATrinityOptions.OnOptionsMenuSlider += PlayOptionsMenuSlider;
            ATrinityOptions.OnOptionsMenuToggle += PlayOptionsMenuToggle;
            ATrinityOptions.OnOptionsMenuButton += PlayOptionsMenuButton;
            ATrinityOptions.OnOptionsMenuNavigate += PlayOptionsMenuNavigate;

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
        else
        {
            ATrinityMainMenu.OnMainMenuNavigate -= PlayMainMenuNavigate;
            ATrinityMainMenu.OnMainMenuSelection -= PlayMainMenuSelect;
        
            ATrinityOptions.OnOptionsMenuSlider -= PlayOptionsMenuSlider;
            ATrinityOptions.OnOptionsMenuToggle -= PlayOptionsMenuToggle;
            ATrinityOptions.OnOptionsMenuButton -= PlayOptionsMenuButton;
            ATrinityOptions.OnOptionsMenuNavigate -= PlayOptionsMenuNavigate;

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
    }

    // ui
    void PlayOptionsMenuSlider() => Play("OptionsSlider");
    void PlayOptionsMenuToggle() => Play("OptionsToggle");
    void PlayOptionsMenuButton() => Play("OptionsButton");
    void PlayOptionsMenuNavigate() => Play("OptionsNavigate");
    void PlayMainMenuNavigate() => Play("MainMenuNavigate");
    void PlayMainMenuSelect() => Play("MainMenuSelect");

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