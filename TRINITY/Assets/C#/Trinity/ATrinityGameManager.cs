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
    
    public static string CurrentScene = "UNINITIALIZED"; 
    static public System.Action OnSceneChanged;
    
    static private AudioMixerGroup SFX_MixerGroup;
    static private AudioMixerGroup UI_MixerGroup;
    static private AudioMixerGroup BGM_MixerGroup;
    static private AudioMixerGroup Ambience_MixerGroup;
    
    public static float MOUSE_SENSITIVITY = .5f;
    public static float GAMEPAD_SENSITIVITY = .5f;
    public static float MASTER_VOLUME = 1f;
    public static float SFX_VOLUME = 1f;
    public static float UI_VOLUME = 1f;
    public static float BGM_VOLUME = 1f;
    public static float AMBIENCE_VOLUME = 1f;
    public static bool CROSSHAIR_ENABLED = true;

    //singleton references
    private static ATrinityAudio AudioReference;
    private static ATrinityFSM PlayerFSM;
    private static ATrinityController PlayerController;
    private static ATrinitySpells SpellsReference;
    private static ATrinityScore ScoreReference;
    private static ATrinityBrain BrainReference;
    private static List<IEnemyController> EnemyControllers;
    private static ATrinityInput InputReference;
    private static ATrinityAnimator AnimationReference;
    private static ATrinityCamera CameraReference;
    private static ATrinityGUI GUIReference;
    
    private static EGameFlowState GameFlowState;
    
    public static bool bCanSkipMainMenu = false;

    void Awake()
    {
        List<ATrinityGameManager> CurrentInstances = FindObjectsOfType<ATrinityGameManager>().ToList();
        
        if (CurrentInstances.Count() > 1)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
        
        EnemyControllers = new List<IEnemyController>();
        CurrentScene = SceneManager.GetActiveScene().name;
    }
    
    // Start is called before the first frame update
    void Start()
    {
        if (CurrentScene == "PORTAL")
        {
            SetGameFlowState(EGameFlowState.MAIN_MENU);
        }
        else
        {
            ATrinityGameManager.bCanSkipMainMenu = true;
            SetGameFlowState(EGameFlowState.PLAY);
        }
        
        CROSSHAIR_ENABLED = PlayerPrefs.GetInt("bCrossHairEnabled", 1) > 0 ? true : false;
        MOUSE_SENSITIVITY = PlayerPrefs.GetFloat("MouseSensitivity", MOUSE_SENSITIVITY);
        GAMEPAD_SENSITIVITY = PlayerPrefs.GetFloat("GamepadSensitivity", GAMEPAD_SENSITIVITY);
        MASTER_VOLUME = PlayerPrefs.GetFloat("MasterVolume", MASTER_VOLUME);
        
        SetEnemyControllers();
        
        OnSceneChanged?.Invoke();
    }


    // Update is called once per frame
    void Update()
    {
        if (GameFlowState != EGameFlowState.DEAD)
        {
            if (PlayerController && PlayerController.HealthComponent.bDead)
            {
                SetGameFlowState(EGameFlowState.DEAD);
                return;
            }
        }

        CheckForSceneTransition();

        
    }
    
    private void CheckForSceneTransition()
    {
        if (SceneManager.GetActiveScene().name != CurrentScene)
        {
            SetEnemyControllers();
            print("ATGM => Scene Transition Detected:" + CurrentScene + " => " + SceneManager.GetActiveScene().name);
            CurrentScene = SceneManager.GetActiveScene().name;
            
            print("ATGM => OnSceneChanged!");
            OnSceneChanged?.Invoke();

            Transform spawnPoint = FindObjectOfType<ATrinitySpawn>().transform;
            Transform player = GetPlayerController().transform;
            player.position = spawnPoint.position;
            player.rotation = spawnPoint.rotation;
            player.localScale = spawnPoint.localScale;
        }
    }


    public static ATrinityScore GetScore()
    {
        if (ScoreReference)
        {
            return ScoreReference;
        }
        else
        {
            Debug.Log("ATrinityScore null on Game Manager");
            return null;
        }
    }
    
    public static ATrinityAudio GetAudio()
    {
        if (AudioReference)
        {
            return AudioReference;
        }
        else
        {
            Debug.Log("ATrinityAudio null on Game Manager");
            return null;
        }
    }

    public static AudioMixerGroup GetAudioMixerGroup(EAudioGroup group)
    {
        switch (group)
        {
            case EAudioGroup.EAG_UI:
                if(UI_MixerGroup == null)
                {
                    UI_MixerGroup = GetAudio().Mixer.FindMatchingGroups("UI")[0];
                }
                return UI_MixerGroup;
            case EAudioGroup.EAG_BGM:
                if (BGM_MixerGroup == null)
                {
                    BGM_MixerGroup = GetAudio().Mixer.FindMatchingGroups("BGM")[0];
                }
                return BGM_MixerGroup;
            case EAudioGroup.EAG_SFX:
                if (SFX_MixerGroup == null)
                {
                    SFX_MixerGroup = GetAudio().Mixer.FindMatchingGroups("SFX")[0];
                }
                return SFX_MixerGroup;
            case EAudioGroup.EAG_AMBIENCE:
                if (Ambience_MixerGroup == null)
                {
                    Ambience_MixerGroup = GetAudio().Mixer.FindMatchingGroups("AMBIENCE")[0];
                }
                return Ambience_MixerGroup;
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
    
    
    
    //==================================================
    //SETTERS
    //=================================================
    
    
    
    private void SetEnemyControllers()
    {
        EnemyControllers = FindObjectsOfType<IEnemyController>().ToList();
    }
    
    private void SetEnemyControllers(Scene arg0, Scene arg1)
    {
        EnemyControllers = FindObjectsOfType<IEnemyController>().ToList();
    }

    public static void SetScore(ATrinityScore score)
    {
        if (ScoreReference != null)
        {
            Debug.Log("Static Score Ref Not Null");
            return;
        }

        ScoreReference = score;
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

    public static void SetAudio(ATrinityAudio audio)
    {
        if (AudioReference != null)
        {
            Debug.Log("Audio ref not null");
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
