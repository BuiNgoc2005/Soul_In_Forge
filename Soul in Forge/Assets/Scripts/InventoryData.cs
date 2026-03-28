using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "InventoryData", menuName = "Game/InventoryData")]
public class InventoryData : ScriptableObject
{
    [Serializable]
    public class Entry
    {
        public ItemSO item;
        public int qty;
    }

    [Header("Settings")]
    public int maxSlots = 20;

    [Header("Runtime (visible)")]
    public List<Entry> entries = new List<Entry>();

    [Header("Currency")]
    public int gold = 0;

    // Event để UI/Shop lắng nghe
    public event Action OnInventoryChanged;

    // ---------- API ----------
    public void ClearAll()
    {
        entries.Clear();
        OnInventoryChanged?.Invoke();
    }

    public int IndexOf(ItemSO item)
    {
        if (item == null) return -1;
        for (int i = 0; i < entries.Count; i++)
            if (entries[i].item == item) return i;
        return -1;
    }

    public bool CanAdd(ItemSO item, int quantity)
    {
        if (item == null) return false;
        if (item.isGold) return true;

        int need = quantity;

        if (item.isStackable)
        {
            for (int i = 0; i < entries.Count; i++)
            {
                var e = entries[i];
                if (e.item == item)
                {
                    int space = item.maxStack - e.qty;
                    need -= Math.Min(space, need);
                    if (need <= 0) return true;
                }
            }
        }

        int currentSlots = entries.Count;
        int freeSlots = Math.Max(0, maxSlots - currentSlots);

        if (item.isStackable)
        {
            int perSlot = item.maxStack;
            int capacity = freeSlots * perSlot;
            return need <= capacity;
        }
        else
        {
            return need <= freeSlots;
        }
    }

    public bool TryAdd(ItemSO item, int quantity)
    {
        if (item == null) return false;
        if (item.isGold)
        {
            gold += quantity;
            OnInventoryChanged?.Invoke();
            return true;
        }

        if (!CanAdd(item, quantity)) return false;

        int remaining = quantity;

        if (item.isStackable)
        {
            foreach (var e in entries)
            {
                if (e.item == item && e.qty < item.maxStack)
                {
                    int add = Math.Min(item.maxStack - e.qty, remaining);
                    e.qty += add;
                    remaining -= add;
                    if (remaining <= 0)
                    {
                        OnInventoryChanged?.Invoke();
                        return true;
                    }
                }
            }
        }

        while (remaining > 0)
        {
            int put = item.isStackable ? Math.Min(item.maxStack, remaining) : 1;
            var en = new Entry() { item = item, qty = put };
            entries.Add(en);
            remaining -= put;
        }

        OnInventoryChanged?.Invoke();
        return true;
    }

    public void NotifyChanged()
    {
        OnInventoryChanged?.Invoke();
    }

    public bool Remove(ItemSO item, int quantity = 1)
    {
        if (item == null) return false;
        int need = quantity;
        for (int i = entries.Count - 1; i >= 0; i--)
        {
            var e = entries[i];
            if (e.item == item)
            {
                if (e.qty > need)
                {
                    e.qty -= need;
                    need = 0;
                    break;
                }
                else
                {
                    need -= e.qty;
                    entries.RemoveAt(i);
                }
            }
            if (need <= 0) break;
        }
        if (need <= 0)
        {
            OnInventoryChanged?.Invoke();
            return true;
        }
        return false;
    }
}
