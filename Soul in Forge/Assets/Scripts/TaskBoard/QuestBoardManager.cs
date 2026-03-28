using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class QuestBoardManager : MonoBehaviour
{
    [Header("Các tờ nhiệm vụ (4 slot cố định)")]
    public Button quest1Button;
    public Image quest1Background;
    public Image tickIcon1;
    public Image crossIcon1;

    public Button quest2Button;
    public Image quest2Background;
    public Image tickIcon2;
    public Image crossIcon2;

    public Button quest3Button;
    public Image quest3Background;
    public Image tickIcon3;
    public Image crossIcon3;

    public Button quest4Button;
    public Image quest4Background;
    public Image tickIcon4;
    public Image crossIcon4;

    [Header("Nút xử lý chung")]
    public Button deliverButton;
    public Button rejectButton;

    [Header("Text hiển thị thông tin quest")]
    public TMP_Text questInfoText;

    [Header("Màu highlight")]
    public Color normalColor = Color.white;
    public Color selectedColor = new Color(1f, 0.9f, 0.4f);

    [Header("Liên kết hệ thống")]
    public TMP_Text expText;
    public int playerExp = 0;

    private int selectedQuest = -1;

    [System.Serializable]
    public class Quest
    {
        public string itemName;
        public int quantity;
        public int rewardExp;
        public int rewardGold;
    }

    private List<Quest> quests = new List<Quest>();

    // Giữ nguyên list item của bạn
    private string[] itemPool = {
        "Iron Sword", "Steel Shield", "Magic Dagger", "Silver Ring",
        "Health Potion", "Mana Potion", "Iron Ore", "Gold Ore",
        "Leather Armor", "Wooden Bow", "Bronze Ore", "Stick", "Ingot", "Silver Ore"
    };

    void Start()
    {
        LoadQuests();

        if (quests.Count == 0)
            GenerateQuests();

        HideAllIcons();
        ResetAllHighlights();

        if (deliverButton) deliverButton.gameObject.SetActive(false);
        if (rejectButton) rejectButton.gameObject.SetActive(false);

        if (questInfoText != null)
            questInfoText.text = "Chọn một nhiệm vụ để xem chi tiết";

        if (quest1Button) quest1Button.onClick.AddListener(() => OnQuestClicked(1));
        if (quest2Button) quest2Button.onClick.AddListener(() => OnQuestClicked(2));
        if (quest3Button) quest3Button.onClick.AddListener(() => OnQuestClicked(3));
        if (quest4Button) quest4Button.onClick.AddListener(() => OnQuestClicked(4));

        if (deliverButton) deliverButton.onClick.AddListener(OnDeliverClicked);
        if (rejectButton) rejectButton.onClick.AddListener(OnRejectClicked);
    }

    void OnQuestClicked(int questNumber)
    {
        if (selectedQuest == questNumber)
        {
            selectedQuest = -1;
            ResetAllHighlights();
            if (deliverButton) deliverButton.gameObject.SetActive(false);
            if (rejectButton) rejectButton.gameObject.SetActive(false);
            if (questInfoText != null) questInfoText.text = "Chọn một nhiệm vụ để xem chi tiết";
            return;
        }

        selectedQuest = questNumber;
        ResetAllHighlights();
        HighlightQuest(questNumber);

        if (deliverButton) deliverButton.gameObject.SetActive(true);
        if (rejectButton) rejectButton.gameObject.SetActive(true);

        Quest q = quests[questNumber - 1];
        if (questInfoText != null)
        {
            // --- KẾT NỐI INVENTORY: Kiểm tra số lượng ---
            int currentQty = 0;
            if (InventoryManager.Instance != null)
            {
                currentQty = InventoryManager.Instance.GetItemQuantity(q.itemName);
            }

            string color = currentQty >= q.quantity ? "green" : "red";

            questInfoText.text = $"<b>Nhiệm vụ {questNumber}</b>\n" +
                                 $"Yêu cầu: {q.itemName} <color={color}>({currentQty}/{q.quantity})</color>\n" +
                                 $"Phần thưởng: {q.rewardExp} EXP + {q.rewardGold} Gold";
        }
    }

    void OnDeliverClicked()
    {
        if (selectedQuest == -1) return;
        
        if (InventoryManager.Instance == null)
        {
            Debug.LogError("Chưa có InventoryManager trong Scene!");
            return;
        }

        Quest q = quests[selectedQuest - 1];

        // --- KẾT NỐI INVENTORY: Xóa đồ và Cộng tiền ---
        
        // 1. Kiểm tra lại lần nữa cho chắc
        int currentQty = InventoryManager.Instance.GetItemQuantity(q.itemName);

        if (currentQty >= q.quantity)
        {
            // 2. Trừ item (Gọi hàm RemoveItem overload string)
            bool success = InventoryManager.Instance.RemoveItem(q.itemName, q.quantity);

            if (success)
            {
                // 3. Cộng tiền
                InventoryManager.Instance.AddGold(q.rewardGold);

                // Cộng EXP (Logic nội bộ)
                playerExp += q.rewardExp;
                if (expText != null) expText.text = $"EXP: {playerExp}";

                Debug.Log($"✅ Giao nhiệm vụ thành công!");

                // Reset Quest
                quests[selectedQuest - 1] = GenerateNewQuest();
                SaveQuests();

                ToggleIcons(GetTick(selectedQuest), GetCross(selectedQuest));
                ResetAllHighlights();
                if (questInfoText != null) questInfoText.text = "Hoàn thành! Nhiệm vụ mới đã được tạo.";
            }
        }
        else
        {
            Debug.LogWarning("❌ Không đủ vật phẩm!");
            if (questInfoText != null)
                questInfoText.text = $"<color=red>Thiếu vật phẩm! Cần {q.quantity} {q.itemName}</color>";
        }

        if (deliverButton) deliverButton.gameObject.SetActive(false);
        if (rejectButton) rejectButton.gameObject.SetActive(false);
        selectedQuest = -1;
    }

    void OnRejectClicked()
    {
        if (selectedQuest == -1) return;

        Quest newQuest = GenerateNewQuest();
        quests[selectedQuest - 1] = newQuest;
        SaveQuests();

        ToggleIcons(GetCross(selectedQuest), GetTick(selectedQuest));

        ResetAllHighlights();
        if (questInfoText != null) questInfoText.text = "Đã từ chối. Nhiệm vụ mới được tạo.";

        if (deliverButton) deliverButton.gameObject.SetActive(false);
        if (rejectButton) rejectButton.gameObject.SetActive(false);
        selectedQuest = -1;
    }

    // --- CÁC HÀM PHỤ TRỢ (KHÔNG ĐỔI) ---

    void GenerateQuests()
    {
        quests.Clear();
        for (int i = 0; i < 4; i++) quests.Add(GenerateNewQuest());
        SaveQuests();
    }

    Quest GenerateNewQuest()
    {
        return new Quest
        {
            itemName = itemPool[Random.Range(0, itemPool.Length)],
            quantity = Random.Range(1, 4),
            rewardExp = Random.Range(30, 100),
            rewardGold = Random.Range(50, 200)
        };
    }

    void HideAllIcons()
    {
        if (tickIcon1) tickIcon1.gameObject.SetActive(false);
        if (crossIcon1) crossIcon1.gameObject.SetActive(false);
        if (tickIcon2) tickIcon2.gameObject.SetActive(false);
        if (crossIcon2) crossIcon2.gameObject.SetActive(false);
        if (tickIcon3) tickIcon3.gameObject.SetActive(false);
        if (crossIcon3) crossIcon3.gameObject.SetActive(false);
        if (tickIcon4) tickIcon4.gameObject.SetActive(false);
        if (crossIcon4) crossIcon4.gameObject.SetActive(false);
    }

    void ToggleIcons(Image primary, Image secondary)
    {
        if (primary) primary.gameObject.SetActive(!primary.gameObject.activeSelf);
        if (secondary) secondary.gameObject.SetActive(false);
    }

    void HighlightQuest(int questNumber)
    {
        Image bg = GetBackground(questNumber);
        if (bg != null) bg.color = selectedColor;
    }

    void ResetAllHighlights()
    {
        if (quest1Background) quest1Background.color = normalColor;
        if (quest2Background) quest2Background.color = normalColor;
        if (quest3Background) quest3Background.color = normalColor;
        if (quest4Background) quest4Background.color = normalColor;
    }

    Image GetBackground(int id) => id switch { 1 => quest1Background, 2 => quest2Background, 3 => quest3Background, 4 => quest4Background, _ => null };
    Image GetTick(int id) => id switch { 1 => tickIcon1, 2 => tickIcon2, 3 => tickIcon3, 4 => tickIcon4, _ => null };
    Image GetCross(int id) => id switch { 1 => crossIcon1, 2 => crossIcon2, 3 => crossIcon3, 4 => crossIcon4, _ => null };

    void SaveQuests()
    {
        for (int i = 0; i < quests.Count; i++)
        {
            Quest q = quests[i];
            PlayerPrefs.SetString($"Quest_{i}_Item", q.itemName);
            PlayerPrefs.SetInt($"Quest_{i}_Qty", q.quantity);
            PlayerPrefs.SetInt($"Quest_{i}_Exp", q.rewardExp);
            PlayerPrefs.SetInt($"Quest_{i}_Gold", q.rewardGold);
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
                Quest q = new Quest
                {
                    itemName = itemName,
                    quantity = PlayerPrefs.GetInt($"Quest_{i}_Qty", 1),
                    rewardExp = PlayerPrefs.GetInt($"Quest_{i}_Exp", 50),
                    rewardGold = PlayerPrefs.GetInt($"Quest_{i}_Gold", 100)
                };
                quests.Add(q);
            }
        }
    }
}