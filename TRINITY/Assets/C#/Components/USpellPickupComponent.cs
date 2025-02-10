using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class USpellPickupComponent : MonoBehaviour
{
    [Header("Spin Settings")]
    public float SpinSpeed = 100f;
    public Vector3 SpinAxis = Vector3.up;

    [Header("Float Settings")]
    public float FloatSpeed = 2f;
    public float FloatAmplitude = 0.5f;
    
    [Header("Appearance")]
    public Sprite PickupSprite;

    private MeshRenderer MeshRenderer;
    private Vector3 StartPosition;
    private float TimeOffset;

    private void Start()
    {
        MeshRenderer = GetComponent<MeshRenderer>();
        StartPosition = transform.position;
        TimeOffset = Random.Range(0f, Mathf.PI * 2); // Random starting phase

        if (PickupSprite != null)
        {
            // Create a material instance to avoid modifying the shared material
            Material materialInstance = new Material(MeshRenderer.material);
            materialInstance.mainTexture = PickupSprite.texture;
            MeshRenderer.material = materialInstance;
        }
        else
        {
            Debug.LogWarning("No sprite assigned to USpellPickupComponent on " + gameObject.name);
        }
    }

    private void Update()
    {
        // Handle spinning
        transform.Rotate(SpinAxis, SpinSpeed * Time.deltaTime);

        // Handle floating motion
        float yOffset = Mathf.Sin((Time.time + TimeOffset) * FloatSpeed) * FloatAmplitude;
        transform.position = StartPosition + Vector3.up * yOffset;
    }
}