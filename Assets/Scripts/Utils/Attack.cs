using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    void OnEnable()
    {
        if (InputManager.Instance != null)
            InputManager.Instance.OnFire += SwordAttack;
    }

    void OnDisable()
    {
        if (InputManager.Instance != null)
            InputManager.Instance.OnFire -= SwordAttack;
    }

    // Update is called once per frame
    void Update()
    {
        // Input is now handled via InputManager event.
    }

    private void SwordAttack()
    {
        Vector2 diff = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
        float rotateZ = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;
        transform.eulerAngles = new Vector3(0, 0, rotateZ);

        transform.GetChild(0).gameObject.SetActive(true);
    }
}
