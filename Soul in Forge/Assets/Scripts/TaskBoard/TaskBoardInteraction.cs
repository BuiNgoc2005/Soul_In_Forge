using UnityEngine;

public class TaskBoardInteraction : MonoBehaviour
{
    [Header("UI Bảng nhiệm vụ")]
    public GameObject taskBoardUI;

    private bool isPlayerNearby = false;

    void Update()
    {
        // Kiểm tra khi người chơi ở gần và bấm Ctrl + E
        if (isPlayerNearby && Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("Mới bấm e");
            bool isActive = taskBoardUI.activeSelf;
            taskBoardUI.SetActive(!isActive);

            // Tạm dừng game khi mở bảng (nếu muốn)
            // Time.timeScale = isActive ? 1f : 0f;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerNearby = true;
            Debug.Log("Player lại gần bảng nhiệm vụ");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerNearby = false;
            Debug.Log("Player rời xa bảng nhiệm vụ");
        }
    }
}
