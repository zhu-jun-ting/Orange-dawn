using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemCoin : MonoBehaviour, IColliderHandler
{
    private GameObject Player;
    private bool inPlayer;
    public float moveSpeed;
    // Start is called before the first frame update
    void Start()
    {
        Player = GameObject.FindGameObjectWithTag("Player");
        inPlayer = false;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (inPlayer == true)
        {
            transform.position = Vector2.MoveTowards(transform.position, Player.transform.position, moveSpeed * Time.deltaTime);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {

        if (collision.gameObject.CompareTag("Player") == true)
        {
            inPlayer = true;
        }
    }

    bool hasGet = false;
    public void HandleTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.CompareTag("Player") == true && !hasGet)
        {
            if (GameEvents.instance != null)
            {
                hasGet = true;
                GameEvents.instance.UpdateCoins(1); // Assuming you have a method to handle coin collection
                Destroy(gameObject);
            }
        }


    }















    public void HandleTriggerExit2D(Collider2D collider)
    {
        // Add logic if needed, or leave empty
    }

    public void HandleCollisionEnter2D(Collision2D collision)
    {
        // Add logic if needed, or leave empty
    }

    public void HandleCollisionExit2D(Collision2D collision)
    {
        // Add logic if needed, or leave empty
    }
}
