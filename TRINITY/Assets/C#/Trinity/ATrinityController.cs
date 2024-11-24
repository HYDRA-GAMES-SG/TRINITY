using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class ATrinityController : MonoBehaviour
{
    [Header("Physics Settings")]
    [SerializeField] 
    private LayerMask GroundLayer;

    [SerializeField] 
    private float Gravity = 9.81f;
    
    [SerializeField] 
    private float GroundDistance = .1f;

    [SerializeField] 
    private bool EnableDebug = false;

    public CapsuleCollider Collider;
    
    public Rigidbody Rigidbody;

    // Movement Variables
    public Vector3 MoveDirection;
    public Vector3 Forward;
    public Vector3 Right;
    public Vector3 Rotation;

    // Velocity and Height
    public float VerticalVelocity;
    public float PlanarVelocity;
    public float Height; // => Collider.bounds.extents.y;
    

    private void Awake()
    {
        // Ensure required components are assigned
        Collider = GetComponent<CapsuleCollider>();
        Rigidbody = GetComponent<Rigidbody>();

        if (Collider == null)
        {
            Debug.LogError("CapsuleCollider is missing!", this);
        }
        else
        {
            Collider.radius = 0.5f;
            Collider.height = 1.7f;
            Collider.center = new Vector3(0f, 1f, 0f);
        }

        if (Rigidbody == null)
        {
            Debug.LogError("Rigidbody is missing!", this);
        }
        else
        {
            Rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        }
    }

    private void Update()
    {
        if (EnableDebug)
        {
            Debug.Log($"MoveDirection: {MoveDirection}, VerticalVelocity: {VerticalVelocity}, PlanarVelocity: {PlanarVelocity}");
        }

        // Placeholder for handling movement logic
        HandleMovement();
    }

    private void HandleMovement()
    {
        // Example movement logic placeholder
        Forward = transform.forward;
        Right = transform.right;

        // Combine forward and right movement based on input (example)
        MoveDirection = (Forward * Input.GetAxis("Vertical")) + (Right * Input.GetAxis("Horizontal"));

        // Gravity simulation (example)
        VerticalVelocity -= Gravity * Time.deltaTime;

        // Apply movement to Rigidbody
        Rigidbody.velocity = new Vector3(MoveDirection.x, VerticalVelocity, MoveDirection.z);
    }

    private bool IsGrounded()
    {
        // Raycast check for ground
        return Physics.Raycast(transform.position, Vector3.down, out _, Collider.bounds.extents.y + GroundDistance, GroundLayer);
    }
}
