
public enum EAudioGroup
{
    EAG_SFX,
    EAG_UI,
    EAG_BGM,
    EAG_AMBIENCE
}

public enum EGameFlowState
{
    PLAY,
    PAUSED,
    DEAD,
    MAIN_MENU
}

public enum EAilmentType
{
    EAT_Ignite,
    EAT_Charge,
    EAT_Chill,
    EAT_None
}

public enum ELightningTotemStatus
{
    ELTS_Summoning,
    ELTS_Summoned,
    ELTS_Enraged,
    ELTS_Unsummoned
}


public enum ERatMovementState
{
    ERMS_Dead,
    ERMS_Reposition,
    ERMS_Idle,
    ERMS_Pursue,
    ERMS_Attack
}

public enum ETrinityMovement
{
    ETM_Grounded,
    ETM_Jumping,
    ETM_Falling,
    ETM_Gliding
}

public enum ETrinityAction 
{
    ETA_None,
    ETA_Stunned,
    ETA_IFrame,
    ETA_Casting,
    ETA_Channeling
}

public enum ETrinityElement 
{
    ETE_Fire,
    ETE_Cold,
    ETE_Lightning,
}

public enum ESpellType
{
    EST_Primary,
    EST_Secondary,
    EST_Utility
}

public enum EMainMenu
{
    EMM_Start,
    EMM_Quit,
    EMM_Options
}