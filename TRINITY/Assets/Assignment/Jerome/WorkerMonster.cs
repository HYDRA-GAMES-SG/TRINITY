using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class WorkerMonster : MonoBehaviour
{
    public GameObject Player;
    private UHealthComponent PlayerHealth;
    private string _currentstate;
    private Animator MonsterController;
    private NavMeshAgent MonsterAgent;
    public List<Transform> Waypoints = new List<Transform>();

    public float AITimer;
    public float AICollectingTime;
    public float StockTime;
    public float StockTimer;
    public float ChaseRange;
    public float AttackRange;
    public float AFKOutOfRange;
    private int WaypointIndex;

    public bool bSelfCompromised;
    public bool bResourcesCompromised;

    // Start is called before the first frame update
    void Start()
    {
        PlayerHealth = Player.GetComponent<UHealthComponent>();
    }

    // Update is called once per frame
    void Update()
    {
        float distanceFromPlayer = (transform.position - Player.transform.position).magnitude;
        if (bSelfCompromised || bResourcesCompromised) 
        {
            if (distanceFromPlayer <= AttackRange)
            {
                //attack player
            }
            else if (bSelfCompromised) 
            {
                //chase player
            }
        }
    }
    bool HasFinishMoving()
    {
        //navMeshAgent_.pathPending is to check if the pathfinder is calculating the path
        return !MonsterAgent.pathPending && !MonsterAgent.hasPath;
    }
    private void ChangeAnimationState(string newState)
    {
        if (newState == _currentstate)
        {
            return;
        }
        MonsterController.Play(newState);

        _currentstate = newState;
    }
    bool IsAnimationPlaying(Animator animator, string stateName)
    {
        if (animator.GetCurrentAnimatorStateInfo(0).IsName(stateName) && animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f)
        {
            return true;
        }

        else
        {
            return false;
        }
    }
}
