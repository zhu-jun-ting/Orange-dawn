using UnityEngine;
using UnityEngine.UI;

public class UIGradientColor : MonoBehaviour
{
    public Gradient gradient; // Set this in the Inspector
    [Range(0, 1)]
    public float t = 0.5f; // Position in the gradient (0=start, 1=end)

    private Image img;

    void Awake()
    {
        img = GetComponent<Image>();
    }

    void OnEnable()
    {
        ApplyGradientColor();
    }

    void OnValidate()
    {
        // Update in editor when t or gradient changes
        if (img == null) img = GetComponent<Image>();
        ApplyGradientColor();
    }

    public void ApplyGradientColor()
    {
        if (img != null && gradient != null)
        {
            img.color = gradient.Evaluate(t);
        }
    }
}
