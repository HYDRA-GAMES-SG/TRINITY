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
    protected ATrinityAnimator AnimationReference;
    protected ATrinityController Controller;
    protected ATrinityBrain BrainReference;
    protected ATrinitySpells SpellsReference;
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


    public void Start()
    {
        Controller = transform.root.Find("Controller").GetComponent<ATrinityController>();
        BrainReference = transform.root.Find("Brain").GetComponent<ATrinityBrain>();
        SpellsReference = transform.parent.GetComponent<ATrinitySpells>();
        AnimationReference = Controller.transform.Find("Graphics").GetComponent<ATrinityAnimator>();
        Initialize();
    }

    public virtual void Initialize()
    {
        
    }

    public void Update()
    {
        UpdateCooldown();
        
        SpellsReference.ManaComponent.Modify(-ManaUpkeepCost * Time.deltaTime);

        if (BrainReference.GetCurrentSpell() == this)
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
        CooldownCountdownTimer = Cooldown;
    }
    

    public void Cast()
    {
        if (!bSpellReady)
        {
            //print("Spell not ready.");
            return;
        }

        if (SpellAction != ETrinityAction.ETA_None && BrainReference.GetAction() != ETrinityAction.ETA_None)
        {
            //print("Cannot cast or channel yet.");
            return;
        }

        if (SpellAction != ETrinityAction.ETA_None)
        {
            BrainReference.SetCurrentSpell(this);
        }
        
        if (SpellAction == ETrinityAction.ETA_Channeling)
        {
            bool bShouldMask = bUseMaskedLayer ||!Controller.CheckGround().transform;
            
            if (bShouldMask)
            {
                AnimationReference.PlayChannelAnimation($"Masked Layer.{gameObject.name}", bShouldMask);
            }
            else
            {
                AnimationReference.PlayChannelAnimation($"Unmasked Layer.{gameObject.name}", bShouldMask);
            }
        }
        else
        {
            AnimationReference.PlayCastAnimation($"Masked Layer.{gameObject.name}");
        }

        SpellsReference.ManaComponent.Modify(-ManaCost);
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
        BrainReference.SetCurrentSpell(null);    

        if (BrainReference.GetAction() == ETrinityAction.ETA_Channeling)
        {
            AnimationReference.ReleaseAnimation();
        }

        if (BrainReference.GetAction() == ETrinityAction.ETA_Casting)
        {
            AnimationReference.ReleaseAnimation();
        }
        
        BrainReference.ChangeAction(ETrinityAction.ETA_None);

        CastEnd();
    }
}