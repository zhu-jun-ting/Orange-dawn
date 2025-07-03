using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ColliderToHandle : MonoBehaviour
{

    public GameObject handlerObject; // Assign a GameObject in the scene with the handler script as component
    private IColliderHandler handler;


    void Awake()
    {
        if (handlerObject != null)
        {
            handler = handlerObject.GetComponent<IColliderHandler>();
            if (handler == null)
            {
                Debug.LogError($"Assigned handlerObject does not have a component implementing IColliderHandler: {handlerObject.name}", this);
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        handler?.HandleTriggerEnter2D(other);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        handler?.HandleTriggerExit2D(other);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        handler?.HandleCollisionEnter2D(collision);
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        handler?.HandleCollisionExit2D(collision);
    }

    public void ChangeColliderRange(float range)
    {
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            if (collider is CircleCollider2D circleCollider)
            {
                circleCollider.radius = range;
            }
            else if (collider is BoxCollider2D boxCollider)
            {
                boxCollider.size = new Vector2(range, range);
            }
            else
            {
                Debug.LogWarning("Unsupported collider type for range adjustment: " + collider.GetType(), this);
            }
        }
        else
        {
            Debug.LogError("No Collider2D component found on this GameObject.", this);
        }
    }
    
    public void SetHandlerObject(GameObject newHandlerObject)
    {
        handlerObject = newHandlerObject;
        if (handlerObject != null)
        {
            handler = handlerObject.GetComponent<IColliderHandler>();
            if (handler == null)
            {
                Debug.LogError($"Assigned handlerObject does not have a component implementing IColliderHandler: {handlerObject.name}", this);
            }
        }
    }
}
