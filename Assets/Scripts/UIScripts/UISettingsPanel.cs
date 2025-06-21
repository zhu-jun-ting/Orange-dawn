using UnityEngine;
using UnityEngine.UI;

using TMPro;

public class UISettingsPanel : MonoBehaviour
{
    [Header("Board Settings")]
    public TMP_InputField inputRows;
    public TMP_InputField inputColumns;
    // public TMP_InputField inputMargin;

    private void Start()
    {
        // Initialize UI with current settings
        var gs = GameSettings.instance;
        if (gs != null)
        {
            inputRows.text = gs.boardRows.ToString();
            inputColumns.text = gs.boardColumns.ToString();
            // inputMargin.text = gs.boardMargin.ToString();
        }
        inputRows.onEndEdit.AddListener(OnRowsChanged);
        inputColumns.onEndEdit.AddListener(OnColumnsChanged);
        // inputMargin.onEndEdit.AddListener(OnMarginChanged);
    }

    public void OnRowsChanged(string value)
    {
        if (int.TryParse(value, out int rows) && GameSettings.instance != null)
        {
            GameSettings.instance.boardRows = rows;
            if (BoardArea.instance != null)
                BoardArea.instance.UpdateGridSize(rows, BoardArea.instance.columns);
        }
    }
    public void OnColumnsChanged(string value)
    {
        if (int.TryParse(value, out int cols) && GameSettings.instance != null)
        {
            GameSettings.instance.boardColumns = cols;
            if (BoardArea.instance != null)
                BoardArea.instance.UpdateGridSize(BoardArea.instance.rows, cols);
        }
    }
    public void OnMarginChanged(string value)
    {
        if (float.TryParse(value, out float margin) && GameSettings.instance != null)
        {
            GameSettings.instance.boardMargin = margin;
        }
    }
}
