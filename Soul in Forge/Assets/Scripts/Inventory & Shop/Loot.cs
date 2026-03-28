using System;
using UnityEngine;

public class Loot : MonoBehaviour  
{
    public ItemSO itemSO;
    public SpriteRenderer sr;

    public int quantity;
    public static event Action<ItemSO, int> OnItemLooted;

    private void OnValidate()
    {
        if (itemSO == null)
            return;

        sr.sprite = itemSO.icon;
        this.name = itemSO.itemName;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            OnItemLooted?.Invoke(itemSO, quantity);
            Destroy(gameObject);
        }
    }

}
