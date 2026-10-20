/*****************************************************************
* COMPONENT OF: Player ball in the prototype gameplay test scene
* REQUIRED DEPENDENCIES: Rigidbody and PlayerInput with a Space-bound Jump action
* DESCRIPTION: Lets the player ball jump from a platform to another platform.
* AUTHOR: Ethan
* VERSION 1.0: Adds a grounded jump for the platform-jumping section.
*****************************************************************/

using UnityEngine;
using UnityEngine.InputSystem;

// The jump callback receives input from the PlayerInput component on the player.
[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(Rigidbody))]
public class PrototypePlayerJumpController : MonoBehaviour
{
    // Set how strongly the jump lifts the player; this is an impulse in physics units.
    [SerializeField] private float jumpImpulse = 6.5f;

    // Set how far below the ball's center the ground check searches for a platform.
    [SerializeField] private float groundCheckDistance = 0.75f;

    // Stores the player's physics body so jumping applies force to the existing Rigidbody.
    private Rigidbody playerBody;

    // Finds the Rigidbody once when the player enters the scene.
    private void Awake()
    {
        playerBody = GetComponent<Rigidbody>();
    }

    // Called by the existing PlayerInput Jump action when the player presses Space.
    public void OnJumpInput(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            TryJump();
        }
    }

    // Adds an upward impulse only when the ball is on a platform and not moving upward.
    private void TryJump()
    {
        if (playerBody.linearVelocity.y > 0.1f || !IsGrounded())
        {
            return;
        }

        playerBody.AddForce(Vector3.up * jumpImpulse, ForceMode.Impulse);
    }

    // Checks downward for a solid platform while ignoring trigger volumes.
    private bool IsGrounded()
    {
        return Physics.Raycast(
            transform.position,
            Vector3.down,
            groundCheckDistance,
            Physics.DefaultRaycastLayers,
            QueryTriggerInteraction.Ignore);
    }
}
