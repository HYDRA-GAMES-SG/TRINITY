using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

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
    [SerializeField] public float GlideGravityModifier = .4f;
    
    [HideInInspector] private int MirrorCounter = 0;
    
    private bool bUnstable = false;
    private bool bCanGlide = false;
    private bool bJumpConsumed = false;
    private bool bFixedUpdate = false;
    
    private ATrinityController Controller;
    private ATrinityInput Input;
    private ATrinityAnimator Animator;
    private Coroutine StunCoroutine;
    private float StunDurationRemaining;

    private Dictionary<string, string> AnimKeys = new Dictionary<string, string>
    {
        { "Move", "Forward" }, { "Strafe", "Strafe" },{ "Vertical", "Vertical" }, { "Jump", "bJump" },
        { "Glide", "bGlide" }, { "Blink", "bBlink" }, { "Mirror", "bMirror" }, { "DeathTrigger", "DeathTrigger" }, 
        { "Stun", "bStunned" }, { "HitTrigger", "HitTrigger" }, { "HitX", "HitX" }, {"HitY", "HitY" },
        { "Grounded", "bGrounded" }, {"Release", "bRelease"}, {"StunDuration", "stunDuration"}, {"Dead", "bdDead"}
    };
    
    public override bool CheckEnterTransition(IState fromState)
    {
        return true;
    }
    
    public override void EnterBehaviour(float dt, IState fromState)
    {
        Controller = ATrinityGameManager.GetPlayerController();
        Input = ATrinityGameManager.GetInput();
        
        SetMovementState(ETrinityMovement.ETM_Grounded);
        TrinityFSM.Animator.SetBool(AnimKeys["Jump"], false);
        TrinityFSM.Animator.SetBool(AnimKeys["Glide"], false);
        
        bCanGlide = false;
        ABlink.OnBlink += OnBlink;
        ATrinityGameManager.GetBrain().OnStunned += HandleStun;
        Controller.HealthComponent.OnDeath += HandleDeath;
        Controller.OnHit += HandleHit;
    }


    private void HandleDeath()
    {
        // if (Controller.CheckGround().transform)
        // {
        TrinityFSM.Animator.SetTrigger(AnimKeys["DeathTrigger"]);
        ATrinityController.OnDeath?.Invoke();
        // }
        // else
        // {
        //     Controller.EnableRagdoll();
        // }
    }

    private void HandleStun(float stunDuration)
    {
        if (!TrinityFSM.Animator.GetCurrentAnimatorStateInfo(0).IsName("Kneel Down"))
        {
            TrinityFSM.Animator.SetBool(AnimKeys["Stun"], true);
        }
        
        if (stunDuration > StunDurationRemaining)
        {
            StunDurationRemaining = stunDuration;
        }
    }
    public override void PreUpdateBehaviour(float dt)
    {
        
    }
    
    public override void UpdateBehaviour(float dt)
    {
        
        
        
        if (!Input.JumpInput && MovementState == ETrinityMovement.ETM_Grounded)
        {
            bJumpConsumed = false;
        }
        
        if (Controller.bTerrainCollision)
        {
            bCanGlide = false;
            SetMovementState(ETrinityMovement.ETM_Falling);
            TrinityFSM.Animator.SetBool(AnimKeys["Glide"], false);
        }

        StunDurationRemaining -= Time.deltaTime;
        
        if (TrinityFSM.Animator.GetCurrentAnimatorStateInfo(0).IsName("Movement") || TrinityFSM.Animator.GetCurrentAnimatorStateInfo(0).IsName("Kneel Down") )
        {
            TrinityFSM.Animator.SetBool(AnimKeys["Stun"], false);
        }
        
        bFixedUpdate = true;
        
        HandleGlideExit();
        HandleFalling();
        HandleUnstableGround();

        if (!ATrinityGameManager.GetBrain().CanAct() || Controller.bUnstable || ATrinityGameManager.GetBrain().GetAction() == ETrinityAction.ETA_Channeling || StunDurationRemaining > 0f)
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
        if (Vector3.Project(Controller.RB.velocity, Controller.MoveDirection).magnitude < GetChargedMaxSpeed()) 
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

    private void HandleGlideExit()
    {
        if (MovementState != ETrinityMovement.ETM_Gliding)
        {
            return;
        }
        
        if (!Input.JumpInput || Controller.CheckGround().transform || ATrinityGameManager.GetGameFlowState() == EGameFlowState.DEAD) // let HandleFalling() handle groundedness
        {
            SetMovementState(ETrinityMovement.ETM_Falling);
            TrinityFSM.Animator.SetBool(AnimKeys["Glide"], false);
            return;
        }
    }
    
    private void HandleAirStrafing()
    {
        if (MovementState == ETrinityMovement.ETM_Gliding)
        {
            return;
        }
        
        Vector3 planarVelocity = Controller.PlanarVelocity;
        if (planarVelocity.magnitude > GlobalSpeedLimit)
        {
            planarVelocity = planarVelocity.normalized * GlobalSpeedLimit;
        }
        
        
        Controller.RB.velocity = planarVelocity + new Vector3(0f, Controller.VerticalVelocity, 0f);

    }

    private void HandleMovement()
    {
        Vector3 moveZ = Controller.Forward * Input.MoveInput.y;
        Vector3 moveX = Controller.Right * Input.MoveInput.x;

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
        if (Input.JumpInput)
        {
            
            if (MovementState == ETrinityMovement.ETM_Grounded && !bJumpConsumed)
            {
                SetMovementState(ETrinityMovement.ETM_Jumping);
                
                if (MovementState == ETrinityMovement.ETM_Jumping) //need to check this
                {
                    bJumpConsumed = true;
                    Controller.RB.AddForce(Controller.Up * GetChargedJumpForce() / Controller.RB.mass, ForceMode.Impulse);
                    TrinityFSM.Animator.SetBool(AnimKeys["Jump"], true);
                    ATrinityController.OnJump?.Invoke();
                    MirrorCounter++; //increment counter
                    TrinityFSM.Animator.SetBool(AnimKeys["Mirror"], MirrorCounter % 2 == 1); //flip flop counter
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
                ATrinityController.OnBeginFalling?.Invoke();
            }
            
            // Perform raycast to check for ground
            if (MovementState != ETrinityMovement.ETM_Jumping)
            {
                if (Controller.CheckGround().transform)
                {
                    if (MovementState == ETrinityMovement.ETM_Gliding)
                    {
                        ATrinityController.OnGlideEnd?.Invoke();

                    }
                    ATrinityController.OnLand?.Invoke(Controller.VerticalVelocity);
                    
                    // Ground detected, ensure movement state remains grounded
                    SetMovementState(ETrinityMovement.ETM_Grounded);
                    bCanGlide = false;
                    TrinityFSM.Animator.SetBool(AnimKeys["Jump"], false);
                    TrinityFSM.Animator.SetBool(AnimKeys["Blink"], false);
                    TrinityFSM.Animator.SetBool(AnimKeys["Glide"], false);
                    
                }
            }
        }
    }
    
    
    private void HandleBlink()
    {
        if (TrinityFSM.Animator.GetBool(AnimKeys["Blink"]) && Controller.CheckGround().transform)
        {
            // Ground detected, ensure movement state remains grounded
            SetMovementState(ETrinityMovement.ETM_Grounded);
            bCanGlide = false;
            TrinityFSM.Animator.SetBool(AnimKeys["Jump"], false);
            TrinityFSM.Animator.SetBool(AnimKeys["Blink"], false);
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
            if (Input.JumpInput && MovementState == ETrinityMovement.ETM_Falling && !Controller.bTerrainCollision)
            {
                SetMovementState(ETrinityMovement.ETM_Gliding);
                TrinityFSM.Animator.SetBool(AnimKeys["Glide"], true);
                ATrinityController.OnGlideStart?.Invoke();
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
        
        TrinityFSM.Animator.SetFloat(AnimKeys["Move"], playerSpaceVelocity.z, .05f, Time.deltaTime);
        TrinityFSM.Animator.SetFloat(AnimKeys["Strafe"], playerSpaceVelocity.x, .05f, Time.deltaTime);
        TrinityFSM.Animator.SetFloat(AnimKeys["Vertical"], Controller.VerticalVelocity);
        TrinityFSM.Animator.SetBool(AnimKeys["Grounded"], MovementState == ETrinityMovement.ETM_Grounded);
        TrinityFSM.Animator.SetBool(AnimKeys["Release"], ATrinityGameManager.GetBrain().GetAction() == ETrinityAction.ETA_None);
        TrinityFSM.Animator.SetFloat(AnimKeys["StunDuration"], StunDurationRemaining);
        TrinityFSM.Animator.SetBool(AnimKeys["Dead"], Controller.HealthComponent.bDead);
        
        if (TrinityFSM.Animator.GetBool(AnimKeys["Blink"]) == true)
        {
            TrinityFSM.Animator.SetBool(AnimKeys["Blink"], false);
        }
    }
    
    public ETrinityMovement GetMovementState()
    {
        return MovementState;
    }
    
    
    private void OnBlink()
    {
        if (TrinityFSM.Animator != null)
        {
            TrinityFSM.Animator.SetBool(AnimKeys["Blink"], true);
        }
    }

    private void HandleHit(FHitInfo hitInfo)
    {
        if (Controller.HealthComponent.bDead)
        {
            return;
        }
        
        bCanGlide = false;
        ATrinityGameManager.GetBrain().SetStunnedState(1f * (hitInfo.Damage / Controller.HealthComponent.MAX));
        
        
        
        Vector3 playerSpaceEnemyCollider = Controller.transform.InverseTransformPoint(hitInfo.CollidingObject.transform.position);
        Vector2 playerSpacePlanarEnemyColliderNormalized = new Vector2(playerSpaceEnemyCollider.x, playerSpaceEnemyCollider.z).normalized;
        
        TrinityFSM.Animator.SetFloat(AnimKeys["HitX"], playerSpacePlanarEnemyColliderNormalized.x);
        TrinityFSM.Animator.SetFloat(AnimKeys["HitY"], playerSpacePlanarEnemyColliderNormalized.y);
        
        if (MovementState != ETrinityMovement.ETM_Grounded)
        {
            TrinityFSM.Animator.Play("Aerial Hit Blend", 0, 0f);
        }
        else
        {
            TrinityFSM.Animator.Play("Hit Blend", 0, 0f);
        }
    }

    public float GetChargedJumpForce()
    {
        float additionalJumpForce = 0f;
        if (ATrinityGameManager.GetEnemyControllers().Count > 0)
        {
            additionalJumpForce = UAilmentComponent.GetChargeAdditionalJumpForce();
        }
        
        return JumpForce + additionalJumpForce;
    }

    public Vector3 GetChargedMovement()
    {

        float chargeMoveModifier = 1f;
        
        if (ATrinityGameManager.GetEnemyControllers().Count > 0)
        {
            chargeMoveModifier = UAilmentComponent.GetChargeMoveModifier();
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

    public float GetChargedMaxSpeed()
    {
        float chargeMaxSpeedModifier = 1f;
        
        if (ATrinityGameManager.GetEnemyControllers().Count > 0)
        {
            chargeMaxSpeedModifier = UAilmentComponent.GetChargeMoveModifier();
        }

        return MaxSpeed * chargeMaxSpeedModifier;
    }
    public void OnDestroy()
    {
        ABlink.OnBlink -= OnBlink;
        Controller.HealthComponent.OnDeath -= HandleDeath;
        Controller.OnHit -= HandleHit;
    }
}