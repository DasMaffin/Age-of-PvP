using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

public class CameraController : MonoBehaviour
{
    public float moveSpeed = 5f; // Adjust the camera movement speed
    public float edgeMargin = 30f; // Distance from the screen edges to start moving the camera
    public float minX = -28f; // Minimum X position
    public float maxX = 28f; // Maximum X position

    private Vector2 moveInput;

    private void Start()
    {
        edgeMargin = Screen.width * edgeMargin / 100; // Convert percentage to pixels
    }

    // Called when the input action for camera movement is triggered
    public void OnMove(CallbackContext value)
    {
        moveInput = value.ReadValue<Vector2>();
    }

    void Update()
    {
        // Move using A and D keys (or custom controls)
        if(moveInput.x < 0)
        {
            transform.Translate(Vector3.left * moveSpeed * Time.deltaTime);
        }
        else if(moveInput.x > 0)
        {
            transform.Translate(Vector3.right * moveSpeed * Time.deltaTime);
        }

        // Move using mouse near the screen edges
        float screenWidth = Screen.width;
        float mouseX = Mouse.current.position.ReadValue().x;

        if(mouseX < edgeMargin) // Mouse is near the left edge
        {
            transform.Translate(Vector3.left * moveSpeed * Time.deltaTime);
        }
        else if(mouseX > screenWidth - edgeMargin) // Mouse is near the right edge
        {
            transform.Translate(Vector3.right * moveSpeed * Time.deltaTime);
        }

        // Clamp the camera's X position
        float clampedX = Mathf.Clamp(transform.position.x, minX, maxX);
        transform.position = new Vector3(clampedX, transform.position.y, transform.position.z);
    }
}
