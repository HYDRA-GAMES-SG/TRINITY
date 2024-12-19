using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using System.Collections;

namespace ElliotAssignment
{
    public enum ECrabState
    {
        Wander,
        Rest,
        Pursue
    }
    
    public class CrabDemo : CrabState
    {
        public ECrabState CurrentState;
        public float PlayerDistance => (CrabAI.transform.position - Player.transform.position).magnitude;

        [Header("AI Setting")] [SerializeField]
        float MoveSpeed = 7;

        [SerializeField] float StopDistance = 9;
        [SerializeField] float ThresholdAngle = 5;

        [Header("Tiredness Settings")] [SerializeField]
        float TiredWaitTime = 2f;

        [SerializeField] float PursueWander = 4f;
        [SerializeField] public float PursueThreshold = 10f;

        [Header("Wander")] 
        public float WanderRadius = 10f;

        public bool bShouldPursue => PlayerDistance < PursueThreshold;


        public ATrinityController Player;
        public NavMeshAgent CrabAI;
        private Coroutine stateCoro;

        private string AnimKeyTurnDirection = "RotateDirection";
        private string AnimKeyMoveValue = "MoveValue";
        private Vector3 WanderPoint = Vector3.one;

        public override bool CheckEnterTransition(IState fromState)
        {
            return true;
        }

        public override void EnterBehaviour(float dt, IState fromState)
        {
            CrabAI = CrabFSM.CrabController.AI;

            CrabAI.speed = MoveSpeed;
            CrabAI.stoppingDistance = StopDistance;
            CrabAI.updateRotation = false;

            // Start the tiredness cycle when entering this state
            StartStateCycle();
            
            CurrentState = ECrabState.Wander;

        }

        private void StartStateCycle()
        {
            // Stop any existing coroutine
            if (stateCoro != null)
            {
                StopCoroutine(stateCoro);
            }

            // Start a new coroutine to manage tiredness cycle
            stateCoro = StartCoroutine(StateCycle());
        }

        private IEnumerator StateCycle()
        {
            while (true)
            {
                if (CurrentState == ECrabState.Wander)
                {
                    CurrentState = ECrabState.Rest;
                    yield return new WaitForSeconds(TiredWaitTime);
                }

                if (bShouldPursue)
                {
                    CurrentState = ECrabState.Pursue;
                }
                else
                {
                    CurrentState = ECrabState.Wander;
                    WanderPoint = GetWanderPoint(CrabAI.transform.position, WanderRadius);

                }
                // Pursue state
                yield return new WaitForSeconds(PursueWander);
            }
        }

        public override void UpdateBehaviour(float dt)
        {
            if (CurrentState == ECrabState.Wander && bShouldPursue)
            {
                CurrentState = ECrabState.Pursue;
            }
            
            if (CurrentState == ECrabState.Pursue)
            {
                RotateAndMoveTowardTarget(CrabFSM.PlayerController.transform.position);
            }
            else if (CurrentState == ECrabState.Wander)
            {
                RotateAndMoveTowardTarget(WanderPoint);
            }
            else
            {
                // Stop moving when tired
                CrabAI.ResetPath();
                CrabFSM.Animator.SetFloat(AnimKeyMoveValue, 0);
                CrabFSM.Animator.SetFloat(AnimKeyTurnDirection, 0);
            }
        }

        public override void ExitBehaviour(float dt, IState toState)
        {
            CrabAI.updateRotation = true;

            // Stop the tiredness coroutine when exiting the state
            if (stateCoro != null)
            {
                StopCoroutine(stateCoro);
            }
        }
        
        public Vector3 GetWanderPoint(Vector3 origin, float wanderRadius)
        {
            /* Generate a random point inside a unit sphere, then scale it by our wanderRadius */
            Vector3 randomDirection = Random.insideUnitSphere * wanderRadius; 
    
            /* Shift this random direction so that it’s relative to our origin point */
            randomDirection += origin;

            /* Try sampling this position on the NavMesh */
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomDirection, out hit, wanderRadius, NavMesh.AllAreas))
            {
                /* If successful, return the sampled position */
                return hit.position;
            }

            /* If no valid position found, just return the original position */
            return origin; 
        }

        public override bool CheckExitTransition(IState toState)
        {
            if (toState is NormalAttack || toState is ComboAttack || toState is JumpSmash || toState is RoarIceSpray ||
                toState is ChargeFastAttack || toState is JumpAway || toState is IcePhaseRoar || toState is GetHit ||
                toState is Death)
            {
                return true;
            }

            return false;
        }

        void RotateAndMoveTowardTarget(Vector3 targetPosition)
        {
            Transform crabTransform = CrabFSM.CrabController.transform;

            Vector3 directionToTarget = (targetPosition - crabTransform.position).normalized;
            float distanceToTarget = Vector3.Distance(targetPosition, crabTransform.position);
            float angleToTarget = RotateTowardTarget(directionToTarget, RotateSpeed);

            if (angleToTarget > ThresholdAngle)
            {
                if (distanceToTarget > StopDistance && angleToTarget <= ThresholdAngle + 30)
                {
                    MoveTowardTarget(targetPosition);
                }
                else
                {
                    TurnInPlace(directionToTarget);
                }
            }
            else
            {
                MoveTowardTarget(targetPosition);
            }
        }

        private void MoveTowardTarget(Vector3 targetPosition)
        {
            CrabFSM.Animator.SetFloat(AnimKeyTurnDirection, 0);
            CrabAI.SetDestination(targetPosition);

            Vector3 localDesiredVelocity = CrabFSM.CrabController.transform.InverseTransformDirection(CrabAI.velocity);
            CrabFSM.Animator.SetFloat(AnimKeyMoveValue, localDesiredVelocity.z);
        }

        private void TurnInPlace(Vector3 directionToTarget)
        {
            float turnDirection =
                Mathf.Sign(Vector3.Cross(CrabFSM.CrabController.transform.forward, directionToTarget).y);
            CrabFSM.Animator.SetFloat(AnimKeyTurnDirection, turnDirection);

            CrabAI.ResetPath();
        }

        private float RotateTowardTarget(Vector3 directionToTarget, float rotateSpeed)
        {
            Vector3 directionToTargetXZ = new Vector3(directionToTarget.x, 0, directionToTarget.z).normalized;
            Quaternion targetRotation = Quaternion.LookRotation(directionToTargetXZ);
            CrabFSM.CrabController.transform.rotation = Quaternion.Slerp(CrabFSM.CrabController.transform.rotation,
                targetRotation, rotateSpeed * Time.deltaTime);

            return Vector3.Angle(CrabFSM.CrabController.transform.forward, directionToTarget);
        }
    }
}