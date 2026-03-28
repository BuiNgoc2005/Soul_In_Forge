using UnityEngine;

public class PlayerExp : MonoBehaviour
{
    public int exp = 0;

    // Cộng EXP cho người chơi
    public void AddExp(int amount)
    {
        exp += amount;
        Debug.Log($"Thêm {amount} EXP. Tổng EXP hiện tại: {exp}");
    }
}
