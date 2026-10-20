/*****************************************************************
* COMPONENT OF: Prototype conveyor belt
* REQUIRED DEPENDENCIES: Trigger Collider
* DESCRIPTION: Pushes the player ball along a graybox conveyor section.
* AUTHOR: Ethan
* VERSION 1.0: Adds a simple moving-surface mechanic for gameplay testing.
*****************************************************************/

using UnityEngine;

public class PrototypeConveyorBelt : MonoBehaviour
{
    // Direction is set in the Inspector as a world-space direction along the conveyor.
    [SerializeField] private Vector3 pushDirection = Vector3.right;

    // Force is measured as acceleration so the push feels similar with different ball masses.
    [SerializeField] private float pushAcceleration = 5f;

    // Unity calls this while a Rigidbody overlaps the conveyor's trigger collider.
    private void OnTriggerStay(Collider other)
    {
        PushPlayer(other);
    }

    // Applies conveyor force only to the tagged player and safely ignores other objects.
    private void PushPlayer(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        Rigidbody playerBody = other.attachedRigidbody;
        if (playerBody != null)
        {
            playerBody.AddForce(pushDirection.normalized * pushAcceleration, ForceMode.Acceleration);
        }
    }
}
