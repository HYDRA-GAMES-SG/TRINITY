using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ESpellType
{
    EST_Primary,
    EST_Secondary,
    EST_Utility
}

public class USpellComponent : MonoBehaviour
{
    public float Cooldown;
    public ETrinityElement Element;
    public ESpellType Type;
    public bool bSpellReady => CooldownTimer <= 0f;

    private float CooldownTimer = 0f;


    void Update()
    {
        UpdateCooldown();

    }

    public bool IsMatch(ETrinityElement searchElement, ESpellType searchSpell)
    {
        if(Element == searchElement)
        {
            if(Type == searchSpell)
            {
                return true;
            }
        }

        return false;
    }

    private void UpdateCooldown()
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
}
