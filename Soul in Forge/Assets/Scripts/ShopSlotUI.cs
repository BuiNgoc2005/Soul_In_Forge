using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopSlotUI : MonoBehaviour
{
    [Header("UI References")]
    public Image iconImage;
    public TMP_Text priceText;
    public Button buyButton;

    [Header("Item Info")]
    public ItemSO itemData;
    public int price;

    // [Header("Data")] 
    // public InventoryData inventoryData; // Không cần thiết nữa vì Manager quản lý hết

    private void Awake()
    {
        if (iconImage == null)
        {
            Transform t = transform.Find("Icon");
            if (t != null) iconImage = t.GetComponent<Image>();
        }
        if (priceText == null)
        {
            var tmp = GetComponentInChildren<TMP_Text>(true);
            if (tmp != null) priceText = tmp;
        }
        if (buyButton == null)
        {
            Transform t = transform.Find("BuyButton");
            if (t != null) buyButton = t.GetComponent<Button>();
        }

        if (buyButton != null)
            buyButton.onClick.RemoveAllListeners();
    }

    private void Start()
    {
        if (buyButton != null)
            buyButton.onClick.AddListener(BuyItem);
    }

    public void Setup(ItemSO item, int itemPrice)
    {
        itemData = item;
        price = itemPrice;

        if (iconImage != null)
        {
            if (item != null && item.icon != null)
            {
                iconImage.sprite = item.icon;
                
                // --- CÁC DÒNG QUAN TRỌNG ĐÃ ĐƯỢC KHÔI PHỤC ---
                iconImage.color = Color.white; // Đặt lại màu trắng (nếu không nó sẽ trong suốt)
                iconImage.rectTransform.sizeDelta = new Vector2(40f, 40f); // Đặt kích thước chuẩn (như code cũ của bạn)
                // ---------------------------------------------
                
                iconImage.preserveAspect = true;
                iconImage.enabled = true;
            }
            else
            {
                iconImage.sprite = null;
                iconImage.enabled = false; 
            }
        }

        if (priceText != null)
        {
            priceText.text = price.ToString();
        }
    }

    private void BuyItem()
    {
        // Kiểm tra Manager tồn tại
        if (InventoryManager.Instance == null)
        {
            Debug.LogError("[ShopSlotUI] InventoryManager.Instance is null!");
            return;
        }

        // 1. Kiểm tra tiền (Dùng hàm mới an toàn)
        if (!InventoryManager.Instance.HasEnoughGold(price))
        {
            Debug.LogWarning("[Shop] Không đủ tiền!");
            return;
        }

        // 2. Thử thêm đồ vào túi
        // (Hàm TryAddItem sẽ tự thêm vào Core Data nếu còn chỗ)
        bool success = InventoryManager.Instance.TryAddItem(itemData, 1);

        if (success)
        {
            // 3. Trừ tiền (Hàm này tự update UI vàng luôn)
            InventoryManager.Instance.SpendGold(price);

            // 4. Cập nhật UI ngân sách của Shop (nếu ShopUIManager đang mở)
            if (ShopUIManager.Instance != null)
                ShopUIManager.Instance.UpdateBudgetDisplay();

            Debug.Log($"[Shop] Đã mua {itemData.itemName} giá {price}G");
        }
        else
        {
            Debug.LogWarning("[Shop] Mua thất bại: Túi đầy!");
        }
    }
}