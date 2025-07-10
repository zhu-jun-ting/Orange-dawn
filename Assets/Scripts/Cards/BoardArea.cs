using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using TMPro;

[RequireComponent(typeof(RectTransform))]
public class BoardArea : MonoBehaviour
{
    // Duplicate method removed: GetNearestGridCell(Vector2, Vector2)
    public static BoardArea instance;
    private RectTransform rectTransform;
    public RectTransform cardHolderTransform; // Assign in inspector to the parent of all cards

    [Header("Grid Settings")]
    public int rows { get { return GameSettings.instance ? GameSettings.instance.boardRows : 3; } set { if (GameSettings.instance) GameSettings.instance.boardRows = value; } }
    public int columns { get { return GameSettings.instance ? GameSettings.instance.boardColumns : 3; } set { if (GameSettings.instance) GameSettings.instance.boardColumns = value; } }
    public float margin { get { return GameSettings.instance ? GameSettings.instance.boardMargin : 10f; } set { if (GameSettings.instance) GameSettings.instance.boardMargin = value; } }

    [Header("Grid Visuals")]
    public GameObject gridLinePrefab; // Assign a UI Image prefab for lines
    public GameObject cardHintPrefab; // Assign a prefab for card hint (e.g. a semi-transparent card slot)
    public GameObject cardSlotPrefab; // Assign a prefab for card slot background
    private GameObject[,] cardHintObjects;
    private GameObject[,] cardSlotObjects;
    private GameObject gridGuidelinesParent;
    private bool guidelinesVisible = false;

    [Header("Bond Hint Visuals")]
    public GameObject bondHintPrefab; // Assign a prefab with TMP component for bond hints
    private List<GameObject> bondHintObjects = new List<GameObject>();


    [Header("Grid State")]
    public CardMaster[,] gridState;
    // New: grid cell open/close state
    public bool[,] gridOpenState;

    [Header("For test, mark ROOT as the left up most grid cell")]
    public List<CardMaster> roots = new List<CardMaster>();

    private static bool isUpdateRootsRegistered = false;
    private CardMaster _lastDraggedCard;
    public CardMaster lastDraggedCard
    {
        get => _lastDraggedCard;
        set => _lastDraggedCard = value;
    }

    void Awake()
    {
        instance = this;
        rectTransform = GetComponent<RectTransform>();
        gridState = new CardMaster[rows, columns];
        gridOpenState = new bool[rows, columns];
        // Initial 3x3 grid is open
        for (int r = 0; r < rows; r++)
            for (int c = 0; c < columns; c++)
                gridOpenState[r, c] = true;
        CreateCardHints();
        HideCardHints();
        // Register bond check to card text update event
        CardMaster.OnUpdateCardTexts += CheckBonds;
    }

    void Start()
    {
        if (GameEvents.instance != null)
        {
            GameEvents.instance.OnLevelCleared += HandleLevelCleared;
        }
    }

    // Subscribe to OnUpdateCardValues event to trigger this for all roots
    void OnEnable()
    {
        if (!isUpdateRootsRegistered)
        {
            CardMaster.OnUpdateCardValues += UpdateAllRoots;
            isUpdateRootsRegistered = true;
        }

        // Trigger card update when board is enabled
        CardDragHandler.TriggerUpdateCards();
    }

    void OnDisable()
    {
        if (isUpdateRootsRegistered)
        {
            CardMaster.OnUpdateCardValues -= UpdateAllRoots;
            isUpdateRootsRegistered = false;
        }
        if (GameEvents.instance != null)
        {
            GameEvents.instance.OnLevelCleared -= HandleLevelCleared;
        }
        CardMaster.OnUpdateCardTexts -= CheckBonds;
    }


    // --- Bond Hint Logic ---
    public void CheckBonds()
    {
        // Destroy previous bond hints
        if (bondHintObjects != null)
        {
            foreach (var go in bondHintObjects)
                if (go != null) Destroy(go);
            bondHintObjects.Clear();
        }
        if (bondHintPrefab == null) return;
        // Row bond hints (show at col = -1)
        for (int r = 0; r < rows; r++)
        {
            var bondCounts = new Dictionary<CardMaster.CardBond, int>();
            for (int c = 0; c < columns; c++)
            {
                var card = gridState[r, c];
                if (card == null) continue;
                if (card.card_bonds != null)
                {
                    foreach (var cb in card.card_bonds)
                    {
                        if (!bondCounts.ContainsKey(cb)) bondCounts[cb] = 0;
                        bondCounts[cb]++;
                    }
                }
            }
            if (bondCounts.Count > 0)
            {
                string hintText = string.Join(" ", bondCounts.Select(kv => $"{kv.Key}: {kv.Value}"));
                var hintGO = Instantiate(bondHintPrefab, cardHolderTransform);
                hintGO.name = $"BondHint_Row_{r}";
                var tmp = hintGO.GetComponent<TMPro.TMP_Text>();
                if (tmp != null) tmp.text = hintText;
                // Place to the left of the row (col = -1)
                Vector2 cellSize = hintGO.GetComponent<RectTransform>().sizeDelta;
                Vector2 pos = GetGridCellPosition(r, -1, cellSize);
                hintGO.GetComponent<RectTransform>().anchoredPosition = pos;
                bondHintObjects.Add(hintGO);
            }
        }
        // Column bond hints (show at row = -1)
        for (int c = 0; c < columns; c++)
        {
            var bondCounts = new Dictionary<CardMaster.CardBond, int>();
            for (int r = 0; r < rows; r++)
            {
                var card = gridState[r, c];
                if (card == null) continue;
                if (card.card_bonds != null)
                {
                    foreach (var cb in card.card_bonds)
                    {
                        if (!bondCounts.ContainsKey(cb)) bondCounts[cb] = 0;
                        bondCounts[cb]++;
                    }
                }
            }
            if (bondCounts.Count > 0)
            {
                string hintText = string.Join(" ", bondCounts.Select(kv => $"{kv.Key}: {kv.Value}"));
                var hintGO = Instantiate(bondHintPrefab, cardHolderTransform);
                hintGO.name = $"BondHint_Col_{c}";
                var tmp = hintGO.GetComponent<TMPro.TMP_Text>();
                if (tmp != null) tmp.text = hintText;
                // Place above the column (row = -1)
                Vector2 cellSize = hintGO.GetComponent<RectTransform>().sizeDelta;
                Vector2 pos = GetGridCellPosition(-1, c, cellSize);
                hintGO.GetComponent<RectTransform>().anchoredPosition = pos;
                bondHintObjects.Add(hintGO);
            }
        }
    }
    

    // Handles propagation of OnCardLevelCleared and triggers card update
    private void HandleLevelCleared()
    {
        if (roots == null) return;
        foreach (var root in roots)
        {
            if (root == null) continue;
            // BFS traversal from root
            var cards = GetOrderedBFSFromRoot(root);
            foreach (var card in cards)
            {
                if (card != null)
                {
                    card.OnCardLevelCleared();
                }
            }
        }
        // After all cards handled, trigger update
        CardDragHandler.TriggerUpdateCards();    
    }

    private void UpdateAllRoots()
    {
        if (roots == null) return;
        foreach (var root in roots)
        {
            if (root != null)
                ResetAndEnableBFSFromRoot(root);
        }
    }













    public bool IsPointInside(Vector2 screenPoint, Camera uiCamera)
    {
        return RectTransformUtility.RectangleContainsScreenPoint(rectTransform, screenPoint, uiCamera);
    }

    public Vector2Int GetNearestGridCell(Vector2 localPoint, Vector2 cardSize)
    {
        // Consider board scaling
        float scale = UIContentScaler.instance != null ? UIContentScaler.instance.transform.localScale.x : 1f;
        float cellWidth = (cardSize.x + margin) * scale;
        float cellHeight = (cardSize.y + margin) * scale;
        Vector2 boardSize = cardHolderTransform.rect.size * scale;
        Vector2 origin = new Vector2(-boardSize.x / 2f, boardSize.y / 2f) * scale;

        // Offset by all parent RectTransforms up to Canvas
        Vector2 parentOffset = Vector2.zero;
        RectTransform t = cardHolderTransform;
        while (t != null && t != t.root)
        {
            parentOffset += (Vector2)t.anchoredPosition;
            t = t.parent as RectTransform;
        }
        origin += parentOffset;
        float x = Mathf.Clamp(localPoint.x, origin.x, origin.x + (columns - 1) * cellWidth);
        float y = Mathf.Clamp(localPoint.y, origin.y - (rows - 1) * cellHeight, origin.y);
        // Find the nearest open cell
        float minDist = float.MaxValue;
        int nearestRow = -1, nearestCol = -1;
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < columns; c++)
            {
                if (!IsCellOpen(r, c)) continue;
                float cellCenterX = origin.x + c * cellWidth;
                float cellCenterY = origin.y - r * cellHeight;
                float dist = (new Vector2(cellCenterX, cellCenterY) - localPoint).sqrMagnitude;
                if (dist < minDist)
                {
                    minDist = dist;
                    nearestRow = r;
                    nearestCol = c;
                }
            }
        }
        if (nearestRow == -1 || nearestCol == -1)
        {
            // fallback: return (0,0) if no open cell found
            return new Vector2Int(0, 0);
        }
        return new Vector2Int(nearestRow, nearestCol);
    }

    public Vector2 GetGridCellPosition(int row, int col, Vector2 cardSize)
    {
        // Consider board scaling
        // float scale = UIContentScaler.instance != null ? UIContentScaler.instance.transform.localScale.x : 1f;
        float cellWidth = (cardSize.x + margin);
        float cellHeight = (cardSize.y + margin);
        Vector2 boardSize = cardHolderTransform.rect.size;
        Vector2 origin = new Vector2(-boardSize.x / 2f, boardSize.y / 2f);
        float snappedX = origin.x + col * cellWidth;
        float snappedY = origin.y - row * cellHeight;
        return new Vector2(snappedX, snappedY);
    }

    public Vector2 ScreenToLocalPoint(Vector2 screenPoint, Camera uiCamera)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenPoint, uiCamera, out Vector2 localPoint);
        return localPoint;
    }


    public bool IsCellOpen(int row, int col)
    {
        if (gridOpenState == null) return false;
        if (row < 0 || row >= rows || col < 0 || col >= columns) return false;
        return gridOpenState[row, col];
    }

    public void ActivateCell(int row, int col)
    {
        if (gridOpenState == null) return;
        // If cell is inside current grid, just activate
        if (row >= 0 && row < rows && col >= 0 && col < columns)
        {
            gridOpenState[row, col] = true;
            // Optionally, update hint visuals here if needed
            return;
        }

        // If cell is outside, expand grid using UpdateGridSize
        int newRows = Mathf.Max(rows, row + 1);
        int newCols = Mathf.Max(columns, col + 1);

        // Create new grid state with all cells inactive except the requested one
        var newGridOpenState = new bool[newRows, newCols];
        // Copy old data
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < columns; c++)
            {
                newGridOpenState[r, c] = gridOpenState[r, c]; 
            }
        }
        // Only activate the requested cell
        newGridOpenState[row, col] = true;
        gridOpenState = newGridOpenState;

        // Update rows/columns properties
        if (GameSettings.instance != null)
        {
            GameSettings.instance.boardRows = newRows;
            GameSettings.instance.boardColumns = newCols;
        }

        UpdateGridSize(newRows, newCols);
    }

    public void DeactivateCell(int row, int col)
    {
        if (gridOpenState == null) return;
        if (row < 0 || row >= rows || col < 0 || col >= columns) return;
        gridOpenState[row, col] = false;
        // Optionally, update hint visuals here if needed
    }

    public bool IsCellOccupied(int row, int col)
    {
        return gridState[row, col] != null;
    }

    public void SetCell(int row, int col, CardMaster card)
    {
        gridState[row, col] = card;

    }

    public CardMaster GetCell(int row, int col)
    {
        if (gridState == null)
            return null;
        if (row < 0 || row >= rows || col < 0 || col >= columns)
            return null;
        return gridState[row, col];
    }

    public void ClearCell(int row, int col)
    {
        gridState[row, col] = null;

    }

    // --- Card Hint Visuals ---
    private void CreateCardHints()
    {
        if (cardHintPrefab == null || cardSlotPrefab == null) return;

        // Destroy existing hints and slots to avoid duplicates
        if (cardHintObjects != null)
        {
            for (int r = 0; r < cardHintObjects.GetLength(0); r++)
                for (int c = 0; c < cardHintObjects.GetLength(1); c++)
                    if (cardHintObjects[r, c] != null)
                        Destroy(cardHintObjects[r, c]);
        }
        if (cardSlotObjects != null)
        {
            for (int r = 0; r < cardSlotObjects.GetLength(0); r++)
                for (int c = 0; c < cardSlotObjects.GetLength(1); c++)
                    if (cardSlotObjects[r, c] != null)
                        Destroy(cardSlotObjects[r, c]);
        }

        cardHintObjects = new GameObject[rows, columns];
        cardSlotObjects = new GameObject[rows, columns];
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < columns; c++)
            {
                if (IsCellOpen(r, c))
                {
                    // Create card slot (background)
                    var slot = Instantiate(cardSlotPrefab, cardHolderTransform);
                    slot.name = $"CardSlot_{r}_{c}";
                    var slotRT = slot.GetComponent<RectTransform>();
                    slotRT.anchorMin = slotRT.anchorMax = new Vector2(0.5f, 0.5f);
                    Vector2 slotSize = slotRT.sizeDelta;
                    Vector2 slotPos = GetGridCellPosition(r, c, slotSize);
                    slotRT.anchoredPosition = slotPos;
                    cardSlotObjects[r, c] = slot;
                    // Set sibling index to be at the back (behind hints and cards)
                    slot.transform.SetSiblingIndex(0);

                    // Create card hint (overlay, for highlight)
                    var hint = Instantiate(cardHintPrefab, cardHolderTransform);
                    hint.name = $"CardHint_{r}_{c}";
                    var hintRT = hint.GetComponent<RectTransform>();
                    hintRT.anchorMin = hintRT.anchorMax = new Vector2(0.5f, 0.5f);
                    Vector2 hintSize = hintRT.sizeDelta;
                    Vector2 hintPos = GetGridCellPosition(r, c, hintSize);
                    hintRT.anchoredPosition = hintPos;
                    cardHintObjects[r, c] = hint;
                    // Only show hint if cell is open (default hidden)
                    hint.SetActive(false);
                }
                else
                {
                    cardSlotObjects[r, c] = null;
                    cardHintObjects[r, c] = null;
                }
            }
        }
    }

    public void ShowCardHints()
    {
        if (cardHintObjects == null) return;
        for (int r = 0; r < rows; r++)
            for (int c = 0; c < columns; c++)
                if (cardHintObjects[r, c] != null)
                    cardHintObjects[r, c].SetActive(IsCellOpen(r, c));
    }
    public void HideCardHints()
    {
        if (cardHintObjects == null) return;
        for (int r = 0; r < rows; r++)
            for (int c = 0; c < columns; c++)
                if (cardHintObjects[r, c] != null)
                    cardHintObjects[r, c].SetActive(false);
    }

    /// <summary>
    /// Updates the grid size and visuals. Expands to right/down, preserves cards. Shrinks from right/down, blocks if cards present.
    /// </summary>
    public void UpdateGridSize(int newRows, int newCols)
    {
        int oldRows = gridState.GetLength(0);
        int oldCols = gridState.GetLength(1);
        // Check for shrink with active cards
        if (newRows < oldRows)
        {
            for (int r = newRows; r < oldRows; r++)
                for (int c = 0; c < oldCols; c++)
                    if (gridState[r, c] != null)
                    {
                        Debug.LogError($"Cannot shrink rows: Card present at [{r},{c}] ({gridState[r, c].name})");
                        return;
                    }
        }
        if (newCols < oldCols)
        {
            for (int c = newCols; c < oldCols; c++)
                for (int r = 0; r < oldRows; r++)
                    if (gridState[r, c] != null)
                    {
                        Debug.LogError($"Cannot shrink columns: Card present at [{r},{c}] ({gridState[r, c].name})");
                        return;
                    }
        }
        // Create new grid
        var newGrid = new CardMaster[newRows, newCols];
        for (int r = 0; r < Mathf.Min(oldRows, newRows); r++)
            for (int c = 0; c < Mathf.Min(oldCols, newCols); c++)
                newGrid[r, c] = gridState[r, c];
        gridState = newGrid;
        // Update hints and guides
        CreateCardHints();
        HideCardHints();
        Debug.Log($"Grid resized to {newRows}x{newCols}");
    }

    // Performs a BFS from the given root and returns a list of CardMaster from endmost node to root (root last)
    public static List<CardMaster> GetOrderedBFSFromRoot(CardMaster root)
    {
        var result = new List<CardMaster>();
        if (root == null) return result;
        var visited = new HashSet<CardMaster>();
        var queue = new Queue<CardMaster>();

        queue.Enqueue(root);
        visited.Add(root);

        // Standard BFS traversal
        while (queue.Count > 0)
        {
            var node = queue.Dequeue();
            result.Add(node);

            // Check links in order: top, left, right, down
            CardMaster[] children = new CardMaster[] {
                node.up_link_cardmaster,
                node.left_link_cardmaster,
                node.right_link_cardmaster,
                node.down_link_cardmaster
            };
            foreach (var child in children)
            {
                if (child != null && !visited.Contains(child))
                {
                    queue.Enqueue(child);
                    visited.Add(child);
                }
            }
        }
        // Reverse the result to have root last
        result.Reverse();

        // Debug.Log($"BFS from root {root.name} found {result.Count} cards.");
        // Debug.Log($"BFS from root {root.name} found cards: {string.Join(", ", result.Select(c => c.name))}"); 

        return result;
    }

    // Traverse BFS from a given root, reset all cards, then call OnCardEnable in order
    public static void ResetAndEnableBFSFromRoot(CardMaster root)
    {
        var ordered = GetOrderedBFSFromRoot(root);
        // Reset all cards first
        foreach (var card in ordered)
        {
            if (card != null)
                card.Reset();
        }
        // Then call OnCardEnable in order
        foreach (var card in ordered)
        {
            if (card != null)
                card.OnCardEnable();
                // Debug.Log($"Enabled card: {card.name} from root {root.name}"); 
        }
    }
}
