/*****************************************************************
* COMPONENT OF: Player ball in the prototype gameplay test scene
* REQUIRED DEPENDENCIES: Rigidbody and PlayerInput with a Space-bound Jump action
* DESCRIPTION: Lets the player ball jump from a platform to another platform.
* AUTHOR: Ethan
* VERSION 1.1: Uses a short ground check beneath the ball so rocking beams do not block grounded jumps.
*****************************************************************/

using UnityEngine;
using UnityEngine.InputSystem;

// The jump callback receives input from the PlayerInput component on the player.
[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(SphereCollider))]
public class PrototypePlayerJumpController : MonoBehaviour
{
    // Set how strongly the jump lifts the player; this is an impulse in physics units.
    [SerializeField] private float jumpImpulse = 6.5f;

    // Set the short distance the ground check searches below the ball's bottom edge, in meters.
    [SerializeField] private float groundCheckDistance = 0.1f;

    // Stores the player's physics body so jumping applies force to the existing Rigidbody.
    private Rigidbody playerBody;

    // Stores the ball collider so the ground check can begin just above the ball's bottom edge.
    private SphereCollider playerCollider;

    // Finds the Rigidbody once when the player enters the scene.
    private void Awake()
    {
        playerBody = GetComponent<Rigidbody>();
        playerCollider = GetComponent<SphereCollider>();
    }

    // Called by the existing PlayerInput Jump action when the player presses Space.
    public void OnJumpInput(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            TryJump();
        }
    }

    // Adds an upward impulse only when the ball is touching a platform beneath its bottom edge.
    private void TryJump()
    {
        if (!IsGrounded())
        {
            return;
        }

        playerBody.AddForce(Vector3.up * jumpImpulse, ForceMode.Impulse);
    }

    // Checks just below the ball's bottom so a nearby platform cannot count while the ball is airborne.
    private bool IsGrounded()
    {
        // Start slightly inside the ball's lower edge; the ray ignores its own starting collider.
        Vector3 rayOrigin = playerCollider.bounds.center -
                            Vector3.up * (playerCollider.bounds.extents.y - 0.05f);

        return Physics.Raycast(
            rayOrigin,
            Vector3.down,
            groundCheckDistance,
            Physics.DefaultRaycastLayers,
            QueryTriggerInteraction.Ignore);
    }
}
