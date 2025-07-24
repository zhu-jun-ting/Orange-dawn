using UnityEngine;

public class MinigameBullet : GunBullet
{
    private bool stopped = false;

    protected override void Start()
    {
        SetAoe(false); // Disable AOE by default
        // Do NOT destroy by lifetime
        // Do NOT auto-destroy when stopped
    }

    void FixedUpdate()
    {
        if (stopped) return;
        var rb2d = GetComponent<Rigidbody2D>();
        if (isDirectional && rb2d != null)
        {
            Vector2 vel = rb2d.linearVelocity;
            if (vel.sqrMagnitude > 0.001f)
            {
                float angle = Mathf.Atan2(vel.y, vel.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0, 0, angle);
            }
        }

        // Apply inertia damping every physics step
        if (inertia > 0f && rb2d.linearVelocity.magnitude > 0.01f)
        {
            rb2d.linearVelocity *= Mathf.Clamp01(1f - inertia * Time.fixedDeltaTime);
        }
        
        if (rb2d != null && rb2d.linearVelocity.sqrMagnitude < 0.01f)
        {
            stopped = true;
            // Set to static so it doesn't move anymore
            rb2d.linearVelocity = Vector2.zero;
            rb2d.bodyType = RigidbodyType2D.Static;
            // Notify the current room
            var room = FloorManager.GetCurrentRoomGrid();
            if (room != null)
            {
                (room as RoomLuckyWheel)?.OnBulletStopped(transform.position);
            }
        }
    }
}
