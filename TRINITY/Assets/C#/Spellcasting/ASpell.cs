using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ESpellType
{
    EST_Primary,
    EST_Secondary,
    EST_Utility
}
[RequireComponent(typeof(AudioSource))]
public class ASpell : MonoBehaviour
{
    public GameObject SpellPrefab;
    public float Cooldown;
    public float ManaCost = 0f;
    public float ManaUpkeepCost = 0f;
    public ETrinityElement SpellElement;
    public ESpellType SpellType;
    public ETrinityAction SpellAction;

    [Header("Anim Properties")]
    public bool bUseMaskedLayer = true;
    
    [HideInInspector]
    public bool bSpellReady => CooldownCountdownTimer <= 0f;

    private float CooldownCountdownTimer = 0f;
    private ATrinityBrain BrainReference;

    public void Start()
    {
        Initialize();
        BrainReference = ATrinityGameManager.GetBrain();
    }

    public virtual void Initialize()
    {
        
    }

    public void Update()
    {
        UpdateCooldown();
        
        if (ATrinityGameManager.GetSpells().ManaComponent.Current < ManaUpkeepCost * Time.deltaTime)
        {
            Release();
        }
        
        if (BrainReference.GetCurrentSpell() == this || (this is AForcefield && BrainReference.bForcefieldActive))
        {
            ATrinityGameManager.GetSpells().ManaComponent.Modify(-ManaUpkeepCost * Time.deltaTime);
            CastUpdate();
        }
    }

    protected void UpdateCooldown()
    {
        CooldownCountdownTimer -= Time.deltaTime;
    }

    public void StartCooldown()
    {
        CooldownCountdownTimer = Cooldown;
    }
    

    public void Cast()
    {
        if (!bSpellReady || ATrinityGameManager.GetSpells().ManaComponent.Current < ManaCost)
        {
            //print("Spell not ready.");
            return;
        }

        if (SpellAction != ETrinityAction.ETA_None && BrainReference.GetAction() != ETrinityAction.ETA_None)
        {
            //print("Cannot cast or channel yet.");
            return;
        }

        if (BrainReference.GetCurrentSpell() != null)
        {
            BrainReference.GetCurrentSpell().Release();
        }
        
        if (SpellAction != ETrinityAction.ETA_None)
        {
            BrainReference.SetCurrentSpell(this);
        }
        
        if (SpellAction == ETrinityAction.ETA_Channeling)
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
            print("non channeled and masked");
            ATrinityGameManager.GetAnimator().PlayCastAnimation($"Masked Layer.{gameObject.name}");
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
}