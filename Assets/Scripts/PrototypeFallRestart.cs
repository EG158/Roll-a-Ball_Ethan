/*****************************************************************
* COMPONENT OF: Player ball in the prototype gameplay test scene
* REQUIRED DEPENDENCIES: PlayerInput, Rigidbody, enabled scene in Build Settings
* DESCRIPTION: Returns the player to the latest section checkpoint after a fall.
* AUTHOR: Ethan
* VERSION 1.2: Respawns at the latest checkpoint after a fall while preserving course progress.
*****************************************************************/

using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PrototypeFallRestart : MonoBehaviour
{
    // Set below the course platforms but above the lower catch ground, in world Y units.
    [SerializeField] private float fallHeight = -1.5f;

    // Assign the existing SFX_Lose clip to give audible feedback before the player respawns.
    [SerializeField] private AudioClip fallClip;

    // Set a short feedback delay in seconds; this is independent of the sound clip length.
    [SerializeField] private float restartDelay = 0.2f;

    // Prevent repeated respawn routines while the short recovery delay is pending.
    private bool restarting;
    // Remember the player's latest safe world position, beginning at the course start.
    private Vector3 checkpointPosition;
    // Cache the physics body so its momentum can be cleared when the player respawns.
    private Rigidbody playerBody;

    private void Awake()
    {
        playerBody = GetComponent<Rigidbody>();
        checkpointPosition = transform.position;
    }

    private void Update()
    {
        CheckForFall();
    }

    // Falling below the course ends the current attempt, even if no lower ground is hit.
    private void CheckForFall()
    {
        if (!restarting && transform.position.y < fallHeight)
        {
            StartCoroutine(RestartCourse());
        }
    }

    private IEnumerator RestartCourse()
    {
        restarting = true;
        // Stop accepting new movement commands during the brief restart delay.
        PlayerInput playerInput = GetComponent<PlayerInput>();
        if (playerInput != null)
        {
            playerInput.DeactivateInput();
        }
        if (fallClip != null)
        {
            // Play near the camera so the cue remains audible as the ball falls away.
            Vector3 soundPosition = Camera.main != null ? Camera.main.transform.position : transform.position;
            AudioSource.PlayClipAtPoint(fallClip, soundPosition);
        }
        // Unscaled time keeps recovery quick even if the game's time scale is changed.
        yield return new WaitForSecondsRealtime(Mathf.Max(0f, restartDelay));
        RespawnPlayer();
    }

    // Update the safe location when the player reaches a completed section.
    public void SetCheckpoint(Vector3 respawnPosition)
    {
        checkpointPosition = respawnPosition;
    }

    // Restore position and remove carried physics momentum before movement resumes.
    private void RespawnPlayer()
    {
        if (playerBody != null)
        {
            playerBody.position = checkpointPosition;
            playerBody.linearVelocity = Vector3.zero;
            playerBody.angularVelocity = Vector3.zero;
        }
        else
        {
            transform.position = checkpointPosition;
        }

        Physics.SyncTransforms();
        PlayerInput playerInput = GetComponent<PlayerInput>();
        if (playerInput != null)
        {
            playerInput.ActivateInput();
        }
        restarting = false;
    }
}
