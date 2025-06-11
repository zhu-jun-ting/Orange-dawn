using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

public class CardDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    private Canvas canvas;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Vector3 originalPosition;
    private Vector2 originalAnchoredPosition;
    private Tween shakeTween;
    private float pointerEnterBlockTime = 0f; // Added threshold time
    private int lastRow = -1, lastCol = -1;
    private CardMaster cardMaster;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        canvas = GetComponentInParent<Canvas>();
        cardMaster = GetComponent<CardMasterHolder>()?.cardMaster; // Assumes CardMasterHolder holds a CardMaster reference
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalPosition = rectTransform.position;
        originalAnchoredPosition = rectTransform.anchoredPosition;
        canvasGroup.blocksRaycasts = false;
        // Scale up for feedback
        rectTransform.DOScale(1.1f, 0.2f).SetEase(Ease.OutBack);
        BoardArea.instance.ShowCardHints();
        // Remove from previous cell if present
        if (lastRow >= 0 && lastCol >= 0)
        {
            BoardArea.instance.ClearCell(lastRow, lastCol);
            lastRow = lastCol = -1;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        rectTransform.DOScale(1f, 0.2f).SetEase(Ease.OutBack);
        if (shakeTween != null && shakeTween.IsActive()) shakeTween.Kill();
        BoardArea.instance.HideCardHints();
        if (BoardArea.instance != null && BoardArea.instance.IsPointInside(Input.mousePosition, canvas.worldCamera))
        {
            Vector2 localPoint = BoardArea.instance.ScreenToLocalPoint(Input.mousePosition, canvas.worldCamera);
            Vector2 cardSize = rectTransform.rect.size;
            Vector2Int cell = BoardArea.instance.GetNearestGridCell(localPoint, cardSize);
            // Prevent overlap
            if (!BoardArea.instance.IsCellOccupied(cell.x, cell.y))
            {
                Vector2 snappedLocal = BoardArea.instance.GetGridCellPosition(cell.x, cell.y, cardSize);
                rectTransform.DOAnchorPos(snappedLocal, 0.2f).SetEase(Ease.OutQuad);
                BoardArea.instance.SetCell(cell.x, cell.y, cardMaster);
                lastRow = cell.x;
                lastCol = cell.y;
            }
            else
            {
                // Snap back to original position if cell is occupied
                rectTransform.DOAnchorPos(originalAnchoredPosition, 0.3f).SetEase(Ease.OutQuad);
            }
        }
        else if (BoardArea.instance != null)
        {
            // Animate back to original anchored position
            rectTransform.DOAnchorPos(originalAnchoredPosition, 0.3f).SetEase(Ease.OutQuad);
        }
        pointerEnterBlockTime = Time.time + 0.5f;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (Time.time < pointerEnterBlockTime) return; // Block shake if within threshold
        // Start shake feedback
        if (shakeTween != null && shakeTween.IsActive()) shakeTween.Kill();
        shakeTween = rectTransform.DOShakePosition(0.3f, strength: new Vector3(10f, 10f, 0), vibrato: 20, randomness: 90, snapping: false, fadeOut: true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Stop shake feedback
        if (shakeTween != null && shakeTween.IsActive()) shakeTween.Kill();
    }
}
