using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class TaskBoard : MonoBehaviour
{
    [Header("=== UI TIỀN TỆ (BUDGET) ===")]
    public TMP_Text budgetText;

    [Header("=== QUEST SLOTS (4 slots cố định) ===")]
    public Button quest1Button;
    public Image quest1Background;
    public Image quest1ItemSprite;
    public TMP_Text quest1QtyText; // <--- THÊM MỚI: Text hiển thị số lượng
    public Image tickIcon1;
    public Image crossIcon1;
    
    public Button quest2Button;
    public Image quest2Background;
    public Image quest2ItemSprite;
    public TMP_Text quest2QtyText; // <--- THÊM MỚI
    public Image tickIcon2;
    public Image crossIcon2;
    
    public Button quest3Button;
    public Image quest3Background;
    public Image quest3ItemSprite;
    public TMP_Text quest3QtyText; // <--- THÊM MỚI
    public Image tickIcon3;
    public Image crossIcon3;
    
    public Button quest4Button;
    public Image quest4Background;
    public Image quest4ItemSprite;
    public TMP_Text quest4QtyText; // <--- THÊM MỚI
    public Image tickIcon4;
    public Image crossIcon4;

    [Header("=== NÚT XỬ LÝ CHUNG ===")]
    public Button deliverButton;
    public Button rejectButton;

    [Header("=== TEXT HIỂN THỊ THÔNG TIN ===")]
    public TMP_Text questInfoText;

    [Header("=== MÀU HIGHLIGHT ===")]
    public Color normalColor = Color.white;
    public Color selectedColor = new Color(1f, 0.9f, 0.4f); // Vàng
    public Color successColor = new Color(0.5f, 1f, 0.5f);  // Xanh lá (Deliver)
    public Color failColor = new Color(1f, 0.5f, 0.5f);     // Đỏ (Reject)

    [Header("=== KHO SPRITE VẬT PHẨM ===")]
    public Sprite[] itemSprites;

    [Header("=== LIÊN KẾT HỆ THỐNG ===")]
    public GameObject player;
    public TMP_Text expText;

    private int selectedQuest = -1;

    [System.Serializable]
    public class UnifiedQuest
    {
        public string itemName;
        public int quantity;
        public int rewardExp;
        public int rewardGold;
        public int spriteIndex;
    }

    private List<UnifiedQuest> quests = new List<UnifiedQuest>();

    // DANH SÁCH TÊN VẬT PHẨM (Phải khớp với ItemSO)
    private string[] allItemNames = new string[]
    {
        "Iron Sword",
        "Bronze Sword",
        "Silver Sword",
        "Gold Sword",
    };

    void Start()
    {
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player");

        LoadQuests();

        if (quests.Count == 0)
            GenerateQuests();
        else
            RefreshAllQuestVisuals();

        HideAllIcons();
        ResetAllHighlights();

        deliverButton.gameObject.SetActive(false);
        rejectButton.gameObject.SetActive(false);

        if (questInfoText != null)
            questInfoText.text = "Chọn một nhiệm vụ để xem chi tiết";

        UpdateExpDisplay();
        UpdateBudgetDisplay();

        if(quest1Button) quest1Button.onClick.AddListener(() => OnQuestClicked(1));
        if(quest2Button) quest2Button.onClick.AddListener(() => OnQuestClicked(2));
        if(quest3Button) quest3Button.onClick.AddListener(() => OnQuestClicked(3));
        if(quest4Button) quest4Button.onClick.AddListener(() => OnQuestClicked(4));

        if(deliverButton) deliverButton.onClick.AddListener(OnDeliverClicked);
        if(rejectButton) rejectButton.onClick.AddListener(OnRejectClicked);
    }

    public void UpdateBudgetDisplay()
    {
        if (budgetText != null && InventoryManager.Instance != null)
            budgetText.text = InventoryManager.Instance.gold.ToString();
    }

    void OnQuestClicked(int questNumber)
    {
        if (selectedQuest == questNumber)
        {
            selectedQuest = -1;
            ResetAllHighlights();
            deliverButton.gameObject.SetActive(false);
            rejectButton.gameObject.SetActive(false);
            if (questInfoText != null) questInfoText.text = "Chọn một nhiệm vụ để xem chi tiết";
            return;
        }

        selectedQuest = questNumber;
        ResetAllHighlights();
        HighlightQuest(questNumber, selectedColor); // Click -> Màu Vàng

        deliverButton.gameObject.SetActive(true);
        rejectButton.gameObject.SetActive(true);

        UnifiedQuest q = quests[questNumber - 1];
        if (questInfoText != null)
        {
            int currentQty = 0;
            if (InventoryManager.Instance != null)
                currentQty = InventoryManager.Instance.GetItemQuantity(q.itemName);

            string statusColor = currentQty >= q.quantity ? "green" : "red";

            questInfoText.text = $"<b>NHIỆM VỤ {questNumber}</b>\n" +
                                 $"Yêu cầu: {q.itemName} <color={statusColor}>({currentQty}/{q.quantity})</color>\n" +
                                 $"Thưởng: {q.rewardExp} EXP + {q.rewardGold} G";
        }
    }

    void UpdateExpDisplay()
    {
        if (expText != null)
        {
            expText.text = $"Lv.{LevelSystem.level} ({LevelSystem.currentExp}/{LevelSystem.requiredExp})";
        }
    }

    void OnDeliverClicked()
    {
        if (selectedQuest == -1) return;
        if (InventoryManager.Instance == null) return;

        UnifiedQuest q = quests[selectedQuest - 1];

        // 1. Kiểm tra đủ đồ
        int currentQty = InventoryManager.Instance.GetItemQuantity(q.itemName);
        
        if (currentQty >= q.quantity)
        {
            // 2. Xóa đồ ngay lập tức
            bool removed = InventoryManager.Instance.RemoveItem(q.itemName, q.quantity);

            if (removed)
            {
                // 3. Cộng thưởng ngay lập tức
                LevelSystem.AddExperienceGlobal(q.rewardExp);
                InventoryManager.Instance.AddGold(q.rewardGold); 
                
                UpdateExpDisplay();
                UpdateBudgetDisplay();

                Debug.Log($"✅ Giao thành công! Đợi 3s để reset...");

                // 4. HIỆN TICK XANH VÀ ĐỔI NỀN XANH LÁ
                ToggleIcons(GetTick(selectedQuest), GetCross(selectedQuest));
                HighlightQuest(selectedQuest, successColor);
                
                // 5. ẨN NÚT VÀ CẬP NHẬT TEXT
                deliverButton.gameObject.SetActive(false);
                rejectButton.gameObject.SetActive(false);
                
                if (questInfoText != null) questInfoText.text = "Nhiệm vụ hoàn thành! Đang tải nhiệm vụ mới...";

                // 6. GỌI COROUTINE ĐỂ DELAY VIỆC TẠO QUEST MỚI
                int questIndex = selectedQuest - 1;
                Image tickIcon = GetTick(selectedQuest);
                selectedQuest = -1; // Bỏ chọn để tránh lỗi

                StartCoroutine(DeliverQuestDelayed(questIndex, tickIcon));
            }
        }
        else
        {
            Debug.LogWarning("❌ Không đủ vật phẩm!");
            if (questInfoText != null)
                questInfoText.text = $"<color=red>Không đủ {q.itemName} x{q.quantity}!</color>";
        }
    }

    // --- COROUTINE MỚI CHO DELIVERY ---
    System.Collections.IEnumerator DeliverQuestDelayed(int questIndex, Image tickIcon)
    {
        // Chờ 3 giây (Người chơi ngắm dấu tích xanh trên quest CŨ)
        yield return new WaitForSeconds(3f);

        // Sau 3 giây: Đổi sang Quest MỚI
        quests[questIndex] = GenerateNewQuest();
        UpdateQuestVisual(questIndex);
        SaveQuests();

        // Ẩn dấu tích xanh đi
        if (tickIcon != null) tickIcon.gameObject.SetActive(false);
        ResetAllHighlights();

        // Thông báo
        if (questInfoText != null) questInfoText.text = "Nhiệm vụ mới đã sẵn sàng!";

    }
    void OnRejectClicked()
    {
        if (selectedQuest == -1) return;

        Image crossIcon = GetCross(selectedQuest);
        Image tickIcon = GetTick(selectedQuest);
        
        if (crossIcon != null)
        {
            crossIcon.gameObject.SetActive(true);
            if (tickIcon != null) tickIcon.gameObject.SetActive(false);
        }

        HighlightQuest(selectedQuest, failColor); // Đổi màu nền đỏ

        if (questInfoText != null) questInfoText.text = "Đã từ chối. Đang tạo nhiệm vụ mới...";

        deliverButton.gameObject.SetActive(false);
        rejectButton.gameObject.SetActive(false);

        int questIndex = selectedQuest - 1;
        selectedQuest = -1;
        
        StartCoroutine(RejectQuestDelayed(questIndex, crossIcon));
    }

    System.Collections.IEnumerator RejectQuestDelayed(int questIndex, Image crossIcon)
    {
        yield return new WaitForSeconds(3f);

        UnifiedQuest newQuest = GenerateNewQuest();
        quests[questIndex] = newQuest;
        UpdateQuestVisual(questIndex);
        SaveQuests();

        if (crossIcon != null) crossIcon.gameObject.SetActive(false);
        ResetAllHighlights();
        if (questInfoText != null) questInfoText.text = "Nhiệm vụ mới đã sẵn sàng!";
    }

    void GenerateQuests()
    {
        quests.Clear();
        for (int i = 0; i < 4; i++) quests.Add(GenerateNewQuest());
        RefreshAllQuestVisuals();
        SaveQuests();
    }

    UnifiedQuest GenerateNewQuest()
    {
        int spriteIndex = Random.Range(0, Mathf.Min(itemSprites.Length, allItemNames.Length));
        string name = (spriteIndex < allItemNames.Length) ? allItemNames[spriteIndex] : "Unknown";

        return new UnifiedQuest
        {
            itemName = name,
            quantity = 1,
            rewardExp = Random.Range(30, 100),
            rewardGold = Random.Range(50, 200),
            spriteIndex = spriteIndex
        };
    }

    void UpdateQuestVisual(int index)
    {
        if (index < 0 || index >= quests.Count) return;
        UnifiedQuest q = quests[index];
        
        // 1. Cập nhật Hình ảnh
        Image spriteImg = GetQuestSprite(index + 1);
        if (spriteImg != null && q.spriteIndex >= 0 && q.spriteIndex < itemSprites.Length)
        {
            spriteImg.sprite = itemSprites[q.spriteIndex];
            spriteImg.color = Color.white; 
            spriteImg.rectTransform.sizeDelta = new Vector2(50f, 50f); 
            spriteImg.enabled = true;
            spriteImg.preserveAspect = true;
        }

        // 2. Cập nhật Text Số lượng (NEW)
        TMP_Text qtyText = GetQtyText(index + 1);
        if (qtyText != null)
        {
            qtyText.text = $"x{q.quantity}";
        }
    }

    void RefreshAllQuestVisuals()
    {
        for (int i = 0; i < quests.Count; i++) UpdateQuestVisual(i);
    }

    void HideAllIcons()
    {
        if(tickIcon1) tickIcon1.gameObject.SetActive(false); if(crossIcon1) crossIcon1.gameObject.SetActive(false);
        if(tickIcon2) tickIcon2.gameObject.SetActive(false); if(crossIcon2) crossIcon2.gameObject.SetActive(false);
        if(tickIcon3) tickIcon3.gameObject.SetActive(false); if(crossIcon3) crossIcon3.gameObject.SetActive(false);
        if(tickIcon4) tickIcon4.gameObject.SetActive(false); if(crossIcon4) crossIcon4.gameObject.SetActive(false);
    }

    void ToggleIcons(Image primary, Image secondary)
    {
        if (primary != null) primary.gameObject.SetActive(!primary.gameObject.activeSelf);
        if (secondary != null) secondary.gameObject.SetActive(false);
    }

    void HighlightQuest(int questNumber, Color color)
    {
        Image bg = GetBackground(questNumber);
        if (bg != null) bg.color = color;
    }

    void ResetAllHighlights()
    {
        if (quest1Background) quest1Background.color = normalColor;
        if (quest2Background) quest2Background.color = normalColor;
        if (quest3Background) quest3Background.color = normalColor;
        if (quest4Background) quest4Background.color = normalColor;
    }

    Image GetBackground(int id) => id switch { 1 => quest1Background, 2 => quest2Background, 3 => quest3Background, 4 => quest4Background, _ => null };
    Image GetQuestSprite(int id) => id switch { 1 => quest1ItemSprite, 2 => quest2ItemSprite, 3 => quest3ItemSprite, 4 => quest4ItemSprite, _ => null };
    
    // Hàm mới để lấy Text Số lượng theo ID
    TMP_Text GetQtyText(int id) => id switch { 
        1 => quest1QtyText, 
        2 => quest2QtyText, 
        3 => quest3QtyText, 
        4 => quest4QtyText, 
        _ => null 
    };

    Image GetTick(int id) => id switch { 1 => tickIcon1, 2 => tickIcon2, 3 => tickIcon3, 4 => tickIcon4, _ => null };
    Image GetCross(int id) => id switch { 1 => crossIcon1, 2 => crossIcon2, 3 => crossIcon3, 4 => crossIcon4, _ => null };

    void SaveQuests()
    {
        for (int i = 0; i < quests.Count; i++)
        {
            UnifiedQuest q = quests[i];
            PlayerPrefs.SetString($"Quest_{i}_Item", q.itemName);
            PlayerPrefs.SetInt($"Quest_{i}_Qty", q.quantity);
            PlayerPrefs.SetInt($"Quest_{i}_Exp", q.rewardExp);
            PlayerPrefs.SetInt($"Quest_{i}_Gold", q.rewardGold);
            PlayerPrefs.SetInt($"Quest_{i}_Sprite", q.spriteIndex);
        }
        PlayerPrefs.Save();
    }

    void LoadQuests()
    {
        quests.Clear();
        for (int i = 0; i < 4; i++)
        {
            string itemName = PlayerPrefs.GetString($"Quest_{i}_Item", "");
            if (!string.IsNullOrEmpty(itemName))
            {
                UnifiedQuest q = new UnifiedQuest
                {
                    itemName = itemName,
                    quantity = PlayerPrefs.GetInt($"Quest_{i}_Qty", 1),
                    rewardExp = PlayerPrefs.GetInt($"Quest_{i}_Exp", 50),
                    rewardGold = PlayerPrefs.GetInt($"Quest_{i}_Gold", 100),
                    spriteIndex = PlayerPrefs.GetInt($"Quest_{i}_Sprite", 0)
                };
                quests.Add(q);
            }
        }
    }
    
    [ContextMenu("Reset All Saved Quests")]
    public void ResetSavedQuests()
    {
        PlayerPrefs.DeleteAll(); // Xóa sạch để tạo mới
        PlayerPrefs.Save();
        Debug.Log("🗑️ Đã xóa save quest! Chạy lại game để thấy thay đổi.");
    }
}