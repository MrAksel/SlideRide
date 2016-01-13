using UnityEngine;
using System.Collections;

[RequireComponent(typeof(PlayerMotor))]
public class PlayerController : MonoBehaviour
{

    private PlayerMotor Motor;

    [SerializeField]
    private float acceleration = 1f;

    [SerializeField]
    private float _JumpForce = 50f;

    private float xAxis;
    private float zAxis;
    private float yRotation;
    private float xRotation;

    void Start()
    {
        Motor = GetComponent<PlayerMotor>();
    }

    void Update()
    {
        if (Input.GetButtonDown("Change Perspective"))
        {
            switch (PlayerPrefs.GetInt("Perspective"))
            {
                case 1:
                    PlayerPrefs.SetInt("Perspective", 3);
                    break;
                case 3:
                    PlayerPrefs.SetInt("Perspective", 1);
                    break;
            }
        }
        float frictionJoystick = Input.GetAxis("FrictionJoystick");
        if (Input.GetButton("Friction") || frictionJoystick > 0)
        {
            Motor.DeactivateFriction();
        }
        else // if (Input.GetButtonUp("Friction") || frictionJoystick <= 0)
        {
            Motor.ActivateFriction();
        }

        //Boost gravity?
        Motor.BoostGravity = Input.GetButton("Boost Gravity");




        //Move player
        xAxis = Input.GetAxisRaw("Horizontal");
        zAxis = Input.GetAxisRaw("Vertical");
        Vector3 MovX = transform.right * xAxis;
        Vector3 MovZ = transform.forward * zAxis;
        Vector3 mov = (MovX + MovZ).normalized * acceleration;
        Motor.Move(mov);

        //Rotate around the y axis
        yRotation = Input.GetAxisRaw("Mouse X");
        Vector3 rot = new Vector3(0f, yRotation, 0f) * PlayerPrefs.GetFloat("LookSensitivity");

        Motor.Rotate(rot);

        //Rotate camera around the y axis
        xRotation = Input.GetAxisRaw("Mouse Y");
        float cameraRot = xRotation * PlayerPrefs.GetFloat("LookSensitivity");

        Motor.RotateCamera(cameraRot);


        //Jetpack
        if (Input.GetButton("Jump"))
        {
            Motor.Jump(_JumpForce);
        }
        else
        {
            Motor.Jump(0f);
        }
    }
}
