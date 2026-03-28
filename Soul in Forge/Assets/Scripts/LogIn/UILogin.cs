using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class UILogin : MonoBehaviour
{
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;

    public void OnClickLogin()
    {
        StartCoroutine(AuthAPI.Instance.Login(
            emailInput.text,
            passwordInput.text,
            (ok, err) =>
            {
                if (ok)
                {
                    Debug.Log("Login OK");
                    StartCoroutine(LoginFlow());
                }
                else
                {
                    Debug.LogError("Login fail: " + err);
                }
            }
        ));
    }

    private IEnumerator LoginFlow()
    {
        // Load dữ liệu trước
        yield return BackendInventorySync.Instance.LoadFromServer();

        Debug.Log("Profile loaded → vào ForgeScene");

        SceneManager.LoadScene("ForgeScene");
    }
}
