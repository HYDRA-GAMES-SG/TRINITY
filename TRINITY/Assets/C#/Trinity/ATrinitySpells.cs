using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UHealthComponent))]
[RequireComponent(typeof(UManaComponent))]
public class ATrinityCharacter : MonoBehaviour
{
    public List<ASpell> Spellbook;
    public ATrinityBrain Brain;
    public Transform CastPos;

    private APlayerInput InputReference;
    
    [Header("Spells")]
    public PrimaryFire Fireball;
    public PrimaryCold Icicles;
    public PrimaryLightning LightningBeam;
    public UManaComponent ManaComponent;
    public UHealthComponent HealthComponent;
    

    // Start is called before the first frame update
    void Start()
    {
        HealthComponent = GetComponent<UHealthComponent>();
        ManaComponent = GetComponent<UManaComponent>();
        InputReference = Brain.GetComponent<APlayerInput>();
        Spellbook = new List<ASpell>();
        Spellbook.Add(Fireball);
        //Spellbook.Add(Icicles);
        Spellbook.Add(LightningBeam);
        
        
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void CastPrimary()
    {
        switch (Brain.GetElement())
        {
            case ETrinityElement.ETE_Cold:
                break;
            case ETrinityElement.ETE_Fire:
                Fireball.CastStart();
                break;
            case ETrinityElement.ETE_Lightning:
                LightningBeam.CastStart();
                break;
        }
    }
    
    public void CastSecondary()
    {
        switch (Brain.GetElement())
        {
            case ETrinityElement.ETE_Cold:
                break;
            case ETrinityElement.ETE_Fire:
                break;
            case ETrinityElement.ETE_Lightning:
                break;
        }
        
    }
    
    public void CastUtility()
    {
        switch (Brain.GetElement())
        {
            case ETrinityElement.ETE_Cold:
                break;
            case ETrinityElement.ETE_Fire:
                break;
            case ETrinityElement.ETE_Lightning:
                break;
        }
        
    }
    

}
