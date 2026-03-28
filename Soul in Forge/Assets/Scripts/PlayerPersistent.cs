using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerPersistent : MonoBehaviour
{
    public static PlayerPersistent Instance;

    [Tooltip("Turn off collider")]
    public float invulnerableAfterLoad = 3f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!string.IsNullOrEmpty(SceneData.nextSpawnPoint))
        {
            GameObject sp = GameObject.Find(SceneData.nextSpawnPoint);
            if (sp != null)
            {
                transform.position = sp.transform.position;
            }
            else
            {
                Debug.LogWarning("SpawnPoint not found: " + SceneData.nextSpawnPoint);
            }

            SceneData.nextSpawnPoint = "";

            StartCoroutine(TemporarilyDisablePlayerColliders(invulnerableAfterLoad));
        }
    }

    private IEnumerator TemporarilyDisablePlayerColliders(float delay)
    {
        Collider2D[] cols = GetComponents<Collider2D>();
        Rigidbody2D rb = GetComponent<Rigidbody2D>();

        if (cols == null || cols.Length == 0)
            yield break;

        foreach (var c in cols) c.enabled = false;

        yield return new WaitForSeconds(delay);

        foreach (var c in cols) c.enabled = true;

    }
}
