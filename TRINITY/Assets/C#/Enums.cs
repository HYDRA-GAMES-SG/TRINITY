
public enum EAudioGroup
{
    EAG_SFX,
    EAG_UI,
    EAG_BGM,
    EAG_AMBIENCE
}

public enum ETrinityScore
{
    ETS_S_PlusPlusPlus = 20,
    ETS_S_PlusPlus = 19,
    ETS_S_Plus = 18,
    ETS_S = 17,
    ETS_S_Minus = 16,
    ETS_A_Plus = 15,
    ETS_A = 14,
    ETS_A_Minus = 13,
    ETS_B_Plus = 12,
    ETS_B =11,
    ETS_B_Minus = 10,
    ETS_C_Plus = 9,
    ETS_C = 8,
    ETS_C_Minus = 7,
    ETS_D_Plus = 6,
    ETS_D = 5,
    ETS_D_Minus = 4,
    ETS_F_Plus = 3,
    ETS_F = 2,
    ETS_F_Minus = 1,
    ETS_F_MinusMinus = 0
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