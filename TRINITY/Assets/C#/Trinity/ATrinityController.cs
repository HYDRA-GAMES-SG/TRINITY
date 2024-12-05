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
    
    [SerializeField] 
    public float GroundDistance = .1f;
    
    [SerializeField]
    public float Height; // => Collider.bounds.extents.y;

    [SerializeField] public float HorizontalRotationSpeed = 5f;

    [Header("Look/Camera Rotation Vertical")]
    public GameObject LookTarget;

    private float VerticalRotation;

    [HideInInspector]
    public CapsuleCollider Collider;
    
    [HideInInspector]
    public Rigidbody RB;

    [SerializeField] public float MaxStableAngle = 50f;

    // Movement Variables
    [HideInInspector]
    public Vector3 MoveDirection;

    [HideInInspector] public Vector3 Position => transform.position;
    [HideInInspector] public Vector3 Forward => transform.forward;
    [HideInInspector] public Vector3 Right => transform.right;
    [HideInInspector] public Vector3 Rotation => transform.rotation.eulerAngles;

    [HideInInspector] public float VerticalVelocity;
    
    [HideInInspector] public Vector3 PlanarVelocity => new Vector3(RB.velocity.x, 0f, RB.velocity.z);
    
    [HideInInspector]
    public bool bForcefieldActive;


    private APlayerInput InputReference;
    private ATrinitySpells SpellsReference;
    private ATrinityBrain BrainReference;
    private ATrinityCamera CameraReference;
    
    

    private void Awake()
    {
        InputReference = transform.parent.Find("Brain").GetComponent<APlayerInput>();
        SpellsReference = transform.parent.Find("Spells").GetComponent<ATrinitySpells>();
        BrainReference = transform.parent.Find("Brain").GetComponent<ATrinityBrain>();
        CameraReference = transform.Find("Camera").GetComponent<ATrinityCamera>();

        
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
        }
    }

    private void Update()
    {
        ApplyRotation();
        ApplyVerticalRotationToLookTarget();
        ZeroLookAtRoll();
    }

    private void ZeroLookAtRoll()
    {
        Vector3 lookRot = LookTarget.transform.rotation.eulerAngles;
        lookRot.z = 0f;
        LookTarget.transform.rotation = Quaternion.Euler(lookRot);
    }

    private void ApplyVerticalRotationToLookTarget()
    {
        // Adjust the vertical rotation based on input
        VerticalRotation -= InputReference.CameraInput.y * CameraReference.VerticalRotationSpeed * Time.deltaTime;

        // Clamp the vertical rotation to the allowed range
        VerticalRotation = Mathf.Clamp(VerticalRotation, CameraReference.VerticalClampMin, CameraReference.VerticalClampMax);

        // Apply the vertical rotation to LookTarget's pitch (x-axis)
        Vector3 lookTargetRotation = LookTarget.transform.localEulerAngles;
        lookTargetRotation.x = VerticalRotation;
        LookTarget.transform.localEulerAngles = lookTargetRotation;
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

    void ApplyRotation()
    {
        transform.Rotate(Vector3.up, InputReference.CameraInput.x * HorizontalRotationSpeed * Time.deltaTime);
        //LookTarget.transform.Rotate(Vector3.up, InputReference.CameraInput.x * xRotationSpeed * Time.deltaTime);
    }

    public void ApplyDamage(float damageNumber)
    {
        float remainingDamage = damageNumber;
        float remainingMana = SpellsReference.ManaComponent.Current;
        float remainingHealth = HealthComponent.Current;

        if (bForcefieldActive)
        {
            if (remainingMana >= remainingDamage / SpellsReference.Forcefield.DamageAbsorbedPerMana)
            {
                // Deduct all damage from mana
                SpellsReference.ManaComponent.Modify(-remainingDamage / SpellsReference.Forcefield.DamageAbsorbedPerMana);
                remainingDamage = 0;
            }
            else
            {
                // Deduct as much as possible from mana
                SpellsReference.ManaComponent.Modify(-remainingMana / SpellsReference.Forcefield.DamageAbsorbedPerMana);
                remainingDamage -= remainingMana;
            }
        }

        if (remainingDamage > 0)
        {
            // Apply any remaining damage to health
            HealthComponent.Modify(-remainingDamage);
        }
    }

    public void EnableRagdoll()
    {
        // Disable the Animator to stop animations
        Animator AnimatorComponent = transform.Find("Graphics").GetComponent<Animator>();
        
        AnimatorComponent.enabled = false;

        RB.isKinematic = false;
    }
}
