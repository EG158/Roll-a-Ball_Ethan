/*****************************************************************
* COMPONENT OF: Invisible checkpoint trigger in the prototype course
* REQUIRED DEPENDENCIES: Trigger Collider, player with PrototypeFallRestart
* DESCRIPTION: Saves a safe respawn position when the player reaches a section checkpoint.
* AUTHOR: Ethan
* VERSION 1.0: Adds checkpoint progress to the prototype course.
*****************************************************************/

using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PrototypeCheckpoint : MonoBehaviour
{
    // Set the world-space position where the player should reappear after falling.
    [SerializeField] private Vector3 respawnPosition;

    // Unity calls this when the player enters the checkpoint's trigger collider.
    private void OnTriggerEnter(Collider other)
    {
        ActivateCheckpoint(other);
    }

    // Save the checkpoint only for the player and only when its restart component exists.
    private void ActivateCheckpoint(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        PrototypeFallRestart fallRestart = other.GetComponent<PrototypeFallRestart>();
        if (fallRestart != null)
        {
            fallRestart.SetCheckpoint(respawnPosition);
        }
    }
}
