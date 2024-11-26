using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ragdoll : MonoBehaviour
{
    Rigidbody[] rigBody;
    Animator animator;
    UHealthComponent Health;
    void Start()
    {
        rigBody = GetComponentsInChildren<Rigidbody>();
        Health = GetComponentInParent<UHealthComponent>();
        AddHitBox();
        animator = GetComponent<Animator>();
        DeactiveRagdoll();
    }
    void AddHitBox()
    {
        foreach (var r in rigBody)
        {
            if (r.gameObject.GetComponent<HitBox>() == null)
            {
                HitBox hb = r.gameObject.AddComponent<HitBox>();
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
