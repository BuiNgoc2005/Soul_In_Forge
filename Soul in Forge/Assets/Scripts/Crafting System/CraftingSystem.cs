using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class CraftingSystem : MonoBehaviour
{
    [Header("Slots Input (có thể 1, 2 hoặc nhiều)")]
    public SlotInventory[] inputSlots;

    [Header("References")]
    public Button craftButton;
    public InventoryManager playerInventory;
    public RecipeSO[] recipes;

    private void Start()
    {
        if (craftButton != null)
            craftButton.onClick.AddListener(TryCraft);
        else
            Debug.LogWarning("[CraftingSystem] Chưa gán Craft Button!");

        // 🔹 Tự động tham chiếu tới InventoryManager
        if (playerInventory == null)
        {
            playerInventory = InventoryManager.Instance;
            if (playerInventory == null)
                Debug.LogWarning("[CraftingSystem] Không tìm thấy InventoryManager.Instance!");
        }
    }



    void TryCraft()
    {
        if (inputSlots == null || inputSlots.Length == 0)
        {
            Debug.LogWarning("[CraftingSystem] Không có ô input nào!");
            return;
        }

        // Lấy danh sách nguyên liệu hiện có trong input slots
        var providedItems = inputSlots
            .Where(s => s != null && s.itemSO != null)
            .Select(s => s.itemSO)
            .ToArray();


        // 🔹 Thêm phần DEBUG này để xem danh sách trong các ô input
        Debug.Log("========== [CRAFT DEBUG] ==========");
        for (int i = 0; i < inputSlots.Length; i++)
        {
            var slot = inputSlots[i];
            if (slot != null && slot.itemSO != null)
                Debug.Log($"Ô {i + 1}: {slot.itemSO.itemName} (ref: {slot.itemSO.GetInstanceID()})");
            else
                Debug.Log($"Ô {i + 1}: trống");
        }
        Debug.Log("===================================");
        // 🔹 Kết thúc phần debug


        if (providedItems.Length == 0)
        {
            Debug.Log("Không có nguyên liệu nào trong ô chế tác!");
            return;
        }

        foreach (var recipe in recipes)
        {
            if (recipe == null) continue;

            if (recipe.Matches(providedItems))
            {
                // Xóa nguyên liệu khỏi slot input
                ClearInputs();

                // Thêm vật phẩm kết quả vào inventory thật
                // 1️⃣ Trừ nguyên liệu thật trong inventory
                if (playerInventory != null)
                {
                    foreach (var input in recipe.inputs)
                    {
                        playerInventory.RemoveItem(input, 1); // hoặc theo số lượng yêu cầu
                    }

                    // 2️⃣ Sau đó mới thêm sản phẩm kết quả
                    playerInventory.AddItem(recipe.output, recipe.outputQuantity);
                    Debug.Log($"✅ Đã chế tạo: {recipe.output.itemName} (x{recipe.outputQuantity})");
                }
                else
                {
                    Debug.LogWarning("[CraftingSystem] Thiếu reference tới Player Inventory!");
                }

                return;
            }
        }

        Debug.Log("❌ Không có công thức phù hợp!");
    }

    /// <summary>
    /// Dọn sạch toàn bộ input slot (dùng khi Craft xong hoặc khi đóng UI)
    /// </summary>
    public void ClearInputs()
    {
        if (inputSlots == null) return;

        foreach (var s in inputSlots)
        {
            if (s == null) continue;

            s.itemSO = null;
            s.quantity = 0;
            s.UpdateUI();
        }

        Debug.Log("[CraftingSystem] Đã reset toàn bộ ô chế tác.");
    }

    /// <summary>
    /// (Tuỳ chọn) Trả lại nguyên liệu chưa dùng về inventory
    /// </summary>
    public void ReturnInputsToInventory()
    {
        if (inputSlots == null || playerInventory == null) return;

        foreach (var s in inputSlots)
        {
            if (s == null || s.itemSO == null) continue;

            playerInventory.AddItem(s.itemSO, s.quantity);

            s.itemSO = null;
            s.quantity = 0;
            s.UpdateUI();
        }

        Debug.Log("[CraftingSystem] Đã hoàn nguyên vật phẩm chưa chế.");
    }
}
