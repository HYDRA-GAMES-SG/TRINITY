using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ESpellType
{
    EST_Primary,
    EST_Utility,
    EST_Secondary
}

public class ASpell : MonoBehaviour
{
    public GameObject SpellPrefab;
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

        if (SpellAction != ETrinityAction.ETA_None)
        {
            BrainReference.SetCurrentSpell(this);    
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
        if (SpellAction != ETrinityAction.ETA_None)
        {
            BrainReference.SetCurrentSpell(null);    
        }

        if (BrainReference.GetAction() == ETrinityAction.ETA_Channeling || BrainReference.GetAction() == ETrinityAction.ETA_Casting)
        {
            BrainReference.ChangeAction(ETrinityAction.ETA_None);
        }

        CastEnd();
    }
}