using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

#if UNITY_EDITOR 
using UnityEditor;
#endif

using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;


public class ATrinityGameManager : MonoBehaviour
{
    
    [SerializeField] 
    static private AudioMixerGroup SFX_MixerGroup;
    [SerializeField] 
    static private AudioMixerGroup UI_MixerGroup;
    [SerializeField] 
    static private AudioMixerGroup BGM_MixerGroup;
    [SerializeField] 
    static private AudioMixerGroup Ambience_MixerGroup;
    
    public static float MOUSE_SENSITIVITY = .5f;
    public static float GAMEPAD_SENSITIVITY = .5f;
    public static float MASTER_VOLUME = 1f;
    public static float SFX_VOLUME = 1f;
    public static float UI_VOLUME = 1f;
    public static float BGM_VOLUME = 1f;
    public static float AMBIENCE_VOLUME = 1f;
    public static bool CROSSHAIR_ENABLED = true;

    private static ATrinityAudio AudioReference;
    private static ATrinityFSM PlayerFSM;
    private static ATrinityController PlayerController;
    private static ATrinitySpells SpellsReference;
    private static ATrinityBrain BrainReference;
    private static List<IEnemyController> EnemyControllers;
    private static ATrinityInput InputReference;
    private static ATrinityAnimator AnimationReference;
    private static ATrinityCamera CameraReference;
    private static ATrinityGUI GUIReference;
    
    private static EGameFlowState GameFlowState;
    

    void Awake()
    {
        EnemyControllers = new List<IEnemyController>();
        List<ATrinityGameManager> CurrentInstances = FindObjectsOfType<ATrinityGameManager>().ToList();
        
        if (CurrentInstances.Count() > 1)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
        
        EnemyControllers = FindObjectsOfType<IEnemyController>().ToList();
    }

    private void TriggerBulletTime()
    {
        
    }

    // Start is called before the first frame update
    void Start()
    {
        CROSSHAIR_ENABLED = PlayerPrefs.GetInt("bCrossHairEnabled", 1) > 0 ? true : false;
        MOUSE_SENSITIVITY = PlayerPrefs.GetFloat("MouseSensitivity", MOUSE_SENSITIVITY);
        GAMEPAD_SENSITIVITY = PlayerPrefs.GetFloat("GamepadSensitivity", GAMEPAD_SENSITIVITY);
        MASTER_VOLUME = PlayerPrefs.GetFloat("MasterVolume", MASTER_VOLUME);

        if (SceneManager.GetActiveScene().name == "PORTAL")
        {
            SetGameFlowState(EGameFlowState.MAIN_MENU);
        }
        else
        {
            SetGameFlowState(EGameFlowState.PLAY);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (GameFlowState != EGameFlowState.DEAD)
        {
            if (PlayerController && PlayerController.HealthComponent.bDead)
            {
                SetGameFlowState(EGameFlowState.DEAD);
            }
        }
        
    }

    public static ATrinityAudio GetAudio()
    {
        return AudioReference;
    }

    public static AudioMixerGroup GetAudioMixerGroup(EAudioGroup group)
    {
        switch (group)
        {
            case EAudioGroup.EAG_UI:
                return UI_MixerGroup;
                break;
            case EAudioGroup.EAG_BGM:
                return BGM_MixerGroup;
                break;
            case EAudioGroup.EAG_SFX:
                return SFX_MixerGroup;
                break;
            case EAudioGroup.EAG_AMBIENCE:
                return Ambience_MixerGroup;
                break;
            default:
                return SFX_MixerGroup;
        }
    }
    public static EGameFlowState GetGameFlowState()
    {
        return GameFlowState;
    }
    
    public static ATrinityFSM GetPlayerFSM()
    {
        return PlayerFSM;
    }
    
    public static ATrinityCamera GetCamera()
    {
        return CameraReference;
    }
    public static ATrinityBrain GetBrain()
    {
        return BrainReference;
    }

    public static ATrinityController GetPlayerController()
    {
        return PlayerController;
    }

    public static ATrinitySpells GetSpells()
    {
        return SpellsReference;
    }

    public static ATrinityInput GetInput()
    {
        return InputReference;
    }
    public static ATrinityGUI GetGUI()
    {
        return GUIReference;
    }

    
    public static List<IEnemyController> GetEnemyControllers()
    {
        return EnemyControllers;
    }

    public static ATrinityAnimator GetAnimator()
    {
        return AnimationReference;
    }

    public static void SetPlayerFSM(ATrinityFSM playerFSM)
    {
        if (PlayerFSM != null)
        {
            Debug.Log("Static player FSM Not Null");
            return;
        }
        
        PlayerFSM = playerFSM;  
    }
    
    public static void SetCamera(ATrinityCamera camera)
    {
        if (CameraReference != null)
        {
            Debug.Log("Static Camera Ref Not Null");
            return;
        }
        
        CameraReference = camera;  
    }

    public static void SetSFX(ATrinityAudio audio)
    {
        if (AudioReference != null)
        {
            Debug.Log("SFX ref not null");
            return;
        }

        AudioReference = audio;
    }
    
    public static void SetGUI(ATrinityGUI gui)
    {
        if (GUIReference != null)
        {
            Debug.Log("GUI Ref not null");
            return;
        }

        GUIReference = gui;
    }
    
    public static void SetPlayerController(ATrinityController player)
    {
        if (PlayerController != null)
        {
            Debug.Log("Static Player Ref Not Null");
            return;
        }
        
        PlayerController = player;
    }
    
    public static void SetInput(ATrinityInput input)
    {
        if (InputReference != null)
        {
            Debug.Log("Static Input Ref Not Null");
            return;
        }

        InputReference = input;
    }
    
    public static void SetAnimator(ATrinityAnimator animation)
    {
        if (AnimationReference != null)
        {
            Debug.Log("Static Animation Ref Not Null");
            return;
        }

        AnimationReference = animation;
    }
    
    public static void SetBrain(ATrinityBrain brain)
    {
        if (BrainReference != null)
        {
            Debug.Log("Static Brain Ref Not Null");
            return;
        }
        
        BrainReference = brain;
    }

    public static void SetSpells(ATrinitySpells spells)
    {
        if (SpellsReference != null)
        {
            Debug.Log("Static Spells Ref Not Null");
            return;
        }
        
        SpellsReference = spells;
    }

    public static void SetGameFlowState(EGameFlowState newGameFlowState)
    {
        if (newGameFlowState != GetGameFlowState())
        {
            Debug.Log(GetGameFlowState() + " -> " + newGameFlowState);
        }
        
        GameFlowState = newGameFlowState;
    }

    public static void SerializeSettings(FGameSettings newSettings)
    {
        CROSSHAIR_ENABLED = newSettings.bCrossHairEnabled;
        MOUSE_SENSITIVITY = newSettings.MouseSensitivity;
        GAMEPAD_SENSITIVITY = newSettings.GamepadSensitivity;
        MASTER_VOLUME = newSettings.MasterVolume;
        
        PlayerPrefs.SetInt("bCrossHairEnabled", CROSSHAIR_ENABLED ? 1 : 0);
        PlayerPrefs.SetFloat("MouseSensitivity", MOUSE_SENSITIVITY);
        PlayerPrefs.SetFloat("GamepadSensitivity", GAMEPAD_SENSITIVITY);
        PlayerPrefs.SetFloat("MasterVolume", MASTER_VOLUME);
    }
}
