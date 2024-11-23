using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class APlayerInput : MonoBehaviour
{
    public Camera CameraReference;
    public InputActionAsset InputActions;
    private PlayerInput InputReference;
    // Start is called before the first frame update
    void Start()
    {
        InputReference = GetComponent<PlayerInput>();

        if (!InputReference)
        {
            Debug.Log("APlayerInput: PlayerInput component null or missing.");
        }
        else
        {
            InputReference.camera = CameraReference;
            
            if (InputActions)
            {
                InputReference.actions = InputActions;
            }
            else
            {
                Debug.Log("APlayerInput: InputActions unassigned.");
            }
            
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
