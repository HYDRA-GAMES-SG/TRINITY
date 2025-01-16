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
    public Transform Mine;
    public Transform MonsterBase;

    public float CollectingTimer;
    public float CollectingTime;
    public float StockTime;
    public float StockTimer;

    public float MonsterBaseRange;
    public float MineRange;

    public float AttackRange;
    public float AFKOutOfRange;

    public bool bSelfCompromised;
    public bool bResourcesCompromised;
    public bool bHoldingResource;

    const string CRAWLFORWARD = "CrawlForward";
    const string TAUNT = "Taunt";
    const string FLYFORWARD = "FlyForward";
    const string FLYCLAWSATTACK = "FlyClawsAttack";

    // Start is called before the first frame update
    void Start()
    {
        PlayerHealth = Player.GetComponent<UHealthComponent>();
        MonsterController = GetComponent<Animator>();
        MonsterAgent = GetComponent<NavMeshAgent>();
        CollectingTimer = CollectingTime;
        StockTimer = StockTime;
        ChangeAnimationState(CRAWLFORWARD);
    }

    // Update is called once per frame
    void Update()
    {
        float distanceFromPlayer = (transform.position - Player.transform.position).magnitude;
        float distanceFromBase = (MonsterBase.position - Player.transform.position).magnitude;
        float distanceFromMine = (Mine.position - Player.transform.position).magnitude;
        print($"Player distance from mine : {distanceFromMine}");
        print($"Player distance from base : {distanceFromBase}");

        if (distanceFromMine < MineRange || distanceFromBase < MonsterBaseRange) 
        {
            bResourcesCompromised = true;  
        }

        if (bSelfCompromised || bResourcesCompromised)
        {
            print("VAS2");
            if (bResourcesCompromised && (distanceFromBase > MonsterBaseRange && distanceFromMine > MineRange)) 
            {
                bResourcesCompromised = false;
                return;
            }

            if (distanceFromPlayer <= AttackRange)
            {
                //attack player
                ChangeAnimationState(FLYCLAWSATTACK);
            }
            else if (bSelfCompromised || bResourcesCompromised)
            {
                //chase player
                MonsterAgent.destination = Player.transform.position;
                ChangeAnimationState(FLYFORWARD);
            }
        }
        else 
        {
            print("VAS");
            if (!bHoldingResource)
            {
                MonsterAgent.destination = Mine.position;
                //ChangeAnimationState(CRAWLFORWARD);
            }
            else 
            {
                MonsterAgent.destination = MonsterBase.position;
                //ChangeAnimationState(CRAWLFORWARD);
            }

            if (HasFinishMoving() && !bHoldingResource)
            {
                ChangeAnimationState(TAUNT);
                CollectingTimer -= Time.deltaTime;
                if (CollectingTimer < 0)
                {
                    bHoldingResource = true;
                    MonsterAgent.destination = MonsterBase.position;
                    CollectingTimer = CollectingTime;
                    ChangeAnimationState(CRAWLFORWARD);
                }
            }
            else if (HasFinishMoving() && bHoldingResource) 
            {
                ChangeAnimationState(TAUNT);
                StockTimer -= Time.deltaTime;
                if (StockTimer < 0) 
                {
                    bHoldingResource = false;
                    MonsterAgent.destination = Mine.position;
                    StockTimer = StockTime;
                    ChangeAnimationState(CRAWLFORWARD);
                }
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
