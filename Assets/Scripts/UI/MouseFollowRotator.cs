using UnityEngine;
using UnityEngine.InputSystem;

public class MouseFollowRotator : MonoBehaviour
{
    [Header("Rotation Settings")]
    [Tooltip("Sensitivity of the mouse movement")]
    public float sensitivity = 2f;
    [Tooltip("Maximum rotation angle on X axis (up/down)")]
    public float maxVerticalAngle = 30f;
    [Tooltip("Maximum rotation angle on Y axis (left/right)")]
    public float maxHorizontalAngle = 30f;
    [Tooltip("Speed of rotation interpolation")]
    public float rotationSpeed = 5f;
    [Tooltip("Should the rotation be inverted on Y axis?")]
    public bool invertY = false;
    [Tooltip("Should the rotation be inverted on X axis?")]
    public bool invertX = false;
    [Header("Optional Settings")]
    [Tooltip("Use screen center as reference point instead of object's initial position")]
    public bool useScreenCenter = true;
    [Tooltip("Only rotate when mouse is over this UI element (leave null for always active)")]
    public RectTransform activeArea;
    private Vector3 initialRotation;
    private Vector3 targetRotation;
    private Vector2 mousePosition;
    private Vector2 screenCenter;
    private Camera uiCamera;
    void Start()
    {
        initialRotation = transform.eulerAngles;
        targetRotation = initialRotation;
        screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        uiCamera = Camera.main;
        if (uiCamera == null)
        {
            uiCamera = FindFirstObjectByType<Camera>();
        }
    }
    void Update()
    {
        if (Mouse.current == null)
            return;
        if (!ShouldRotate())
        {
            targetRotation = initialRotation;
        }
        else
        {
            mousePosition = Mouse.current.position.ReadValue();
            CalculateTargetRotation();
        }
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(targetRotation), Time.unscaledDeltaTime * rotationSpeed);
    }
    private bool ShouldRotate()
    {
        if (Mouse.current == null)
            return false;
        if (activeArea == null)
            return true;
        Vector2 localMousePosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            activeArea,
            Mouse.current.position.ReadValue(),
            uiCamera,
            out localMousePosition
        );
        return activeArea.rect.Contains(localMousePosition);
    }
    private void CalculateTargetRotation()
    {
        Vector2 referencePoint = useScreenCenter ? screenCenter : new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        Vector2 mouseOffset = mousePosition - referencePoint;
        float normalizedX = (mouseOffset.x / (Screen.width * 0.5f)) * sensitivity;
        float normalizedY = (mouseOffset.y / (Screen.height * 0.5f)) * sensitivity;
        if (invertX) normalizedX = -normalizedX;
        if (invertY) normalizedY = -normalizedY;
        float targetY = Mathf.Clamp(normalizedX * maxHorizontalAngle, -maxHorizontalAngle, maxHorizontalAngle);
        float targetX = Mathf.Clamp(-normalizedY * maxVerticalAngle, -maxVerticalAngle, maxVerticalAngle);
        targetRotation = initialRotation + new Vector3(targetX, targetY, 0f);
    }
    public void ResetRotation()
    {
        targetRotation = initialRotation;
        transform.rotation = Quaternion.Euler(initialRotation);
    }
    public void SetSensitivity(float newSensitivity)
    {
        sensitivity = Mathf.Max(0f, newSensitivity);
    }
    public void SetEnabled(bool enabled)
    {
        this.enabled = enabled;
        if (!enabled)
        {
            ResetRotation();
        }
    }
    void OnDrawGizmosSelected()
    {
        if (activeArea != null)
        {
            Gizmos.color = Color.yellow;
            Vector3[] corners = new Vector3[4];
            activeArea.GetWorldCorners(corners);
            for (int i = 0; i < 4; i++)
            {
                Gizmos.DrawLine(corners[i], corners[(i + 1) % 4]);
            }
        }
    }
}
