using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMotor : MonoBehaviour
{

    [Header("Jetpack")]

    [SerializeField]
    ParticleSystem[] emitters;

    public float maxFuel;
    public float fuel;
    public float fuelRefillRate;
    public float fuelUsageRate;
    public float fuelIgnitionUsage;

    [Header("Physics")]

    [SerializeField]
    PhysicMaterial frictionMaterial;

    [SerializeField]
    PhysicMaterial noFrictionMaterial;

    [SerializeField]
    private float camLimit = 80f;

    [Header("Objects")]

    [SerializeField]
    private Camera cam;

    [SerializeField]
    private Transform head;

    public bool BoostGravity = false;

    [SerializeField]
    float GravityBoost = 200f;
    Vector3 gravityForce = Vector3.down * 9.81f;

    Vector3 movementForce = Vector3.zero;
    Vector3 rotation = Vector3.zero;
    Vector3 jumpForce = Vector3.zero;

    internal bool jetpackRefueling;
    internal bool jetpackMustWaitForFuel;

    float camRotationX = 0f;

    public float maxVelocity;
    public float jetpackMaxVelocity;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        fuel = maxFuel;
        jetpackRefueling = false;
        jetpackMustWaitForFuel = false;
    }

    void FixedUpdate()
    {
        ApplyGravity();
        ApplyJetpack();
        PerformMovement();
        PerformRotation();
    }

    void Update()
    {
        if (jumpForce != Vector3.zero && fuel > 0)
        {
            //Generate jetpack particles
            foreach (ParticleSystem e in emitters)
            {
                e.Emit(100);
            }
        }
    }

    public void Move(Vector3 _movement)
    {
        movementForce = _movement;
    }

    void ApplyGravity()
    {
        //Boost gravity
        if (BoostGravity)
        {
            rb.AddForce(gravityForce * GravityBoost, ForceMode.Force);
        }
    }

    void ApplyJetpack()
    {
        // If the jetpack reached 0 fuel it starts to refuel, so don't let the player use it before we reached at least 50%. 
        // If we're already using fuel and flying lets continue till we reach 0f.
        if (jumpForce != Vector3.zero && !jetpackMustWaitForFuel)
        {
            if (jetpackRefueling) // This is the start of jetpack usage- apply an ignition cost
            {
                fuel -= fuelIgnitionUsage;
            }
            jetpackRefueling = false;
            //Generate force and draw fuel
            if (rb.velocity.y < jetpackMaxVelocity)
            {
                rb.AddForce(jumpForce, ForceMode.Force); // Add jetpack force since we're below maximum velocity
            }
            else
            {
                rb.AddForce(-gravityForce, ForceMode.Force); // Add just enough to keep us at same velocity in air
            }
            fuel -= fuelUsageRate;
            if (fuel <= 0f)
            {
                jetpackMustWaitForFuel = true;
            }
        }
        else
        {
            jetpackRefueling = true;
            if (fuel >= maxFuel * 0.5f) // Enough fuel to start the jetpack again
            {
                jetpackMustWaitForFuel = false;
            }
        }
        if (fuel < maxFuel)
            fuel += fuelRefillRate; // Add fuel regardless
    }

    void PerformMovement()
    {
        if (movementForce != Vector3.zero)
        {
            // Velocity left before max velocity. We only care about horizontal movement.
            float remaingingVelocity = maxVelocity - Mathf.Sqrt(rb.velocity.x * rb.velocity.x + rb.velocity.z * rb.velocity.z);
            if (remaingingVelocity >= 0)
            {
                // We can still accelerate, so lets do so
                //float velocityMultiplier = remaingingVelocity / maxVelocity; //As velocity->maxVelocity the multiplier->0 and no force is added
                //float directionalMultiplier = GetAngleMultiplier(rb.velocity, movementForce);

                rb.AddForce(movementForce, ForceMode.Force);
            }
            else // Max velocity
            {
                // We can't accelerate any more, but we should still be able to change direction
                Vector3 force = GetMovementForce4(movementForce, rb.velocity);

                rb.AddForce(force, ForceMode.Force);
            }
        }
    }

    private Vector3 GetMovementForce1(Vector3 moveDirection, Vector3 velocityDirection)
    {
        Vector2 forward = new Vector2(velocityDirection.x, velocityDirection.z);
        Vector2 direction = new Vector2(movementForce.x, movementForce.z);

        // Remove the component that goes in the same direction as the velocity
        float forwardPart = Vector2.Dot(forward, direction);
        Vector3 directionalForce = movementForce;
        if (forwardPart > 0) // There is a part of the vector that pushes the same way as velocity, remove it
            directionalForce -= forwardPart * velocityDirection.normalized;

        return directionalForce;
    }

    private Vector3 GetMovementForce2(Vector3 moveDirection, Vector3 velocityDirection)
    {
        // Difference in angle between direction we are moving and direction of movement
        float angleDiff = Vector2.Angle(velocityDirection, moveDirection);
        //float angleDiff = Mathf.Atan2(Vector3.Dot(Vector3.up, Vector3.Cross(forward, direction)), Vector3.Dot(forward, direction));
        while (angleDiff > Mathf.PI)
            angleDiff -= Mathf.PI * 2;
        while (angleDiff < Mathf.PI)
            angleDiff += Mathf.PI * 2;

        // If we add force in the same way we're already traveling (at max velocity), this multiplier is zero.
        // Else, this goes gradually from 0 to 1 as the angle goes from forward to straight backwards. I think.
        float angleMultiplier = Mathf.Abs(angleDiff / Mathf.PI);

        return movementForce * angleMultiplier;
    }

    private Vector3 GetMovementForce3(Vector3 moveDirection, Vector3 velocityDirection)
    {
        Vector3 forward = new Vector3(velocityDirection.x, 0f, velocityDirection.z);
        Vector3 direction = new Vector3(moveDirection.x, 0f, moveDirection.z);

        // Remove the component that goes in the same direction as the velocity
        Vector3 forwardPart = Vector3.Project(direction, velocityDirection);
        Vector3 directionalForce = moveDirection - forwardPart;
        return directionalForce;
    }

    private Vector3 GetMovementForce4(Vector3 moveDirection, Vector3 velocityDirection)
    {
        return moveDirection * GetAngleMultiplier(velocityDirection, moveDirection);
    }

    private Vector3 GetMovementForce5(Vector3 moveDirection, Vector3 velocityDirection)
    {
        Vector2 forward = new Vector3(velocityDirection.x, velocityDirection.z);
        Vector2 direction = new Vector3(moveDirection.x, moveDirection.z);

        float angleDiff = Vector2.Angle(forward, direction); // Should be between 0 and +-180, right? (just in radians)
        float angleMultiplier;

        if (angleDiff >= Mathf.PI / 2f) // Over 90 degrees (so, backwards)
            angleMultiplier = 1f;
        else
            angleMultiplier = Mathf.Sin(angleDiff); // Gradually go from 0 to 1

        Debug.LogFormat("A {0:0.0}, M {1:0.0}", angleDiff, angleMultiplier);

        return moveDirection * angleMultiplier;
    }

    private float GetAngleMultiplier(Vector3 currVelocity, Vector3 requestedMovement)
    {
        Vector2 forward = new Vector3(currVelocity.x, currVelocity.z);
        Vector2 direction = new Vector3(requestedMovement.x, requestedMovement.z);

        float angleDiff = Mathf.Abs(Vector2.Angle(forward, direction)); // Should be between 0 and +180, right? (just in radians)
        float angleMultiplier;

        if (angleDiff >= 90f) // Backwards
        {
            angleMultiplier = 1f;
        }
        else
        {
            float angleInRads = (angleDiff / 360f * Mathf.PI);                 // Angle halved to 0-90 and converted to radians
            angleMultiplier = Mathf.Sin(angleInRads);
        }

        Debug.LogFormat("A {0:0.0}, M {1:0.0}", angleDiff, angleMultiplier);
        return angleMultiplier;
    }

    public void Rotate(Vector3 _rotation)
    {
        rotation = _rotation;
    }

    public void RotateCamera(float _camRotationX)
    {
        camRotationX = _camRotationX;
    }


    float currentCameraRotX;
    void PerformRotation()
    {
        rb.MoveRotation(rb.rotation * Quaternion.Euler(rotation));
        currentCameraRotX -= camRotationX;
        currentCameraRotX = Mathf.Clamp(currentCameraRotX, -camLimit, camLimit);
        head.transform.localEulerAngles = new Vector3(currentCameraRotX, 0f, 0f);
    }

    public void Jump(float _jumpForce)
    {
        jumpForce = Vector3.up * _jumpForce;
    }

    public void DeactivateFriction()
    {
        rb.drag = 0;
        gameObject.GetComponent<Collider>().material = noFrictionMaterial;
    }


    public void ActivateFriction()
    {
        rb.drag = 0;
        gameObject.GetComponent<Collider>().material = frictionMaterial;
    }


    //Die
    public void die()
    {
        GameManager.UnRegisterPlayer(gameObject.name);
        GameManager.respawn(gameObject);
    }
}
