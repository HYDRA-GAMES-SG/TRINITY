using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlantFSM : MonoBehaviour
{
    public enum PlantState
    {
        Hide, Idle, ATtack
    }

    public PlantState State;
    public Transform target;
    public float AttackDistance;

    public float waitTime;
    float wt;

    public float bulletFireTime;
    float bf;

    Animator animator;

    public float timeToHide;
    float tth;

    bool canHide;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        wt = waitTime;
        bf = bulletFireTime;
        tth = timeToHide;
    }

    // Update is called once per frame
    void Update()
    {

        float distance = Vector3.Distance(target.position, transform.position);
        Debug.Log(distance);
        Vector3 direction = (target.position - transform.position).normalized;
        wt -= Time.deltaTime;
        if (wt <= 0)
        {
            switch (State)
            {
                case PlantState.Hide:
                    if (distance <= AttackDistance)
                    {
                        State = PlantState.ATtack;
                    }
                    break;
                case PlantState.ATtack:
                    break;
                case PlantState.Idle:
                    if (distance <= AttackDistance)
                    {
                        tth = timeToHide;
                        State = PlantState.ATtack;
                    }

                    break;
            }
            wt = waitTime;
        }

        if (State == PlantState.ATtack)
        {
            if (distance <= AttackDistance)
            {
                tth = timeToHide;
                bf -= Time.deltaTime;
                if (bf <= 0)
                {
                    animator.SetBool("Attack", true);
                    bf = bulletFireTime;
                }
            }
            else
            {
                animator.SetBool("Idle", true);
                State = PlantState.Idle;
            }
        }

        if (State == PlantState.Idle)
        {
            tth -= Time.deltaTime;
            if (tth <= 0)
            {
                State = PlantState.Hide;
            }
        }


    }
}
