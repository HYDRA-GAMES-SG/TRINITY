using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ATrinityAnimator : MonoBehaviour
{
    public bool ENABLE_DEBUG = false;
    [HideInInspector]
    public Animator AnimComponent;
    private bool bChanneling = false;
    private bool bMasked = true;
    
    private int MaskedLayerIndex = 1;
    public float MaskedLayerWeight = .7f;
    private int UnmaskedLayerIndex = 2;
    public float UnmaskedLayerWeight = 1f;

    private bool bUnmaskedLayerIdle =>
        AnimComponent.GetCurrentAnimatorStateInfo(UnmaskedLayerIndex).normalizedTime >= 1f ||
        AnimComponent.GetCurrentAnimatorStateInfo(UnmaskedLayerIndex).IsName("Null");
    
    private bool bMaskedLayerIdle =>
        AnimComponent.GetCurrentAnimatorStateInfo(MaskedLayerIndex).normalizedTime >= 1f ||
        AnimComponent.GetCurrentAnimatorStateInfo(MaskedLayerIndex).IsName("Null");
    // Start is called before the first frame update
    void Start()
    {
        
        AnimComponent = GetComponent<Animator>();
        AnimComponent.SetLayerWeight(MaskedLayerIndex, 0f);
        AnimComponent.SetLayerWeight(UnmaskedLayerIndex, 0f);
        bChanneling = false;
        bMasked = false;

    }

    // Update is called once per frame
    void Update()
    {
        if (bMasked)
        {
            if (bMaskedLayerIdle && !bChanneling)
            {
                AnimComponent.SetLayerWeight(MaskedLayerIndex, 0f);
            }
            else
            {
                AnimComponent.SetLayerWeight(MaskedLayerIndex, MaskedLayerWeight);
            }
        }
        else
        {
            if (bUnmaskedLayerIdle && !bChanneling)
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
        AnimComponent.Play(stateName, MaskedLayerIndex, 0f);
    }

    public void PlayChannelAnimation(string stateName, bool bMask = true)
    {
        bMasked = bMask;
        bChanneling = true;
        if (bMask)
        {
            AnimComponent.Play(stateName, MaskedLayerIndex, 0f);
        }
        else
        {
            AnimComponent.Play(stateName, UnmaskedLayerIndex, 0f);
        }
        
    }

    public void ReleaseAnimation()
    {
        bChanneling = false;
    }
}
