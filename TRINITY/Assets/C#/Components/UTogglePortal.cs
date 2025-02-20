using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UTogglePortal : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        if (gameObject.name == "CrabPortal" && ATrinityGameManager.GetCrabDefeated() && !ATrinityGameManager.GetDSDefeated())
        {
            gameObject.SetActive(false);
        }
        else if (gameObject.name == "CrabPortal" && ATrinityGameManager.GetCrabDefeated() && ATrinityGameManager.GetDSDefeated())
        {
            gameObject.SetActive(true);
        }
        else if (gameObject.name == "CrabPortal" && !ATrinityGameManager.GetCrabDefeated() && !ATrinityGameManager.GetDSDefeated())
        {
            gameObject.SetActive(true);
        }

        if (gameObject.name == "DSPortal" && ATrinityGameManager.GetCrabDefeated())
        {
            gameObject.SetActive(true);
        }
        else if (gameObject.name == "DSPortal" && ATrinityGameManager.GetCrabDefeated() && ATrinityGameManager.GetDSDefeated())
        {
            gameObject.SetActive(true);
        }
        else if(gameObject.name == "DSPortal" && !ATrinityGameManager.GetCrabDefeated() && !ATrinityGameManager.GetDSDefeated())
        {
            gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
