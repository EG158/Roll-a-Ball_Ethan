/*****************************************************************
* COMPONENT OF: Prototype rotating barrier
* REQUIRED DEPENDENCIES: Rigidbody component set to Is Kinematic
* DESCRIPTION: Rotates a graybox obstacle around the vertical world axis.
* AUTHOR: Ethan
* VERSION 1.1: Uses Rigidbody rotation in physics steps for reliable moving collisions.
*****************************************************************/

using UnityEngine;

public class PrototypeRotatingBarrier : MonoBehaviour
{
    // Rotation speed in degrees per second; adjust this value in the Inspector to tune difficulty.
    [SerializeField] private float rotationSpeed = 35f;

    // Cache the kinematic physics body before the first simulation step.
    private Rigidbody barrierBody;

    private void Awake()
    {
        barrierBody = GetComponent<Rigidbody>();
    }

    // FixedUpdate calls a focused method so collision movement follows the physics timestep.
    private void FixedUpdate()
    {
        RotateBarrier();
    }

    // Rotates the obstacle around the world y-axis at the same rate on different computers.
    private void RotateBarrier()
    {
        // MoveRotation lets the physics engine move the collider with the rotating body.
        barrierBody.MoveRotation(barrierBody.rotation * Quaternion.Euler(0f, rotationSpeed * Time.fixedDeltaTime, 0f));
    }
}
