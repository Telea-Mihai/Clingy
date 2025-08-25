using UnityEngine;
using UnityEngine.InputSystem;

public class SimpleMouseLookAt : MonoBehaviour
{
    [Header("Simple Mouse Look Settings")]
    [Range(0.1f, 5f)]
    public float sensitivity = 1f;
    [Range(5f, 45f)]
    public float maxRotationAngle = 20f;
    [Range(1f, 10f)]
    public float smoothSpeed = 3f;
    private Vector3 initialRotation;
    private Vector3 targetRotation;
    void Start()
    {
        initialRotation = transform.eulerAngles;
        targetRotation = initialRotation;
    }
    void Update()
    {
        if (Mouse.current == null)
            return;
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
        Vector2 offset = (mousePos - screenCenter) / screenCenter;
        float rotationY = offset.x * maxRotationAngle * sensitivity;
        float rotationX = -offset.y * maxRotationAngle * sensitivity;
        rotationY = Mathf.Clamp(rotationY, -maxRotationAngle, maxRotationAngle);
        rotationX = Mathf.Clamp(rotationX, -maxRotationAngle, maxRotationAngle);
        targetRotation = initialRotation + new Vector3(rotationX, rotationY, 0f);
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(targetRotation), Time.unscaledDeltaTime * smoothSpeed);
    }
}
