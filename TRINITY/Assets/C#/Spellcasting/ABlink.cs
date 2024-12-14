using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Unity.VisualScripting;
using UnityEngine;

public class ABlink : ASpell
{
    public LayerMask CollisionLayer; // Layer to check for collisions (e.g., Default)
    public LayerMask BossLayer; // Layer to check for bosses
    public float BlinkDistance = 10f;
    public float MinimumDistance = 1f;
    public float CollisionAdjustment = 1f;
    public float BossShereCheckRadius = 1f;
    public float CheckStepDistance = 0.5f;
    public bool DEBUG_ENABLE;
    private Vector3 BlinkPoint;

    public static System.Action OnBlink;

    public override void Initialize()
    {
    }


    
    public override void CastStart()
    {
        float distance = BlinkDistance;
        BlinkPoint = Brain.Controller.Position;
        Vector3 startPos = Brain.Controller.Position + Brain.Controller.Height * Vector3.up;
        Vector3 direction = Spells.CastDirection;

        bool bInvalidBlink = true;

        while (distance > MinimumDistance && bInvalidBlink)
        {

            // Debug the initial conditions of each loop iteration
            if(DEBUG_ENABLE){Debug.Log($"Attempting blink with distance: {distance}, starting at: {startPos}");}

            if (Physics.Raycast(startPos, direction, out RaycastHit hit, distance, CollisionLayer))
            {
                if(DEBUG_ENABLE){Debug.Log($"Raycast hit: {hit.collider.name}, distance: {hit.distance}");}

                if (hit.distance < MinimumDistance)
                {
                    BlinkPoint = startPos;
                    if(DEBUG_ENABLE){Debug.Log("Blink point too close. Using start position.");}
                    break;
                }
                else
                {
                    BlinkPoint = hit.point - CollisionAdjustment * direction;
                    if(DEBUG_ENABLE){Debug.Log($"Adjusted blink point: {BlinkPoint}");}
                }
            }
            else
            {
                BlinkPoint = startPos + distance * direction;
            }
            

            // Spherecast for boss layer
            bInvalidBlink = Physics.SphereCast(new Ray(BlinkPoint, direction), BossShereCheckRadius, out RaycastHit bossHit, 0.1f, BossLayer);

            if (bInvalidBlink)
            {
                if (DEBUG_ENABLE)
                {
                    Debug.Log($"Blink point invalidated by boss at: {bossHit.point}");
                }
                // Reduce distance and log
                distance -= CheckStepDistance;
                if(DEBUG_ENABLE){Debug.Log($"Decreased distance to: {distance}");}
            }

        }

        // Set position and log final result
        if (!bInvalidBlink)
        {
            OnBlink?.Invoke();
            Brain.Controller.transform.position = BlinkPoint;
            if(DEBUG_ENABLE){Debug.Log($"Blink successful to: {BlinkPoint}");}
        }
        else
        {
            if(DEBUG_ENABLE){Debug.LogWarning("Failed to find a valid blink point.");}
        }
    }

    public override void CastUpdate()
    {
    }

    public override void CastEnd()
    {
        // Debug end logic if needed
    }

    public void OnDrawGizmos()
    {
        if (DEBUG_ENABLE)
        {
            Gizmos.DrawSphere(BlinkPoint, BossShereCheckRadius);
        }
    }
}