using UnityEngine;
using UnityEngine.EventSystems;

public class BoardArea : MonoBehaviour
{
    public static BoardArea instance;
    private RectTransform rectTransform;

    void Awake()
    {
        instance = this;
        rectTransform = GetComponent<RectTransform>();
    }

    public bool IsPointInside(Vector2 screenPoint, Camera uiCamera)
    {
        return RectTransformUtility.RectangleContainsScreenPoint(rectTransform, screenPoint, uiCamera);
    }
}
