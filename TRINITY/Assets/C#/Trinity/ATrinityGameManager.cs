using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;


public class ATrinityGameManager : MonoBehaviour
{
    public static event Action<List<IEnemyController>> OnNewEnemies;
    private static ATrinityFSM PlayerFSM;
    private static ATrinityController PlayerController;
    private static ATrinitySpells SpellsReference;
    private static ATrinityBrain BrainReference;
    private static List<IEnemyController> EnemyControllers;
    private static APlayerInput InputReference;
    private static ATrinityAnimator AnimationReference;
    private static ATrinityCamera CameraReference;

    void Awake()
    {
        List<ATrinityGameManager> CurrentInstances = FindObjectsOfType<ATrinityGameManager>().ToList();
        EnemyControllers = new List<IEnemyController>();
        
        if (CurrentInstances.Count() > 1)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
    }
    // Start is called before the first frame update
    void Start()
    {
        EditorApplication.playModeStateChanged += OnPlay;
        SceneManager.sceneLoaded += NewScene;
        SceneManager.sceneUnloaded += CloseScene;
    }

    // Update is called once per frame
    void Update()
    {
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
    
    public void NewScene(Scene newScene, LoadSceneMode mode)
    {
        if (EnemyControllers.Count == 0)
        {
            EnemyControllers = FindObjectsOfType<IEnemyController>().ToList();
            OnNewEnemies?.Invoke(EnemyControllers);
        }
    }

    public void CloseScene(Scene closedScene)
    {
        EnemyControllers.Clear();
    }
    
    
    private void OnPlay(PlayModeStateChange playState)
    {
        if (playState == PlayModeStateChange.EnteredPlayMode)
        {
            if (EnemyControllers.Count == 0)
            {
                EnemyControllers = FindObjectsOfType<IEnemyController>().ToList();
                OnNewEnemies?.Invoke(EnemyControllers);
            }
        }
        else if (playState == PlayModeStateChange.ExitingPlayMode)
        {
            
        }
    }
}
