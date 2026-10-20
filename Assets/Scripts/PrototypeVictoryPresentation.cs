/*****************************************************************
* COMPONENT OF: Game Manager in the prototype gameplay test scene
* REQUIRED DEPENDENCIES: Player Rigidbody/PlayerInput, follow camera, victory UI, particle systems
* DESCRIPTION: Presents a visible celebration after the unlocked finish is crossed.
* VERSION 1.0: Adds a ball celebration, camera transition, confetti and an animated victory card.
*****************************************************************/

using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PrototypeVictoryPresentation : MonoBehaviour
{
    // Assign the player ball; its controls and physics pause only after a confirmed victory.
    [SerializeField] private Transform playerTransform;

    // Assign the main camera used for the short celebration shot.
    [SerializeField] private Camera celebrationCamera;

    // Assign a marker above the flat finish floor where the ball celebrates.
    [SerializeField] private Transform celebrationPoint;

    // Assign the normal progress/controls group so it can hide behind the victory presentation.
    [SerializeField] private GameObject gameplayHUD;

    // Assign the victory overlay's CanvasGroup to fade in the complete message together.
    [SerializeField] private CanvasGroup victoryOverlay;

    // Assign the central card RectTransform for the brief pop-and-settle animation.
    [SerializeField] private RectTransform victoryCard;

    // Assign the countdown label so players know when the next attempt will start.
    [SerializeField] private TextMeshProUGUI restartText;

    // Assign the two finish confetti emitters; they remain stopped during normal gameplay.
    [SerializeField] private ParticleSystem[] confetti;

    // Set how many seconds the celebration remains visible before GameManager reloads the scene.
    [SerializeField, Min(2f)] private float celebrationDuration = 6f;

    private Vector3 cameraStartPosition;
    private Quaternion cameraStartRotation;
    private Vector3 playerStartPosition;
    private float cardFitScale = 1f;
    private int lastCountdown = -1;

    // GameManager runs this coroutine after confirming that every collectible has been collected.
    public IEnumerator PlayCelebration()
    {
        BeginCelebration();
        float elapsed = 0f;
        while (elapsed < celebrationDuration)
        {
            AnimatePlayer(elapsed);
            AnimateCamera(elapsed);
            AnimateVictoryCard(elapsed);
            UpdateCountdown(elapsed);
            yield return null;
            // Use real seconds so the presentation is independent of the physics timestep.
            elapsed += Time.unscaledDeltaTime;
        }
    }

    private void BeginCelebration()
    {
        PausePlayer();
        PrepareCamera();
        playerStartPosition = celebrationPoint.position;
        playerTransform.position = playerStartPosition;
        gameplayHUD.SetActive(false);
        victoryOverlay.gameObject.SetActive(true);
        victoryOverlay.alpha = 0f;
        // Keep the card inside smaller Game views as well as full-screen play.
        RectTransform overlayRect = victoryOverlay.GetComponent<RectTransform>();
        cardFitScale = Mathf.Min(1f, Mathf.Max(0.2f, (overlayRect.rect.width - 40f) / 660f),
            Mathf.Max(0.2f, overlayRect.rect.height * 0.4f / 240f));
        lastCountdown = -1;
        foreach (ParticleSystem emitter in confetti)
        {
            emitter.Play(true);
        }
    }

    private void PausePlayer()
    {
        playerTransform.GetComponent<PlayerInput>().DeactivateInput();
        // Victory owns the ball briefly so movement, jumping or falling cannot interrupt the shot.
        playerTransform.GetComponent<PlayerController>().enabled = false;
        playerTransform.GetComponent<PrototypePlayerJumpController>().enabled = false;
        playerTransform.GetComponent<PrototypeFallRestart>().enabled = false;
        Rigidbody body = playerTransform.GetComponent<Rigidbody>();
        body.linearVelocity = Vector3.zero;
        body.angularVelocity = Vector3.zero;
        body.interpolation = RigidbodyInterpolation.None;
        body.isKinematic = true;
    }

    private void PrepareCamera()
    {
        // Disabling follow also restores any walls that were transparent during the last gameplay frame.
        celebrationCamera.GetComponent<PrototypeRearCameraFollow>().enabled = false;
        cameraStartPosition = celebrationCamera.transform.position;
        cameraStartRotation = celebrationCamera.transform.rotation;
    }

    private void AnimatePlayer(float elapsed)
    {
        // A small, decaying victory bounce is visual only; the flat floor itself never moves.
        float bounce = Mathf.Abs(Mathf.Sin(elapsed * 5.5f)) * 0.45f * Mathf.Exp(-elapsed * 0.7f);
        playerTransform.position = playerStartPosition + Vector3.up * bounce;
    }

    private void AnimateCamera(float elapsed)
    {
        float progress = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / 1.2f));
        Vector3 destination = celebrationPoint.position + new Vector3(-6f, 3.3f, -3f);
        Vector3 lookTarget = celebrationPoint.position + Vector3.up * 0.5f;
        Quaternion destinationRotation = Quaternion.LookRotation(lookTarget - destination);
        celebrationCamera.transform.SetPositionAndRotation(
            Vector3.Lerp(cameraStartPosition, destination, progress),
            Quaternion.Slerp(cameraStartRotation, destinationRotation, progress));
    }

    private void AnimateVictoryCard(float elapsed)
    {
        victoryOverlay.alpha = Mathf.Clamp01(elapsed / 0.35f);
        // Grow slightly past the final size, then settle back to make the result feel responsive.
        float scale = elapsed < 0.35f
            ? Mathf.Lerp(0.8f, 1.05f, Mathf.SmoothStep(0f, 1f, elapsed / 0.35f))
            : Mathf.Lerp(1.05f, 1f, Mathf.Clamp01((elapsed - 0.35f) / 0.2f));
        victoryCard.localScale = Vector3.one * scale * cardFitScale;
    }

    private void UpdateCountdown(float elapsed)
    {
        int seconds = Mathf.CeilToInt(celebrationDuration - elapsed);
        if (seconds != lastCountdown)
        {
            restartText.text = "Next run in " + seconds + " seconds";
            lastCountdown = seconds;
        }
    }
}
