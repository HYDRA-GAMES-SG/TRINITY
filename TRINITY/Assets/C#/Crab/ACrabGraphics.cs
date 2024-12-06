using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss : MonoBehaviour
{
    Rigidbody[] rigBody;
    Animator animator;
    UHealthComponent Health;
    void Start()
    {
        rigBody = GetComponentsInChildren<Rigidbody>();
        Health = GetComponentInParent<UHealthComponent>();
        animator = GetComponentInParent<Animator>();

        AddHitBoxScriptToAllParts();

        DeactiveRagdoll();
    }
    //if hitting the collider on the crab boss body part
    //it take damage and - the UHealthComponent helath
    void AddHitBoxScriptToAllParts()  //HitBox script is added to every part
    {
        foreach (var r in rigBody)
        {
            if (r.gameObject.GetComponent<ACrabHitBox>() == null)
            {
                ACrabHitBox hb = r.gameObject.AddComponent<ACrabHitBox>();
                hb.Health = Health;
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
