using UnityEngine;

public interface IColliderHandler
{
    void HandleTriggerEnter2D(Collider2D other);
    void HandleTriggerExit2D(Collider2D other);
    void HandleCollisionEnter2D(Collision2D collision);
    void HandleCollisionExit2D(Collision2D collision);
}
