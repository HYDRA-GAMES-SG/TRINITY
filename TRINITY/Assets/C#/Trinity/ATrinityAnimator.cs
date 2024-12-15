using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ATrinityAnimator : MonoBehaviour
{
    private float CastingLayerWeight = .7f;
    private int CastingLayerIndex = 1;
    private bool bChanneling = false;
    private Animator AnimComponent;
    // Start is called before the first frame update
    void Start()
    {
        AnimComponent = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
            if(AnimComponent.GetCurrentAnimatorStateInfo(CastingLayerIndex).normalizedTime >= 1f && !bChanneling)
            {
                AnimComponent.SetLayerWeight(CastingLayerIndex, 0f);
            }
            else
            {
                AnimComponent.SetLayerWeight(CastingLayerIndex, CastingLayerWeight);
            }
    }

    public void PlayCastAnimation(string stateName)
    {
        AnimComponent.Play(stateName, CastingLayerIndex, 0f);
    }

    public void PlayChannelAnimation(string stateName)
    {
        AnimComponent.Play(stateName, CastingLayerIndex, 0f);
        bChanneling = true;
    }

    public void ReleaseChannelAnimation(string stateName)
    {
        AnimComponent.Play(stateName, CastingLayerIndex, 0f);
        bChanneling = false;
    }
}
