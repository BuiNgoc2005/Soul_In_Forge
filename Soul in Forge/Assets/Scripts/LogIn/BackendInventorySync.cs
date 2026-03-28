using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BackendInventorySync : MonoBehaviour
{
    public static BackendInventorySync Instance;

    public ItemDatabase itemDatabase;

    [Header("Auto Save")]
    public float saveDelay = 1.0f; // chống spam

    private bool _dirty = false;
    private bool _saving = false;

    // CHẶN AUTOSAVE CHO ĐẾN KHI LOAD PROFILE XONG
    private bool ready = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else { Destroy(gameObject); return; }
    }

    void OnEnable()
    {
        InventoryManager.OnInventoryChangedGlobal += MarkDirty;
        LevelSystem.OnExpOrLevelChanged += MarkDirty;
    }

    void OnDisable()
    {
        InventoryManager.OnInventoryChangedGlobal -= MarkDirty;
        LevelSystem.OnExpOrLevelChanged -= MarkDirty;
    }

    void Start()
    {
        // Chỉ load nếu token hợp lệ
        if (!string.IsNullOrEmpty(AuthAPI.Instance.Token))
        {
            StartCoroutine(LoadFromServer());
        }
        else
        {
            Debug.LogWarning("⛔ Không có token → chưa login → không load profile.");
        }
    }

    public void MarkDirty()
    {
        // Nếu chưa load profile xong → không autosave
        if (!ready)
        {
            Debug.LogWarning("⛔ AutoSave blocked: profile chưa sẵn sàng.");
            return;
        }

        _dirty = true;
        if (!_saving)
            StartCoroutine(AutoSaveRoutine());
    }

    IEnumerator AutoSaveRoutine()
    {
        _saving = true;
        yield return new WaitForSeconds(saveDelay);

        // Không save nếu chưa login xong
        if (!ready)
        {
            _saving = false;
            yield break;
        }

        if (_dirty)
        {
            _dirty = false;
            yield return SaveToServer();
        }

        _saving = false;
    }

    // --------- BUILD / APPLY PROFILE ---------

    GameDataAPI.ProfileDto BuildProfile()
    {
        var profile = new GameDataAPI.ProfileDto();

        profile.gold = InventoryManager.Instance.gold;
        profile.exp = LevelSystem.currentExp;

        foreach (var rec in InventoryManager.Instance.coreInventory)
        {
            if (rec.itemSO == null) continue;

            profile.inventory.Add(new GameDataAPI.InventoryItemDto
            {
                itemId = rec.itemSO.itemId,
                quantity = rec.quantity
            });
        }

        return profile;
    }

    void ApplyProfile(GameDataAPI.ProfileDto p)
    {
        if (p == null) return;

        InventoryManager.Instance.gold = p.gold;
        LevelSystem.currentExp = p.exp;

        InventoryManager.Instance.coreInventory.Clear();
        foreach (var it in p.inventory)
        {
            var so = itemDatabase.GetById(it.itemId);
            if (so == null)
            {
                Debug.LogWarning("⚠ Không tìm thấy ItemSO cho itemId: " + it.itemId);
                continue;
            }

            InventoryManager.Instance.coreInventory.Add(
                new InventoryItemRecord(so, it.quantity)
            );
        }

        InventoryManager.Instance.SendMessage("RefreshUI", SendMessageOptions.DontRequireReceiver);

        var lvl = FindFirstObjectByType<LevelSystem>();
        if (lvl != null) lvl.UpdateUI();
    }

    // --------- COROUTINES ---------

    public IEnumerator LoadFromServer()
    {
        yield return GameDataAPI.Instance.LoadProfile((ok, profile, err) =>
        {
            if (ok && profile != null)
            {
                ApplyProfile(profile);
                Debug.Log("✅ Loaded profile from server");

                ready = true;  // ← CHỈ BẬT AUTOSAVE KHI LOAD XONG
            }
            else
            {
                Debug.LogWarning("⚠ Không load được profile: " + err);
            }
        });
    }

    IEnumerator SaveToServer()
    {
        // Không save nếu chưa login hoặc chưa ready
        if (!ready)
        {
            Debug.LogWarning("⛔ Save bị chặn: chưa load profile xong!");
            yield break;
        }

        var profile = BuildProfile();

        yield return GameDataAPI.Instance.SaveProfile(profile, (ok, err) =>
        {
            if (ok) Debug.Log("💾 Saved profile to server");
            else Debug.LogWarning("⛔ Save error: " + err);
        });
    }
}
