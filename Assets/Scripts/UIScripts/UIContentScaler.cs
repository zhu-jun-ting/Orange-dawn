using UnityEngine;

public class UIContentScaler : MonoBehaviour
{
    public float scaleSpeed = 0.1f;
    public float minScale = 0.5f;
    public float maxScale = 2f;

    public static UIContentScaler instance { get; private set; }

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        // float scroll = Input.GetAxis("Mouse ScrollWheel");
        // if (Mathf.Abs(scroll) > 0.01f)
        // {
        //     Vector3 scale = transform.localScale;
        //     scale += Vector3.one * scroll * scaleSpeed;
        //     scale.x = Mathf.Clamp(scale.x, minScale, maxScale);
        //     scale.y = Mathf.Clamp(scale.y, minScale, maxScale);
        //     scale.z = 1f;
        //     transform.localScale = scale;
        // }
    }
}