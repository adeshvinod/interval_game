using UnityEngine;
using UnityEngine.EventSystems;

public class pointerclick : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        // This will be called for both mouse clicks and touch
        Debug.Log("UI Element Clicked/Touched!");

        // Check if it's a touch or mouse click
        if (Input.touchCount > 0)
        {
            Debug.Log("Touch detected on mobile device");
            // You can get touch position
            Touch touch = Input.GetTouch(0);
            Debug.Log($"Touch position: {touch.position}");
        }
        else
        {
            // It's a mouse click
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                Debug.Log("Left mouse click");
            }
            else if (eventData.button == PointerEventData.InputButton.Right)
            {
                Debug.Log("Right mouse click");
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // This will be called when the user first touches/clicks
        Debug.Log("Pointer Down - Touch/Click started");
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // This will be called when the user releases the touch/click
        Debug.Log("Pointer Up - Touch/Click ended");
    }
}
