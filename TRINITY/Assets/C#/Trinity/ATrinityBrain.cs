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
    ETrinityElement Element;

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
    }

    public void CastPrimarySpell (ETrinityElement currentElement)
    {
        GameObject spellPrefab;
        switch (currentElement)
        {
            case ETrinityElement.ETE_Fire: 
                {
                    spellPrefab = Instantiate(TrinitySpells.Fireball.gameObject, CastPos.position, Quaternion.identity);
                    break;
                }
            case ETrinityElement.ETE_Cold:
                {
                    spellPrefab = Instantiate(TrinitySpells.Icicles.gameObject, CastPos.position, Quaternion.identity);
                    break;
                }
            case ETrinityElement.ETE_Lightning:
                {
                    spellPrefab = Instantiate(TrinitySpells.LightningBeam.gameObject, CastPos.position, Quaternion.identity);
                    break;
                }
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
