using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(UHealthComponent))]
public class ATrinityController : MonoBehaviour
{
    public bool bDebug = false;
    
    [HideInInspector]
    public UHealthComponent HealthComponent;
    
    [Header("Physics Settings")]
    [SerializeField] 
    public LayerMask GroundLayer;

    [SerializeField] 
    public float Gravity = 9.81f;

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

    // Movement Variables
    [HideInInspector]
    public Vector3 MoveDirection;

    [HideInInspector] public Vector3 Position => transform.position;
    [HideInInspector] public Quaternion RotationQuat => transform.rotation;
    [HideInInspector] public Vector3 Up => transform.up;
    [HideInInspector] public Vector3 Forward => transform.forward;
    [HideInInspector] public Vector3 Right => transform.right;
    [HideInInspector] public Vector3 Rotation => transform.rotation.eulerAngles;
    [HideInInspector] public float VerticalVelocity => RB.velocity.y;
    [HideInInspector] public Vector3 PlanarVelocity => new Vector3(RB.velocity.x, 0f, RB.velocity.z);

    public System.Action<FHitInfo> OnHit; 
    
    

    private void Awake()
    {
        ATrinityGameManager.SetPlayerController(this);
        
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
            Collider.radius = 0.5f;
            Collider.height = 1.7f;
            Collider.center = new Vector3(0f, .8f, 0f);
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
    }

    private void OnDestroy()
    {
    }

    private void Update()
    {
        if (ATrinityGameManager.GetGameFlowState() == EGameFlowState.DEAD)
        {
            HandleDeath();
        }
    }

    private void FixedUpdate()
    {
        if (ATrinityGameManager.GetGameFlowState() == EGameFlowState.PAUSED)
        {
            return;
        }
        
        HandleGravity();
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
    }

    
    private void HandleGravity()
    {
        RB.AddForce(-Up * Gravity);
    }

    public RaycastHit CheckGround()
    {
        RaycastHit hit;

        bool isHit = Physics.Raycast(transform.position, Vector3.down, out hit, GroundDistance, GroundLayer);
        
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

    public RaycastHit FindUnstableGround()
    {
        RaycastHit hit;

        bool isHit = Physics.Raycast(transform.position, Vector3.down, out hit, GroundDistance * 10f, GroundLayer);
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
    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Vector3 rayOrigin = transform.position;

        // Draw the ground-check raycast
        Gizmos.DrawLine(rayOrigin, rayOrigin + Vector3.down * GroundDistance * 10f);
        Gizmos.DrawLine(rayOrigin, rayOrigin + Forward * 2f);
        Gizmos.DrawSphere(rayOrigin + Vector3.down * GroundDistance, 0.01f);
    }

    void AlignWithCameraYaw()
    {
        Vector3 cameraYaw = new Vector3(0f, ATrinityGameManager.GetCamera().transform.rotation.eulerAngles.y, 0f);
        transform.rotation = Quaternion.Euler(cameraYaw);
    }

    public void ApplyHit(FHitInfo hitInfo)
    {
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
        }

        if (remainingDamage > 0)
        {
            // Apply any remaining damage to health
            HealthComponent.Modify(-remainingDamage);
            OnHit?.Invoke(hitInfo);
        }
    }
    
    private void HandleDeath()
    {
        RB.isKinematic = true;
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