using System.Collections;
using System.Collections.Generic;
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
    public bool bDebug;

    private Vector3 BlinkPoint;

    public static System.Action OnBlink;

    void Start()
    {
        // Check if required components are found
        Brain = transform.root.Find("Brain")?.GetComponent<ATrinityBrain>();
        Spells = transform.parent?.GetComponent<ATrinitySpells>();

        if (Brain == null || Spells == null)
        {
            Debug.LogError("Failed to initialize: Brain or Spells is null.");
        }
    }

    void Update()
    {
        UpdateCooldown();
    }

    public virtual void CastStart()
    {
        float distance = BlinkDistance;
        BlinkPoint = Brain.Controller.Position;
        Vector3 startPos = Brain.Controller.Position;
        Vector3 direction = Brain.Controller.Forward;

        bool bInvalidBlink = true;

        while (distance > MinimumDistance && bInvalidBlink)
        {

            // Debug the initial conditions of each loop iteration
            if(bDebug){Debug.Log($"Attempting blink with distance: {distance}, starting at: {startPos}");}

            if (Physics.Raycast(startPos, direction, out RaycastHit hit, distance, CollisionLayer))
            {
                // Log raycast hit
                if (bDebug)
                {
                    if (bDebug)
                    {
                        if(bDebug){Debug.Log($"Raycast hit: {hit.collider.name}, distance: {hit.distance}");}
                    }
                }

                if (hit.distance < MinimumDistance)
                {
                    BlinkPoint = startPos;
                    if(bDebug){Debug.Log("Blink point too close. Using start position.");}
                    break;
                }
                else
                {
                    BlinkPoint = hit.point - CollisionAdjustment * direction;
                    if(bDebug){Debug.Log($"Adjusted blink point: {BlinkPoint}");}
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
                if (bDebug)
                {
                    Debug.Log($"Blink point invalidated by boss at: {bossHit.point}");
                }
            }

            // Reduce distance and log
            distance -= CheckStepDistance;
            if(bDebug){Debug.Log($"Decreased distance to: {distance}");}
        }

        // Set position and log final result
        if (!bInvalidBlink)
        {
            Brain.Controller.transform.position = BlinkPoint;
            Brain.Controller.transform.Find("CameraBoom").transform.position = startPos;
            OnBlink?.Invoke();
            if(bDebug){Debug.Log($"Blink successful to: {BlinkPoint}");}
        }
        else
        {
            if(bDebug){Debug.LogWarning("Failed to find a valid blink point.");}
        }
    }

    public virtual void CastUpdate()
    {
        // Debug update logic if needed
    }

    public virtual void CastEnd()
    {
        // Debug end logic if needed
    }

    public void OnDrawGizmos()
    {
        Gizmos.DrawSphere(BlinkPoint, BossShereCheckRadius);
    }
}