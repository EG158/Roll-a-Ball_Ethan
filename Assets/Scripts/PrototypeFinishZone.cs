/*****************************************************************
* COMPONENT OF: Prototype finish area
* REQUIRED DEPENDENCIES: Trigger Collider and GameManager
* DESCRIPTION: Accepts the finish only after every course collectible is collected.
* AUTHOR: Ethan
* VERSION 1.1: Routes finish detection through the collectible gate and win system.
*****************************************************************/

using UnityEngine;

public class PrototypeFinishZone : MonoBehaviour
{
    // Assign the scene's Game Manager, which owns progress and the win condition.
    [SerializeField] private GameManager gameManager;

    // Unity calls this when an object enters the finish trigger.
    private void OnTriggerEnter(Collider other)
    {
        ShowCompletion(other);
    }

    // Only the player can complete the prototype course.
    private void ShowCompletion(Collider other)
    {
        if (other.CompareTag("Player") && gameManager != null)
        {
            gameManager.TryCompleteCourse();
        }
    }
}
