/*****************************************************************
* RELEASE NOTES VERSION 1.1: Add a behavior so that the Collectible rotates slowly about the y-axis.
* COMPONENT OF: Collectible
* REQUIRED DEPENDENCIES: GameManager
* DESCRIPTION: Slowly rotates the collectible around its y-axis and controls what happens when the player collects it.
* AUTHOR: Ethan
* VERSION 1.1: Slowly rotates the collectible around its y-axis.
*****************************************************************/

using UnityEngine;

public class CollectibleController : MonoBehaviour
{
    // Rotation speed in degrees per second, used to control how slowly the collectible turns.
    [SerializeField] private float rotationSpeed = 45f;

    [SerializeField] private AudioClip collectSound;
    [SerializeField] private GameObject collectParticlePrefab;
    private GameManager gameManager;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Finds the Game Manager in the Scene
        gameManager = FindAnyObjectByType<GameManager>();    
    }

    // Update is called once per frame
    void Update()
    {
        RotateCollectible();
    }

    // Rotates the collectible around its local y-axis at a frame-rate-independent speed.
    private void RotateCollectible()
    {
        transform.Rotate(0f, rotationSpeed * Time.deltaTime, 0f);
    }

        private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            gameManager.UpdateRemaining();
            AudioSource.PlayClipAtPoint(collectSound, transform.position);

            Instantiate(
                collectParticlePrefab,
                transform.position,
                Quaternion.identity
         );

         Destroy(gameObject);
        }
    }
}
