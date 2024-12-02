using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ASpell : MonoBehaviour
{
    public GameObject SpellPrefab;
    protected ATrinityBrain Brain;
    protected ATrinitySpells Spells;
    public float Cooldown;
    public ETrinityElement Element;
    public ESpellType Type;
    
    [HideInInspector]
    public bool bSpellReady => CooldownTimer <= 0f;

    private float CooldownTimer = 0f;


    void Update()
    {

    }

    protected void UpdateCooldown()
    {
        if(CooldownTimer > Cooldown)
        {
            CooldownTimer -= Time.deltaTime;
        }
    }

    public void StartCooldown()
    {
        CooldownTimer = Cooldown;
    }
    
    public virtual void Start()
    {
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