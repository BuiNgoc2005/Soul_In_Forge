using UnityEngine;

public class NPCShopInteract : MonoBehaviour
{
    [Header("Optional: a small UI prompt (e.g. 'E' bubble)")]
    public GameObject interactPrompt; // assign a small UI or sprite to show "Press E"

    [Header("Behavior")]
    public bool closeShopOnExit = true; // nếu true: rời vùng thì đóng shop

    bool playerInRange = false;

    void Start()
    {
        if (interactPrompt != null) interactPrompt.SetActive(false);
    }

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            // toggle shop via singleton manager
            if (ShopUIManager.Instance != null)
            {
                ShopUIManager.Instance.ToggleShop();
            }
            else
            {
                Debug.LogWarning("ShopUIManager.Instance == null. Hãy đảm bảo ShopManager có trong scene.");
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            if (interactPrompt != null) interactPrompt.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            if (interactPrompt != null) interactPrompt.SetActive(false);

            if (closeShopOnExit && ShopUIManager.Instance != null)
            {
                // nếu shop đang mở thì đóng
                if (ShopUIManager.Instance != null)
                {
                    // ensure shopPanel reference exists and is active - toggle if active
                    if (ShopUIManager.Instance.shopPanel != null && ShopUIManager.Instance.shopPanel.activeSelf)
                        ShopUIManager.Instance.ToggleShop();
                }
            }
        }
    }
}
