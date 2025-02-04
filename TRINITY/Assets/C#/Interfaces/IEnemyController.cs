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

    private Rigidbody[] Rigidbodies;

    private static Transform TransformReference;

    public Transform CoreCollider;

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

    [HideInInspector]
    public IAudioManager Audio;

    private void Awake()
    {
        if (!Audio)
        {
            Audio = GetComponentInChildren<IAudioManager>();
            if (!Audio)
            {
                Debug.Log($"Audio not find on {gameObject.name}");
            }
        }

        TransformReference = transform;

        AI = GetComponent<NavMeshAgent>();
        RB = GetComponent<Rigidbody>();
        Animator = GetComponent<Animator>();
        EnemyStatus = GetComponent<UEnemyStatusComponent>();
        Rigidbodies = GetComponentsInChildren<Rigidbody>();
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

        if (Vector3.Distance(transform.position, playerController.Position) > ATrinityGameManager.GetCamera().BulletTimeDistance)
        {
            return;
        }

        Vector3 directionToPlayer = (playerController.Position - transform.position).normalized;

        // check if boss and player are facing each other
        float bossToPlayerDot = Vector3.Dot(transform.forward, directionToPlayer);
        float playerToBossDot = Vector3.Dot(playerController.Forward, -directionToPlayer);

        // use 0.85f dot product for ~30 degree cone
        if (bossToPlayerDot < 0.85f || playerToBossDot < 0.85f)
        {
            return;
        }

        OnBulletTime?.Invoke();
    }

    public void LightCameraShake(float duration = .3f)
    {

        if (ATrinityGameManager.GetPlayerFSM().CurrentState is NormalMovement normalMovement)
        {
            if (normalMovement.GetMovementState() == ETrinityMovement.ETM_Grounded)
            {
                return;  //dont send small camera shakes if the player is not on the ground
            }
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

    void AttachColliderComponentsToRB()
    {
        foreach (var r in Rigidbodies)
        {
            if (r.gameObject.GetComponent<UEnemyColliderComponent>() == null)
            {
                UEnemyColliderComponent enemyCollider = r.gameObject.AddComponent<UEnemyColliderComponent>();
            }
        }
    }

    public void DeactiveRagdoll()
    {
        foreach (var r in Rigidbodies)
        {
            r.isKinematic = true;
        }
        Animator.enabled = true;
    }

    public void ActivateRagdoll()
    {
        foreach (var r in Rigidbodies)
        {
            Collider[] colliders = r.GetComponents<Collider>();

            foreach (var collider in colliders)
            {
                collider.enabled = false;
            }
        }

    }
}
