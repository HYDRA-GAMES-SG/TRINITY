using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

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
    public IAA_TrinityControls TrinityControls;
    public GameObject Beam;
    ETrinityElement Element;
    ETrinityAction Action;

    public Transform CastPos;

    public event Action<ETrinityElement> OnElementChanged;

    // Start is called before the first frame update
    void Start()
    {
        
        Element = ETrinityElement.ETE_Fire;
        Cursor.lockState = CursorLockMode.Locked;
        
        InputReference.OnElementPressed += ChangeElement;
    }

    void Destroy()
    {
        InputReference.OnElementPressed -= ChangeElement;
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
        }
        else if (Input.GetAxis("Mouse ScrollWheel") < 0f)
        {
            PreviousElement();
        }
    }
    
    public void NextElement()
    {
        int intElement = (int)Element;
        intElement++;
        ETrinityElement newElement = (ETrinityElement)(intElement % Enum.GetValues(typeof(ETrinityElement)).Length);
        ChangeElement(newElement);
        OnElementChanged?.Invoke(newElement);
    }

    public void PreviousElement()
    {
        int intElement = (int)Element;
        intElement--;
        if (intElement < 0)
        {
            intElement = Enum.GetValues(typeof(ETrinityElement)).Length - 1;
        }
        ETrinityElement newElement = (ETrinityElement)intElement;
        ChangeElement(newElement);
        OnElementChanged?.Invoke(newElement);
    }

    public void ChangeElement(ETrinityElement newElement)
    {
        Element = newElement;
        OnElementChanged?.Invoke(Element);
    }
    
}
