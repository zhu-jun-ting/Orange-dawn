using UnityEngine;

public class UIStar : MonoBehaviour
{
    public Sprite yellowStar;
    public Sprite whiteStar;

    public enum StarType
    {
        Yellow,
        White
    }

    public void SetStarType(StarType type)
    {
        var image = GetComponent<UnityEngine.UI.Image>();
        if (image == null) return;

        switch (type)
        {
            case StarType.Yellow:
                image.sprite = yellowStar;
                break;
            case StarType.White:
                image.sprite = whiteStar;
                break;
        }
    }
}
