using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;

    [HideInInspector] public SlotInventory slot;

    private Image draggedIcon;
    private Transform originalParent;

    void Awake()
    {
        slot = GetComponent<SlotInventory>();
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = gameObject.AddComponent<CanvasGroup>();
        canvas = GetComponentInParent<Canvas>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (slot.itemSO == null) return;

        draggedIcon = new GameObject("DraggedIcon").AddComponent<Image>();
        draggedIcon.sprite = slot.itemSO.icon;
        draggedIcon.transform.SetParent(canvas.transform, false);
        draggedIcon.raycastTarget = false;
        draggedIcon.rectTransform.sizeDelta = rectTransform.sizeDelta;

        canvasGroup.alpha = 0.6f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (draggedIcon != null)
            draggedIcon.transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Destroy(draggedIcon);
        canvasGroup.alpha = 1f;
    }
}
