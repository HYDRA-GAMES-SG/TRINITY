using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;


public class ChangeLevel_Demonstration : MonoBehaviour
{

    void Update()
    {
        if (Input.GetKeyDown (KeyCode.L))
        {
            Application.LoadLevel("Scene_Demonstration");
        }
    }
}