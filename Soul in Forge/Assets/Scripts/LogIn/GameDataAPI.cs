using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class GameDataAPI : MonoBehaviour
{
    public static GameDataAPI Instance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    // -------------------- DTO --------------------

    [Serializable]
    public class InventoryItemDto
    {
        public string itemId;
        public int quantity;
    }

    [Serializable]
    public class ProfileDto
    {
        public int exp;            // ✔ KHỚP BACKEND
        public int gold;           // ✔ KHỚP BACKEND
        public List<InventoryItemDto> inventory = new();
    }

    // -------------------- LOAD PROFILE --------------------

    public IEnumerator LoadProfile(Action<bool, ProfileDto, string> callback)
    {
        string url = AuthAPI.Instance.baseUrl + "/user/profile";

        using (var req = UnityWebRequest.Get(url))
        {
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Authorization", "Bearer " + AuthAPI.Instance.Token);

            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.Success)
            {
                // Parse backend response
                var profile = JsonUtility.FromJson<ProfileDto>(req.downloadHandler.text);
                callback?.Invoke(true, profile, null);
            }
            else
            {
                callback?.Invoke(false, null, req.downloadHandler.text);
            }
        }
    }

    // -------------------- SAVE PROFILE --------------------

    public IEnumerator SaveProfile(ProfileDto data, Action<bool, string> callback = null)
    {
        string url = AuthAPI.Instance.baseUrl + "/user/save";
        string json = JsonUtility.ToJson(data);

        using (var req = new UnityWebRequest(url, "POST"))
        {
            req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
            req.downloadHandler = new DownloadHandlerBuffer();

            req.SetRequestHeader("Content-Type", "application/json");
            req.SetRequestHeader("Authorization", "Bearer " + AuthAPI.Instance.Token);

            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.Success)
                callback?.Invoke(true, null);
            else
                callback?.Invoke(false, req.downloadHandler.text);
        }
    }
}
