using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;
using System;
using System.Collections.Generic;

public class CardDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Drag Positioning")]
    private Canvas canvas;
    private RectTransform rectTransform;
    private RectTransform rectTransformZoomableOffset; // RectTransform for link visuals, if different from card
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
    private Color colorCanPlace => GameSettings.instance ? GameSettings.instance.colorCanPlace : Color.green;
    private Color colorCannotPlace => GameSettings.instance ? GameSettings.instance.colorCannotPlace : Color.red;
    private Color colorLinkInactive => GameSettings.instance ? GameSettings.instance.colorLinkInactive : new Color(0, 0, 0, 0.5f);
    private Color colorLinkActive => GameSettings.instance ? GameSettings.instance.colorLinkActive : new Color(0, 1, 0, 1f);

    [Header("Link GameObjects")]
    [Tooltip("GameObject to show the up link connection in the UI")]
    public GameObject up_link_gameobject;
    public GameObject left_link_gameobject;
    public GameObject right_link_gameobject;
    public GameObject down_link_gameobject;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        canvas = GetComponentInParent<Canvas>();
        cardMaster = GetComponent<CardMaster>(); // Assumes CardMasterHolder holds a CardMaster reference
        // Find the first UIContentMover in parents and get its RectTransform
        var mover = GetComponentInParent<UIContentMover>();
        if (mover != null)
            rectTransformZoomableOffset = mover.GetComponent<RectTransform>();
        else
            rectTransformZoomableOffset = null;
    }

    void Start()
    {
        if (cardMaster != null)
        {
            if (cardMaster.up_link_enabled)
                cardMaster.SetLinkHalfTransparentBlack("up");
            else
                cardMaster.SetLinkAlpha(up_link_gameobject, 0f);
            if (cardMaster.down_link_enabled)
                cardMaster.SetLinkHalfTransparentBlack("down");
            else
                cardMaster.SetLinkAlpha(down_link_gameobject, 0f);
            if (cardMaster.left_link_enabled)
                cardMaster.SetLinkHalfTransparentBlack("left");
            else
                cardMaster.SetLinkAlpha(left_link_gameobject, 0f);
            if (cardMaster.right_link_enabled)
                cardMaster.SetLinkHalfTransparentBlack("right");
            else
                cardMaster.SetLinkAlpha(right_link_gameobject, 0f);
        }
    }

    private Gun GetCurrentPlayerGun()
    {
        // Assumes PlayerController.instance.guns[gunNum] is the current gun
        var player = PlayerController.instance;
        if (player != null && player.guns != null && player.guns.Count > 0)
        {
            // gunNum is private, so use reflection or expose a public getter if needed
            var gunNumField = typeof(PlayerController).GetField("gunNum", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            int gunNum = gunNumField != null ? (int)gunNumField.GetValue(player) : 0;
            if (gunNum >= 0 && gunNum < player.guns.Count && player.guns[gunNum] != null)
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
            cardMaster.OnCardEnable();
        }
    }

    private void CallOnCardDisableIfOffGrid()
    {
        if (cardMaster != null)
        {
            // If this card is a root and is being removed from the board, remove from roots list
            if (cardMaster.is_root && BoardArea.instance != null && BoardArea.instance.roots != null)
            {
                BoardArea.instance.roots.Remove(cardMaster);
            }
            cardMaster.OnCardDisable();
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
            if (leftCard != null && leftCard.right_link_enabled && CardMaster.LinkTypesEqual(leftCard.right_link_type, cardMaster.left_link_type))
                return true;
            hasAdjacent = hasAdjacent || (leftCard != null);
        }
        // Check right
        if (col < cols - 1 && card.right_link_enabled)
        {
            var rightCard = grid[row, col + 1];
            if (rightCard != null && rightCard.left_link_enabled && CardMaster.LinkTypesEqual(rightCard.left_link_type, card.right_link_type))
                return true;
            hasAdjacent = hasAdjacent || (rightCard != null);
        }
        // Check up
        if (row > 0 && card.up_link_enabled)
        {
            var upCard = grid[row - 1, col];
            if (upCard != null && upCard.down_link_enabled && CardMaster.LinkTypesEqual(upCard.down_link_type, card.up_link_type))
                return true;
            hasAdjacent = hasAdjacent || (upCard != null);
        }
        // Check down
        if (row < rows - 1 && card.down_link_enabled)
        {
            var downCard = grid[row + 1, col];
            if (downCard != null && downCard.up_link_enabled && CardMaster.LinkTypesEqual(downCard.up_link_type, card.down_link_type))
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

    // Helper to preview which links will be active if placed at (row, col)
    private void PreviewLinkVisuals(int row, int col)
    {
        if (cardMaster == null || BoardArea.instance == null) return;
        var grid = BoardArea.instance.gridState;
        int rows = BoardArea.instance.rows;
        int cols = BoardArea.instance.columns;
        bool up = false, down = false, left = false, right = false;
        // Up
        if (row > 0 && cardMaster.up_link_enabled)
        {
            var upCard = grid[row - 1, col];
            up = upCard != null && upCard.down_link_enabled && CardMaster.LinkTypesEqual(upCard.down_link_type, cardMaster.up_link_type);
            if (up && upCard != null)
                upCard.GetComponent<CardMaster>()?.SetActiveLinksGreenAndVisible(false, false, false, true); // upCard's down
        }
        // Down
        if (row < rows - 1 && cardMaster.down_link_enabled)
        {
            var downCard = grid[row + 1, col];
            down = downCard != null && downCard.up_link_enabled && CardMaster.LinkTypesEqual(downCard.up_link_type, cardMaster.down_link_type);
            if (down && downCard != null)
                downCard.GetComponent<CardMaster>()?.SetActiveLinksGreenAndVisible(true, false, false, false); // downCard's up
        }
        // Left
        if (col > 0 && cardMaster.left_link_enabled)
        {
            var leftCard = grid[row, col - 1];
            left = leftCard != null && leftCard.right_link_enabled && CardMaster.LinkTypesEqual(leftCard.right_link_type, cardMaster.left_link_type);
            if (left && leftCard != null)
                leftCard.GetComponent<CardMaster>()?.SetActiveLinksGreenAndVisible(false, false, true, false); // leftCard's right
        }
        // Right
        if (col < cols - 1 && cardMaster.right_link_enabled)
        {
            var rightCard = grid[row, col + 1];
            right = rightCard != null && rightCard.left_link_enabled && CardMaster.LinkTypesEqual(rightCard.left_link_type, cardMaster.right_link_type);
            if (right && rightCard != null)
                rightCard.GetComponent<CardMaster>()?.SetActiveLinksGreenAndVisible(false, true, false, false); // rightCard's left
        }
        cardMaster.SetActiveLinksGreenAndVisible(up, left, right, down);
    }

    private void SetPlacedLinksGreenAndVisible(int row, int col)
    {
        if (cardMaster == null || BoardArea.instance == null) return;
        var grid = BoardArea.instance.gridState;
        int rows = BoardArea.instance.rows;
        int cols = BoardArea.instance.columns;
        bool up = false, down = false, left = false, right = false;
        // Up
        if (row > 0 && cardMaster.up_link_enabled)
        {
            var upCard = grid[row - 1, col];
            up = upCard != null && upCard.down_link_enabled && CardMaster.LinkTypesEqual(upCard.down_link_type, cardMaster.up_link_type);
            if (up && upCard != null)
                upCard.GetComponent<CardMaster>()?.SetPlacedLinksGreenAndVisible(false, false, false, true); // upCard's down
            else if (upCard != null)
                upCard.GetComponent<CardMaster>()?.SetAllLinksInvisible();
        }
        // Down
        if (row < rows - 1 && cardMaster.down_link_enabled)
        {
            var downCard = grid[row + 1, col];
            down = downCard != null && downCard.up_link_enabled && CardMaster.LinkTypesEqual(downCard.up_link_type, cardMaster.down_link_type);
            if (down && downCard != null)
                downCard.GetComponent<CardMaster>()?.SetPlacedLinksGreenAndVisible(true, false, false, false); // downCard's up
            else if (downCard != null)
                downCard.GetComponent<CardMaster>()?.SetAllLinksInvisible();
        }
        // Left
        if (col > 0 && cardMaster.left_link_enabled)
        {
            var leftCard = grid[row, col - 1];
            left = leftCard != null && leftCard.right_link_enabled && CardMaster.LinkTypesEqual(leftCard.right_link_type, cardMaster.left_link_type);
            if (left && leftCard != null)
                leftCard.GetComponent<CardMaster>()?.SetPlacedLinksGreenAndVisible(false, false, true, false); // leftCard's right
            else if (leftCard != null)
                leftCard.GetComponent<CardMaster>()?.SetAllLinksInvisible();
        }
        // Right
        if (col < cols - 1 && cardMaster.right_link_enabled)
        {
            var rightCard = grid[row, col + 1];
            right = rightCard != null && rightCard.left_link_enabled && CardMaster.LinkTypesEqual(rightCard.left_link_type, cardMaster.right_link_type);
            if (right && rightCard != null)
                rightCard.GetComponent<CardMaster>()?.SetPlacedLinksGreenAndVisible(false, true, false, false); // rightCard's left
            else if (rightCard != null)
                rightCard.GetComponent<CardMaster>()?.SetAllLinksInvisible();
        }
        cardMaster.SetPlacedLinksGreenAndVisible(up, left, right, down);
    }

    // Utility: Set all links on all cards (board and hand) to black 50% transparent
    private void ResetAllCardLinksHalfTransparentBlack()
    {
        // Board
        var board = BoardArea.instance;
        if (board != null && board.gridState != null)
        {
            int rows = board.rows;
            int cols = board.columns;
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    var card = board.gridState[r, c];
                    if (card != null)
                    {
                        card.SetLinkHalfTransparentBlack("up");
                        card.SetLinkHalfTransparentBlack("down");
                        card.SetLinkHalfTransparentBlack("left");
                        card.SetLinkHalfTransparentBlack("right");
                    }
                }
            }
        }
        // Hand
        var hand = HandArea.instance;
        if (hand != null && hand.handCards != null)
        {
            foreach (var card in hand.handCards)
            {
                if (card != null)
                {
                    card.SetLinkHalfTransparentBlack("up");
                    card.SetLinkHalfTransparentBlack("down");
                    card.SetLinkHalfTransparentBlack("left");
                    card.SetLinkHalfTransparentBlack("right");
                }
            }
        }
    }

    // Utility: For any card, set links with no connected CardMaster to fully transparent
    private void SetDisconnectedLinksInvisible(CardMaster card)
    {
        if (card == null) return;
        var handler = card.GetComponent<CardDragHandler>();
        if (card.up_link_cardmaster == null) card.SetLinkAlpha(handler?.up_link_gameobject, 0f);
        if (card.down_link_cardmaster == null) card.SetLinkAlpha(handler?.down_link_gameobject, 0f);
        if (card.left_link_cardmaster == null) card.SetLinkAlpha(handler?.left_link_gameobject, 0f);
        if (card.right_link_cardmaster == null) card.SetLinkAlpha(handler?.right_link_gameobject, 0f);
    }

    // Utility: For any card, set links green if there is an active connected CardMaster reference
    private void SetActiveBoardLinksGreen()
    {
        var board = BoardArea.instance;
        if (board == null || board.gridState == null) return;
        int rows = board.rows;
        int cols = board.columns;
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                var card = board.gridState[r, c];
                if (card == null) continue;

                // if link has reference then set that to green, otherwise set to black 50% transparent
                bool up = card.up_link_cardmaster != null;
                bool down = card.down_link_cardmaster != null;
                bool left = card.left_link_cardmaster != null;
                bool right = card.right_link_cardmaster != null;
                card.SetPlacedLinksColorAndAlpha(up, left, right, down, colorLinkActive, colorLinkActive.a);
            
                // otherwise set to black 50% transparent
                up = card.up_link_enabled && card.up_link_cardmaster == null;
                down = card.down_link_enabled && card.down_link_cardmaster == null;
                left = card.left_link_enabled && card.left_link_cardmaster == null;
                right = card.right_link_enabled && card.right_link_cardmaster == null;
                card.SetPlacedLinksColorAndAlpha(up, left, right, down, colorLinkInactive, colorLinkInactive.a);
            }
        }
    }

    // --- Fields for drag UI layering ---
    private Transform _originalParent;
    private int _originalSiblingIndex;
    private Canvas rootCanvas;

    public void OnBeginDrag(PointerEventData eventData)
    {
        if ((!cardMaster || cardMaster.card_conditions.Contains(CardMaster.CardCondition.IsUndraggable)) && (cardMaster.gridLocation.x != -1 || cardMaster.gridLocation.y != -1))
        {
            if (GameEvents.instance != null)
                GameEvents.instance.ShowMessage("This card cannot be dragged off.", GameEvents.MessageType.FullWarning);
            return; // If card is not draggable, do nothing
        }
        originalPosition = rectTransform.position;
        originalAnchoredPosition = rectTransform.anchoredPosition;
        canvasGroup.blocksRaycasts = false;
        // Scale up for feedback
        rectTransform.DOScale(1.1f, 0.2f).SetEase(Ease.OutBack);
        BoardArea.instance.ShowCardHints();

        // --- Bring this card to the top of the root canvas (absolute top-most UI) ---
        if (rootCanvas == null)
            rootCanvas = canvas != null && canvas.rootCanvas != null ? canvas.rootCanvas : GetComponentInParent<Canvas>().rootCanvas;
        _originalParent = rectTransform.parent;
        _originalSiblingIndex = rectTransform.GetSiblingIndex();
        if (rootCanvas != null)
        {
            rectTransform.SetParent(rootCanvas.transform, true);
            rectTransform.SetAsLastSibling();
        }

        // --- Reset all links to black 50% transparent on this card, but only if enabled AND type is not Common; otherwise set invisible ---
        if (cardMaster != null)
        {
            if (cardMaster.up_link_enabled)
                cardMaster.SetLinkHalfTransparentBlack("up");
            else
                cardMaster.SetLinkAlpha(up_link_gameobject, 0f);
            if (cardMaster.down_link_enabled)
                cardMaster.SetLinkHalfTransparentBlack("down");
            else
                cardMaster.SetLinkAlpha(down_link_gameobject, 0f);
            if (cardMaster.left_link_enabled)
                cardMaster.SetLinkHalfTransparentBlack("left");
            else
                cardMaster.SetLinkAlpha(left_link_gameobject, 0f);
            if (cardMaster.right_link_enabled)
                cardMaster.SetLinkHalfTransparentBlack("right");
            else
                cardMaster.SetLinkAlpha(right_link_gameobject, 0f);
        }

        // --- Reset all previously linked neighbor cards' corresponding links ---
        if (lastRow >= 0 && lastCol >= 0 && lastRow < BoardArea.instance.rows && lastCol < BoardArea.instance.columns)
        {
            var grid = BoardArea.instance.gridState;
            int rows = BoardArea.instance.rows;
            int cols = BoardArea.instance.columns;
            int row = lastRow, col = lastCol;
            // Up neighbor
            if (row > 0)
            {
                var upCard = grid[row - 1, col];
                if (upCard != null && upCard.down_link_cardmaster == cardMaster)
                {
                    if (upCard.down_link_enabled && upCard.down_link_type != CardMaster.LinkType.Common)
                        upCard.SetLinkHalfTransparentBlack("down");
                    else
                        upCard.SetLinkAlpha(upCard.GetComponent<CardDragHandler>()?.down_link_gameobject, 0f);
                }
            }
            // Down neighbor
            if (row < rows - 1)
            {
                var downCard = grid[row + 1, col];
                if (downCard != null && downCard.up_link_cardmaster == cardMaster)
                {
                    if (downCard.up_link_enabled && downCard.up_link_type != CardMaster.LinkType.Common)
                        downCard.SetLinkHalfTransparentBlack("up");
                    else
                        downCard.SetLinkAlpha(downCard.GetComponent<CardDragHandler>()?.up_link_gameobject, 0f);
                }
            }
            // Left neighbor
            if (col > 0)
            {
                var leftCard = grid[row, col - 1];
                if (leftCard != null && leftCard.right_link_cardmaster == cardMaster)
                {
                    if (leftCard.right_link_enabled && leftCard.right_link_type != CardMaster.LinkType.Common)
                        leftCard.SetLinkHalfTransparentBlack("right");
                    else
                        leftCard.SetLinkAlpha(leftCard.GetComponent<CardDragHandler>()?.right_link_gameobject, 0f);
                }
            }
            // Right neighbor
            if (col < cols - 1)
            {
                var rightCard = grid[row, col + 1];
                if (rightCard != null && rightCard.left_link_cardmaster == cardMaster)
                {
                    if (rightCard.left_link_enabled && rightCard.left_link_type != CardMaster.LinkType.Common)
                        rightCard.SetLinkHalfTransparentBlack("left");
                    else
                        rightCard.SetLinkAlpha(rightCard.GetComponent<CardDragHandler>()?.left_link_gameobject, 0f);
                }
            }
        }

        // Save the card in the cell before clearing, but check upper bounds
        if (lastRow >= 0 && lastCol >= 0 && lastRow < BoardArea.instance.rows && lastCol < BoardArea.instance.columns)
        {
            tmpCardMaster = BoardArea.instance.GetCell(lastRow, lastCol);
            BoardArea.instance.ClearCell(lastRow, lastCol);
        }
    }
        
    private bool isDragging;
    public bool IsDragging
    {
        get { return isDragging; }
        set { isDragging = value; }
    }
    public void OnDrag(PointerEventData eventData)
    {
        if ((!cardMaster || cardMaster.card_conditions.Contains(CardMaster.CardCondition.IsUndraggable)) && (cardMaster.gridLocation.x != -1 || cardMaster.gridLocation.y != -1)) return;
        isDragging = true;
        if (!canvas) canvas = GetComponentInParent<Canvas>();
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
        // 1. Reset all links on all cards to black 50% transparent
        // ResetAllCardLinksHalfTransparentBlack();
        // 1a. Also reset this card's links to black 50% transparent only if enabled AND type is not Common; otherwise set invisible before hint
        if (cardMaster != null)
        {
            if (cardMaster.up_link_enabled)
                cardMaster.SetLinkHalfTransparentBlack("up");
            else
                cardMaster.SetLinkAlpha(up_link_gameobject, 0f);
            if (cardMaster.down_link_enabled)
                cardMaster.SetLinkHalfTransparentBlack("down");
            else
                cardMaster.SetLinkAlpha(down_link_gameobject, 0f);
            if (cardMaster.left_link_enabled)
                cardMaster.SetLinkHalfTransparentBlack("left");
            else
                cardMaster.SetLinkAlpha(left_link_gameobject, 0f);
            if (cardMaster.right_link_enabled)
                cardMaster.SetLinkHalfTransparentBlack("right");
            else
                cardMaster.SetLinkAlpha(right_link_gameobject, 0f);
        }
        // 2. For all cards on the board, set links with no connected CardMaster to transparent
        var board = BoardArea.instance;
        if (board != null && board.gridState != null)
        {
            int rows = board.rows;
            int cols = board.columns;
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    var card = board.gridState[r, c];
                    if (card != null)
                        SetDisconnectedLinksInvisible(card);
                }
            }
        }
        // 3. Only set green links for valid connections (board) or drag preview (hint)
        if (BoardArea.instance != null)
        {
            Vector2 localPoint = BoardArea.instance.ScreenToLocalPoint(Input.mousePosition, canvas.worldCamera);
            Vector2 cardSize = rectTransform.rect.size;
            Vector2Int cell = BoardArea.instance.GetNearestGridCell(localPoint, cardSize);
            bool canPlace = true;
            // Only update if cell changed
            if (cell.x != lastHintRow || cell.y != lastHintCol)
            {
                if (lastHintRow >= 0 && lastHintCol >= 0)
                    ResetHintColor(lastHintRow, lastHintCol);

                canPlace = !BoardArea.instance.IsCellOccupied(cell.x, cell.y) && CanPlaceCardAtCell(cell.x, cell.y, cardMaster);
                SetHintColor(cell.x, cell.y, canPlace ? colorCanPlace : colorCannotPlace);
                if (canPlace) cardMaster.UpdateUIStars(new Vector2Int(cell.x, cell.y)); // Update stars based on placement
                else cardMaster.ResetUIStars(); // Reset stars if cannot place

                lastHintRow = cell.x;
                lastHintCol = cell.y;
            }
            if (canPlace) PreviewLinkVisuals(cell.x, cell.y); // This will set green links for the drag preview

        }
        // After all, update all board cards' links to green if actively connected
        SetActiveBoardLinksGreen();
    }

    // --- LEGACY: droppedOnHand and droppedOnBoard logic for compatibility ---
    private bool droppedOnBoard = false;
    private bool droppedOnHand = false;

    public void OnEndDrag(PointerEventData eventData)
    {
        if ((!cardMaster || cardMaster.card_conditions.Contains(CardMaster.CardCondition.IsUndraggable)) && (cardMaster.gridLocation.x != -1 || cardMaster.gridLocation.y != -1)) return;
        isDragging = false;
        canvasGroup.blocksRaycasts = true;
        rectTransform.DOScale(1f, 0.2f).SetEase(Ease.OutBack);
        if (shakeTween != null && shakeTween.IsActive()) shakeTween.Kill();
        BoardArea.instance.HideCardHints();
        BoardArea.instance.lastDraggedCard = cardMaster;

        // --- Restore original parent and sibling index if changed during drag ---
        if (_originalParent != null && rectTransform.parent != _originalParent)
        {
            rectTransform.SetParent(_originalParent, true);
        }
        if (_originalParent != null && rectTransform.GetSiblingIndex() != _originalSiblingIndex)
        {
            rectTransform.SetSiblingIndex(Mathf.Min(_originalSiblingIndex, rectTransform.parent.childCount - 1));
        }

        // --- Unified hand/board drop logic ---
        Camera uiCamera = null;
        if (BoardArea.instance != null && BoardArea.instance.gameObject.GetComponent<Canvas>() != null)
            uiCamera = BoardArea.instance.gameObject.GetComponent<Canvas>().worldCamera;
        Vector2 screenPoint = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        Vector2 localPoint = Vector2.zero;
        Vector2Int nearestCell = new Vector2Int(-1, -1);
        Vector2 gridCellPos = Vector2.zero;
        float dist = float.MaxValue;
        droppedOnBoard = false;
        droppedOnHand = false;
        if (BoardArea.instance != null && BoardArea.instance.gameObject.TryGetComponent<RectTransform>(out var boardRect))
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(boardRect, screenPoint, uiCamera, out localPoint);
            Vector2 cardSize = rectTransform != null ? rectTransform.sizeDelta : new Vector2(100, 140);
            nearestCell = BoardArea.instance.GetNearestGridCell(localPoint, cardSize);
            gridCellPos = BoardArea.instance.GetGridCellPosition(nearestCell.x, nearestCell.y, cardSize);

            var mover = GetComponentInParent<UIContentMover>();
            if (mover != null) rectTransformZoomableOffset = mover.GetComponent<RectTransform>();
            dist = Vector2.Distance(localPoint - rectTransformZoomableOffset.anchoredPosition, gridCellPos);
            // Debug.Log($"CardDragHandler: OnEndDrag - local point: {localPoint}, grid cell pos: {gridCellPos}");
        }
        if (dist < 50f)
        {
            droppedOnBoard = true;
            droppedOnHand = false;
        }
        else
        {
            droppedOnHand = true;
            droppedOnBoard = false;
        }

        if (droppedOnHand)
        {
            if (lastRow >= 0 && lastCol >= 0)
            {
                // Destroy links in all directions
                var grid = BoardArea.instance.gridState;
                int rows = BoardArea.instance.rows;
                int cols = BoardArea.instance.columns;
                int row = lastRow, col = lastCol;
                // Up
                if (row > 0)
                {
                    var upCard = grid[row - 1, col];
                    if (upCard != null && upCard.down_link_cardmaster == cardMaster)
                    {
                        upCard.down_link_cardmaster = null;
                        cardMaster.up_link_cardmaster = null;
                    }
                }
                // Down
                if (row < rows - 1)
                {
                    var downCard = grid[row + 1, col];
                    if (downCard != null && downCard.up_link_cardmaster == cardMaster)
                    {
                        downCard.up_link_cardmaster = null;
                        cardMaster.down_link_cardmaster = null;
                    }
                }
                // Left
                if (col > 0)
                {
                    var leftCard = grid[row, col - 1];
                    if (leftCard != null && leftCard.right_link_cardmaster == cardMaster)
                    {
                        leftCard.right_link_cardmaster = null;
                        cardMaster.left_link_cardmaster = null;
                    }
                }
                // Right
                if (col < cols - 1)
                {
                    var rightCard = grid[row, col + 1];
                    if (rightCard != null && rightCard.left_link_cardmaster == cardMaster)
                    {
                        rightCard.left_link_cardmaster = null;
                        cardMaster.right_link_cardmaster = null;
                    }
                }
                BoardArea.instance.ClearCell(lastRow, lastCol);
                lastRow = lastCol = -1;
                // Do not call CallOnCardDisableIfOffGrid here
            }
            // Optionally: HandArea.instance.AddCard(cardMaster);
            // Set parent to hand area, but do not snap to center
            if (HandArea.instance != null)
            {
                bool overlaps = false;
                var handRect = HandArea.instance.rectTransform;
                // Check for overlap with other cards in hand area
                foreach (Transform sibling in handRect)
                {
                    if (sibling == rectTransform) continue;
                    var otherRect = sibling as RectTransform;
                    if (otherRect == null) continue;
                    if (RectTransformOverlaps(rectTransform, otherRect))
                    {
                        // Move this card slightly to the right to separate
                        // rectTransform.anchoredPosition += new Vector2(otherRect.rect.width + 10f, 0);
                        // HandArea.instance.AddCardObject(gameObject, rectTransform); 
                        rectTransform.SetParent(handRect, true);
                        HandArea.instance.AddCard(cardMaster);
                        overlaps = true;
                    }
                }
                if (!overlaps)
                {
                    rectTransform.SetParent(handRect, true);
                    HandArea.instance.AddCard(cardMaster);
                }
            }
            cardMaster.gridLocation = new Vector2Int(-1, -1); // Clear grid location since it's not on the board
        }
        else if (droppedOnBoard)
        {
            Vector2 boardLocalPoint = BoardArea.instance.ScreenToLocalPoint(Input.mousePosition, canvas.worldCamera);
            Vector2 cardSize = rectTransform.rect.size;
            Vector2Int cell = BoardArea.instance.GetNearestGridCell(boardLocalPoint, cardSize);
            if (!BoardArea.instance.IsCellOccupied(cell.x, cell.y) && CanPlaceCardAtCell(cell.x, cell.y, cardMaster))
            {
                // if (lastRow >= 0 && lastCol >= 0)
                // {
                //     lastRow = lastCol = -1;
                // }
                if (HandArea.instance != null && HandArea.instance.ContainsCard(cardMaster))
                {
                    HandArea.instance.RemoveCard(cardMaster);
                }


                // --- Destroy links in all directions from the card's original board position ---
                if (lastRow >= 0 && lastCol >= 0)
                {
                    var prevGrid = BoardArea.instance.gridState;
                    int prevRows = BoardArea.instance.rows;
                    int prevCols = BoardArea.instance.columns;
                    int prevRow = lastRow, prevCol = lastCol;
                    // Up
                    if (prevRow > 0)
                    {
                        var upCard = prevGrid[prevRow - 1, prevCol];
                        if (upCard != null && upCard.down_link_cardmaster == cardMaster)
                        {
                            upCard.down_link_cardmaster = null;
                        }
                        cardMaster.up_link_cardmaster = null;
                    }
                    // Down
                    if (prevRow < prevRows - 1)
                    {
                        var downCard = prevGrid[prevRow + 1, prevCol];
                        if (downCard != null && downCard.up_link_cardmaster == cardMaster)
                        {
                            downCard.up_link_cardmaster = null;
                        }
                        cardMaster.down_link_cardmaster = null;
                    }
                    // Left
                    if (prevCol > 0)
                    {
                        var leftCard = prevGrid[prevRow, prevCol - 1];
                        if (leftCard != null && leftCard.right_link_cardmaster == cardMaster)
                        {
                            leftCard.right_link_cardmaster = null;
                        }
                        cardMaster.left_link_cardmaster = null;
                    }
                    // Right
                    if (prevCol < prevCols - 1)
                    {
                        var rightCard = prevGrid[prevRow, prevCol + 1];
                        if (rightCard != null && rightCard.left_link_cardmaster == cardMaster)
                        {
                            rightCard.left_link_cardmaster = null;
                        }
                        cardMaster.right_link_cardmaster = null;
                    }
                }

                // Create links in all directions
                var grid = BoardArea.instance.gridState;
                int rows = BoardArea.instance.rows;
                int cols = BoardArea.instance.columns;
                int row = cell.x, col = cell.y;
                // Up
                if (row > 0 && cardMaster.up_link_enabled)
                {
                    var upCard = grid[row - 1, col];
                    if (upCard != null && upCard.down_link_enabled && CardMaster.LinkTypesEqual(upCard.down_link_type, cardMaster.up_link_type))
                    {
                        cardMaster.up_link_cardmaster = upCard;
                        upCard.down_link_cardmaster = cardMaster;
                        // Debug.Log("up card found: " + cardMaster.up_link_cardmaster);
                    }
                }
                // Down
                if (row < rows - 1 && cardMaster.down_link_enabled)
                {
                    var downCard = grid[row + 1, col];
                    if (downCard != null && downCard.up_link_enabled && CardMaster.LinkTypesEqual(downCard.up_link_type, cardMaster.down_link_type))
                    {
                        cardMaster.down_link_cardmaster = downCard;
                        downCard.up_link_cardmaster = cardMaster;
                        // Debug.Log("down card found: " + cardMaster.down_link_cardmaster);
                    }
                }
                // Left
                if (col > 0 && cardMaster.left_link_enabled)
                {
                    var leftCard = grid[row, col - 1];
                    if (leftCard != null && leftCard.right_link_enabled && CardMaster.LinkTypesEqual(leftCard.right_link_type, cardMaster.left_link_type))
                    {
                        cardMaster.left_link_cardmaster = leftCard;
                        leftCard.right_link_cardmaster = cardMaster;
                        // Debug.Log("left card found: " + cardMaster.left_link_cardmaster);
                    }
                }
                // Right
                if (col < cols - 1 && cardMaster.right_link_enabled)
                {
                    var rightCard = grid[row, col + 1];
                    if (rightCard != null && rightCard.left_link_enabled && CardMaster.LinkTypesEqual(rightCard.left_link_type, cardMaster.right_link_type))
                    {
                        cardMaster.right_link_cardmaster = rightCard;
                        rightCard.left_link_cardmaster = cardMaster;
                        // Debug.Log("right card found: " + cardMaster.right_link_cardmaster);
                    }
                }

                // Actually Set the card in the grid. 
                Vector2 snappedLocal = BoardArea.instance.GetGridCellPosition(cell.x, cell.y, cardSize);
                rectTransform.SetParent(BoardArea.instance.cardHolderTransform, true);
                rectTransform.DOAnchorPos(snappedLocal, 0.2f).SetEase(Ease.OutQuad);
                BoardArea.instance.SetCell(cell.x, cell.y, cardMaster);
                cardMaster.gridLocation = new Vector2Int(cell.x, cell.y);
                lastRow = cell.x;
                lastCol = cell.y;


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
            if (cardMaster != null) cardMaster.SetAllLinksHalfTransparent();
            rectTransform.DOAnchorPos(originalAnchoredPosition, 0.3f).SetEase(Ease.OutQuad);
        }

        pointerEnterBlockTime = Time.time + 0.5f;
        if (lastHintRow >= 0 && lastHintCol >= 0)
        {
            ResetHintColor(lastHintRow, lastHintCol);
            // lastHintRow = lastHintCol = -1;
        }

        // --- NEW LOGIC: Rebuild roots list based on BFS traversal from all is_root cards ---
        // 1. Find all cards on the board with is_root == true
        var gridState = BoardArea.instance.gridState;
        int boardRows = BoardArea.instance.rows;
        int boardCols = BoardArea.instance.columns;
        var allRoots = new List<CardMaster>();
        var traversedSet = new HashSet<CardMaster>();
        // Collect all is_root cards
        for (int r = 0; r < boardRows; r++)
        {
            for (int c = 0; c < boardCols; c++)
            {
                var card = gridState[r, c];
                if (card != null && card.is_root)
                {
                    allRoots.Add(card);
                }
            }
        }
        var newRoots = new List<CardMaster>();
        // Traverse each root if not already traversed
        foreach (var root in allRoots)
        {
            if (!traversedSet.Contains(root))
            {
                var bfs = BoardArea.GetOrderedBFSFromRoot(root);
                foreach (var card in bfs)
                {
                    traversedSet.Add(card);
                }
                newRoots.Add(root);
            }
        }
        // Set the roots list to the new list
        BoardArea.instance.roots = newRoots;

        // Only trigger update after all changes
        TriggerUpdateCards();

        // After all, update all board cards' links to green if actively connected
        SetActiveBoardLinksGreen();
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
        // // Check for nulls before accessing components
        // var cardCommon = GetComponent<CardCommon>();
        // if (cardCommon == null) return;
        // if (Time.time < pointerEnterBlockTime && !cardCommon.CanInteract) return; // Block shake if within threshold

        // // Start shake feedback
        // if (shakeTween != null && shakeTween.IsActive()) shakeTween.Kill();
        // if (rectTransform != null)
        //     shakeTween = rectTransform.DOShakePosition(0.3f, strength: new Vector3(2f, 2f, 0), vibrato: 20, randomness: 90, snapping: false, fadeOut: true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Stop shake feedback
        // if (shakeTween != null && shakeTween.IsActive()) shakeTween.Kill();
    }

    private static float lastUpdateCardsTime = -100f;

    // --- Fields for drag UI layering ---
    private Transform originalParent;
    private int originalSiblingIndex;
    public static void TriggerUpdateCards()
    {
        if (Time.time - lastUpdateCardsTime < 0.1f) return;
        lastUpdateCardsTime = Time.time;
        CardMaster.InvokeUpdateCardValues(); // normal value cards to add value changes
        CardMaster.InvokeLateUpdateCardValues(); // update card self conditions and board buffs
        CardMaster.InvokeApplyValuesToGuns(); // apply values to Gun, Base, Action
        CardMaster.InvokeUpdateBaseDesctipion(); // update ? 
        CardMaster.InvokeUpdateCardTexts(); // update descriptions and texts

        // If you need to clear the event, call a static method on CardMaster instead:
        CardMaster.ClearOnApplyValuesToGuns(); // cleaning the cache for cards of updated cards (to prevent from same card value update multiple times)

    }
}
