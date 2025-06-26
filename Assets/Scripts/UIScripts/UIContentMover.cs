using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Attach to a UI panel to allow dragging the panel and its contents with the mouse.
/// Only responds if the drag starts on this panel (not on cards or UI above).
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class UIContentMover : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private Vector2 lastPointerPosition;
    private bool isDragging = false;

    [Header("Inertia Settings")]
    [Tooltip("How quickly inertia slows down (0 = never, 1 = instant stop)")]
    [Range(0f,1f)]
    public float inertiaDamping = 0.1f;
    [Tooltip("Minimum velocity (pixels/frame) before stopping inertia")] 
    public float minInertiaVelocity = 1f;
    private Vector2 inertiaVelocity = Vector2.zero;
    private bool isInertiaActive = false;

    [Header("Movement Settings")]
    [Tooltip("How fast the panel moves with the mouse (1 = 1:1, <1 = slower, >1 = faster)")]
    public Vector2 movementSpeed = Vector2.one;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    void Update()
    {
        if (isInertiaActive)
        {
            rectTransform.anchoredPosition += inertiaVelocity;
            inertiaVelocity *= (1f - inertiaDamping);
            if (inertiaVelocity.magnitude < minInertiaVelocity)
            {
                isInertiaActive = false;
                inertiaVelocity = Vector2.zero;
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // Only start drag if this is the topmost raycast target (not a card or child UI above)
        if (eventData.pointerEnter == gameObject)
        {
            lastPointerPosition = eventData.position;
            isDragging = true;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // Only allow drag if started on this panel
        if (isDragging)
        {
            lastPointerPosition = eventData.position;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging) return;
        Vector2 currentPointerPosition = eventData.position;
        Vector2 delta = currentPointerPosition - lastPointerPosition;
        delta.x *= movementSpeed.x;
        delta.y *= movementSpeed.y;
        Vector3 worldDelta;
        var canvas = GetComponentInParent<Canvas>();
        if (canvas != null && RectTransformUtility.ScreenPointToWorldPointInRectangle(rectTransform, currentPointerPosition, eventData.pressEventCamera, out var worldPointCurrent)
            && RectTransformUtility.ScreenPointToWorldPointInRectangle(rectTransform, lastPointerPosition, eventData.pressEventCamera, out var worldPointLast))
        {
            worldDelta = worldPointCurrent - worldPointLast;
            worldDelta.x *= movementSpeed.x;
            worldDelta.y *= movementSpeed.y;
            rectTransform.position += worldDelta;
        }
        else
        {
            rectTransform.anchoredPosition += delta;
        }
        inertiaVelocity = delta; // Store for inertia
        lastPointerPosition = currentPointerPosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
        isInertiaActive = inertiaVelocity.magnitude > minInertiaVelocity;
    }
}
