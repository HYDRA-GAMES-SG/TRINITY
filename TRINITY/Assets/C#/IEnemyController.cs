using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(UEnemyStatusComponent))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(NavMeshAgent))]
public class IEnemyController : MonoBehaviour
{
    private static Transform TransformReference;
    
    public string Name = "Default";
    
    [HideInInspector]
    public Rigidbody RB;

    [HideInInspector]
    public Animator Animator;
    
    [HideInInspector]
    public UEnemyStatusComponent EnemyStatus;
    
    [HideInInspector]
    public NavMeshAgent AI;
    
    public float NormalAttack;
    

    public float AttackForce = 150f;
    
    public bool bDead => EnemyStatus.Health.bDead;

    public System.Action OnBulletTime;
    
    private void Awake()
    {
        TransformReference = transform;
        
        AI = GetComponent<NavMeshAgent>();
        RB = GetComponent<Rigidbody>();
        Animator = GetComponent<Animator>();
        EnemyStatus = GetComponent<UEnemyStatusComponent>();
    }
    
    public virtual void TriggerGetHit()
    {
        
    }

    public virtual float GetCurrentAttackDamage()
    {
        return NormalAttack;
    }

    public virtual float GetParticleAttack()
    {
        return 0f;
    }

    public void BulletTime()
    {
        //if player is not close enough to trigger bullet time  
        ATrinityController playerController = ATrinityGameManager.GetPlayerController();
        
        if(Vector3.Distance(transform.position, playerController.Position) > ATrinityGameManager.GetCamera().BulletTimeDistance)
        {
            return;
        }

        Vector3 directionToPlayer = (playerController.Position - transform.position).normalized;
    
        // check if boss and player are facing each other
        float bossToPlayerDot = Vector3.Dot(transform.forward, directionToPlayer);
        float playerToBossDot = Vector3.Dot(playerController.Forward, -directionToPlayer);

        // use 0.85f dot product for ~30 degree cone
        if(bossToPlayerDot < 0.85f || playerToBossDot < 0.85f)
        {
            return;
        }

        OnBulletTime?.Invoke();
    }
    
    public void LightCameraShake(float duration = .3f)
    {
        
        if (ATrinityGameManager.GetPlayerController().CheckGround().transform)
        {
            return;  //dont send small camera shakes if the player is not on the ground
        }

        
        ATrinityGameManager.GetCamera().CameraShakeComponent.ShakeCameraFrom(0.05f, duration, TransformReference);
    }

    public void MediumCameraShake(float duration = .5f)
    {
        ATrinityGameManager.GetCamera().CameraShakeComponent.ShakeCameraFrom(0.6f, duration, TransformReference);
    }

    public void HeavyCameraShake(float duration = 1.3f) //global
    {
        ATrinityGameManager.GetCamera().CameraShakeComponent.ShakeCamera(1f, duration);
    }
}
