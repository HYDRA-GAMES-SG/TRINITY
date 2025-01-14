using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public enum ETrinityMovement
{
    ETM_Grounded,
    ETM_Jumping,
    ETM_Falling,
    ETM_Gliding
}

public class NormalMovement : TrinityState
{
    public bool ENABLE_DEBUG = false;
    
    private ETrinityMovement MovementState;

    [Header("Normal Movement")] 
    [SerializeField] private float UnstableSlideAcceleration = 40f;
    [SerializeField] private float AirStrafeModifier = .5f;
    [SerializeField] private float AirMoveModifier = .5f;
    [SerializeField] private float MoveAirAcceleration = 5f;
    [SerializeField] private float StrafeAirAcceleration = 5f;
    [SerializeField] private float MoveAcceleration = 10f;
    [SerializeField] private float StrafeAcceleration = 10f;
    [SerializeField] private float MaxSpeed = 5f;
    [SerializeField] private float GlobalSpeedLimit = 7f;
    [SerializeField] private float JumpForce = 10f;
    
    [Header("Glide Movement")]
    [SerializeField] private float GlideMoveAcceleration = 40f;
    [SerializeField] private float GlideStrafeAcceleration = 40f;
    [SerializeField] private float GlideGravityModifier = .4f;
    
    [HideInInspector] private int MirrorCounter = 0;
    
    private bool bUnstable = false;
    private bool bCanGlide = false;
    private bool bFixedUpdate = false;
    private ATrinityController Controller;


    private Dictionary<string, string> AnimKeys = new Dictionary<string, string>
    {
        { "Move", "Forward" }, { "Strafe", "Strafe" },{ "Vertical", "Vertical" }, { "Jump", "bJump" },
        { "Glide", "bGlide" }, { "Blink", "bBlink" }, { "Mirror", "bMirror" }, { "Death", "bDeath" }, 
        { "Stunned", "bStunned" }, { "HitTrigger", "HitTrigger" }, { "HitX", "HitX" }, {"HitY", "HitY" },
        { "Grounded", "bGrounded" }
    };
    
    public override bool CheckEnterTransition(IState fromState)
    {
        return true;
    }
    
    public override void EnterBehaviour(float dt, IState fromState)
    {
        Controller = ATrinityGameManager.GetPlayerController();
        
        SetMovementState(ETrinityMovement.ETM_Grounded);
        ATrinityGameManager.GetAnimator().AnimComponent.SetBool(AnimKeys["Jump"], false);
        ATrinityGameManager.GetAnimator().AnimComponent.SetBool(AnimKeys["Glide"], false);
        
        bCanGlide = false;
        ABlink.OnBlink += OnBlink;
        ATrinityGameManager.GetPlayerController().HealthComponent.OnDeath += HandleDeath;
        ATrinityGameManager.GetPlayerController().OnHit += HandleHit;
    }


    private void HandleDeath()
    {
        // if (Controller.CheckGround().transform)
        // {
        ATrinityGameManager.GetAnimator().AnimComponent.SetTrigger(AnimKeys["Death"]);
        // }
        // else
        // {
        //     Controller.EnableRagdoll();
        // }
    }


    public override void PreUpdateBehaviour(float dt)
    {
        
    }
    
    public override void UpdateBehaviour(float dt)
    {
        bFixedUpdate = true;
        
        HandleGliding();
        HandleFalling();
        HandleUnstableGround();

        if (!ATrinityGameManager.GetBrain().CanAct() || bUnstable || Controller.HealthComponent.bDead 
            || ATrinityGameManager.GetBrain().GetAction() == ETrinityAction.ETA_Channeling)
        {
            UpdateAnimParams();
            return;
        }

        HandleMovement();
        HandleJumping();
        HandleBlink();
        TryEnterGlide();

        Controller.MoveDirection = GetChargedMovement();
        
        // if speed in the direction of the movedirection is not faster than max speed
        if (Vector3.Project(Controller.RB.velocity, Controller.MoveDirection).magnitude < MaxSpeed) 
        {
            Controller.RB.AddForce(Controller.MoveDirection);
        }
        
        HandleAirStrafing();
        UpdateAnimParams();
    }

    public override void PostUpdateBehaviour(float dt)
    {
       
    }

    public override void ExitBehaviour(float dt, IState toState)
    {
        ABlink.OnBlink -= OnBlink;
        Controller.HealthComponent.OnDeath -= HandleDeath;

    }

    public override bool CheckExitTransition(IState toState)
    {
        
        return false;
    }

    private void HandleGliding()
    {
        if (MovementState != ETrinityMovement.ETM_Gliding)
        {
            return;
        }
        
        if (!ATrinityGameManager.GetInput().JumpInput || Controller.CheckGround().transform) // let HandleFalling() handle groundedness
        {
            SetMovementState(ETrinityMovement.ETM_Falling);
            Controller.Gravity = ATrinityController.GRAVITY_CONSTANT;
            return;
        }

        float chargeGravityModifier = 1f;

        if (ATrinityGameManager.GetEnemyControllers().Count > 0)
        {
            chargeGravityModifier = UAilmentComponent.ChargeGlideGravityModifier;
        }
        //Handle Glide
        Controller.Gravity = ATrinityController.GRAVITY_CONSTANT * GlideGravityModifier * chargeGravityModifier;
    }
    
    private void HandleAirStrafing()
    {
        Vector3 planarVelocity = Controller.PlanarVelocity;
        if (planarVelocity.magnitude > GlobalSpeedLimit)
        {
            planarVelocity = planarVelocity.normalized * GlobalSpeedLimit;
        }
        
        
        Controller.RB.velocity = planarVelocity + new Vector3(0f, Controller.VerticalVelocity, 0f);

    }

    private void HandleMovement()
    {
        Vector3 moveZ = Controller.Forward * ATrinityGameManager.GetInput().MoveInput.y;
        Vector3 moveX = Controller.Right * ATrinityGameManager.GetInput().MoveInput.x;

        switch (MovementState)
        {
            case ETrinityMovement.ETM_Grounded:
                moveZ *= MoveAcceleration;
                moveX *= StrafeAcceleration;
                break;
            case ETrinityMovement.ETM_Gliding:
                moveZ *= GlideMoveAcceleration;
                moveX *= GlideStrafeAcceleration;
                break;
            default: //jumping or falling
                moveZ *= MoveAirAcceleration;
                moveX *= StrafeAirAcceleration;
                break;
        }
        
        Controller.MoveDirection = moveZ;
        Controller.MoveDirection += moveX;
    }
    
    private void HandleJumping()
    {  
        if (ATrinityGameManager.GetInput().JumpInput)
        {
            if (MovementState == ETrinityMovement.ETM_Grounded)
            {
                SetMovementState(ETrinityMovement.ETM_Jumping);
                
                if (MovementState == ETrinityMovement.ETM_Jumping) //need to check this
                {
                    Controller.RB.AddForce(Controller.Up * GetChargedJumpForce() / Controller.RB.mass, ForceMode.Impulse);
                    ATrinityGameManager.GetAnimator().AnimComponent.SetBool(AnimKeys["Jump"], true);
                    MirrorCounter++; //increment counter
                    ATrinityGameManager.GetAnimator().AnimComponent.SetBool(AnimKeys["Mirror"], MirrorCounter % 2 == 1); //flip flop counter
                }
            }
        }
    }

    private void HandleFalling()
    {
        if (MovementState != ETrinityMovement.ETM_Grounded)
        {
            if (MovementState == ETrinityMovement.ETM_Jumping && Controller.VerticalVelocity < 0f)
            {
                SetMovementState(ETrinityMovement.ETM_Falling);
            }
            
            // Perform raycast to check for ground
            if (MovementState != ETrinityMovement.ETM_Jumping)
            {
                if (Controller.CheckGround().transform)
                {
                    // Ground detected, ensure movement state remains grounded
                    SetMovementState(ETrinityMovement.ETM_Grounded);
                    bCanGlide = false;
                    ATrinityGameManager.GetAnimator().AnimComponent.SetBool(AnimKeys["Jump"], false);
                    ATrinityGameManager.GetAnimator().AnimComponent.SetBool(AnimKeys["Blink"], false);
                    ATrinityGameManager.GetAnimator().AnimComponent.SetBool(AnimKeys["Glide"], false);
                }
            }
        }
    }
    
    
    private void HandleBlink()
    {
        if (ATrinityGameManager.GetAnimator().AnimComponent.GetBool(AnimKeys["Blink"]) && Controller.CheckGround().transform)
        {
            // Ground detected, ensure movement state remains grounded
            SetMovementState(ETrinityMovement.ETM_Grounded);
            bCanGlide = false;
            ATrinityGameManager.GetAnimator().AnimComponent.SetBool(AnimKeys["Jump"], false);
            ATrinityGameManager.GetAnimator().AnimComponent.SetBool(AnimKeys["Blink"], false);
        }
    }
    
    private void HandleUnstableGround()
    {
        // Vector3 groundNormal = Controller.FindUnstableGround().normal;
        //
        // Vector3 slideDirection = Vector3.Cross(Vector3.Cross(groundNormal, Vector3.up), groundNormal).normalized;
        //
        // Controller.MoveDirection += slideDirection * UnstableSlideAcceleration * Time.deltaTime;
        //
        // //check if the angle is steep enough to transition into falling
        // float groundAngle = Vector3.Angle(Vector3.up, groundNormal);
        //
        // if (groundAngle > Controller.MaxStableAngle)
        // {
        //     SetMovementState(ETrinityMovement.ETM_Falling);
        //     Controller.MoveDirection = slideDirection.normalized * UnstableSlideAcceleration;
        //     bCanGlide = false;
        //     bUnstable = true;
        // }
        // else
        // {
        //     bUnstable = false;
        // }
    }
    
    

    private void TryEnterGlide()
    {
        if (MovementState == ETrinityMovement.ETM_Falling || MovementState == ETrinityMovement.ETM_Jumping)
        {
            // if (!InputReference.JumpInput)
            // {
                bCanGlide = true;
            //}
        }

        if (bCanGlide)
        {
            if (ATrinityGameManager.GetInput().JumpInput && MovementState == ETrinityMovement.ETM_Falling)
            {
                SetMovementState(ETrinityMovement.ETM_Gliding);
                ATrinityGameManager.GetAnimator().AnimComponent.SetBool(AnimKeys["Glide"], true);
            }
        }
    }

    private void SetMovementState(ETrinityMovement newMovementState)
    {
        if (!bFixedUpdate)
        {
            return;
        }
        
        if (newMovementState != MovementState)
        {
            if (ENABLE_DEBUG)
            {
                print("Transition: " + MovementState + "->" + newMovementState);
            }

            bFixedUpdate = false; //flag to prevent new movement if physics hasnt been updated
            MovementState = newMovementState;
        }
    }
    
    private void UpdateAnimParams()
    {
        // less damping if we are landing
        float maxSpeedThreshold = MaxSpeed * .6f;
        Vector3 playerSpaceVelocity = Controller.transform.InverseTransformVector(Controller.RB.velocity) / maxSpeedThreshold;
        
        ATrinityGameManager.GetAnimator().AnimComponent.SetFloat(AnimKeys["Move"], playerSpaceVelocity.z, .05f, Time.deltaTime);
        ATrinityGameManager.GetAnimator().AnimComponent.SetFloat(AnimKeys["Strafe"], playerSpaceVelocity.x, .05f, Time.deltaTime);
        ATrinityGameManager.GetAnimator().AnimComponent.SetFloat(AnimKeys["Vertical"], Controller.VerticalVelocity);
        ATrinityGameManager.GetAnimator().AnimComponent.SetBool(AnimKeys["Stunned"], ATrinityGameManager.GetBrain().bIsStunned);
        ATrinityGameManager.GetAnimator().AnimComponent.SetBool(AnimKeys["Grounded"], MovementState == ETrinityMovement.ETM_Grounded);
    }
    
    public ETrinityMovement GetMovementState()
    {
        return MovementState;
    }
    
    
    private void OnBlink()
    {
        ATrinityGameManager.GetAnimator().AnimComponent.SetBool(AnimKeys["Blink"], true);
    }

    private void HandleHit(FHitInfo hitInfo)
    {
        bCanGlide = false;
        ATrinityGameManager.GetBrain().SetStunnedState(1f * (hitInfo.Damage / Controller.HealthComponent.MAX));
        
        
        
        Vector3 playerSpaceEnemyCollider = Controller.transform.InverseTransformPoint(hitInfo.CollidingObject.transform.position);
        Vector2 playerSpacePlanarEnemyColliderNormalized = new Vector2(playerSpaceEnemyCollider.x, playerSpaceEnemyCollider.z).normalized;
        
        ATrinityGameManager.GetAnimator().AnimComponent.SetFloat(AnimKeys["HitX"], playerSpacePlanarEnemyColliderNormalized.x);
        ATrinityGameManager.GetAnimator().AnimComponent.SetFloat(AnimKeys["HitY"], playerSpacePlanarEnemyColliderNormalized.y);
        
        if (MovementState != ETrinityMovement.ETM_Grounded)
        {
            ATrinityGameManager.GetAnimator().AnimComponent.Play("Aerial Hit Blend", 0, 0f);
        }
        else
        {
            ATrinityGameManager.GetAnimator().AnimComponent.Play("Hit Blend", 0, 0f);
        }
    }

    public float GetChargedJumpForce()
    {
        float additionalJumpForce = 0f;
        if (ATrinityGameManager.GetEnemyControllers().Count > 0)
        {
            additionalJumpForce = UAilmentComponent.ChargeAdditionalJumpForce;
        }
        
        return JumpForce + additionalJumpForce;
    }

    public Vector3 GetChargedMovement()
    {

        float chargeMoveModifier = 1f;
        
        if (ATrinityGameManager.GetEnemyControllers().Count > 0)
        {
            chargeMoveModifier = UAilmentComponent.ChargeMoveModifier;
        }

        float chargedX = Controller.MoveDirection.x * chargeMoveModifier;
        float chargedZ = Controller.MoveDirection.z * chargeMoveModifier;
        
        switch (MovementState)
        {
            case ETrinityMovement.ETM_Grounded:
                return new Vector3(chargedX, Controller.VerticalVelocity, chargedZ);
            case ETrinityMovement.ETM_Gliding:
                return new Vector3(chargedX, Controller.VerticalVelocity, chargedZ);
            default:
                return new Vector3(chargedX * AirMoveModifier, Controller.VerticalVelocity, chargedZ * AirStrafeModifier);
        }
    }
}