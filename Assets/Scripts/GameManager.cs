/************************************************************
* COMPONENT OF: Game Manager
* REQUIRED DEPENDENCIES: TextMeshProUGUI, AudioSource
* DESCRIPTION: Handles collectible tracking and win condition 
*              for the game, including the prototype finish gate.
* AUTHOR: Ethan Guo
* VERSION 1.2: Plays the prototype victory presentation before restarting the completed course.
*************************************************************/

using UnityEngine;
using TMPro;  // Brings in the Text Mesh script package into this script
using System.Collections;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{ 
    // UI text that shows collectibles remaining (assign in inspector)
    [SerializeField] private TextMeshProUGUI RemainingTextUI;
    // Assign the victory sound played after the player completes the level.
    [SerializeField] private AudioClip winClip;

    // Enable in the prototype scene to require crossing the finish after collecting every item.
    [SerializeField] private bool requireFinishZone;

    // Assign the solid finish gate; it disappears when every collectible has been collected.
    [SerializeField] private GameObject finishGate;

    // Assign the short sound that announces the finish has unlocked.
    [SerializeField] private AudioClip unlockClip;

    // Assign the prototype celebration component; older scenes may leave this empty.
    [SerializeField] private PrototypeVictoryPresentation victoryPresentation;

    // Keep the original total for the collected/total progress display.
    private int totalCollectibles;
    private bool gameCompleted;

    // The finish zone checks this property before accepting a win.
    public bool IsFinishUnlocked => numberOfCollectibles == 0 && !gameCompleted;

    // Audio source for victory sound
    private AudioSource audioSource;

    // Tracks how many collectibles are left to collect
    private int numberOfCollectibles;

    // Initialize everything at start
    void Start()
    {
        // Assigns the AudioSource component to the audioSource field        
        audioSource = GetComponent<AudioSource>();
        
        // Determines how many Collectibles are in this scene
        numberOfCollectibles = GameObject.FindGameObjectsWithTag("Collectible").Length;
        
        totalCollectibles = numberOfCollectibles;
        if (finishGate != null)
        {
            finishGate.SetActive(numberOfCollectibles > 0);
        }

        // Initialize the UI display
        UpdateRemainingUI();
    }

    // Called by the CollectibleController when a collectible is picked up - decrements count
    public void UpdateRemaining()
    {
        // Ignore repeated callbacks once the level has ended or all items are collected.
        if (gameCompleted || numberOfCollectibles <= 0)
        {
            return;
        }

        numberOfCollectibles--;
        if (requireFinishZone && numberOfCollectibles == 0)
        {
            UnlockFinish();
        }
        UpdateRemainingUI(); // Update display immediately when score changes
    }
    // Updates UI text - shows remaining count or "You Win" message
    private void UpdateRemainingUI()
    {
        // Prototype progress stays visible after the last pickup, until the finish is crossed.
        if (requireFinishZone)
        {
            if (RemainingTextUI != null)
            {
                RemainingTextUI.text = "Collectibles: " + (totalCollectibles - numberOfCollectibles)
                    + "/" + totalCollectibles + (numberOfCollectibles == 0 ? "  |  Finish unlocked!" : "  |  Finish locked");
            }
            return;
        }

        if (RemainingTextUI != null && numberOfCollectibles > 0)
        {
            RemainingTextUI.text = "Collectibles Remaining: " + numberOfCollectibles;
        }
        else
        {
            StartCoroutine(EndGame());
        }
    }
    // Remove the physical gate and play a separate cue when the final item is collected.
    private void UnlockFinish()
    {
        if (finishGate != null)
        {
            finishGate.SetActive(false);
        }
        if (audioSource != null && unlockClip != null)
        {
            audioSource.PlayOneShot(unlockClip);
        }
    }

    // Called by the finish trigger; the guard prevents early or repeated victories.
    public void TryCompleteCourse()
    {
        if (requireFinishZone && IsFinishUnlocked)
        {
            StartCoroutine(EndGame());
        }
    }

    IEnumerator EndGame()
    {
        gameCompleted = true;
        if (RemainingTextUI != null)
        {
            RemainingTextUI.text = "You Win";
        }
        if (audioSource != null && winClip != null)
        {
            audioSource.clip = winClip;
            audioSource.Play();
        }

        // Wait for the prototype celebration, or the exact audio duration in the original scenes.
        if (requireFinishZone && victoryPresentation != null)
        {
            yield return victoryPresentation.PlayCelebration();
        }
        else
        {
            yield return new WaitForSeconds(winClip != null ? winClip.length : 2f);
        }

        // Reloads the currently active scene
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }
}

