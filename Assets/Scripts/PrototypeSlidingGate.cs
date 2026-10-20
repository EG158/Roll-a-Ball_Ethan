/*****************************************************************
* COMPONENT OF: Sliding gates between the machinery platforms
* REQUIRED DEPENDENCIES: Kinematic Rigidbody and BoxCollider
* DESCRIPTION: Sweeps a tall gate across a narrow bridge to create repeatable timing windows.
* VERSION 1.0: Adds smooth crosswise gate movement during physics steps.
*****************************************************************/

using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(BoxCollider))]
public class PrototypeSlidingGate : MonoBehaviour
{
    // Assign the world-space direction across the bridge, normally (0, 0, 1).
    [SerializeField] private Vector3 travelDirection = Vector3.forward;

    // Set the distance in metres from the bridge centre to either end of the gate's travel.
    [SerializeField] private float travelDistance = 2.2f;

    // Set seconds for a complete out-and-back cycle; this controls the timing difficulty.
    [SerializeField, Min(0.1f)] private float cycleDuration = 3.4f;

    // Set the starting cycle angle in radians so the two gates do not open simultaneously.
    [SerializeField] private float phaseOffset;

    private Rigidbody gateBody;
    private Vector3 startingPosition;

    private void Awake()
    {
        gateBody = GetComponent<Rigidbody>();
        startingPosition = gateBody.position;
    }

    private void FixedUpdate()
    {
        MoveGate();
    }

    private void MoveGate()
    {
        // Sine movement slows near each end, providing a readable opening before it sweeps back.
        float phase = Time.fixedTime * 2f * Mathf.PI / Mathf.Max(0.1f, cycleDuration) + phaseOffset;
        Vector3 offset = travelDirection.normalized * (Mathf.Sin(phase) * travelDistance);
        gateBody.MovePosition(startingPosition + offset);
    }
}
