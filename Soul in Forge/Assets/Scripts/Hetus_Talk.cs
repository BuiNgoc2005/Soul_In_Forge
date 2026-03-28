using UnityEngine;

public class Hetus_Talk : MonoBehaviour
{
    public Animator interactAnim;

    private void OnEnable()
    {
        if (interactAnim != null)
            interactAnim.Play("Open");
    }

    private void OnDisable()
    {
        if (interactAnim != null)
            interactAnim.Play("Close");
    }
}
