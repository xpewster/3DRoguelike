using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public CharacterController controller;

    private float dt;

    [Header("Movement")]
    public float movespeed = 5f;
    public float friction = 5f;
    public float accel = 15f;
    public float airaccel = 5f;
    public float jumpspd = 7f;
    public float gravity = -9.8f;
    public float airspeed = 7f;
    
    [Header("Checks")]
    public float groundCheckDist = 1f;
    public float airGroundCheckDist = 0.1f;
    public float jumpCheckTime = 0.15f;

    

    public LayerMask groundMask;
    public bool isGrounded { get; private set; }

    private Vector3 normGround;
    private Vector3 velocity;
    private float jumpTimeLast;


    // Start is called before the first frame update
    void Start()
    {
        controller.enableOverlapRecovery = true;
    }

    // Update is called once per frame
    void Update()
    {
        dt = Time.deltaTime;

 
        PlayerMovement();

        //controller.Move((transform.right * xaxis + transform.forward * zaxis) * movespeed * dt);
    }

    // Handle player movement.
    void PlayerMovement()
    {
        bool wasGrounded = isGrounded;
        CheckGround();
        if (!wasGrounded && isGrounded)
        {
            //landing .. falldmg
        }

        float xaxis = Input.GetAxisRaw("Horizontal");
        float zaxis = Input.GetAxisRaw("Vertical");

        Vector3 move = Vector3.ClampMagnitude(new Vector3(xaxis, 0f, zaxis), 1);

        // transform local space vector to a world space vector based on transform rotation
        Vector3 wMove = transform.TransformVector(move);

        // handle ground movement
        if (isGrounded)
        {
            // friction
            float speed = velocity.magnitude;
            if (speed != 0)
                velocity *= Mathf.Max(speed - speed * friction * dt, 0) / speed;


            // target vector redirected onto slope direction
            Vector3 target = wMove * movespeed;
            target = GetDirectionReorientedOnSlope(target.normalized, normGround) * target.magnitude;
            // interpolate between current velocity and target velocity (acceleration). decelerate more when stopping
            // if (zaxis == 0 && xaxis == 0)
            //     velocity = Vector3.Lerp(velocity, target, accel * 2.5f * dt);
            // else
            //velocity = Vector3.Lerp(velocity, target, accel * dt);
            float velocityProj = Vector3.Dot(velocity, wMove.normalized);
            float accelVal = accel * dt;
            //Vector3 vx = velocity-velocityProj;

            if (velocityProj + accelVal > movespeed)
            {
                accelVal = movespeed - velocityProj;
            }
            //velocity = vx+Vector3.ClampMagnitude(velocityProj, maxAirSpeed) + Vector3.up * velocity.y;

            velocity += Vector3.ClampMagnitude(wMove, 1) * accelVal;
            

            // handle jumping
            if (Input.GetKeyDown(KeyCode.Space))
            {
                velocity = new Vector3(velocity.x, jumpspd, velocity.z);

                jumpTimeLast = Time.time;
                normGround = Vector3.up;
                isGrounded = false;
            }
        }
        // handle air movement
        else
        {
            //velocity += wMove * airaccel * dt;

            // limit horizontal air motion
            float velocityProj = Vector3.Dot(velocity, wMove.normalized);
            Debug.Log(wMove +" ~ "+velocityProj + " ~ " + Vector3.ProjectOnPlane(velocity, Vector3.up).magnitude);
            float accelVal = airaccel * dt;
            //Vector3 vx = velocity-velocityProj;

            if (velocityProj + accelVal > airspeed)
            {
                accelVal = airspeed - velocityProj;
            }
            //velocity = vx+Vector3.ClampMagnitude(velocityProj, maxAirSpeed) + Vector3.up * velocity.y;
            
            velocity += Vector3.ClampMagnitude(wMove, 1) * accelVal + Vector3.up * gravity * dt;

            // velocity = Vector3.ClampMagnitude(Vector3.ProjectOnPlane(velocity, Vector3.up) + wMove * accelVal, airspeed) + Vector3.up * velocity.y;
            // velocity += Vector3.up * gravity;
        }

        Vector3 capBotPreMove = GetCapsuleBottomHemisphere();
        Vector3 capTopPreMove = GetCapsuleTopHemisphere(controller.height);

        controller.Move(velocity * dt);

        // // detect obstructions while moving (?) .... this fixes sliding on edges
        if (Physics.CapsuleCast(capBotPreMove, capTopPreMove, controller.radius, velocity.normalized, out RaycastHit hit, velocity.magnitude * dt, groundMask, QueryTriggerInteraction.Ignore))
        {
            // remove velocity component normal to surface
            velocity = Vector3.ProjectOnPlane(velocity, hit.normal);
        }
    }

    // Check for ground and snap to if necessary
    void CheckGround()
    {
        // ground check distance in ground vs air
        float checkDist = isGrounded ? (controller.skinWidth + groundCheckDist) : airGroundCheckDist;

        isGrounded = false;
        normGround = Vector3.up;

        // check ground only if its been a little since last jump
        if (Time.time >= jumpTimeLast + jumpCheckTime)
        {
            if (Physics.CapsuleCast(GetCapsuleBottomHemisphere(), GetCapsuleTopHemisphere(controller.height), controller.radius, Vector3.down, out RaycastHit hit, checkDist, groundMask, QueryTriggerInteraction.Ignore))
            {
                normGround = hit.normal;

                // only valid collision if hit normal is in same direction and within slope limit
                if (Vector3.Angle(transform.up, normGround) <= controller.slopeLimit && Vector3.Dot(normGround, transform.up) > 0f)
                {
                    isGrounded = true;

                    // snap to ground
                    if (hit.distance > controller.skinWidth)
                        controller.Move(Vector3.down * hit.distance);
                }
            }
        }
    }

    // Gets the center point of the bottom hemisphere of the character controller capsule    
    Vector3 GetCapsuleBottomHemisphere()
    {
        return transform.position + (transform.up * controller.radius);
    }

    // Gets the center point of the top hemisphere of the character controller capsule    
    Vector3 GetCapsuleTopHemisphere(float atHeight)
    {
        return transform.position + (transform.up * (atHeight - controller.radius));
    }

    // Gets a reoriented normalized direction that is tangent to a given slope
    public Vector3 GetDirectionReorientedOnSlope(Vector3 direction, Vector3 slopeNormal)
    {
        Vector3 directionRight = Vector3.Cross(direction, transform.up);
        return Vector3.Cross(slopeNormal, directionRight).normalized;
    }
}

