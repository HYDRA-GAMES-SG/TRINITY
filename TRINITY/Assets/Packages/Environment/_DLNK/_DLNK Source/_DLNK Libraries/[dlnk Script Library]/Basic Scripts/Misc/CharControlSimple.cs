using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharControlSimple : MonoBehaviour
{
    [Header("[Char Properties]")]
    public float WalkSpeed = 5f;
    public KeyCode RunKey = KeyCode.LeftShift;
    public float RunSpeed = 10f;
    public float RotateSpeed = 3f;
    public float JumpSpeed = 5f;
    public float Gravity = 100f;
    public bool Invisible;
    public KeyCode InvisibleKey = KeyCode.I;
    public KeyCode LightKey = KeyCode.F;
    public GameObject LightFocus;
    public bool CameraFollow = true;
    public KeyCode FollowKey = KeyCode.K;
    [Header("[Char Source]")]
    public CharacterController Charcon;
    public GameObject jumpEffect;
    public GameObject Camera;
    public GameObject CharacterMesh;
    private Vector3 CamRot;
    private Vector3 Jander = new Vector3(0f,0f,0f);

    // Start is called before the first frame update
    void Start()
    {
        // Set camera if not defined
        if (Camera == null)
        {
            Camera = GameObject.FindWithTag("MainCamera");
        }
        // Get original transform
        CamRot = Camera.transform.eulerAngles;
    }

    // Update is called once per frame
    void Update()
    {
        // Get Values
        CharacterController controller = GetComponent<CharacterController>();
        float moveHorizontal = Input.GetAxis("Horizontal");
        Jander = new Vector3(0, 0, Input.GetAxis("Vertical"));
        Jander = transform.TransformDirection(Jander);
        if (Input.GetKey(RunKey))
            Jander *= RunSpeed;
        else     Jander *= WalkSpeed;
        // Check camera active
        if (Camera != GameObject.FindWithTag("MainCamera"))
        {
            Camera = GameObject.FindWithTag("MainCamera");
        }
        // Jump action
        if (Input.GetButton("Jump"))
        {
            Jander.y = JumpSpeed;
            jumpEffect.SetActive(true);
        }
        else jumpEffect.SetActive(false);
        // Define movement
        Jander.y -= Gravity * Time.deltaTime;
        controller.Move(Jander * Time.deltaTime);
        float diferencia = (Camera.transform.eulerAngles.y - CamRot.y);
        // Move character
        if (!CameraFollow)
        gameObject.transform.Rotate(0,moveHorizontal*RotateSpeed,0);
        else
        gameObject.transform.Rotate(0, (moveHorizontal + diferencia)*RotateSpeed, 0);
        // Get camera rotation this frame
        CamRot = Camera.transform.eulerAngles;

        // Check if character visible
        if (Input.GetKeyUp(InvisibleKey))
        {
            if (Invisible)
            {
                Invisible = false;
                CharacterMesh.SetActive(true);
            }
            else
            {
                Invisible = true;
                CharacterMesh.SetActive(false);
            }
        }
        // Check camera follow
        if (Input.GetKeyDown(FollowKey))
        {
            if (CameraFollow)
                CameraFollow = false;
            else
                CameraFollow = true;
        }
        // Check light status
        if (Input.GetKeyUp(LightKey) && (LightFocus != null))
        {
            if (LightFocus.activeInHierarchy) LightFocus.SetActive(false);
            else LightFocus.SetActive(true);
        }
    }
}
