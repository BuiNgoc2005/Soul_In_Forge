using UnityEngine;

public class InventoryView : MonoBehaviour
{
    [Header("Data")]
    public InventoryData inventoryData; // gán InventoryData_Main (hoặc runtime copy)

    [Header("UI slots (assign existing SlotInventory objects)")]
    public SlotInventory[] viewSlots;

    private void Awake()
    {
        if (viewSlots == null || viewSlots.Length == 0)
            viewSlots = GetComponentsInChildren<SlotInventory>(true);
    }

    private void OnEnable()
    {
        if (inventoryData != null)
            inventoryData.OnInventoryChanged += Refresh;
        Refresh();
    }

    private void OnDisable()
    {
        if (inventoryData != null)
            inventoryData.OnInventoryChanged -= Refresh;
    }

    public void Refresh()
    {
        if (inventoryData == null || viewSlots == null) return;

        // populate slots from entries; if fewer entries than slots => clear rest
        int entriesCount = inventoryData.entries != null ? inventoryData.entries.Count : 0;
        for (int i = 0; i < viewSlots.Length; i++)
        {
            var dst = viewSlots[i];
            if (i < entriesCount)
            {
                var e = inventoryData.entries[i];
                dst.itemSO = e.item;
                dst.quantity = e.qty;
            }
            else
            {
                dst.itemSO = null;
                dst.quantity = 0;
            }
            dst.UpdateUI();
        }
    }
}
