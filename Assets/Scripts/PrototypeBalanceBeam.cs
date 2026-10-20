/*****************************************************************
* COMPONENT OF: Narrow path beam segments
* REQUIRED DEPENDENCIES: Kinematic Rigidbody and BoxCollider
* DESCRIPTION: Gently tilts a narrow beam sideways so the ball needs balance corrections.
* VERSION 1.0: Rocks each beam around its own long axis in physics steps.
*****************************************************************/

using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(BoxCollider))]
public class PrototypeBalanceBeam : MonoBehaviour
{
    // Set the maximum sideways lean in degrees; small angles preserve a traversable surface.
    [SerializeField] private float tiltAngle = 7f;

    // Set seconds per complete left-right rocking cycle in the Inspector.
    [SerializeField] private float cycleDuration = 4f;

    // Set different starting phases in radians so adjacent beams do not all lean together.
    [SerializeField] private float phaseOffset;

    private Rigidbody beamBody;
    private Quaternion startingRotation;

    private void Awake()
    {
        beamBody = GetComponent<Rigidbody>();
        startingRotation = beamBody.rotation;
    }

    private void FixedUpdate()
    {
        TiltBeam();
    }

    private void TiltBeam()
    {
        // Local X runs along each beam; rotating around it tilts the walking surface sideways.
        float phase = Time.fixedTime * (2f * Mathf.PI / Mathf.Max(0.1f, cycleDuration)) + phaseOffset;
        float angle = Mathf.Sin(phase) * tiltAngle;
        beamBody.MoveRotation(startingRotation * Quaternion.AngleAxis(angle, Vector3.right));
    }
}
