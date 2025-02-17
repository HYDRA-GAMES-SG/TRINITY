using System;
using System.Collections.Generic;
using ThirdPersonCamera;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(UHealthComponent))]
public class ATrinityController : MonoBehaviour
{
    //events
    public static System.Action OnBeginFalling;
    public System.Action<FHitInfo> OnHit;
    public System.Action<float> OnHealthHit;
    public System.Action<FHitInfo> OnForcefieldHit;
    public static System.Action OnJump;
    public static System.Action<float> OnLand;
    public static System.Action OnGlideStart;
    public static System.Action OnGlideEnd;
    public static System.Action OnTerrainCollision;
    public static System.Action OnDeath;
    
    public bool bDebug = false;
    
    [HideInInspector]
    public UHealthComponent HealthComponent;
    
    [Header("Physics Settings")]
    [SerializeField] 
    public LayerMask GroundLayer;
    
    [SerializeField] 
    private float Gravity = 9.81f;

    public const float GRAVITY_CONSTANT = 9.81f;
    
    [SerializeField] 
    public float GroundDistance = .1f;
    
    [SerializeField]
    public float Height => Collider.bounds.extents.y;

    [HideInInspector]
    public CapsuleCollider Collider;
    
    [HideInInspector]
    public Rigidbody RB;

    [Header("Unstable Ground")]
    [SerializeField] public float MaxStableAngle = 50f;
    
    [Header("Step Up Settings")]
    [SerializeField] private float MaxStepHeight = 0.3f;
    [SerializeField] private float StepSearchOvershoot = 0.01f;
    [SerializeField] private float MaxStepUpAngle = 60f;
    [SerializeField] private LayerMask StepUpLayer;
    // Movement Variables
    [HideInInspector]
    public Vector3 MoveDirection;

    [HideInInspector] public bool bUnstable = false; 
    [HideInInspector] public Vector3 Position => transform.position;
    [HideInInspector] public Quaternion RotationQuat => transform.rotation;
    [HideInInspector] public Vector3 Up => transform.up;
    [HideInInspector] public Vector3 Forward => transform.forward;
    [HideInInspector] public Vector3 Right => transform.right;
    [HideInInspector] public Vector3 Rotation => transform.rotation.eulerAngles;
    [HideInInspector] public float VerticalVelocity => RB.velocity.y;
    [HideInInspector] public Vector3 PlanarVelocity => new Vector3(RB.velocity.x, 0f, RB.velocity.z);

    public List<GameObject> TerrainList;
    public bool bTerrainCollision => TerrainList.Count > 0;
    
    


    private void Awake()
    {
        ATrinityGameManager.SetPlayerController(this);
        TerrainList = new List<GameObject>();
        
        // Ensure required components are assigned
        Collider = GetComponent<CapsuleCollider>();
        RB = GetComponent<Rigidbody>();
        HealthComponent = GetComponent<UHealthComponent>();

        if (Collider == null)
        {
            Debug.LogError("CapsuleCollider is missing!", this);
        }
        else
        {
            Collider.radius = 0.28f;
            Collider.height = 1.75f;
            Collider.center = new Vector3(0f, .86f, 0f);
        }

        if (RB == null)
        {
            Debug.LogError("Rigidbody is missing!", this);
        }
        else
        {
            RB.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
            RB.drag = 1.5f;
        }
        
        RB.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
    }

    private void Start()
    {
        //audio events
        OnJump += ATrinityGameManager.GetAudio().PlayJump;
        OnLand += ATrinityGameManager.GetAudio().PlayLand;
        OnGlideEnd += ATrinityGameManager.GetAudio().EndGlideLoop;
        OnGlideStart += ATrinityGameManager.GetAudio().PlayGlideLoop;
        OnTerrainCollision += ATrinityGameManager.GetAudio().PlayTerrainCollision;
        OnDeath += ATrinityGameManager.GetAudio().PlayDeath;
        OnJump += ATrinityGameManager.GetAudio().PlayJumpGrunt;
        OnDeath += ATrinityGameManager.GetAudio().PlayGameOver;
        OnBeginFalling += ATrinityGameManager.GetAudio().PlayBeginFalling;
    }

    private void Update()
    {
        if (ATrinityGameManager.GetGameFlowState() == EGameFlowState.DEAD)
        {
            HandleDeath();
        }

        RemoveNullOrInactiveTerrainCollisions();
        
    }

    private void FixedUpdate()
    {
        if (ATrinityGameManager.GetGameFlowState() == EGameFlowState.PAUSED ||
            ATrinityGameManager.GetGameFlowState() == EGameFlowState.VICTORY)
        {
            return;
        }
        
        HandleGravity();
        HandleStepUp();
    }
    
    private void LateUpdate()
    {
        if (ATrinityGameManager.GetGameFlowState() != EGameFlowState.PLAY)        
        {
            return;
        }

        if (ATrinityGameManager.GetBrain().bCanRotatePlayer)
        {
            AlignWithCameraYaw();
        }

        bUnstable = IsUnstableGround();
    }

    
    private void HandleGravity()
    {
        float glideGravityModifier = 1f;
        float chargeGravityModifier = 1f;

        if (ATrinityGameManager.GetPlayerFSM().CurrentState is NormalMovement nmState)
        {
            if (nmState.GetMovementState() == ETrinityMovement.ETM_Gliding)
            {
                glideGravityModifier = nmState.GlideGravityModifier;
            }
            
            if (ATrinityGameManager.GetEnemyControllers().Count > 0)
            {
                chargeGravityModifier  = UAilmentComponent.GetChargeGlideGravityModifier();
            }
            
            if(nmState.GetMovementState() == ETrinityMovement.ETM_Falling)
            {
                chargeGravityModifier = 1f;
                glideGravityModifier = 1f;
            }

            if (ATrinityGameManager.GetPlayerFSM().Animator.GetBool("bStunned") == true)
            {
                chargeGravityModifier = 1f;
                glideGravityModifier = 2f;
            }
        }

        //Handle Glide
        Gravity = ATrinityController.GRAVITY_CONSTANT * glideGravityModifier * chargeGravityModifier;
        
        RB.AddForce(-Up * Gravity);
    }

    public RaycastHit CheckGround()
    {
        RaycastHit hit;

        bool isHit = Physics.Raycast(transform.position, Vector3.down, out hit, GroundDistance, GroundLayer, QueryTriggerInteraction.Ignore);
        
        if (isHit)
        {
            if(bDebug){Debug.Log($"Ground hit: {hit.collider.name}, Normal: {hit.normal}");}
        }
        else
        {
            if(bDebug){Debug.Log("No ground detected!");}
        }

        return hit;
    }

    public bool IsUnstableGround()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, GroundDistance, GroundLayer, QueryTriggerInteraction.Ignore))
        {
            float angle = Vector3.Angle(hit.normal, Up);
        
            if (bDebug)
            {
                Debug.Log($"Ground hit: {hit.collider.name}, Angle: {angle} degrees");
            }
        
            if (angle > MaxStableAngle)
            {
                return true;
            }
        }
        return false;
    }
    
    private void HandleStepUp()
    {
        Vector3 moveDir = PlanarVelocity.normalized;
        
        // Cast a ray forward to detect potential steps
        RaycastHit lowHit;
        if (!Physics.Raycast(transform.position, moveDir, out lowHit, 
            Collider.radius + StepSearchOvershoot, StepUpLayer))
        {
            return; // No obstacle detected
        }

        // Check if the angle is too steep
        float obstacleAngle = Vector3.Angle(lowHit.normal, Up);
        if (obstacleAngle > MaxStepUpAngle)
        {
            //print("too steep");
            return; // Too steep to step up
        }

        // Cast a ray from above to find the top of the step
        Vector3 highOrigin = transform.position + Up * MaxStepHeight;
        RaycastHit highHit;
        if (!Physics.Raycast(highOrigin, moveDir, out highHit, Collider.radius + StepSearchOvershoot, StepUpLayer, QueryTriggerInteraction.Ignore))
        {
            ///print("no upper surface");
            return; // No upper surface found
        }

        // Cast down to find the landing point
        Vector3 targetPoint = highHit.point + moveDir * Collider.radius;
        RaycastHit downHit;
        if (!Physics.Raycast(targetPoint + Up * MaxStepHeight, -Up, out downHit, MaxStepHeight, StepUpLayer, QueryTriggerInteraction.Ignore))
        {
            //print("no valid landing point");
            return; // No valid landing point
        }

        // Check if the step height is within our limit
        float stepHeight = downHit.point.y - transform.position.y;
        if (stepHeight > MaxStepHeight)
        {
            //print("step too high);");
            return; // Step is too high
        }

        // Move the character up the step
        Vector3 targetPos = downHit.point;
        RB.position = new Vector3(RB.position.x, targetPos.y, RB.position.z);
        
        if (bDebug)
        {
            Debug.DrawLine(transform.position, lowHit.point, Color.red);
            Debug.DrawLine(highOrigin, highHit.point, Color.green);
            Debug.DrawLine(targetPoint + Up * MaxStepHeight, downHit.point, Color.blue);
        }
    }
    
    private void RemoveNullOrInactiveTerrainCollisions()
    {
        List<GameObject> ToRemove = new List<GameObject>();
        foreach (GameObject go in TerrainList)
        {
            if (go == null)
            {
                ToRemove.Add(go);
            }
            else
            {
                if (go.GetComponent<Collider>().enabled == false)
                {
                    ToRemove.Add(go);
                }
            }
        }

        foreach (GameObject go in ToRemove)
        {
            TerrainList.Remove(go);
        }
    }
    
    private void OnDrawGizmos()
    {
        if (bDebug)
        {
            Gizmos.color = Color.red;
            Vector3 rayOrigin = transform.position;

            // Draw the ground-check raycast
            Gizmos.DrawLine(rayOrigin, rayOrigin + Vector3.down * GroundDistance);
            Gizmos.DrawLine(rayOrigin, rayOrigin + Forward * 2f);
            //Gizmos.DrawSphere(rayOrigin + Vector3.down * GroundDistance, 0.01f);

            Gizmos.color = Color.yellow;
            Vector3 stepCheckStart = transform.position + Forward * Collider.radius;
            Vector3 stepCheckEnd = stepCheckStart + Up * MaxStepHeight;
            Gizmos.DrawLine(stepCheckStart, stepCheckEnd);
        }
    }

    void AlignWithCameraYaw()
    {
        Vector3 cameraYaw = new Vector3(0f, ATrinityGameManager.GetCamera().transform.rotation.eulerAngles.y, 0f);
        transform.rotation = Quaternion.Euler(cameraYaw);
    }

    public void ClearTerrainList()
    {
        TerrainList.Clear();
    }
    
    public void ApplyHit(FHitInfo hitInfo)
    {
        if (ATrinityGameManager.GetPlayerController().HealthComponent.bInvulnerable == true)
        {
            return;
        }
        ATrinitySpells spellReference = ATrinityGameManager.GetSpells(); 
        float remainingDamage = hitInfo.Damage;
        float remainingMana = spellReference.ManaComponent.Current;
        float remainingHealth = HealthComponent.Current;

        if (ATrinityGameManager.GetBrain().bForcefieldActive)
        {
            if (remainingMana >= remainingDamage / spellReference.Forcefield.DamageAbsorbedPerMana)
            {
                // Deduct all damage from mana
                spellReference.ManaComponent.Modify(-remainingDamage / spellReference.Forcefield.DamageAbsorbedPerMana);
                remainingDamage = 0;
            }
            else
            {
                // Deduct as much as possible from mana
                spellReference.ManaComponent.Modify(-remainingMana / spellReference.Forcefield.DamageAbsorbedPerMana);
                remainingDamage -= remainingMana;
            }

            OnForcefieldHit?.Invoke(hitInfo);
            print("sparks");
        }

        if (remainingDamage > 0)
        {
            // Apply any remaining damage to health
            HealthComponent.Modify(-remainingDamage);
            OnHit?.Invoke(hitInfo);
            OnHealthHit?.Invoke(remainingDamage);
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("EnemyAttack"))
        {
            return;
        }
        
        if(other.transform.root.gameObject )
        if (other.isTrigger)
        {
            return;
        }
        
        if (other.gameObject.GetComponent<UTutorialTriggerComponent>())
        {
            return;
        }
        
        if (other.gameObject.layer == LayerMask.NameToLayer("Default")
           && (!other.gameObject.CompareTag("Ground") || !other.gameObject.CompareTag("Snow") || !other.gameObject.CompareTag("Rock")))
        {
            TerrainList.Add(other.gameObject);
            OnTerrainCollision?.Invoke();
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("EnemyAttack"))
        {
            return;
        }
        
        if (other.isTrigger)
        {
            return;
        }
        
        if (other.gameObject.GetComponent<UTutorialTriggerComponent>())
        {
            return;
        }
        
        if (other.gameObject.layer == LayerMask.NameToLayer("Default")
             && (!other.gameObject.CompareTag("Ground") || !other.gameObject.CompareTag("Snow") || !other.gameObject.CompareTag("Rock")))
        {
            TerrainList.Remove(other.gameObject);

        }
    }

    private void HandleDeath()
    {
        if (CheckGround().transform)
        {
            RB.isKinematic = true;
        }
    }

    public void ResetPlayer()
    {
        ClearTerrainList();
        HealthComponent.Reset();
        OnHealthHit += ATrinityGameManager.GetScore().AddDamageTaken;
        HealthComponent.OnHealthModified += ATrinityGameManager.GetGUI().UpdateHealthBar;
        HealthComponent.OnDeath += ATrinityGameManager.GetGUI().DisplayGameOver;

        if (ATrinityGameManager.CurrentScene == "PORTAL" && ATrinityGameManager.GetGUI().GetMainMenu().bCanSkipMainMenu)
        {
            transform.position = new Vector3(437.75f,18.75f,-32.0f);
            transform.rotation = Quaternion.Euler(new Vector3(-180, -148, -180f));
        }
    }
    
    private void OnDestroy()
    {
        HealthComponent.OnHealthModified -= ATrinityGameManager.GetGUI().UpdateHealthBar;
        HealthComponent.OnDeath -= ATrinityGameManager.GetGUI().DisplayGameOver;
        OnHealthHit -= ATrinityGameManager.GetScore().AddDamageTaken;
        
        //audio events
        OnJump -= ATrinityGameManager.GetAudio().PlayJump;
        OnLand -= ATrinityGameManager.GetAudio().PlayLand;
        OnGlideEnd -= ATrinityGameManager.GetAudio().EndGlideLoop;
        OnGlideStart -= ATrinityGameManager.GetAudio().PlayGlideLoop;
        OnTerrainCollision -= ATrinityGameManager.GetAudio().PlayTerrainCollision;
        OnDeath -= ATrinityGameManager.GetAudio().PlayDeath;
        OnJump -= ATrinityGameManager.GetAudio().PlayJumpGrunt;
        OnDeath -= ATrinityGameManager.GetAudio().PlayGameOver;
        OnBeginFalling -= ATrinityGameManager.GetAudio().PlayBeginFalling;
        
    }
    
    
    
    // public void EnableRagdoll()
    // {
    //     // Disable the Animator to stop animations
    //     Animator AnimatorComponent = transform.Find("Graphics").GetComponent<Animator>();
    //     
    //     AnimatorComponent.enabled = false;
    //
    //     RB.isKinematic = false;
    // }
}