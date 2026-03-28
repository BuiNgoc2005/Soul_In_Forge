using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance;

    [Header("UI References (try to assign in Inspector, otherwise name FadeImage)")]
    public Image fadeImage;         // full-screen Image (black) to fade
    public Slider progressBar;      // (optional)

    [Header("Settings")]
    public float fadeDuration = 0.6f;
    [Header("Optional: Scene -> Theme mapping")]
    public SceneThemeMapping themeMapping; // asset mapping in Inspector


    void Awake()
    {
        // Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("[STM] Instance created & DontDestroyOnLoad");
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Try to ensure we have a valid fadeImage reference now
        EnsureFadeImageReference();

        // --- play initial scene theme immediately (optional) ---
        if (themeMapping != null && AudioManager.Instance != null)
        {
            AudioClip initial = themeMapping.GetClipFor(SceneManager.GetActiveScene().name);
            if (initial != null)
            {
                AudioManager.Instance.PlayClipImmediate(initial);
            }
        }

        // start transparent
        if (fadeImage != null)
        {
            SetImageAlpha(0f);
            fadeImage.raycastTarget = false;
        }
        if (progressBar != null) progressBar.gameObject.SetActive(false);

        // Optional: re-find fadeImage after every scene load in case scenes have their own canvas
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void Start()
    {
        // Play the correct music for the current scene at startup
        if (themeMapping != null && AudioManager.Instance != null)
        {
            AudioClip current = themeMapping.GetClipFor(SceneManager.GetActiveScene().name);
            if (current != null)
            {
                AudioManager.Instance.PlayClipImmediate(current);
            }
        }
    }


    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // After a new scene loads, try to re-assign fadeImage if current reference is missing or belongs to old scene
        EnsureFadeImageReference();
    }

    void EnsureFadeImageReference()
    {
        if (fadeImage != null) return;

        // 1) try find by name in current scene(s)
        GameObject go = GameObject.Find("FadeImage");
        if (go != null)
        {
            fadeImage = go.GetComponent<Image>();
            if (fadeImage != null)
            {
                Debug.Log("[STM] Found FadeImage by name and assigned.");
                return;
            }
        }

        // 2) try find inside this object (child)
        Transform t = transform.Find("TransitionCanvas/FadeImage");
        if (t != null)
        {
            fadeImage = t.GetComponent<Image>();
            if (fadeImage != null)
            {
                Debug.Log("[STM] Found FadeImage as child of manager.");
                return;
            }
        }

        // 3) try find any object called "TransitionCanvas" then child "FadeImage"
        GameObject canvas = GameObject.Find("TransitionCanvas");
        if (canvas != null)
        {
            Transform f = canvas.transform.Find("FadeImage");
            if (f != null) fadeImage = f.GetComponent<Image>();
            if (fadeImage != null)
            {
                Debug.Log("[STM] Found FadeImage under TransitionCanvas.");
                return;
            }
        }

        // 4) give up (fadeImage stays null) — caller will handle it
        Debug.LogWarning("[STM] fadeImage not assigned and not found in scene. Assign in Inspector or name an Image object 'FadeImage'.");
    }

    // Public API
    public void TransitionToScene(string sceneName)
    {
        if (fadeImage == null)
            EnsureFadeImageReference(); // try again before starting

        Debug.Log("[STM] TransitionToScene -> " + sceneName);

        // --- request music crossfade for target scene (if mapping + AudioManager exist) ---
        if (themeMapping != null && AudioManager.Instance != null)
        {
            AudioClip nextClip = themeMapping.GetClipFor(sceneName);
            // if nextClip == null then CrossfadeToClip(null, ...) fade out music
            AudioManager.Instance.CrossfadeToClip(nextClip, fadeDuration);
        }

        StartCoroutine(DoSceneTransition(sceneName));

    }

    private IEnumerator DoSceneTransition(string sceneName)
    {
        // make sure fadeImage exists, else do an immediate load fallback
        if (fadeImage == null)
        {
            Debug.LogWarning("[STM] fadeImage missing — performing instant scene load as fallback.");
            SceneManager.LoadScene(sceneName);
            yield break;
        }

        // ensure starting alpha is 0 before fade-out
        SetImageAlpha(0f);
        yield return StartCoroutine(Fade(0f, 1f, fadeDuration));

        if (progressBar != null)
        {
            progressBar.gameObject.SetActive(true);
            progressBar.value = 0f;
        }

        AsyncOperation ao = SceneManager.LoadSceneAsync(sceneName);
        ao.allowSceneActivation = false;

        while (ao.progress < 0.9f)
        {
            if (progressBar != null) progressBar.value = ao.progress;
            yield return null;
        }
        if (progressBar != null) progressBar.value = 1f;

        yield return new WaitForSeconds(0.05f);

        ao.allowSceneActivation = true;
        while (!ao.isDone) yield return null;

        if (progressBar != null) progressBar.gameObject.SetActive(false);

        // after new scene activated, ensure fadeImage reference is valid (scene might have replaced UI)
        EnsureFadeImageReference();
        // if fadeImage found, fade in
        if (fadeImage != null)
            yield return StartCoroutine(Fade(1f, 0f, fadeDuration));
    }

    private IEnumerator Fade(float from, float to, float duration)
    {
        if (fadeImage == null)
            yield break;

        float t = 0f;
        fadeImage.raycastTarget = true;
        // ensure immediate start value
        SetImageAlpha(from);

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float a = Mathf.Lerp(from, to, t / duration);
            SetImageAlpha(a);
            yield return null;
        }
        SetImageAlpha(to);
        fadeImage.raycastTarget = (to > 0.5f);
    }

    private void SetImageAlpha(float a)
    {
        if (fadeImage == null) return;
        Color c = fadeImage.color;
        c.a = Mathf.Clamp01(a);
        fadeImage.color = c;
    }
}
