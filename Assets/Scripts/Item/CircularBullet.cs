using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircularBullet : Bullet
{
    [Header("Circular Motion Parameters")]
    public Vector2 center = Vector2.zero;
    public float radius = 1f;
    public float angularSpeed = 180f; // degrees per second
    public float initialAngle = 0f; // degrees

    private float currentAngle;

    protected override void Start()
    {
        base.Start();
        // Set initial angle based on current position if not set
        if (radius <= 0f)
        {
            radius = Vector2.Distance(transform.position, center);
        }
        if (Mathf.Approximately(initialAngle, 0f))
        {
            Vector2 dir = ((Vector2)transform.position - center).normalized;
            initialAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        }
        currentAngle = initialAngle;
    }

    void FixedUpdate()
    {
        // Update angle
        currentAngle += angularSpeed * Time.deltaTime;
        if (currentAngle > 360f) currentAngle -= 360f;
        // Calculate new position
        float rad = currentAngle * Mathf.Deg2Rad;
        Vector2 offset = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * radius;
        transform.position = center + offset;
    }
}
