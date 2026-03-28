using UnityEngine;

public class Furnace : MonoBehaviour, IInteractable
{
    public GameObject FurnaceUI;
    public SlotInventory[] viewSlots; // new

    private Movement playerMovement;
    private bool isOpen = false;
    private InventoryManager inventoryManager;

    private void Awake()
    {
        if (viewSlots == null || viewSlots.Length == 0)
            viewSlots = FurnaceUI != null ? FurnaceUI.GetComponentsInChildren<SlotInventory>(true) : GetComponentsInChildren<SlotInventory>(true);
    }

    private void Start()
    {
        if (InventoryManager.Instance != null)
            inventoryManager = InventoryManager.Instance;
        else
            inventoryManager = GameObject.FindGameObjectWithTag("Player")?.GetComponent<InventoryManager>();
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
                Debug.Log("UI đang mở, không thể bật Furnace");
                return;
            }
            OpenUI();
        }
        else CloseUI();
    }

    public void OpenUI()
    {
        FurnaceUI.SetActive(true);
        playerMovement = Object.FindFirstObjectByType<Movement>();
        if (playerMovement != null) playerMovement.enabled = false;
        if (inventoryManager != null) inventoryManager.otherUIOpen = true;
        isOpen = true;

        RefreshView();
    }

    public void CloseUI()
    {
        FurnaceUI.SetActive(false);
        if (playerMovement != null) playerMovement.enabled = true;
        if (inventoryManager != null) inventoryManager.otherUIOpen = false;
        isOpen = false;
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
}
