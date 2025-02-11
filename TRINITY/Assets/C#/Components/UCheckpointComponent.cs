using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class UCheckpointComponent : MonoBehaviour
{
    [Header("Spin Settings")]
    public float SpinSpeed = 100f;
    public Vector3 SpinAxis = Vector3.up;

    [Header("Float Settings")]
    public float FloatSpeed = 2f;
    public float FloatAmplitude = 0.5f;
    private Vector3 StartPosition;
    private float TimeOffset;

    private void Start()
    {
        StartPosition = transform.position;
        TimeOffset = Random.Range(0f, Mathf.PI * 2); // Random starting phase
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