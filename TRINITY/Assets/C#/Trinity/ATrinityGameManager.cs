using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

#if UNITY_EDITOR 
using UnityEditor;
#endif

using UnityEngine;
using UnityEngine.SceneManagement;


public enum EGameFlowState
{
    PLAY,
    PAUSED,
    DEAD
        
}

public class ATrinityGameManager : MonoBehaviour
{
    public static float MOUSE_SENSITIVITY = .5f;
    public static float GAMEPAD_SENSITIVITY = .5f;
    public static float MASTER_VOLUME = 1f;
    public static bool CROSSHAIR_ENABLED = true;
    private static ATrinityFSM PlayerFSM;
    private static ATrinityController PlayerController;
    private static ATrinitySpells SpellsReference;
    private static ATrinityBrain BrainReference;
    private static List<IEnemyController> EnemyControllers;
    private static APlayerInput InputReference;
    private static ATrinityAnimator AnimationReference;
    private static ATrinityCamera CameraReference;
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

    public static APlayerInput GetInput()
    {
        return InputReference;
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
    public static void SetPlayerController(ATrinityController player)
    {
        if (PlayerController != null)
        {
            Debug.Log("Static Player Ref Not Null");
            return;
        }
        
        PlayerController = player;
    }
    
    public static void SetInput(APlayerInput input)
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
        GameFlowState = newGameFlowState;
    }
}
