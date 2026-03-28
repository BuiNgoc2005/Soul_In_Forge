using UnityEngine;


public enum ItemType
{
    Material,
    Intermediate,  // Bán thành phẩm: thân kiếm, thân cung...
    Equipment,     // Thành phẩm cuối: kiếm, cung...
    Tool,
    Quest
}

[System.Flags]
public enum ItemTag
{
    None = 0,
    Ore = 1 << 0, // Quặng
    Ingot = 1 << 1, // Thỏi
    Body = 1 << 2, // Thân vũ khí (bán thành phẩm)
    Handle = 1 << 3, // Cán/gậy gỗ
    Final = 1 << 4  // Thành phẩm hoàn chỉnh
}

[CreateAssetMenu(fileName = "New Item")]

public class ItemSO : ScriptableObject
{
    [Header("Identity")]
    public string itemId;             // ví dụ: "ore_copper", "ingot_copper"
    public string itemName;
    [TextArea] public string itemDescription;

    [Header("Visual")]
    public Sprite icon;

    [Header("Classification")]
    public ItemType type = ItemType.Material;
    public ItemTag tags = ItemTag.None;

    [Header("Stack & Economy")]
    public bool isStackable = true;
    public int maxStack = 99;
    public bool isGold;               // nếu bạn cần "vàng" tiền tệ
    public int price;           // giá bán trong shop

    [Header("Temporary (optional)")]
    public float duration;            // cho buff/đồ tạm thời nếu có
}
