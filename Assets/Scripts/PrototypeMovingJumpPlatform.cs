/*****************************************************************
* COMPONENT OF: Moving landing platforms in the third course section
* REQUIRED DEPENDENCIES: Kinematic Rigidbody and BoxCollider
* DESCRIPTION: Moves a small landing surface sideways, requiring timed jumps and landing control.
* VERSION 1.0: Adds smooth sideways platform travel during physics steps.
*****************************************************************/

using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(BoxCollider))]
public class PrototypeMovingJumpPlatform : MonoBehaviour
{
    // Set the sideways travel from the platform's centre in metres.
    [SerializeField] private float travelDistance = 1.1f;

    // Set seconds per complete out-and-back cycle; longer cycles are easier to time.
    [SerializeField, Min(0.1f)] private float cycleDuration = 4.8f;

    // Set the initial cycle angle in radians to give the two moving platforms different timing.
    [SerializeField] private float phaseOffset;

    private Rigidbody platformBody;
    private Vector3 startingPosition;

    private void Awake()
    {
        platformBody = GetComponent<Rigidbody>();
        startingPosition = platformBody.position;
    }

    private void FixedUpdate()
    {
        MovePlatform();
    }

    private void MovePlatform()
    {
        // Keep the top level and move only sideways so each jump has a readable landing surface.
        float phase = Time.fixedTime * 2f * Mathf.PI / Mathf.Max(0.1f, cycleDuration) + phaseOffset;
        Vector3 offset = Vector3.forward * (Mathf.Sin(phase) * travelDistance);
        platformBody.MovePosition(startingPosition + offset);
    }
}
