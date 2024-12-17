using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Unity.VisualScripting;
using UnityEngine;

public class ABlink : ASpell
{
    public AudioClip SFX;
    public LayerMask CollisionLayer; // Layer to check for collisions (e.g., Default)
    public LayerMask BossLayer; // Layer to check for bosses
    public float BlinkDistance = 10f;
    public float MinimumDistance = 1f;
    public float CollisionAdjustment = 1f;
    public float BossSphereCheckRadius = 1f;
    public float CheckStepDistance = 0.5f;
    public bool DEBUG_ENABLE;
    private Vector3 BlinkPoint;
    private AudioSource AudioComponent;
    public static System.Action OnBlink;
    
    public override void Initialize()
    {
        AudioComponent = GetComponent<AudioSource>();
    }
    
    public override void CastStart()
    {
        float distance = BlinkDistance;
        BlinkPoint = BrainReference.Controller.Position;
        Vector3 startPos = BrainReference.Controller.Position + BrainReference.Controller.Height * Vector3.up;

        Vector3 direction = Controller.MoveDirection.normalized;

        if (direction.magnitude < float.Epsilon)
        {
            direction = Controller.Forward;
        }

        Vector3 rotateAxis = Vector3.Cross(Vector3.up, direction);

        float rotatePitch = SpellsReference.CameraReference.Camera.transform.eulerAngles.x;
        
        Quaternion rotateQuat = Quaternion.AngleAxis(rotatePitch, rotateAxis);

        Vector3 rotatedDirection = rotateQuat * direction; 
        
        bool bInvalidBlink = true;

        while (distance > MinimumDistance && bInvalidBlink)
        {

            if(DEBUG_ENABLE){Debug.Log($"Attempting blink with distance: {distance}, starting at: {startPos}");}

            if (Physics.Raycast(startPos, rotatedDirection, out RaycastHit hit, distance, CollisionLayer))
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
                    BlinkPoint = hit.point - CollisionAdjustment * rotatedDirection;
                    if(DEBUG_ENABLE){Debug.Log($"Adjusted blink point: {BlinkPoint}");}
                }
            }
            else
            {
                BlinkPoint = startPos + distance * rotatedDirection;
            }
            

            // Spherecast for boss layer
            bInvalidBlink = Physics.SphereCast(new Ray(BlinkPoint, rotatedDirection), BossSphereCheckRadius,
                out RaycastHit bossHit, 0.1f, BossLayer);

            
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
            
            GameObject SmokeCloudLeaving = Instantiate(SpellPrefab, SpellsReference.CastPoint.position, Quaternion.identity);
            SmokeCloudLeaving.transform.SetParent(this.gameObject.transform);
            Destroy(SmokeCloudLeaving, 5f);
            
            OnBlink?.Invoke();

            bool bBlinkingIntoGround = Physics.Raycast(BlinkPoint, Vector3.down, out RaycastHit ground, BrainReference.Controller.Height, CollisionLayer);

            //prevent blinking through the ground when we readjust the blink point to the controller position at the feet
            if (bBlinkingIntoGround)
            {
                Controller.transform.position = ground.point;
            }
            else
            {
                Controller.transform.position = BlinkPoint - BrainReference.Controller.Height * Vector3.up;
            }
            
            if(DEBUG_ENABLE){Debug.Log($"Blink successful to: {BlinkPoint}");}

            if (AudioComponent != null && SFX != null)
            {
                AudioComponent.clip = SFX;
                AudioComponent.Play();
            }
            
            GameObject SmokeCloudArriving = Instantiate(SpellPrefab, SpellsReference.CastPoint.position, Quaternion.identity);
            SmokeCloudArriving.transform.SetParent(this.gameObject.transform);
            Destroy(SmokeCloudArriving, 5f);
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
            Gizmos.DrawSphere(BlinkPoint, BossSphereCheckRadius);
        }
    }
}