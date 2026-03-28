using UnityEngine;

public class CraftingTable : MonoBehaviour, IInteractable
{
    [Header("References")]
    public GameObject CraftingUI;
    public SlotInventory[] viewSlots; // new

    private Movement playerMovement;
    private InventoryManager inventoryManager;
    private bool isOpen = false;

    void Awake()
    {
        if (viewSlots == null || viewSlots.Length == 0)
            viewSlots = CraftingUI != null ? CraftingUI.GetComponentsInChildren<SlotInventory>(true) : GetComponentsInChildren<SlotInventory>(true);
    }

    void Start()
    {
        if (InventoryManager.Instance != null)
            inventoryManager = InventoryManager.Instance;
        else
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                inventoryManager = player.GetComponent<InventoryManager>();
                playerMovement = player.GetComponent<Movement>();
            }
            else
            {
                Debug.LogWarning("[CraftingTable] Không thấy Player (tag=Player).");
            }
        }
    }

    private void OnEnable()
    {
        InventoryManager.OnInventoryChangedGlobal += RefreshView;
    }

    private void OnDisable()
    {
        InventoryManager.OnInventoryChangedGlobal -= RefreshView;
    }

    public void Interact()
    {
        if (!isOpen)
        {
            if (inventoryManager != null && inventoryManager.otherUIOpen)
            {
                Debug.Log("[CraftingTable] UI khác đang mở, không thể bật Crafting.");
                return;
            }
            OpenUI();
        }
        else CloseUI();
    }

    public void OpenUI()
    {
        if (CraftingUI == null)
        {
            Debug.LogError("[CraftingTable] Chưa gán CraftingUI!");
            return;
        }

        CraftingUI.SetActive(true);

        if (playerMovement != null)
            playerMovement.enabled = false;

        if (inventoryManager != null)
            inventoryManager.otherUIOpen = true;

        isOpen = true;

        // refresh ngay
        RefreshView();

        Debug.Log("[CraftingTable] Open");
    }

    public void CloseUI()
    {
        if (CraftingUI != null)
            CraftingUI.SetActive(false);

        if (playerMovement != null)
            playerMovement.enabled = true;

        if (inventoryManager != null)
            inventoryManager.otherUIOpen = false;

        isOpen = false;
        Debug.Log("[CraftingTable] Close");
    }

    private void RefreshView()
    {
        if (viewSlots == null || viewSlots.Length == 0) return;
        var src = InventoryManager.Instance != null ? InventoryManager.Instance.itemSlots : null;
        for (int i = 0; i < viewSlots.Length; i++)
        {
            if (src != null && i < src.Length && src[i] != null)
            {
                viewSlots[i].itemSO = src[i].itemSO;
                viewSlots[i].quantity = src[i].quantity;
            }
            else
            {
                viewSlots[i].itemSO = null;
                viewSlots[i].quantity = 0;
            }
            viewSlots[i].UpdateUI();
        }
    }

    public string GetPrompt() => "Nhấn E để mở Bàn chế tác";
}
