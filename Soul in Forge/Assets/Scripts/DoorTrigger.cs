using UnityEngine;
using UnityEngine.SceneManagement;

public class DoorTrigger : MonoBehaviour
{
    [Tooltip("scene(Build Settings)")]
    public string targetScene;

    [Tooltip("Spawn point name (GameObject name)")]
    public string spawnPointName;

    [Tooltip("(Optional) Spawn point name")]
    public string returnSpawnPointName;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        SceneData.previousScene = SceneManager.GetActiveScene().name;

        SceneData.previousSpawnPoint = returnSpawnPointName;

        SceneData.nextSpawnPoint = spawnPointName;

        if (SceneTransitionManager.Instance != null)
            SceneTransitionManager.Instance.TransitionToScene(targetScene);
        else
            Debug.LogError("SceneTransitionManager not found!");
    }
}
