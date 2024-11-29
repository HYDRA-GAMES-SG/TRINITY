using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ETrinityAction 
{
    ETA_None,
    ETA_Stunned,
    ETA_IFrame,
    ETA_Casting,
    ETA_Channeling
}
public enum ETrinityElement 
{
    ETE_Fire,
    ETE_Cold,
    ETE_Lightning,
}

public class ATrinityBrain : MonoBehaviour
{
    public APlayerInput InputReference; //reference
    public ATrinityCharacter TrinityCharacter; //reference
    public ATrinitySpells TrinitySpells; //reference
    public ATrinityController TrinityController; //reference
    public GameObject Beam;
    ETrinityElement Element;
    ETrinityAction Action;

    public Transform CastPos;

    // Start is called before the first frame update
    void Start()
    {
        Element = ETrinityElement.ETE_Fire;
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0)) //need to use InputReference
        {
            CastPrimarySpell(Element);
        }
        if (Input.GetMouseButtonUp(0)) 
        {
            Action = ETrinityAction.ETA_None;
            if (Beam != null) 
            {
                Beam.SetActive(false);
            }
        }

        ToggleElement();
    }

    public void CastPrimarySpell (ETrinityElement currentElement)
    {
        if (TrinitySpells.PrimaryCooldown > 0)
        {
            return;
        }
        GameObject spellPrefab;
        switch (currentElement)
        {
            case ETrinityElement.ETE_Fire: 
                {
                    PrimaryFire fireball = TrinitySpells.Fireball;
                    spellPrefab = Instantiate(fireball.gameObject, CastPos.position, Quaternion.identity);
                    TrinitySpells.PrimaryCooldown = fireball.Cooldown;
                    break;
                }
            case ETrinityElement.ETE_Cold:
                {
                    //spellPrefab = Instantiate(TrinitySpells.Icicles.gameObject, CastPos.position, Quaternion.identity);
                    break;
                }
            case ETrinityElement.ETE_Lightning:
                {
                    Action = ETrinityAction.ETA_Casting;

                    if (Beam == null)
                    {
                        spellPrefab = Instantiate(TrinitySpells.LightningBeam.gameObject, CastPos.position, Quaternion.Euler(0, 90, 0));
                        spellPrefab.transform.parent = TrinityController.transform;
                        Beam = spellPrefab;
                    }
                    else 
                    {
                        Beam.SetActive(true);
                    }
                    break;
                }
        }
    }
    public void CastSecondarySpell(ETrinityElement currentElement) { }
    public void CastUtilitySpell(ETrinityElement currentElement) { }
    public void ToggleElement() 
    {
        if (Input.GetAxis("Mouse ScrollWheel") > 0f)
        {
            NextElement();
            print(Element);
        }
        else if (Input.GetAxis("Mouse ScrollWheel") < 0f)
        {
            PreviousElement();
            print(Element);
        }
    }
    public void NextElement() 
    {
        switch (Element)
        {
            case ETrinityElement.ETE_Fire: 
                {
                    Element = ETrinityElement.ETE_Cold;
                    break;
                }
            case ETrinityElement.ETE_Cold:
                {
                    Element = ETrinityElement.ETE_Lightning;
                    break;
                }
            case ETrinityElement.ETE_Lightning:
                {
                    Element = ETrinityElement.ETE_Fire;
                    break;
                }
        }
    }
    public void PreviousElement() 
    {
        switch (Element)
        {
            case ETrinityElement.ETE_Fire:
                {
                    Element = ETrinityElement.ETE_Lightning;
                    break;
                }
            case ETrinityElement.ETE_Cold:
                {
                    Element = ETrinityElement.ETE_Fire;
                    break;
                }
            case ETrinityElement.ETE_Lightning:
                {
                    Element = ETrinityElement.ETE_Cold;
                    break;
                }
        }
    }
}
