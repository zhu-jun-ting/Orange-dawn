using UnityEngine;

public class ItemPowerUpBox : ItemMaster
{
    [Header("PowerUpBox Settings")]
    [Tooltip("Amount of temporary damage to add to the gun (set 0 to ignore)")]
    public float tempDamage = 1f;
    [Tooltip("Amount of temporary speed to add to the gun (set 0 to ignore)")]
    public float tempSpeed = 0f;
    [Tooltip("Show a tip when powerup is applied")]
    public string tip = "TIME +{0} {1}";

    public override void OnHit(Collision2D collision)
    {
        base.OnHit(collision);
        // Try to find the bullet and its gun
        var bullet = collision.collider.GetComponent<GunBullet>();
        if (bullet != null && bullet.gun != null)
        {
            if (tempDamage != 0f)
            {
                bullet.gun.tempDamage += tempDamage;
                ShowMessageLocal(GameSettings.AddIcon(string.Format(tip, tempDamage, "DAMAGE")));
            }
            if (tempSpeed != 0f)
            {
                bullet.gun.tempSpeed += tempSpeed;
                ShowMessageLocal(GameSettings.AddIcon(string.Format(tip, tempSpeed, "SPEED")));
            }
        }
    }
}
