using UnityEngine;

public class Anvil : MonoBehaviour, IInteractable
{
    [Header("References")]
    public GameObject AnvilUI;           // UI chính của bàn rèn

    // --- new: local view slots for the anvil UI (assign in Inspector or auto-find)
    public SlotInventory[] viewSlots;

    private Movement playerMovement;     // script di chuyển player
    private InventoryManager inventoryManager; // để check UI khác đang mở

    private bool isOpen = false;

    private void Awake()
    {
        // auto-find viewSlots nếu chưa gán
        if (viewSlots == null || viewSlots.Length == 0)
            viewSlots = AnvilUI != null ? AnvilUI.GetComponentsInChildren<SlotInventory>(true) : GetComponentsInChildren<SlotInventory>(true);
    }

    private void Start()
    {
        // Prefer singleton instance
        if (InventoryManager.Instance != null)
            inventoryManager = InventoryManager.Instance;
        else
        {
            // fallback: try find on Player
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                inventoryManager = player.GetComponent<InventoryManager>();
            else
                Debug.LogWarning("Không tìm thấy Player trong scene!");
        }

        // cache Movement
        playerMovement = FindFirstObjectByType<Movement>();
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
                Debug.Log("UI khác đang mở, không thể bật Anvil");
                return;
            }
            OpenUI();
        }
        else CloseUI();
    }

    public void OpenUI()
    {
        if (AnvilUI == null)
        {
            Debug.LogError("AnvilUI chưa được gán!");
            return;
        }

        // bật Anvil UI
        AnvilUI.SetActive(true);

        // 👉 (THÊM MỚI) Ghim cờ "đang mở UI khác" để chặn bấm B
        if (inventoryManager != null)
            inventoryManager.otherUIOpen = true;

        if (playerMovement != null)
            playerMovement.enabled = false;

        isOpen = true;

        // quan trọng: refresh view khi mở để đồng bộ ngay
        RefreshView();

        Debug.Log("Đã mở UI Anvil");
    }

    public void CloseUI()
    {
        if (AnvilUI != null)
            AnvilUI.SetActive(false);

        if (inventoryManager != null && inventoryManager.inventoryUI != null)
            inventoryManager.inventoryUI.SetActive(false);

        if (playerMovement != null)
            playerMovement.enabled = true;

        if (inventoryManager != null)
            inventoryManager.otherUIOpen = false;

        isOpen = false;
        Debug.Log("Đã đóng UI Anvil");
    }

    // --- new: copy data from InventoryManager.itemSlots -> viewSlots
    private void RefreshView()
    {
        if (viewSlots == null || viewSlots.Length == 0) return;
        if (InventoryManager.Instance == null || InventoryManager.Instance.itemSlots == null)
        {
            // clear all slots
            for (int i = 0; i < viewSlots.Length; i++)
            {
                viewSlots[i].itemSO = null;
                viewSlots[i].quantity = 0;
                viewSlots[i].UpdateUI();
            }
            return;
        }

        var src = InventoryManager.Instance.itemSlots;
        for (int i = 0; i < viewSlots.Length; i++)
        {
            if (i < src.Length && src[i] != null)
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
}
