using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleCamera : MonoBehaviour
{
    [SerializeField] private float cameraSpeed = 1;
    [SerializeField] private float cameraSensitivity = 1;
    [SerializeField] private float cameraAcceleration = 2;

    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void LateUpdate()
    {
        if(Input.GetMouseButton(1))
        {
            this.transform.Translate(GetInput(), Space.Self);
            this.transform.Rotate(new Vector3(0 , Input.GetAxis("Mouse X") ,0 ) * cameraSensitivity * Time.deltaTime, Space.World);
            this.transform.Rotate(new Vector3(-Input.GetAxis("Mouse Y")  , 0 ,0 ) * cameraSensitivity * Time.deltaTime, Space.Self);  
        }

    }

    private Vector3 GetInput() 
    { 
        Vector3 inputVector = new Vector3();
        float acceleration = cameraSpeed * Time.deltaTime;

        if (Input.GetKey(KeyCode.W)) inputVector += new Vector3(0, 0 , 1);
       
        if (Input.GetKey(KeyCode.S)) inputVector += new Vector3(0, 0, -1);
        
        if (Input.GetKey(KeyCode.A)) inputVector += new Vector3(-1, 0, 0);
        
        if (Input.GetKey(KeyCode.D)) inputVector += new Vector3(1, 0, 0);

        if (Input.GetKey (KeyCode.E)) inputVector += new Vector3(0, 1, 0);
        
        if (Input.GetKey(KeyCode.Q)) inputVector += new Vector3(0, -1, 0);

        if (Input.GetKey(KeyCode.LeftShift)) acceleration *= cameraAcceleration;
        
        return inputVector * acceleration;
    }

}
