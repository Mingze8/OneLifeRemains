using UnityEngine;
using UnityEngine.UI;

public class LootChestInteraction : MonoBehaviour
{
    private DistributionManager distributionManager;

    private bool isPlayerNear = false;
    private GameObject player;
    private Animator chestAnimator;

    private Image interactionPromptImage;

    public static event System.Action OnChestOpened;

    private void Start()
    {
        chestAnimator = GetComponent<Animator>();

        interactionPromptImage = GameObject.Find("InteractionPrompt").GetComponent<Image>();
        if (interactionPromptImage != null)
        {
            interactionPromptImage.enabled = false; // Ensure it's initially hidden
        }
        else
        {
            Debug.LogError("InteractionPromptUI not found in the scene!");
        }

        distributionManager = FindObjectOfType<DistributionManager>();
    } 

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {            
            isPlayerNear = true;
            player = collision.gameObject;
            if (interactionPromptImage != null)
            {
                interactionPromptImage.enabled = true; // Show the image (make it visible)
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log("Player Left Chest Area");
            isPlayerNear = false;
            player = null;
            if (interactionPromptImage != null)
            {
                interactionPromptImage.enabled = false; // Hide the image (make it invisible)
            }
        }
    }

    private void Update()
    {
        if (isPlayerNear && player != null)
        {
            if (Input.GetKeyDown(KeyCode.Z))
            {
                OpenLootChest();
                chestAnimator.SetTrigger("openChest");

                if (interactionPromptImage != null)
                {
                    interactionPromptImage.enabled = false;
                }
            }
        }
    }

    private void OpenLootChest()
    {
        OnChestOpened?.Invoke(); // Add this line
        distributionManager.OnChestOpened(gameObject);
    }

    public void destroyChest()
    {
        Destroy(gameObject);
    }
}