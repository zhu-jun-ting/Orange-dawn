using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(RectTransform))]
public class BoardArea : MonoBehaviour
{
    public static BoardArea instance;
    private RectTransform rectTransform;

    [Header("Grid Settings")]
    public int rows = 3;
    public int columns = 3;
    public float margin = 10f;

    [Header("Grid Visuals")]
    public GameObject gridLinePrefab; // Assign a UI Image prefab for lines
    public GameObject cardHintPrefab; // Assign a prefab for card hint (e.g. a semi-transparent card slot)
    private GameObject[,] cardHintObjects;
    private GameObject gridGuidelinesParent;
    private bool guidelinesVisible = false;

    [Header("Grid State")]
    public CardMaster[,] gridState;

    [Header("For test, mark ROOT as the left up most grid cell")]
    public List<CardMaster> roots = new List<CardMaster>();

    private static bool isUpdateRootsRegistered = false;

    void Awake()
    {
        instance = this;
        rectTransform = GetComponent<RectTransform>();
        gridState = new CardMaster[rows, columns];
        CreateCardHints();
        HideCardHints();
        CreateGridGuidelines();
        HideGridGuidelines();

    }

    public bool IsPointInside(Vector2 screenPoint, Camera uiCamera)
    {
        return RectTransformUtility.RectangleContainsScreenPoint(rectTransform, screenPoint, uiCamera);
    }

    public Vector2Int GetNearestGridCell(Vector2 localPoint, Vector2 cardSize)
    {
        float cellWidth = cardSize.x + margin;
        float cellHeight = cardSize.y + margin;
        Vector2 boardSize = rectTransform.rect.size;
        Vector2 origin = new Vector2(-boardSize.x / 2f, boardSize.y / 2f);
        float x = Mathf.Clamp(localPoint.x, origin.x, origin.x + (columns - 1) * cellWidth);
        float y = Mathf.Clamp(localPoint.y, origin.y - (rows - 1) * cellHeight, origin.y);
        int col = Mathf.RoundToInt((x - origin.x) / cellWidth);
        int row = Mathf.RoundToInt((origin.y - y) / cellHeight);
        col = Mathf.Clamp(col, 0, columns - 1);
        row = Mathf.Clamp(row, 0, rows - 1);
        return new Vector2Int(row, col);
    }

    public Vector2 GetGridCellPosition(int row, int col, Vector2 cardSize)
    {
        float cellWidth = cardSize.x + margin;
        float cellHeight = cardSize.y + margin;
        Vector2 boardSize = rectTransform.rect.size;
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

    // --- Grid Visuals ---
    private void CreateGridGuidelines()
    {
        if (gridLinePrefab == null) return;
        gridGuidelinesParent = new GameObject("GridGuidelines");
        gridGuidelinesParent.transform.SetParent(transform, false);
        gridGuidelinesParent.transform.SetAsLastSibling();
        var rt = gridGuidelinesParent.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        // Draw vertical lines
        for (int c = 1; c < columns; c++)
        {
            var line = Instantiate(gridLinePrefab, gridGuidelinesParent.transform);
            var lineRT = line.GetComponent<RectTransform>();
            lineRT.anchorMin = new Vector2((float)c / columns, 0);
            lineRT.anchorMax = new Vector2((float)c / columns, 1);
            lineRT.sizeDelta = new Vector2(2, 0);
        }
        // Draw horizontal lines
        for (int r = 1; r < rows; r++)
        {
            var line = Instantiate(gridLinePrefab, gridGuidelinesParent.transform);
            var lineRT = line.GetComponent<RectTransform>();
            lineRT.anchorMin = new Vector2(0, 1f - (float)r / rows);
            lineRT.anchorMax = new Vector2(1, 1f - (float)r / rows);
            lineRT.sizeDelta = new Vector2(0, 2);
        }
    }

    // --- Card Hint Visuals ---
    private void CreateCardHints()
    {
        if (cardHintPrefab == null) return;
        cardHintObjects = new GameObject[rows, columns];
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < columns; c++)
            {
                var hint = Instantiate(cardHintPrefab, transform);
                hint.name = $"CardHint_{r}_{c}";
                var hintRT = hint.GetComponent<RectTransform>();
                hintRT.anchorMin = hintRT.anchorMax = new Vector2(0.5f, 0.5f);
                Vector2 cardSize = hintRT.sizeDelta;
                Vector2 pos = GetGridCellPosition(r, c, cardSize);
                hintRT.anchoredPosition = pos;
                cardHintObjects[r, c] = hint;
            }
        }
    }

    public void ShowCardHints()
    {
        if (cardHintObjects == null) return;
        for (int r = 0; r < rows; r++)
            for (int c = 0; c < columns; c++)
                if (cardHintObjects[r, c] != null)
                    cardHintObjects[r, c].SetActive(true);
    }
    public void HideCardHints()
    {
        if (cardHintObjects == null) return;
        for (int r = 0; r < rows; r++)
            for (int c = 0; c < columns; c++)
                if (cardHintObjects[r, c] != null)
                    cardHintObjects[r, c].SetActive(false);
    }

    public void ShowGridGuidelines()
    {
        if (gridGuidelinesParent != null) gridGuidelinesParent.SetActive(true);
        guidelinesVisible = true;
    }
    public void HideGridGuidelines()
    {
        if (gridGuidelinesParent != null) gridGuidelinesParent.SetActive(false);
        guidelinesVisible = false;
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
    }
    void OnDisable()
    {
        if (isUpdateRootsRegistered)
        {
            CardMaster.OnUpdateCardValues -= UpdateAllRoots;
            isUpdateRootsRegistered = false;
        }
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
}

