using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SlotInventory : MonoBehaviour
{
    public ItemSO itemSO;
    public int quantity;

    public Image itemImage;
    public TMP_Text quantityText;

    private void Awake()
    {
        // auto bind nếu bạn quên gán trong prefab / UI
        if (itemImage == null)
            itemImage = GetComponentInChildren<Image>(true);

        if (quantityText == null)
            quantityText = GetComponentInChildren<TMP_Text>(true);
    }

    public void UpdateUI()
    {
        // luôn kiểm tra null trước khi dùng
        if (itemSO != null)
        {
            if (itemImage != null)
            {
                itemImage.sprite = itemSO.icon;
                itemImage.gameObject.SetActive(true);
            }
            else
            {
                Debug.LogWarning("[SlotInventory] itemImage is null on " + gameObject.name);
            }

            if (quantityText != null)
            {
                quantityText.text = quantity.ToString();
                quantityText.gameObject.SetActive(true);
            }
            else
            {
                Debug.LogWarning("[SlotInventory] quantityText is null on " + gameObject.name);
            }
        }
        else
        {
            if (itemImage != null) itemImage.gameObject.SetActive(false);
            if (quantityText != null) quantityText.gameObject.SetActive(false);
        }
    }
}
