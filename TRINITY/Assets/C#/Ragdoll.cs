using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ragdoll : MonoBehaviour
{
    Rigidbody[] rigBody;
    Animator animator;
    UHealthComponent Health;
    UEnemyStatusComponent EnemyEntity;
    IEnemyController Controller;
    void Start()
    {
        rigBody = GetComponentsInChildren<Rigidbody>();
        Health = GetComponentInParent<UHealthComponent>();
        animator = GetComponentInParent<Animator>();
        EnemyEntity = GetComponentInParent<UEnemyStatusComponent>();
        Controller = GetComponentInParent<IEnemyController>();


        AddHitBoxScriptToAllParts();

        DeactiveRagdoll();
    }
    //if hitting the collider on the crab boss body part
    //it take damage and - the UHealthComponent helath
    void AddHitBoxScriptToAllParts()  //HitBox script is added to every part
    {
        foreach (var r in rigBody)
        {
            if (r.gameObject.GetComponent<HitBox>() == null)
            {
                HitBox hb = r.gameObject.AddComponent<HitBox>();
                hb.Health = Health;
                hb.EnemyStatus = EnemyEntity;
                hb.EnemyController = Controller;
            }
        }
    }
    public void DeactiveRagdoll()
    {
        foreach (var r in rigBody)
        {
            r.isKinematic = true;
        }
        animator.enabled = true;
    }
    public void ActivateRagdoll()
    {
        foreach (var r in rigBody)
        {
            r.isKinematic = false;
        }
        animator.enabled = false;
    }
}
