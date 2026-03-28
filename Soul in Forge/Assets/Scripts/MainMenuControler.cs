using UnityEngine;
using UnityEngine.SceneManagement;

public class StartMenuController : MonoBehaviour
{
    // Hàm chạy khi bấm nút Start
    public void OnStartClick()
    {
        SceneManager.LoadScene("ForgeScene");
    }


    public void OnLoginClick()
    {
        SceneManager.LoadScene("StartScene");
    }


    // Hàm chạy khi bấm nút Exit
    public void OnExitClick()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;   // Thoát Play mode trong Editor
#else
        Application.Quit();  // Thoát game sau khi build
#endif
    }
}
