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
    protected ATrinityBrain Brain;
    protected ATrinitySpells Spells;
    public float Cooldown;
    public ETrinityElement SpellElement;
    public ESpellType SpellType;
    public ETrinityAction SpellAction;
    
    [HideInInspector]
    public bool bSpellReady => CooldownCountdownTimer <= 0f;

    private float CooldownCountdownTimer = 0f;


    public void Start()
    {
        
        Brain = transform.root.Find("Brain").GetComponent<ATrinityBrain>();
        Spells = transform.parent.GetComponent<ATrinitySpells>();
        Initialize();
    }

    public virtual void Initialize()
    {
        
    }

    public void Update()
    {
        UpdateCooldown();
        CastUpdate();

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

        if (SpellAction != ETrinityAction.ETA_None && Brain.GetAction() != ETrinityAction.ETA_None)
        {
            //print("Cannot cast or channel yet.");
            return;
        }

        if (SpellAction != ETrinityAction.ETA_None)
        {
            Brain.SetCurrentSpell(this);    
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
}