using UnityEngine;

public class Button : MonoBehaviour
{
    public SceneControllerMenu controller;

    private void OnMouseDown()
    {
        controller.StartCountdown();
    }
}
