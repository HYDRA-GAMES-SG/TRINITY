using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public enum EBulletAcceleration
{
    EBA_None,
    EBA_Tracking
    
}

public enum EBulletMotion
{
    EBM_InitialSpawnerForward,
    EBM_InitialSpawnerNormal,
    EBM_InitialSpawnerInverseNormal,
    EBM_EnemyForward,
    EBM_InitialPlayer,
    EBM_PredictedPlayer
}

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class ABullet : MonoBehaviour
{
    
    [Tooltip("Move when spawner is finished.")]
    public bool bWaitForSpawner;

    [Tooltip("How long the bullet should last, 0 if it should last until it's spawner is destroyed.")]
    public float Lifetime = 5f;

    [Header("Movement Settings")] 
    public float Speed;
    

    public EBulletMotion MotionType = EBulletMotion.EBM_InitialSpawnerForward;
    public EBulletAcceleration AccelType = EBulletAcceleration.EBA_None;

    private Vector3 Rotation = Vector3.zero;
    private Vector3 Velocity = Vector3.zero;
    private Vector3 Acceleration = Vector3.zero;
    
    [SerializeField]
    private bool bCanMove = true;
    
    private GameObject Spawner = null;
    private Rigidbody RB = null;

    [HideInInspector]
    public UBulletPool Pool;

    private Animator AnimationController;

    private Transform InitialSpawnerTransform;
    private Transform InitialPlayerTransform;

    public static System.Action<Vector3> OnPlayerCollide;

    // Start is called before the first frame update

    void OnEnable()
    {
        if (bWaitForSpawner)
        {
            bCanMove = false;
        }
    }
    void Awake()
    {
        AnimationController = GetComponentInChildren<Animator>();
        RB = GetComponent<Rigidbody>();
    }
    
    void Start()
    {

        
        InitialSpawnerTransform = Spawner.transform;
        InitialPlayerTransform = ATrinityManager.GetPlayer().transform;
        
        SetVelocity();
        SetInitialValues();
        SetRotation();

    }

    private void SetRotation()
    {
        transform.rotation = Quaternion.LookRotation(Velocity.normalized);
    }

    private void SetVelocity()
    {
        Vector3 direction = Vector3.zero;

        switch (MotionType)
        {
            case EBulletMotion.EBM_InitialSpawnerForward:
                if (Spawner != null)
                {
                    direction = InitialSpawnerTransform.forward;
                }
                break;
            case EBulletMotion.EBM_EnemyForward:
                if (Spawner != null)
                {
                    direction = Spawner.transform.forward;
                }
                break;
            case EBulletMotion.EBM_InitialSpawnerInverseNormal:
                if (Spawner != null)
                {
                    direction = (InitialSpawnerTransform.position - transform.position).normalized;
                }
                break;
            case EBulletMotion.EBM_InitialSpawnerNormal:
                if (Spawner != null)
                {
                    direction = (transform.position - InitialSpawnerTransform.position).normalized;
                }
                break;
            case EBulletMotion.EBM_InitialPlayer:

                    Vector3 directionToPlayer = ATrinityManager.GetPlayer().transform.position - transform.position;
                    direction = directionToPlayer.normalized;
                break;
            case EBulletMotion.EBM_PredictedPlayer:
                        // Ensure the player reference and its Rigidbody component are valid
                        Rigidbody playerRigidbody = ATrinityManager.GetPlayer().RB;
                        if (playerRigidbody != null)
                        {
                            // Calculate the distance to the player
                            float distanceToPlayer = Vector3.Distance(transform.position,
                                ATrinityManager.GetPlayer().transform.position);

                            // Calculate the time to reach the player (assuming constant speed)
                            // Note: You may need to adjust this depending on the bullet speed
                            float timeToReachPlayer = distanceToPlayer / Speed;

                            // Predict the future position of the player
                            Vector3 futurePlayerPosition = ATrinityManager.GetPlayer().transform.position +
                                                           playerRigidbody.velocity * timeToReachPlayer;

                            // Calculate the direction to the player's future position
                            Vector3 directionToPlayerInFuture = futurePlayerPosition - transform.position;

                            // Normalize the direction vector if needed
                            Vector3 normalizedDirection = directionToPlayerInFuture.normalized;

                            direction = normalizedDirection;
                }
                break;
            default:
                direction = Vector3.zero;
                break;
        }

        Velocity = direction * Speed;
    }

    public void Reset()
    {
        bCanMove = false;
        RB.velocity = Vector3.zero;
        Velocity = Vector3.zero;
        Rotation = Vector3.zero;
        Acceleration = Vector3.zero;
    }

    public void SetInitialValues()
    {
        
        // set the velocity
        // set the acceleration
        //set the rotation
    }


    public void EnableMotion()
    {
        bCanMove = true;
        if (AnimationController != null)
        {
            AnimationController.SetBool("bCanMove", true);
        }

        InitialSpawnerTransform = Spawner.transform;
        InitialPlayerTransform = ATrinityManager.GetPlayer().transform;
        SetVelocity();
    }

    // Update is called once per frame
    void Update()
    {

        //SetVelocity();
        
        if (bCanMove)
        {
            UpdateVelocity(Time.deltaTime);
        }


    }

    private void UpdateVelocity(float dt)
    {
        RB.velocity = Velocity + Acceleration * dt;
        
    }

    public void SetSpawner(GameObject go)
    {
        Spawner = go;
    }

    public void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.GetComponent<ABullet>() != null
            || other.gameObject.CompareTag("Enemy"))
        {
            return;
        }
        
        if (other.gameObject.GetComponent<ATrinityController>() != null)
        {
            OnPlayerCollide?.Invoke(other.impulse);
        }
        else
        {
            if (Pool != null)
            {
                Pool.ReturnBullet(gameObject);
            }
        }
    }
}