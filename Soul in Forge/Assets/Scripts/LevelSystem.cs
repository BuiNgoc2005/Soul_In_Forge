using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelSystem : MonoBehaviour
{
    // === CÁC THÔNG SỐ CƠ BẢN (STATIC - BẤT TỬ) ===
    public static int level;
    public static int currentExp;
    public static int requiredExp;

    private static bool isInitialized = false;

    // === KẾT NỐI VỚI UI (GIAO DIỆN) ===
    public Image expBarFill;
    public TextMeshProUGUI levelText;

    private void Start()
    {
        // 1. Khởi tạo dữ liệu lần đầu
        if (!isInitialized)
        {
            level = 1;
            currentExp = 0;
            
            // SỬA: Tăng lên 100 để cân bằng với Quest (Quest thưởng 30-100 exp)
            requiredExp = 100; 
            
            isInitialized = true;
            Debug.Log("LEVEL SYSTEM: Khởi tạo dữ liệu mới.");
        }

        // 2. Cập nhật UI ngay khi vào Scene
        UpdateUI();
        
        // SỬA: Đã XÓA đoạn đăng ký Loot.OnItemLooted để tắt tính năng "nhặt đồ lên cấp"
    }

    // --- HÀM MỚI: CHO PHÉP TASKBOARD GỌI VÀO ---
    public static void AddExperienceGlobal(int amount)
    {
        // Cộng vào dữ liệu tĩnh
        currentExp += amount;
        Debug.Log($"[LevelSystem] Nhận {amount} EXP! (Tổng: {currentExp}/{requiredExp})");

        // Kiểm tra lên cấp
        CheckLevelUp();

        // THÊM: báo cho hệ thống save biết là exp/level đã đổi
        NotifyChanged();

        // Tìm UI đang hoạt động để cập nhật (nếu có)
        // Vì script này gắn trên UI, ta cần tìm instance đang sống
        LevelSystem instance = FindFirstObjectByType<LevelSystem>();
        if (instance != null)
        {
            instance.UpdateUI();
        }
    }

    // Logic kiểm tra lên cấp (Static để chạy ngầm được)
    private static void CheckLevelUp()
    {
        while (currentExp >= requiredExp)
        {
            currentExp -= requiredExp;
            level++;
            // Tăng khó lên một chút mỗi khi lên cấp (tùy chọn)
            requiredExp += 50; 
            Debug.Log($"LEVEL UP! Đã đạt cấp {level}! Cần {requiredExp} cho cấp tiếp theo.");
        }
    }

    // Cập nhật thanh UI (Instance method)
    public void UpdateUI()
    {
        if (requiredExp == 0) requiredExp = 100; // Safety check

        float fill = (float)currentExp / (float)requiredExp;
        
        if (expBarFill != null)
            expBarFill.fillAmount = fill;

        if (levelText != null)
            levelText.text = $"{level}";
    }

    public static event Action OnExpOrLevelChanged;
    private static void NotifyChanged()
    {
        OnExpOrLevelChanged?.Invoke();
    }
}