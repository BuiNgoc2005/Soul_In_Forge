using UnityEngine;

public class BackTrigger : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (!string.IsNullOrEmpty(SceneData.previousScene))
        {
            if (!string.IsNullOrEmpty(SceneData.previousSpawnPoint))
                SceneData.nextSpawnPoint = SceneData.previousSpawnPoint;

            SceneTransitionManager.Instance?.TransitionToScene(SceneData.previousScene);
        }
        else
        {
            Debug.LogWarning("No previous scene stored in SceneData. Can't return.");
        }
    }
}
