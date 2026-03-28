using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Game/ItemDatabase")]
public class ItemDatabase : ScriptableObject
{
    public List<ItemSO> allItems = new List<ItemSO>();

    private Dictionary<string, ItemSO> _byId;

    void OnEnable()
    {
        BuildLookup();
    }

    void BuildLookup()
    {
        _byId = new Dictionary<string, ItemSO>();
        foreach (var item in allItems)
        {
            if (item == null || string.IsNullOrEmpty(item.itemId))
                continue;

            if (!_byId.ContainsKey(item.itemId))
                _byId.Add(item.itemId, item);
            else
                Debug.LogWarning($"Trùng itemId: {item.itemId}");
        }
    }

    public ItemSO GetById(string id)
    {
        if (_byId == null) BuildLookup();
        _byId.TryGetValue(id, out var so);
        return so;
    }
}
