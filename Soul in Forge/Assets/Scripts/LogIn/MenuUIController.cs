using UnityEngine;

public class MenuUIController : MonoBehaviour
{
    public GameObject mainMenuUI;
    public GameObject loginUI;
    public GameObject signinUI;

    void Start()
    {
        // Khi chạy game: chỉ hiện main menu
        mainMenuUI.SetActive(true);
        loginUI.SetActive(false);
        signinUI.SetActive(false);
    }

    public void OpenLoginUI()
    {
        mainMenuUI.SetActive(false);
        loginUI.SetActive(true);
        signinUI.SetActive(false);
    }

    public void OpenSigninUI()
    {
        mainMenuUI.SetActive(false);
        loginUI.SetActive(false);
        signinUI.SetActive(true);
    }

    public void BackToMenu()
    {
        mainMenuUI.SetActive(true);
        loginUI.SetActive(false);
        signinUI.SetActive(false);
    }
}
