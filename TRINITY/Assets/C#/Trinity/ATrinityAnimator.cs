using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ATrinityAnimator : MonoBehaviour
{
    public AvatarMask UpperMask;
    private float CastingLayerWeight = .7f;
    private float UnmaskedLayerWeight = 1f;
    private int CastingLayerIndex = 1;
    private int UnmaskedLayerIndex = 2;
    private bool bChanneling = false;
    private bool bMasked = true;
    private Animator AnimComponent;
    
    // Start is called before the first frame update
    void Start()
    {
        AnimComponent = GetComponent<Animator>();
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
        AnimComponent.Play(stateName, CastingLayerIndex, 0f);
    }

    public void PlayChannelAnimation(string stateName, bool bMask = true)
    {
        bMasked = bMask;
        if (bMask)
        {
            AnimComponent.Play(stateName, CastingLayerIndex, 0f);
        }
        else
        {
            AnimComponent.Play(stateName, UnmaskedLayerIndex, 0f);
        }
        
        bChanneling = true;
    }

    public void ReleaseChannelAnimation(string stateName, bool bMask = true)
    {
        bMasked = bMask;
        if (bMask)
        {
            AnimComponent.Play(stateName, CastingLayerIndex, 0f);
        }
        else
        {
            AnimComponent.Play(stateName, UnmaskedLayerIndex, 0f);
        }

        bChanneling = false;
    }
}
