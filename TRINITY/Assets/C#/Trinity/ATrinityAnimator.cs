using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ATrinityAnimator : MonoBehaviour
{
    public bool ENABLE_DEBUG = false;
    [HideInInspector]
    public Animator AnimComponent;
    public bool bChanneling = false;
    public bool bMasked = true;
    
    private int CastingLayerIndex = 1;
    private float CastingLayerWeight = .7f;
    private int UnmaskedLayerIndex = 2;
    private float UnmaskedLayerWeight = 1f;
    // Start is called before the first frame update
    void Start()
    {
        
        AnimComponent = GetComponent<Animator>();
        AnimComponent.SetLayerWeight(CastingLayerIndex, 0f);
        AnimComponent.SetLayerWeight(UnmaskedLayerIndex, 0f);

    }

    // Update is called once per frame
    void Update()
    {
        if (bMasked)
        {
            if (AnimComponent.GetCurrentAnimatorStateInfo(CastingLayerIndex).normalizedTime >= 1f && !bChanneling)
            {
                AnimComponent.SetLayerWeight(CastingLayerIndex, 0f);
            }
            else
            {
                AnimComponent.SetLayerWeight(CastingLayerIndex, CastingLayerWeight);
            }
        }
        else
        {
            if (AnimComponent.GetCurrentAnimatorStateInfo(UnmaskedLayerIndex).normalizedTime >= 1f && !bChanneling)
            {
                AnimComponent.SetLayerWeight(UnmaskedLayerIndex, 0f);
            }
            else
            {
                AnimComponent.SetLayerWeight(UnmaskedLayerIndex, UnmaskedLayerWeight);
            }
        }
    }

    public void PlayCastAnimation(string stateName)
    {
        bMasked = true;
        bChanneling = false;
        AnimComponent.Play(stateName, CastingLayerIndex, 0f);
    }

    public void PlayChannelAnimation(string stateName, bool bMask = true)
    {
        bMasked = bMask;
        bChanneling = true;
        if (bMask)
        {
            AnimComponent.Play(stateName, CastingLayerIndex, 0f);
        }
        else
        {
            AnimComponent.Play(stateName, UnmaskedLayerIndex, 0f);
        }
        
    }

    public void ReleaseChannelAnimation(string stateName)
    {
        AnimComponent.Play(stateName, CastingLayerIndex, 0f);
        bChanneling = false;
    }

    public void ReleaseCastAnimation(string stateName)
    {
        AnimComponent.Play(stateName, CastingLayerIndex, 0f);
        bChanneling = false;
    }
}
