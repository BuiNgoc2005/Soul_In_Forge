using UnityEngine;
using TMPro;

public class ShopUIManager : MonoBehaviour
{
    public static ShopUIManager Instance;

    [Header("Shop Settings")]
    public GameObject shopPanel;
    public Transform slotParent;
    public ShopSlotUI slotTemplate;
    public TMP_Text budgetText;

    [Header("Item Data")]
    public ItemSO[] shopItems;
    public int[] itemPrices;

    private void Awake()
    {
        Instance = this;
        //if (shopPanel != null)
            //shopPanel.SetActive(false);
    }

    void Start()
    {
        Debug.Log("[Shop] ShopUIManager.Start called. shopItems=" + (shopItems == null ? 0 : shopItems.Length) + " slotTemplate=" + (slotTemplate == null));
        GenerateShopSlots();
    }

public void GenerateShopSlots()
{
    if (slotTemplate == null)
    {
        Debug.LogWarning("[Shop] slotTemplate is null!");
        return;
    }
    if (slotParent == null)
    {
        Debug.LogWarning("[Shop] slotParent is null!");
        return;
    }
    if (shopItems == null || shopItems.Length == 0)
    {
        Debug.LogWarning("[Shop] shopItems empty!");
        return;
    }

    // xóa slot con (nếu có) — tránh tạo chồng khi gọi nhiều lần
    for (int i = slotParent.childCount - 1; i >= 0; i--)
    {
        Destroy(slotParent.GetChild(i).gameObject);
    }

    for (int i = 0; i < shopItems.Length; i++)
    {
            // slotTemplate là ShopSlotUI (component) prefab => Instantiate trả về ShopSlotUI
            ShopSlotUI newSlot = Instantiate(slotTemplate, slotParent);
            newSlot.gameObject.SetActive(true);

            // bảo đảm không null (thường không xảy ra vì slotTemplate không null)
            if (newSlot == null)
            {
                Debug.LogWarning("[Shop] Failed to instantiate slotTemplate.");
                continue;
            }

            int price = shopItems[i].price; // dùng price trong ItemSO
            newSlot.Setup(shopItems[i], price);
            Debug.Log($"[Shop] Spawned slot for {shopItems[i].itemName} price={price}");


        }
    }

    public void UpdateBudgetDisplay()
    {
        if (budgetText != null && InventoryManager.Instance != null)
            budgetText.text = InventoryManager.Instance.gold.ToString();
    }


    public void ToggleShop()
    {
        if (shopPanel == null)
        {
            Debug.LogWarning("ShopUIManager: shopPanel chưa gán!");
            return;
        }

        bool isActive = shopPanel.activeSelf;
        shopPanel.SetActive(!isActive);

        if (!isActive) // shop đang được mở sau toggle
            UpdateBudgetDisplay();

        // Khi mở shop → khóa chuyển động player + đánh dấu UI khác mở
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.otherUIOpen = !isActive;

            var playerMove = GameObject.FindGameObjectWithTag("Player").GetComponent<Movement>();
            if (playerMove != null)
                playerMove.enabled = isActive; // true khi shop đóng, false khi shop mở
        }
    }
}
