using TMPro;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

// Class lưu trữ dữ liệu (Bắt buộc giữ để không mất đồ khi chuyển cảnh)
[System.Serializable]
public class InventoryItemRecord
{
    public ItemSO itemSO;
    public int quantity;
    public InventoryItemRecord(ItemSO i, int q) { itemSO = i; quantity = q; }
}

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    [Header("References")]
    public GameObject inventoryUI;
    public SlotInventory[] itemSlots;
    [SerializeField] private Transform slotParent;
    public TMP_Text goldText;
    public int gold;

    // Dữ liệu lõi (Core Data) - Bất tử qua các Scene
    public List<InventoryItemRecord> coreInventory = new List<InventoryItemRecord>();

    private bool isOpen = false;
    public bool otherUIOpen = false;
    private Movement playerMovement; 

    // Events
    public delegate void InventoryChanged();
    public static event InventoryChanged OnInventoryChangedGlobal;
    public static void TriggerInventoryChanged() => OnInventoryChangedGlobal?.Invoke();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        Loot.OnItemLooted -= AddItem;
    }

    private void OnEnable() => Loot.OnItemLooted += AddItem;
    private void OnDisable() => Loot.OnItemLooted -= AddItem;

    void OnSceneLoaded(Scene scene, LoadSceneMode mode) => Initialize();
    void Start() => Initialize();

    // --- KHỞI TẠO & KẾT NỐI ---
    void Initialize()
    {
        isOpen = false;
        otherUIOpen = false;
        playerMovement = null;

        // 1. Tự động tìm UI
        if (inventoryUI == null)
        {
            Canvas childCanvas = GetComponentInChildren<Canvas>(true);
            if (childCanvas != null) inventoryUI = childCanvas.gameObject;
            
            if (inventoryUI == null)
                inventoryUI = GameObject.Find("InventoryUI") ?? GameObject.Find("InventoryPanel");
        }

        // 2. Setup UI
        if (inventoryUI != null)
        {
            Canvas cv = inventoryUI.GetComponent<Canvas>();
            if (cv != null)
            {
                cv.renderMode = RenderMode.ScreenSpaceOverlay;
                cv.sortingOrder = 999;
            }

            if (slotParent == null)
            {
                Transform[] kids = inventoryUI.GetComponentsInChildren<Transform>(true);
                foreach (var t in kids)
                {
                    if (t.name.Contains("Slot") && t.name.Contains("Container")) {
                        slotParent = t; break;
                    }
                }
                if (slotParent == null) slotParent = inventoryUI.transform;
            }

            if (slotParent != null)
                itemSlots = slotParent.GetComponentsInChildren<SlotInventory>(true);
            
            if (goldText != null) goldText.text = gold.ToString();

            RefreshUI(); // Nạp lại dữ liệu

            inventoryUI.SetActive(false);
        }

        // 3. Kết nối Player
        var pObj = GameObject.FindGameObjectWithTag("Player");
        if (pObj != null)
        {
            // Tìm script Movement (đổi tên class nếu cần cho khớp với project của bạn)
            playerMovement = pObj.GetComponent<Movement>(); 
            if (playerMovement != null) playerMovement.enabled = true;
        }
    }

    // --- HÀM BẬT/TẮT TÚI ĐỒ ---
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            if (isOpen) ToggleInventory();
            else if (!otherUIOpen) ToggleInventory();
        }
    }

    void ToggleInventory()
    {
        if (inventoryUI == null) Initialize();
        if (inventoryUI == null) return;

        isOpen = !isOpen;
        inventoryUI.SetActive(isOpen);
        otherUIOpen = isOpen;

        if (playerMovement == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p) playerMovement = p.GetComponent<Movement>();
        }

        if (playerMovement != null)
            playerMovement.enabled = !isOpen;
    }

    // --- ĐỒNG BỘ DỮ LIỆU ---
    private void RefreshUI()
    {
        if (itemSlots == null) return;

        foreach (var slot in itemSlots) { slot.itemSO = null; slot.quantity = 0; slot.UpdateUI(); }
        
        if (goldText != null) goldText.text = gold.ToString();

        int index = 0;
        foreach (var data in coreInventory)
        {
            if (index < itemSlots.Length)
            {
                itemSlots[index].itemSO = data.itemSO;
                itemSlots[index].quantity = data.quantity;
                itemSlots[index].UpdateUI();
                index++;
            }
        }
    }

    // --- LOGIC QUẢN LÝ VÀNG (GOLD) ---
    
    public bool HasEnoughGold(int amount)
    {
        return gold >= amount;
    }

    public void SpendGold(int amount)
    {
        gold -= amount;
        if (gold < 0) gold = 0;
        RefreshUI(); // Cập nhật Text ngay lập tức
    }

    public void AddGold(int amount)
    {
        gold += amount;
        RefreshUI();
    }

    // --- LOGIC ITEM ---
    public void AddItem(ItemSO itemSO, int quantity)
    {
        if (itemSO.isGold) {
            AddGold(quantity);
            return;
        }

        bool added = false;

        // 1. Stack
        if (itemSO.isStackable)
        {
            foreach (var data in coreInventory)
            {
                if (data.itemSO == itemSO && data.quantity < itemSO.maxStack)
                {
                    int space = itemSO.maxStack - data.quantity;
                    int add = Mathf.Min(space, quantity);
                    data.quantity += add; quantity -= add;
                    if (quantity <= 0) { added = true; break; }
                }
            }
        }

        // 2. Empty
        while (quantity > 0)
        {
            if (coreInventory.Count >= 20) break;
            int add = (itemSO.isStackable) ? Mathf.Min(itemSO.maxStack, quantity) : 1;
            coreInventory.Add(new InventoryItemRecord(itemSO, add));
            quantity -= add; added = true;
        }

        if (added) { RefreshUI(); TriggerInventoryChanged(); }
    }

    public bool TryAddItem(ItemSO itemSO, int quantity)
    {
        if (itemSO.isGold) { AddGold(quantity); return true; }
        
        // Kiểm tra nhanh xem còn chỗ không (đơn giản hóa)
        if (coreInventory.Count >= 20) return false; 

        AddItem(itemSO, quantity);
        return true;
    }

    public bool RemoveItem(string itemName, int amount = 1)
    {
        int needed = amount;
        for (int i = coreInventory.Count - 1; i >= 0; i--)
        {
            if (coreInventory[i].itemSO.itemName == itemName)
            {
                if (coreInventory[i].quantity >= needed) {
                    coreInventory[i].quantity -= needed; needed = 0;
                } else {
                    needed -= coreInventory[i].quantity; coreInventory[i].quantity = 0;
                }
                if (coreInventory[i].quantity <= 0) coreInventory.RemoveAt(i);
                if (needed <= 0) break;
            }
        }
        if (needed < amount) { RefreshUI(); TriggerInventoryChanged(); return true; }
        return false;
    }
    
    public bool RemoveItem(ItemSO itemSO, int quantity = 1)
    {
        if (itemSO.isGold) {
             if (HasEnoughGold(quantity)) { SpendGold(quantity); return true; }
             return false;
        }
        
        int needed = quantity;
        for (int i = coreInventory.Count - 1; i >= 0; i--)
        {
            if (coreInventory[i].itemSO == itemSO)
            {
                if (coreInventory[i].quantity >= needed) {
                    coreInventory[i].quantity -= needed; needed = 0;
                } else {
                    needed -= coreInventory[i].quantity; coreInventory[i].quantity = 0;
                }
                if (coreInventory[i].quantity <= 0) coreInventory.RemoveAt(i);
                if (needed <= 0) break;
            }
        }
        if (needed < quantity) { RefreshUI(); TriggerInventoryChanged(); return true; }
        return false;
    }
    public int GetItemQuantity(string itemName)
    {
        Debug.Log($"❓ TaskBoard đang tìm: '{itemName}'");
        int total = 0;
        foreach (var data in coreInventory)
        {
            Debug.Log($"   🎒 Trong túi có: '{data.itemSO.itemName}' - Số lượng: {data.quantity}");
            if (data.itemSO.itemName == itemName)
            {
                total += data.quantity;
            }
        }
        Debug.Log($"   => TỔNG KẾT: Tìm thấy {total} cái '{itemName}'.");
        return total;
    }
}