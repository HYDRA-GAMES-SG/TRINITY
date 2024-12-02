using UnityEngine;

public class NoisyShakeMovement : MonoBehaviour
{
    public float noiseSpeed = 1.0f; // Speed of the noise movement
    public float noiseScale = 1.0f; // Scale of the noise movement
    public Vector3 intensity = Vector3.one; // Intensity of the shake movement on each axis

    private Vector3 initialLocalPosition; // Initial local position of the object
    private float initialSeed; // Initial seed for the noise

    void Start()
    {
        // Store the initial local position of the object
        initialLocalPosition = transform.localPosition;

        // Generate a random initial seed for the noise
        initialSeed = Random.value;
    }

    void Update()
    {
        // Calculate the noise values based on time, speed, and initial seed
        float noiseX = Mathf.PerlinNoise((Time.time + initialSeed) * noiseSpeed, 0.0f);
        float noiseY = Mathf.PerlinNoise(0.0f, (Time.time + initialSeed) * noiseSpeed);
        float noiseZ = Mathf.PerlinNoise((Time.time + initialSeed) * noiseSpeed, (Time.time + initialSeed) * noiseSpeed);

        // Calculate the shake movement based on the noise values, intensity, and scale
        float shakeX = noiseX * intensity.x * noiseScale;
        float shakeY = noiseY * intensity.y * noiseScale;
        float shakeZ = noiseZ * intensity.z * noiseScale;

        // Apply the shake movement to the object's local position
        Vector3 shakeMovement = new Vector3(shakeX, shakeY, shakeZ);
        transform.localPosition = initialLocalPosition + shakeMovement;
    }
}
