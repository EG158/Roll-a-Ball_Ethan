/************************************************************
* COMPONENT OF: Player
* REQUIRED DEPENDENCIES: Rigidbody Component
* DESCRIPTION: It listens for WASD and Arrow key presses to set 
*              vertical and horizontal directions. It uses those
*              to push the player's Rigidbody in that direction
*              at a preset force. W/S follow course forward/back;
*              A/D move left/right relative to the fixed camera.
* AUTHOR: Ethan Guo
* VERSION 1.1: Corrects camera-relative steering and tunes capped movement for platform jumps.
*************************************************************/

using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    // Movement fields
    private float horizontalMovement;
    private float verticalMovement;
    // Set the rolling force in the Inspector; the prototype builder assigns 16 for its gaps.
    [SerializeField] private float force = 4.75f;

    // Set the maximum horizontal speed in units per second to keep narrow paths controllable.
    [SerializeField] private float maximumSpeed = 6f;

    // Cache the Rigidbody so physics movement does not search for it every step.
    private Rigidbody playerBody;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerBody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        MovePlayer();
        LimitHorizontalSpeed();
    }

    // Moves the player
    private void MovePlayer()
    {
        Vector3 direction = GetCourseRelativeDirection();
        // Clamp diagonal input so two keys do not apply more force than one.
        playerBody.AddForce(Vector3.ClampMagnitude(direction, 1f) * force);
    }  

    // Limit only horizontal velocity; leave upward velocity available for jumping.
    private void LimitHorizontalSpeed()
    {
        Vector3 velocity = playerBody.linearVelocity;
        Vector3 horizontalVelocity = new Vector3(velocity.x, 0f, velocity.z);
        horizontalVelocity = Vector3.ClampMagnitude(horizontalVelocity, maximumSpeed);
        playerBody.linearVelocity = horizontalVelocity + Vector3.up * velocity.y;
    }

    // Maps W/S to the course's forward axis and A/D to the camera's fixed right axis.
    private Vector3 GetCourseRelativeDirection()
    {
        Vector3 courseForward = Vector3.right * verticalMovement;
        // With the camera facing +X, screen-right points toward world -Z.
        Vector3 courseRight = Vector3.back * horizontalMovement;
        return courseForward + courseRight;
    }
        
    // Gets the user key input and uses it to assign movement directions
    private void SetMoveDirection(Vector2 input)
    {
        horizontalMovement = input.x;
        verticalMovement = input.y;
    }

    // Listens for WASD and arrow key input then calls SetMoveDirection
    public void OnMoveInput(InputAction.CallbackContext ctx)
    {
        SetMoveDirection(ctx.ReadValue<Vector2>());
    }

}
