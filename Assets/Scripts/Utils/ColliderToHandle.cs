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
}
