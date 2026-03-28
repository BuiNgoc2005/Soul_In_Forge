using UnityEngine;

public class NPC : MonoBehaviour
{
    public enum NPCState { Default, Idle, Talk }
    public NPCState currentState = NPCState.Idle;

    [SerializeField] private Hetus_Talk talk;  // gán trong Inspector
    [SerializeField] private GameObject T;     // icon bong bóng gợi ý

    private Movement playerMovement;
    private bool isTalking = false;

    void Start()
    {
        // Cache sẵn
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player) playerMovement = player.GetComponent<Movement>();

        // Trạng thái mặc định khi vào game
        SwitchState(currentState);
        if (T) T.SetActive(false);        // ẩn bong bóng khi chưa đứng gần
    }

    public void SwitchState(NPCState newState)
    {
        currentState = newState;
        if (talk) talk.enabled = (newState == NPCState.Talk);
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (!col.CompareTag("Player")) return;

        // Người chơi vào vùng → hiện bong bóng, cho phép nói
        if (T) T.SetActive(true);
        SwitchState(NPCState.Talk);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        // Chỉ khi bấm F lần đầu mới bắt đầu nói
        if (Input.GetKeyDown(KeyCode.F) && !isTalking)
        {
            isTalking = true;

            if (playerMovement) playerMovement.enabled = false;   // khoá di chuyển
            if (talk) talk.enabled = true;                        // hoặc talk.StartDialogue();
            if (T) T.SetActive(false);                            // ẩn bong bóng khi đã nói
        }
        // Không có nhánh else "quét sạch" nữa để tránh tắt bong bóng sai lúc
    }

    private void OnTriggerExit2D(Collider2D col)
    {
        if (!col.CompareTag("Player")) return;

        // Rời vùng → reset
        if (playerMovement) playerMovement.enabled = true;
        isTalking = false;
        if (T) T.SetActive(false);
        SwitchState(NPCState.Idle);

    }

}
