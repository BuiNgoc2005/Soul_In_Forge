using UnityEngine;
using UnityEngine.EventSystems;

public class InventoryDropHandler : MonoBehaviour, IDropHandler
{
    public SlotInventory targetSlot;

    public void OnDrop(PointerEventData eventData)
    {
        var sourceHandler = eventData.pointerDrag?.GetComponent<InventoryDragHandler>();
        if (sourceHandler == null || sourceHandler.slot.itemSO == null)
            return;

        var sourceSlot = sourceHandler.slot;

        // Nếu target đang có item → không cho chồng (có thể tùy chỉnh)
        if (targetSlot.itemSO != null)
        {
            Debug.Log("Ô chế đã có vật phẩm!");
            return;
        }

        // --- Sao chép item sang ô chế ---
        targetSlot.itemSO = sourceSlot.itemSO;
        targetSlot.quantity = 1;
        targetSlot.UpdateUI();

        // --- Trừ hoặc xóa vật phẩm khỏi inventory ---
        sourceSlot.quantity -= 1;
        if (sourceSlot.quantity <= 0)
        {
            sourceSlot.itemSO = null;
            sourceSlot.quantity = 0;
        }
        sourceSlot.UpdateUI();

        Debug.Log($"Đã chuyển {targetSlot.itemSO.itemName} từ Inventory sang ô Crafting.");
    }
}
