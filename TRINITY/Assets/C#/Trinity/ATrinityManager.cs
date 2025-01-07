using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ATrinityManager : MonoBehaviour
{
    private static ATrinityController PlayerController;
    private static ATrinitySpells SpellsController;

    void Awake()
    {
        List<ATrinityManager> currentInstances = FindObjectsOfType<ATrinityManager>().ToList();
        
        if (currentInstances.Count() > 1)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
    }

    public static ATrinityController GetPlayer()
    {
        return PlayerController;
    }

    public static ATrinitySpells GetSpells()
    {
        return SpellsController;
    }

    public static void SetPlayer(ATrinityController player)
    {
        if (PlayerController != null)
        {
            Debug.Log("Static Player Ref Not Null");
            return;
        }
        
        PlayerController = player;
    }

    public static void SetSpells(ATrinitySpells spells)
    {
        if (SpellsController != null)
        {
            Debug.Log("Static Spells Ref Not Null");
            return;
        }
        
        SpellsController = spells;
    }
}
