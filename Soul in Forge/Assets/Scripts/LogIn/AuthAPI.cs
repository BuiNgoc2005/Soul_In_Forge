using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class AuthAPI : MonoBehaviour
{
    public static AuthAPI Instance;

    [Header("Backend")]
    public string baseUrl = "http://localhost:3000";

    public string Token { get; private set; }

    const string TOKEN_KEY = "auth_token";

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Token = PlayerPrefs.GetString(TOKEN_KEY, "");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    [Serializable]
    private class AuthRequest
    {
        public string email;
        public string password;
    }

    [Serializable]
    private class LoginResponse
    {
        public string token;
    }

    public IEnumerator Register(string email, string password, Action<bool, string> callback)
    {
        var reqObj = new AuthRequest { email = email, password = password };
        string json = JsonUtility.ToJson(reqObj);

        using (var req = new UnityWebRequest(baseUrl + "/auth/register", "POST"))
        {
            byte[] body = Encoding.UTF8.GetBytes(json);
            req.uploadHandler = new UploadHandlerRaw(body);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");

            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.Success)
                callback?.Invoke(true, null);
            else
                callback?.Invoke(false, req.downloadHandler.text);
        }
    }

    public IEnumerator Login(string email, string password, Action<bool, string> callback)
    {
        var reqObj = new AuthRequest { email = email, password = password };
        string json = JsonUtility.ToJson(reqObj);

        using (var req = new UnityWebRequest(baseUrl + "/auth/login", "POST"))
        {
            byte[] body = Encoding.UTF8.GetBytes(json);
            req.uploadHandler = new UploadHandlerRaw(body);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");

            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.Success)
            {
                var res = JsonUtility.FromJson<LoginResponse>(req.downloadHandler.text);
                Token = res.token;
                PlayerPrefs.SetString(TOKEN_KEY, Token);
                callback?.Invoke(true, null);
            }
            else
            {
                callback?.Invoke(false, req.downloadHandler.text);
            }
        }
    }
}
