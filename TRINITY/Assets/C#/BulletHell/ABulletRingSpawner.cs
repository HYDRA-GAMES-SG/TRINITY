using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ABulletRingSpawner : IBulletSpawner
{
    [Header("Spawner Settings")]
    
    [Tooltip("Radius of the ring.")] 
    public float RingRadius = 5f;

    [Tooltip("Radius change after each object is spawned.")]
    public float RadiusModifier = 0f;

    [Tooltip(
        "Size of the ring in Euler angles, 360 will be a full ring. Values less than 360 will create arcs, values greater will produce more revolutions.")]
    public int RingAngle = 360;

    [Tooltip("Should the ring reorient itself towards the player.")]
    public bool AimRing = false;
    
    private float Radius; //we have a private radius since we are modifying it
    private float AngleStep = 0f;

    protected override void Start()
    {
        base.Start();
        AngleStep = RingAngle / (float)nBullets;
        Radius = RingRadius;
        float angleStep = RingAngle / (float)nBullets;

    }

    protected override void Reset()
    {
        Radius = RingRadius; //reset Radius size;

    }
    
    

    protected override void HandleModifiers()
    {
        if (RadiusModifier != 0f)
        {
            Radius += RadiusModifier;
        }
        
        if (AimRing)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                Vector3 directionToPlayer = player.transform.position - transform.position;
                if (directionToPlayer != Vector3.zero)
                {
                    Quaternion lookRotation = Quaternion.LookRotation(directionToPlayer);
                    SpawnRotation = lookRotation.eulerAngles + Rotation;
                }
            }
        }
        else
        {
            SpawnRotation = Rotation;
        }
    }

    protected override Vector3 GetBulletSpawnRotation(int currentBulletNumber)
    {
        float angle = currentBulletNumber * AngleStep;

        return new Vector3(0f, angle, 0f);

    }
    
    protected override Vector3 GetBulletSpawnLocation(int currentBulletNumber)
    {
        float angle = currentBulletNumber * AngleStep;

        return CalculatePositionOnRing(angle, Radius);
    }
    
    private Vector3 CalculatePositionOnRing(float angle, float radius)
    {
        float angleInRadians = angle * Mathf.Deg2Rad;
        float x = Mathf.Sin(angleInRadians) * radius;
        float z = Mathf.Cos(angleInRadians) * radius;
        Vector3 localPosition = new Vector3(x, 0, z);


        // Apply the rotation
        Quaternion rotationQuat = Quaternion.Euler(SpawnRotation);
        Vector3 rotatedPosition = rotationQuat * localPosition;

        return rotatedPosition + transform.position + Offset;
    }

    protected override void DrawGizmos()
    {
        float GizmoRadius = RingRadius; // Use a local variable for radius
        
        bool bSkipping = false;
        float angleStep = RingAngle / (float)nBullets;

        for (int i = 0; i < NumRepeats; i++)
        {

            for (int j = 0; j < nBullets; j++)
            {

                if (SkipInterval > 0 && (j + 1) % SkipInterval == 0)
                {
                    bSkipping = true; //set skip flag
                }

                if (bSkipping)
                {
                    SkipStride--;

                    if (SkipStride <= 0)
                    {
                        bSkipping = false; //reset skip flag once skip stride is over
                    }

                    continue;
                }
                
                float angle = j * angleStep;

                Vector3 spawnLocation = CalculatePositionOnRing(angle, GizmoRadius);

                Gizmos.DrawWireSphere(spawnLocation, 0.5f);

                if (RadiusModifier != 0f)
                {
                    GizmoRadius += RadiusModifier;
                }
            }

            GizmoRadius = RingRadius;
            bSkipping = false;
        }
    }
}
