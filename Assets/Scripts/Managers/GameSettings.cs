using UnityEngine;

public class GameSettings : MonoBehaviour
{
    public static GameSettings instance;

    [Header("Board Settings")]
    public int boardRows = 3;
    public int boardColumns = 3;
    public float boardMargin = 10f;

    [Header("Card Drag Colors")]
    public Color colorCanPlace = Color.green;
    public Color colorCannotPlace = Color.red;
    public Color colorLinkInactive = new Color(0, 0, 0, 0.5f); // Black, 50% transparent
    public Color colorLinkActive = new Color(0, 1, 0, 1f); // Green, fully opaque

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(this.gameObject);
    }
}
