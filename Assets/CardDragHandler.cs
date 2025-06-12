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
    private int lastHintRow = -1, lastHintCol = -1;
    private Color? originalHintColor = null;
    private CardMaster tmpCardMaster; // only for tmp use

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        canvas = GetComponentInParent<Canvas>();
        cardMaster = GetComponent<CardMasterHolder>()?.cardMaster; // Assumes CardMasterHolder holds a CardMaster reference
    }

    private Gun GetCurrentPlayerGun()
    {
        // Assumes PlayerController.instance.guns[gunNum] is the current gun
        var player = PlayerController.instance;
        if (player != null && player.guns != null && player.guns.Length > 0)
        {
            // gunNum is private, so use reflection or expose a public getter if needed
            var gunNumField = typeof(PlayerController).GetField("gunNum", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            int gunNum = gunNumField != null ? (int)gunNumField.GetValue(player) : 0;
            if (gunNum >= 0 && gunNum < player.guns.Length && player.guns[gunNum] != null)
            {
                return player.guns[gunNum].GetComponent<Gun>();
            }
        }
        return null;
    }

    private void CallOnCardEnableIfOnGrid()
    {
        if (cardMaster != null && lastRow >= 0 && lastCol >= 0)
        {
            cardMaster.OnCardEnable(GetCurrentPlayerGun());
        }
    }

    private void CallOnCardDisableIfOffGrid()
    {
        if (cardMaster != null)
        {
            cardMaster.OnCardDisable(GetCurrentPlayerGun());
        }
    }

    private bool CanPlaceCardAtCell(int row, int col, CardMaster card)
    {
        if (card == null || card.is_free_card) return true;
        var grid = BoardArea.instance.gridState;
        int rows = BoardArea.instance.rows;
        int cols = BoardArea.instance.columns;
        bool hasAdjacent = false;
        // Check left
        if (col > 0 && card.left_link_enabled)
        {
            var leftCard = grid[row, col - 1];
            if (leftCard != null && leftCard.right_link_enabled && leftCard.right_link_type == card.left_link_type)
                return true;
            hasAdjacent = hasAdjacent || (leftCard != null);
        }
        // Check right
        if (col < cols - 1 && card.right_link_enabled)
        {
            var rightCard = grid[row, col + 1];
            if (rightCard != null && rightCard.left_link_enabled && rightCard.left_link_type == card.right_link_type)
                return true;
            hasAdjacent = hasAdjacent || (rightCard != null);
        }
        // Check up
        if (row > 0 && card.up_link_enabled)
        {
            var upCard = grid[row - 1, col];
            if (upCard != null && upCard.down_link_enabled && upCard.down_link_type == card.up_link_type)
                return true;
            hasAdjacent = hasAdjacent || (upCard != null);
        }
        // Check down
        if (row < rows - 1 && card.down_link_enabled)
        {
            var downCard = grid[row + 1, col];
            if (downCard != null && downCard.up_link_enabled && downCard.up_link_type == card.down_link_type)
                return true;
            hasAdjacent = hasAdjacent || (downCard != null);
        }
        // If no adjacent cards, cannot place (unless free card)
        return false;
    }

    private void SetHintColor(int row, int col, Color color)
    {
        if (BoardArea.instance == null) return;
        var hintObjectsField = typeof(BoardArea).GetField("cardHintObjects", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var hintObjects = (GameObject[,])hintObjectsField.GetValue(BoardArea.instance);
        if (hintObjects != null && row >= 0 && row < BoardArea.instance.rows && col >= 0 && col < BoardArea.instance.columns)
        {
            var hint = hintObjects[row, col];
            if (hint != null)
            {
                var img = hint.GetComponent<Image>();
                if (img != null)
                {
                    if (!originalHintColor.HasValue)
                        originalHintColor = img.color;
                    img.color = color;
                }
            }
        }
    }

    private void ResetHintColor(int row, int col)
    {
        if (BoardArea.instance == null || !originalHintColor.HasValue) return;
        var hintObjectsField = typeof(BoardArea).GetField("cardHintObjects", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var hintObjects = (GameObject[,])hintObjectsField.GetValue(BoardArea.instance);
        if (hintObjects != null && row >= 0 && row < BoardArea.instance.rows && col >= 0 && col < BoardArea.instance.columns)
        {
            var hint = hintObjects[row, col];
            if (hint != null)
            {
                var img = hint.GetComponent<Image>();
                if (img != null)
                {
                    img.color = originalHintColor.Value;
                }
            }
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalPosition = rectTransform.position;
        originalAnchoredPosition = rectTransform.anchoredPosition;
        canvasGroup.blocksRaycasts = false;
        // Scale up for feedback
        rectTransform.DOScale(1.1f, 0.2f).SetEase(Ease.OutBack);
        BoardArea.instance.ShowCardHints();

        // Save the card in the cell before clearing, but check upper bounds
        if (lastRow >= 0 && lastCol >= 0 && lastRow < BoardArea.instance.rows && lastCol < BoardArea.instance.columns)
        {
            tmpCardMaster = BoardArea.instance.GetCell(lastRow, lastCol);
            BoardArea.instance.ClearCell(lastRow, lastCol);
        } 
        
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
        // Visual cue for placement
        if (BoardArea.instance != null)
        {
            Vector2 localPoint = BoardArea.instance.ScreenToLocalPoint(Input.mousePosition, canvas.worldCamera);
            Vector2 cardSize = rectTransform.rect.size;
            Vector2Int cell = BoardArea.instance.GetNearestGridCell(localPoint, cardSize);
            // Only update if cell changed
            if (cell.x != lastHintRow || cell.y != lastHintCol)
            {
                if (lastHintRow >= 0 && lastHintCol >= 0)
                    ResetHintColor(lastHintRow, lastHintCol);
                bool canPlace = !BoardArea.instance.IsCellOccupied(cell.x, cell.y) && CanPlaceCardAtCell(cell.x, cell.y, cardMaster);
                SetHintColor(cell.x, cell.y, canPlace ? Color.green : Color.red);
                lastHintRow = cell.x;
                lastHintCol = cell.y;
            }
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        rectTransform.DOScale(1f, 0.2f).SetEase(Ease.OutBack);
        if (shakeTween != null && shakeTween.IsActive()) shakeTween.Kill();
        BoardArea.instance.HideCardHints();

        bool droppedOnHand = HandArea.instance != null && HandArea.instance.IsPointInside(Input.mousePosition, canvas.worldCamera);
        bool droppedOnBoard = BoardArea.instance != null && BoardArea.instance.IsPointInside(Input.mousePosition, canvas.worldCamera);

        if (droppedOnHand)
        {
            // If card was on board, remove from board and add to hand
            if (lastRow >= 0 && lastCol >= 0)
            {
                BoardArea.instance.ClearCell(lastRow, lastCol);
                lastRow = lastCol = -1;
                CallOnCardDisableIfOffGrid();
            }
            // Optionally: HandArea.instance.AddCard(cardMaster);
            // Set parent to hand area, but do not snap to center
            if (HandArea.instance != null)
            {
                var handRect = HandArea.instance.GetComponent<RectTransform>();
                rectTransform.SetParent(handRect, true);
                // Check for overlap with other cards in hand area
                foreach (Transform sibling in handRect)
                {
                    if (sibling == rectTransform) continue;
                    var otherRect = sibling as RectTransform;
                    if (otherRect == null) continue;
                    if (RectTransformOverlaps(rectTransform, otherRect))
                    {
                        // Move this card slightly to the right to separate
                        rectTransform.anchoredPosition += new Vector2(otherRect.rect.width + 10f, 0);
                    }
                }
                // Otherwise, leave the card where it is released
            }
        }
        else if (droppedOnBoard)
        {
            Vector2 localPoint = BoardArea.instance.ScreenToLocalPoint(Input.mousePosition, canvas.worldCamera);
            Vector2 cardSize = rectTransform.rect.size;
            Vector2Int cell = BoardArea.instance.GetNearestGridCell(localPoint, cardSize);
            if (!BoardArea.instance.IsCellOccupied(cell.x, cell.y) && CanPlaceCardAtCell(cell.x, cell.y, cardMaster))
            {
                if (lastRow >= 0 && lastCol >= 0)
                {
                    lastRow = lastCol = -1;
                    CallOnCardDisableIfOffGrid();
                }
                if (HandArea.instance != null && HandArea.instance.ContainsCard(cardMaster))
                {
                    HandArea.instance.RemoveCard(cardMaster);
                }
                Vector2 snappedLocal = BoardArea.instance.GetGridCellPosition(cell.x, cell.y, cardSize);
                rectTransform.SetParent(BoardArea.instance.transform, true);
                rectTransform.DOAnchorPos(snappedLocal, 0.2f).SetEase(Ease.OutQuad);
                BoardArea.instance.SetCell(cell.x, cell.y, cardMaster);
                lastRow = cell.x;
                lastCol = cell.y;
                CallOnCardEnableIfOnGrid();
            }
            else
            {
                rectTransform.DOAnchorPos(originalAnchoredPosition, 0.3f).SetEase(Ease.OutQuad);
                if (tmpCardMaster != null)
                {
                    BoardArea.instance.SetCell(lastRow, lastCol, tmpCardMaster);
                    tmpCardMaster = null;
                }
            }
        }
        else
        {
            rectTransform.DOAnchorPos(originalAnchoredPosition, 0.3f).SetEase(Ease.OutQuad);
        }
        pointerEnterBlockTime = Time.time + 0.5f;
        if (lastHintRow >= 0 && lastHintCol >= 0)
        {
            ResetHintColor(lastHintRow, lastHintCol);
            lastHintRow = lastHintCol = -1;
        }
    }

    // Helper to check overlap between two RectTransforms (in local space of their parent)
    private bool RectTransformOverlaps(RectTransform a, RectTransform b)
    {
        Vector3[] aCorners = new Vector3[4];
        Vector3[] bCorners = new Vector3[4];
        a.GetWorldCorners(aCorners);
        b.GetWorldCorners(bCorners);
        Rect aRect = new Rect(aCorners[0], aCorners[2] - aCorners[0]);
        Rect bRect = new Rect(bCorners[0], bCorners[2] - bCorners[0]);
        return aRect.Overlaps(bRect);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (Time.time < pointerEnterBlockTime) return; // Block shake if within threshold
        // Start shake feedback
        if (shakeTween != null && shakeTween.IsActive()) shakeTween.Kill();
        shakeTween = rectTransform.DOShakePosition(0.3f, strength: new Vector3(2f, 2f, 0), vibrato: 20, randomness: 90, snapping: false, fadeOut: true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Stop shake feedback
        if (shakeTween != null && shakeTween.IsActive()) shakeTween.Kill();
    }
}
