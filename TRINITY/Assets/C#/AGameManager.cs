using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AGameManager : MonoBehaviour
{
    [HideInInspector]
    static public ATrinityCharacter PlayerCharacter;

    [HideInInspector]
    static public ATrinityController PlayerController;

    [HideInInspector]
    static public GameObject Boss;

    // Start is called before the first frame update
    void Start()
    {
        PlayerCharacter = FindObjectOfType<ATrinityCharacter>();
        PlayerController = FindObjectOfType<ATrinityController>();
    }

    // Update is called once per frame
    void Update()
    {
    }
    
    public static void SetBoss(GameObject bossObject)
    {
        Boss = bossObject;
    }
}
