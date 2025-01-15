using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct FCameraShakeInfo
{
    public float NormalizedIntensity;
    public float Duration;

    public FCameraShakeInfo(float normalizedIntensity, float duration)
    {
        NormalizedIntensity = normalizedIntensity;
        Duration = duration;
    }
}

public class UCameraShakeComponent : MonoBehaviour
{
    public GameObject LookAtParentObj;
    
    /// <summary>
    /// Camera Shake Intensity
    /// </summary>
    public float LookAtDisplacementMultiplier = .5f;

    public float DistanceBasedIntensityAttenuation = -.02f;
    public float DistanceBasedIntensityOffset = 1.5f;
    
    [Header("Perlin Noise")]
    public float CameraResetSpeed = 5f;
    public float PerlinNoiseSpeed = 30f; // Speed of noise sampling
    
    //shake Timer
    private float ShakeTimer = 0f;
    private bool bShaking => ShakeTimer > 0;
    
    // Perlin Noise sampling positions
    private float PerlinNoiseSeed;
    private Vector2 PerlinNoiseOffset;
    private FCameraShakeInfo CurrentShake;
    
    void Start()
    {
        // Initialize random seed for Perlin noise
        PerlinNoiseSeed = Random.Range(0f, 1000f);
        PerlinNoiseOffset = new Vector2(PerlinNoiseSeed, PerlinNoiseSeed);
    }

    private void TestShake()
    {
        System.Random random = new System.Random();
        double randomShake = random.NextDouble();
        ShakeCameraFrom(.6f, .5f, ATrinityGameManager.GetEnemyControllers()[0].transform);
    }
    

    void Update()
    {
        if (bShaking)
        {
            ShakeTimer -= Time.deltaTime;
            HandleCameraShake();
        }
        else
        {
            // Smoothly return to original rotation when shake ends
            LookAtParentObj.transform.localPosition = Vector3.Lerp(LookAtParentObj.transform.localPosition, Vector3.zero, Time.deltaTime * CameraResetSpeed);
        }
    }

    private void HandleCameraShake()
    {

        // Calculate shake intensity based on remaining time
        float shakeIntensity = (ShakeTimer / CurrentShake.Duration) * CurrentShake.NormalizedIntensity;
        
        // Sample Perlin noise for X and Y rotation
        PerlinNoiseOffset.x += Time.deltaTime * PerlinNoiseSpeed;
        PerlinNoiseOffset.y += Time.deltaTime * PerlinNoiseSpeed;
        
        // Get noise values between -1 and 1
        float xNoise = Mathf.PerlinNoise(PerlinNoiseOffset.x, PerlinNoiseSeed);
        float yNoise = Mathf.PerlinNoise(PerlinNoiseSeed, PerlinNoiseOffset.y);
        float xNoiseNormalized = (xNoise - 0.5f) * 2f;
        float yNoiseNormalized = (yNoise - 0.5f) * 2f;
        
        // Calculate rotation angles
        float xAngle = xNoiseNormalized * LookAtDisplacementMultiplier * shakeIntensity;
        float yAngle = yNoiseNormalized * LookAtDisplacementMultiplier * shakeIntensity;
        
        // Apply rotation
        LookAtParentObj.transform.localPosition = new Vector3(xAngle, yAngle, 0f);
    }

    public void ShakeCamera(float normalizedIntensity, float duration)
    {
        FCameraShakeInfo newShake = new FCameraShakeInfo(normalizedIntensity, duration);
        
        if (bShaking)
        {
            if (CurrentShake.NormalizedIntensity < newShake.NormalizedIntensity)
            {
                CurrentShake = newShake;
                ShakeTimer = CurrentShake.Duration;
            }
        }
        else
        {
            CurrentShake = newShake;
            ShakeTimer = CurrentShake.Duration;
        }
    }

    public void ShakeCameraFrom(float normalizedIntensity, float duration, Transform origin)
    {
        float distance = Vector3.Distance(ATrinityGameManager.GetPlayerController().Position, origin.position);
        
        //desmos graph
        //\min\left(-.03x+1.5,\ 1\right)
        float newNormalizedIntensity = Mathf.Clamp(DistanceBasedIntensityAttenuation * distance + DistanceBasedIntensityOffset, 0, 1) * normalizedIntensity;
        
        ShakeCamera(newNormalizedIntensity, duration);
    }
}
