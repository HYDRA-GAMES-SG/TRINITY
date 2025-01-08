using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ABulletSphereSpawner : IBulletSpawner
{
        [Header("Sphere Spawner Settings")] [Tooltip("Radius of the sphere.")]
        public float SphereRadius = 5f;

        [Tooltip("Radius change after each object is spawned.")]
        public float RadiusModifier = 0f;

        [Tooltip("Whether to randomize the rotation after each spawn.")]
        public bool bRandomizeRotation = false;

        private float Radius; //we have a private radius since we are modifying it

        protected override void Start()
        {
            base.Start();
            
            Radius = SphereRadius;
        }

        protected override void Reset()
        {
            SpawnRotation = Random.onUnitSphere * 360f;
            Radius = SphereRadius; //reset Radius size;
        }

        protected override void HandleModifiers()
        {
            if (RadiusModifier != 0f)
            {
                Radius += RadiusModifier;
            }
        }

        protected override Vector3 GetBulletSpawnRotation(int currentBulletNumber)
        {
            float phi = Mathf.Acos(2 * (currentBulletNumber / (float)nBullets) - 1);
            float theta = Mathf.PI * (1 + Mathf.Sqrt(5)) * currentBulletNumber;

            return new Vector3(phi * Mathf.Rad2Deg, theta * Mathf.Rad2Deg, 0f);
        }

        protected override Vector3 GetBulletSpawnLocation(int currentBulletNumber)
        {
            float phi = Mathf.Acos(2 * (currentBulletNumber / (float)nBullets) - 1);
            float theta = Mathf.PI * (1 + Mathf.Sqrt(5)) * currentBulletNumber;

            return CalculatePositionOnSphere(phi, theta, Radius);
        }

        private Vector3 CalculatePositionOnSphere(float phi, float theta, float radius)
        {
            float x = radius * Mathf.Sin(phi) * Mathf.Cos(theta);
            float y = radius * Mathf.Sin(phi) * Mathf.Sin(theta);
            float z = radius * Mathf.Cos(phi);
            Vector3 localPosition = new Vector3(x, y, z);

            // Apply the rotation
            Quaternion rotationQuat = Quaternion.Euler(SpawnRotation);
            Vector3 rotatedPosition = rotationQuat * localPosition;

            return rotatedPosition + transform.position + Offset;
        }

        protected override void DrawGizmos()
        {
        //     float GizmoRadius = SphereRadius; // Use a local variable for radius
        //
        //     bool bSkipping = false;
        //
        //     for (int i = 0; i < NumRepeats; i++)
        //     {
        //         for (int j = 0; j < nBullets; j++)
        //         {
        //             if (SkipInterval > 0 && (j + 1) % SkipInterval == 0)
        //             {
        //                 bSkipping = true; //set skip flag
        //             }
        //
        //             if (bSkipping)
        //             {
        //                 SkipStride--;
        //
        //                 if (SkipStride <= 0)
        //                 {
        //                     bSkipping = false; //reset skip flag once skip stride is over
        //                 }
        //
        //                 continue;
        //             }
        //
        //             float phi = Mathf.Acos(2 * (j / (float)nBullets) - 1);
        //             float theta = Mathf.PI * (1 + Mathf.Sqrt(5)) * j;
        //
        //             Vector3 spawnLocation = CalculatePositionOnSphere(phi, theta, GizmoRadius);
        //
        //             Gizmos.DrawWireSphere(spawnLocation, 0.5f);
        //
        //             if (RadiusModifier != 0f)
        //             {
        //                 GizmoRadius += RadiusModifier;
        //             }
        //         }
        //
        //         GizmoRadius = SphereRadius;
        //         bSkipping = false;
        //     }
    }
}
