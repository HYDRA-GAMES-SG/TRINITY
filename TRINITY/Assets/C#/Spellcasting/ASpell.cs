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
    public ETrinityElement SpellElement;
    public ESpellType SpellType;
    public ETrinityAction SpellAction;
    
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
        
        BrainReference.SetCurrentSpell(this);
        if (SpellAction != ETrinityAction.ETA_Channeling && SpellAction != ETrinityAction.ETA_Casting)
        {
            AnimationReference.PlayCastAnimation($"Casting Layer.{gameObject.name}");
        }
        else
        {
            AnimationReference.PlayChannelAnimation($"Casting Layer.{gameObject.name}");
        }
        
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

        if (BrainReference.GetAction() == ETrinityAction.ETA_Channeling || BrainReference.GetAction() == ETrinityAction.ETA_Casting)
        {
            AnimationReference.ReleaseChannelAnimation($"Casting Layer.{gameObject.name} Release");
            BrainReference.ChangeAction(ETrinityAction.ETA_None);
        }

        CastEnd();
    }
}