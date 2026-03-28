using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    private IInteractable nearbyObject;

    void Update()
    {
        if (nearbyObject != null && Input.GetKeyDown(KeyCode.E))
        {
            nearbyObject.Interact();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        IInteractable interactable = other.GetComponent<IInteractable>();
        if (interactable != null)
        {
            nearbyObject = interactable;
            Debug.Log("Player near " + other.name);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        IInteractable interactable = other.GetComponent<IInteractable>();
        if (interactable == nearbyObject)
        {
            nearbyObject = null;
            Debug.Log("Player left " + other.name);
        }
    }
}
