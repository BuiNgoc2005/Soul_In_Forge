using UnityEngine;
using TMPro;

public class UIRegister : MonoBehaviour
{
    public TMP_InputField emailField;
    public TMP_InputField passwordField;
    public MenuUIController ui;

    public void OnRegisterClick()
    {
        StartCoroutine(AuthAPI.Instance.Register(
            emailField.text,
            passwordField.text,

            (ok, err) =>
            {
                if (ok)
                {
                    Debug.Log("Register OK");
                    ui.BackToMenu();   // Chỉ chạy sau khi backend trả OK
                }
                else
                {
                    Debug.LogError("Register fail: " + err);
                }
            }
        ));
    }

}
