/*****************************************************************
* COMPONENT OF: Main Camera in the prototype gameplay test scene
* REQUIRED DEPENDENCIES: Player Transform, PrototypeCameraOccluder on fadeable walls
* DESCRIPTION: Follows the player while keeping a fixed course-facing camera angle.
* AUTHOR: Ethan
* VERSION 1.1: Fades walls blocking the ball and restores walls after the view clears.
*****************************************************************/

using System.Collections.Generic;
using UnityEngine;

public class PrototypeRearCameraFollow : MonoBehaviour
{
    // Assign the player ball whose position the camera follows.
    [SerializeField] private Transform playerTransform;

    // Set the course's fixed forward direction; the camera will not turn with player velocity.
    [SerializeField] private Vector3 startingForward = Vector3.right;

    // Set how far behind the player the camera should remain, in Unity world units.
    [SerializeField] private float followDistance = 6.49f;

    // Set the camera height above the player, in Unity world units.
    [SerializeField] private float followHeight = 2.78f;

    // Set how far above the player's center the camera aims, in Unity world units.
    [SerializeField] private float lookAtHeight = 0.35f;

    // Set the radius of the visibility check to cover the ball rather than only its center.
    [SerializeField] private float visibilityRadius = 0.45f;

    // Reuse hit buffers and sets to avoid allocating lists every frame.
    private readonly RaycastHit[] visibilityHits = new RaycastHit[64];
    private readonly Collider[] cameraOverlaps = new Collider[32];
    private HashSet<PrototypeCameraOccluder> obscuredWalls = new HashSet<PrototypeCameraOccluder>();
    private HashSet<PrototypeCameraOccluder> currentWalls = new HashSet<PrototypeCameraOccluder>();

    // Stores the fixed course direction so the camera never swings when the ball changes direction.
    private Vector3 followDirection;

    // Sets a valid fixed direction for the camera when the scene starts.
    private void Awake()
    {
        followDirection = startingForward.sqrMagnitude > 0.001f
            ? startingForward.normalized
            : Vector3.forward;
    }

    // LateUpdate keeps the camera position current after the player's physics movement.
    private void LateUpdate()
    {
        FollowPlayer();
        UpdateWallVisibility();
    }

    // Follows the ball's position while keeping the camera aligned to the fixed course direction.
    private void FollowPlayer()
    {
        if (playerTransform == null)
        {
            return;
        }

        UpdateCameraPosition();
        AimAtPlayer();
    }

    // Places the camera at the configured height and distance behind the player.
    private void UpdateCameraPosition()
    {
        transform.position = playerTransform.position
            + Vector3.up * followHeight
            - followDirection * followDistance;
    }

    // Tilts the camera toward the ball so the view looks across the course instead of straight down.
    private void AimAtPlayer()
    {
        Vector3 lookTarget = playerTransform.position + Vector3.up * lookAtHeight;
        transform.LookAt(lookTarget);
    }

    // A sphere sweep catches walls over any part of the ball, including overlapping walls.
    private void UpdateWallVisibility()
    {
        currentWalls.Clear();
        if (playerTransform != null)
        {
            Vector3 toPlayer = playerTransform.position - transform.position;
            int count = Physics.SphereCastNonAlloc(transform.position, visibilityRadius,
                toPlayer.normalized, visibilityHits, toPlayer.magnitude,
                Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
            for (int index = 0; index < count; index++)
            {
                AddObstructingWall(visibilityHits[index].collider);
            }

            // Sphere casts omit colliders containing their origin; check the lens volume too.
            int overlapCount = Physics.OverlapSphereNonAlloc(transform.position, visibilityRadius,
                cameraOverlaps, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
            for (int index = 0; index < overlapCount; index++)
            {
                AddObstructingWall(cameraOverlaps[index]);
            }
        }
        RestoreClearWalls();
        // Swap the reused sets: this frame becomes the previous frame for the next check.
        HashSet<PrototypeCameraOccluder> previousWalls = obscuredWalls;
        obscuredWalls = currentWalls;
        currentWalls = previousWalls;
    }

    private void AddObstructingWall(Collider wallCollider)
    {
        PrototypeCameraOccluder wall = wallCollider.GetComponent<PrototypeCameraOccluder>();
        if (wall != null && wall.isActiveAndEnabled && currentWalls.Add(wall))
        {
            wall.SetObscured(true);
        }
    }

    // Restore the exact original material as soon as the wall no longer blocks the camera.
    private void RestoreClearWalls()
    {
        foreach (PrototypeCameraOccluder wall in obscuredWalls)
        {
            if (wall != null && !currentWalls.Contains(wall))
            {
                wall.SetObscured(false);
            }
        }
    }

    private void OnDisable()
    {
        currentWalls.Clear();
        RestoreClearWalls();
        obscuredWalls.Clear();
    }
}
