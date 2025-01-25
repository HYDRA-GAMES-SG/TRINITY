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
    public static System.Action BlinkCamera;

    private GameObject DepartCloud;
    private GameObject ArrivalCloud;
    
    public override void Initialize()
    {
        AudioComponent = GetComponent<AudioSource>();
    }
    
    public override void CastStart()
    {
        float distance = BlinkDistance;
        BlinkPoint = ATrinityGameManager.GetPlayerController().Position;
        Vector3 startPos = ATrinityGameManager.GetPlayerController().Position + ATrinityGameManager.GetPlayerController().Height * Vector3.up;

        float inputY = ATrinityGameManager.GetInput().MoveInput.y;
        float inputX = ATrinityGameManager.GetInput().MoveInput.x;
        
        Vector2 input = new Vector2(inputX, inputY);

        Vector3 direction = new Vector3(ATrinityGameManager.GetPlayerController().MoveDirection.x, 0f, ATrinityGameManager.GetPlayerController().MoveDirection.z).normalized;

        Vector3 rotatedDirection = new Vector3();


        NormalMovement playerMovement = null;

        if (ATrinityGameManager.GetPlayerFSM().CurrentState is NormalMovement)
        {
            playerMovement = (NormalMovement)ATrinityGameManager.GetPlayerFSM().CurrentState;
            
        }

        //if the direction is zeroish
        if (Mathf.Abs(input.y) < float.Epsilon && Mathf.Abs(input.x) < float.Epsilon)
        {
            if (playerMovement.GetMovementState() != ETrinityMovement.ETM_Grounded)
            {
                if(DEBUG_ENABLE){print("vertical blink");}
                rotatedDirection = ATrinityGameManager.GetPlayerController().Up;
            }
            else
            {
                if(DEBUG_ENABLE){print("no input blink");}
                rotatedDirection = ATrinityGameManager.GetPlayerController().Forward; //blink forward
                BlinkCamera?.Invoke();
            }
        }
        else if(Mathf.Abs(input.x) < float.Epsilon || input.y < 0f)
        {
            if(DEBUG_ENABLE){print("forward or backwards blink");}
            rotatedDirection = direction; //if the lateral input is zero or we are moving backwards, blink in teh direction of the movement vector
            BlinkCamera?.Invoke();
        }
        else if (Mathf.Abs(input.x) > float.Epsilon && playerMovement != null)
        {
                if(playerMovement.GetMovementState() != ETrinityMovement.ETM_Grounded)
                {
                    if(DEBUG_ENABLE){print("blink in direction of movement");}
                    rotatedDirection = direction;
                }
                else
                {
                    if(DEBUG_ENABLE){print("rotated blink");}
                    Vector3 rotateAxis = Vector3.Cross(Vector3.up, direction);

                    float rotatePitch = ATrinityGameManager.GetCamera().Camera.transform.eulerAngles.x;
    
                    Quaternion rotateQuat = Quaternion.AngleAxis(rotatePitch, rotateAxis);

                    rotatedDirection = rotateQuat * direction; 
                }
        }
        else
        {
            if(DEBUG_ENABLE){print("rotated blink");}
            Vector3 rotateAxis = Vector3.Cross(Vector3.up, direction);

            float rotatePitch = ATrinityGameManager.GetCamera().Camera.transform.eulerAngles.x;
        
            Quaternion rotateQuat = Quaternion.AngleAxis(rotatePitch, rotateAxis);

            rotatedDirection = rotateQuat * direction; 
        }

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
            if (DepartCloud)
            {
                Destroy(DepartCloud.gameObject);
            }
            Destroy(DepartCloud, 7);
            
            GameObject SmokeCloudDeparting = Instantiate(SpellPrefab, ATrinityGameManager.GetSpells().CastPoint.position, Quaternion.identity);
            DepartCloud = SmokeCloudDeparting;
            SmokeCloudDeparting.transform.SetParent(this.gameObject.transform);
            
            OnBlink?.Invoke();

            ATrinityController playerController = ATrinityGameManager.GetPlayerController();
            
            bool bBlinkingIntoGround = Physics.Raycast(BlinkPoint, Vector3.down, out RaycastHit ground, playerController.Height, CollisionLayer);

            //prevent blinking through the ground when we readjust the blink point to the controller position at the feet
            if (bBlinkingIntoGround)
            {
                playerController.transform.position = ground.point;
            }
            else
            {
                playerController.transform.position = BlinkPoint - playerController.Height * Vector3.up;
            }
            
            if(DEBUG_ENABLE){Debug.Log($"Blink successful to: {BlinkPoint}");}

            if (AudioComponent != null && SFX != null)
            {
                AudioComponent.clip = SFX;
                AudioComponent.Play();
            }

            if (ArrivalCloud)
            {
                Destroy(ArrivalCloud.gameObject);
            }
            GameObject SmokeCloudArriving = Instantiate(SpellPrefab, ATrinityGameManager.GetSpells().CastPoint.position, Quaternion.identity);
            ArrivalCloud = SmokeCloudArriving;
            SmokeCloudArriving.transform.SetParent(this.gameObject.transform);
            Destroy(ArrivalCloud, 7);
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