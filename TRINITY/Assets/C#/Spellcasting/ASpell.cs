using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class ASpell : MonoBehaviour
{
    public static bool bTutorialComplete = false;

    public GameObject SpellPrefab;
    public float Cooldown;
    public float ManaCost = 0f;
    public float ManaUpkeepCost = 0f;
    public ETrinityElement SpellElement;
    public ESpellType SpellType;
    public ETrinityAction SpellAction;
    protected float ManaToComplete;

    [Header("Anim Properties")]
    public bool bUseMaskedLayer = true;

    [HideInInspector]
    public bool bSpellReady => CooldownCountdownTimer <= 0f;

    [SerializeField]private float CooldownCountdownTimer = 0f;
    private ATrinityBrain BrainReference;
    private AudioSource AudioSource;

    public System.Action OnNotEnoughMana;
    public System.Action OnSpellNotReady;
    public void Start()
    {
        Initialize();
        BrainReference = ATrinityGameManager.GetBrain();
        AudioSource = GetComponent<AudioSource>();
        AudioSource.outputAudioMixerGroup = ATrinityGameManager.GetAudioMixerGroup(EAudioGroup.EAG_SFX);
        OnNotEnoughMana += ATrinityGameManager.GetGUI().ShowNoMana;
        //OnSpellNotReady += ATrinityGameManager.GetGUI().SpellOnCooldown;
    }

    public virtual void Initialize()
    {

    }

    public void Update()
    {
        if (ATrinityGameManager.GetGameFlowState() == EGameFlowState.PAUSED)
        {
            return;
        }

        UpdateCooldown();

        if (ATrinityGameManager.GetGameFlowState() == EGameFlowState.DEAD)
        {
            if (BrainReference.GetCurrentSpell())
            {
                BrainReference.GetCurrentSpell().Release();
                BrainReference.SetCurrentSpell(null);
            }
            return;
        }

        if (BrainReference.GetCurrentSpell() == this && ManaUpkeepCost > 0 && ATrinityGameManager.GetSpells().ManaComponent.Current < 1)
        {
            Release();
        }

        if (BrainReference.GetCurrentSpell() == this || (this is AForcefield && BrainReference.bForcefieldActive))
        {
            ATrinityGameManager.GetSpells().ManaComponent.Modify(-ManaUpkeepCost * Time.deltaTime);
            CastUpdate();
        }

        if (this is AUtilityFire)
        {
            CastUpdate();
        }

        if (this is AUtilityCold && ATrinityGameManager.GetSpells().UtilityCold.bActive == true)
        {
            CastUpdate();
        }
    }

    protected void UpdateCooldown()
    {
        CooldownCountdownTimer -= Time.deltaTime;
    }

    public void StartCooldown()
    {
        if (this is AUtilityLightning)
        {
            AUtilityLightning aUtilityLightning = (AUtilityLightning)this;
            if (aUtilityLightning.bCanBlink)
            {
                return;
            }
        }
        CooldownCountdownTimer = Cooldown;
    }


    public void Cast()
    {
        bool bNotEnoughMana = ATrinityGameManager.GetSpells().ManaComponent.Current < ManaCost;
        bool bUnableToFinishChannel = ATrinityGameManager.GetSpells().ManaComponent.Current < ManaToComplete;
        if (!bSpellReady) 
        {
            OnSpellNotReady?.Invoke();
            return;
        }
        if (bNotEnoughMana || bUnableToFinishChannel)
        {
            OnNotEnoughMana?.Invoke();
            return;
        }

        if (SpellAction != ETrinityAction.ETA_None && BrainReference.GetAction() != ETrinityAction.ETA_None)
        {
            Debug.Log("Cannot cast or channel yet.");
            return;
        }

        if (BrainReference.GetCurrentSpell() != null && this is not AForcefield)
        {
            BrainReference.GetCurrentSpell().Release();
        }

        if (SpellAction != ETrinityAction.ETA_None)
        {
            BrainReference.SetCurrentSpell(this);
        }

        if (SpellAction == ETrinityAction.ETA_Channeling || this is AUtilityCold)
        {
            bool bShouldMask = bUseMaskedLayer || !ATrinityGameManager.GetPlayerController().CheckGround().transform;

            if (bShouldMask)
            {
                ATrinityGameManager.GetAnimator().PlayChannelAnimation($"Masked Layer.{gameObject.name}", bShouldMask);
            }
            else
            {
                ATrinityGameManager.GetAnimator().PlayChannelAnimation($"Unmasked Layer.{gameObject.name}", bShouldMask);
            }
        }
        else
        {
            if (this is not AForcefield)
            {
                //Debug.Log("non channeled and masked");
                ATrinityGameManager.GetAnimator().PlayCastAnimation($"Masked Layer.{gameObject.name}");
            }
        }

        ATrinityGameManager.GetSpells().ManaComponent.Modify(-ManaCost);
        CastStart();
        StartCooldown();
    }

    public virtual void CastStart()
    {
    }

    public virtual void CastUpdate()
    {

    }

    public virtual void CastEnd()
    {
    }

    public virtual void Release()
    {

        if (BrainReference.GetAction() == ETrinityAction.ETA_Channeling || BrainReference.GetAction() == ETrinityAction.ETA_Casting)
        {
            ATrinityGameManager.GetAnimator().ReleaseAnimation();
        }

        BrainReference.SetCurrentSpell(null);

        CastEnd();
    }

    public float GetCooldownNormalized()
    {
        if (Cooldown == 0f || bSpellReady)
        {
            return 0f;
        }

        return Mathf.Clamp(CooldownCountdownTimer / Cooldown, 0f, 1f);
    }

    private void OnDestroy()
    {
        OnNotEnoughMana -= ATrinityGameManager.GetGUI().ShowNoMana;
        //OnSpellNotReady -= ATrinityGameManager.GetGUI().SpellOnCooldown;
    }

}