using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Foundational player controller. Handles WASD/Arrow key movement clamped to the
/// main camera's orthographic bounds (accounting for the CircleCollider2D radius),
/// and smoothly rotates the player toward its movement direction.
/// </summary>
[RequireComponent(typeof(CircleCollider2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 10f;

    // Cached component references
    private Camera mainCamera;
    private CircleCollider2D circleCollider;

    // Input actions
    private InputAction moveAction;

    private void Awake()
    {
        mainCamera = Camera.main;
        circleCollider = GetComponent<CircleCollider2D>();
        SetupInputActions();
    }

    private void OnEnable()
    {
        moveAction.Enable();
    }

    private void OnDisable()
    {
        moveAction.Disable();
    }

    private void OnDestroy()
    {
        moveAction.Dispose();
    }

    private void Update()
    {
        HandleMovement();
    }

    /// <summary>
    /// Reads movement input, moves the player, clamps to camera bounds, and rotates toward movement direction.
    /// </summary>
    private void HandleMovement()
    {
        Vector2 inputVector = moveAction.ReadValue<Vector2>();

        if (inputVector.sqrMagnitude > 0.01f)
        {
            // Translate position
            Vector3 movement = new Vector3(inputVector.x, inputVector.y, 0f) * moveSpeed * Time.deltaTime;
            transform.position += movement;

            // Clamp position within camera bounds, keeping the collider fully inside
            transform.position = ClampToCameraBounds(transform.position);

            // Rotate smoothly toward movement direction
            float targetAngle = Mathf.Atan2(inputVector.y, inputVector.x) * Mathf.Rad2Deg - 90f;
            Quaternion targetRotation = Quaternion.Euler(0f, 0f, targetAngle);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    /// <summary>
    /// Clamps the given world position within the main camera's orthographic viewport bounds,
    /// inset by the CircleCollider2D's world-space radius so the collider never exits the screen.
    /// </summary>
    private Vector3 ClampToCameraBounds(Vector3 position)
    {
        float verticalExtent = mainCamera.orthographicSize;
        float horizontalExtent = verticalExtent * mainCamera.aspect;

        // Scale the collider radius by the largest uniform scale axis to get the world-space radius
        Vector3 lossyScale = transform.lossyScale;
        float worldRadius = circleCollider.radius * Mathf.Max(Mathf.Abs(lossyScale.x), Mathf.Abs(lossyScale.y));

        Vector3 cameraPos = mainCamera.transform.position;

        position.x = Mathf.Clamp(position.x, cameraPos.x - horizontalExtent + worldRadius, cameraPos.x + horizontalExtent - worldRadius);
        position.y = Mathf.Clamp(position.y, cameraPos.y - verticalExtent + worldRadius, cameraPos.y + verticalExtent - worldRadius);

        return position;
    }

    /// <summary>
    /// Configures the movement InputAction using WASD and Arrow key composites.
    /// </summary>
    private void SetupInputActions()
    {
        moveAction = new InputAction("Move", InputActionType.Value, expectedControlType: "Vector2");

        // WASD composite
        moveAction.AddCompositeBinding("2DVector")
            .With("Up",    "<Keyboard>/w")
            .With("Down",  "<Keyboard>/s")
            .With("Left",  "<Keyboard>/a")
            .With("Right", "<Keyboard>/d");

        // Arrow Keys composite
        moveAction.AddCompositeBinding("2DVector")
            .With("Up",    "<Keyboard>/upArrow")
            .With("Down",  "<Keyboard>/downArrow")
            .With("Left",  "<Keyboard>/leftArrow")
            .With("Right", "<Keyboard>/rightArrow");
    }
}
