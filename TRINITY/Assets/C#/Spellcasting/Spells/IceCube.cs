using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceCube : MonoBehaviour
{
    public MeshRenderer Mesh;
    
    private int InitialChillStacks;
    private int ChillStacksPerSecond;
    [HideInInspector]
    public float Duration;
    
    // Start is called before the first frame update
    void Start()
    {
        Duration = ATrinityGameManager.GetSpells().SecondaryCold.Duration;
        InitialChillStacks = ATrinityGameManager.GetSpells().SecondaryCold.InitialChillStacks;
        ChillStacksPerSecond = ATrinityGameManager.GetSpells().SecondaryCold.ChillStacksPerSecond;
    }

    // Update is called once per frame
    void Update()
    {
        Duration -= Time.deltaTime;

        if (Duration <= 0f)
        {
            Reset();
        }

    }

    void Reset()
    {
        gameObject.transform.localScale = Vector3.one;
        gameObject.SetActive(false);
    }

}
