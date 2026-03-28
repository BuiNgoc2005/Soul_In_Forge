using UnityEngine;

public class InventoryDataMigrator : MonoBehaviour
{
    public InventoryManager sourceManager; // gán
    public InventoryData targetData; // gán

    [ContextMenu("MigrateOnce")]
    public void MigrateOnce()
    {
        if (sourceManager == null || targetData == null) { Debug.LogError("Assign both"); return; }
        targetData.entries.Clear();
        foreach (var s in sourceManager.itemSlots)
        {
            if (s != null && s.itemSO != null)
            {
                var e = new InventoryData.Entry() { item = s.itemSO, qty = s.quantity };
                targetData.entries.Add(e);
            }
        }
        targetData.gold = sourceManager.gold;
        targetData.NotifyChanged();
        Debug.Log("Inventory migrated to InventoryData.");
    }
}
