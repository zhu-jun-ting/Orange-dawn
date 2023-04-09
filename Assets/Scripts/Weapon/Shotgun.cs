using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shotgun : Gun
{
    
    public int bulletNum = 3;
    public float bulletAngle = 15;

    protected override void Fire()
    {
        animator.SetTrigger("Shoot");

        int median = bulletNum / 2;
        for (int i = 0; i < bulletNum; i++)
        {
            GameObject bullet = ObjectPool.Instance.GetObject(bulletPrefab);
            bullet.transform.position = muzzlePos.position;
            bullet.GetComponent<GunBullet>().trigger_tags.Add("Enemy");
            bullet.GetComponent<GunBullet>().att = damage;
            bullet.GetComponent<GunBullet>().hit_back = 0.1f;
            bullet.GetComponent<GunBullet>().SetOwner(gameObject);

            if (bulletNum % 2 == 1)
            {
                bullet.GetComponent<GunBullet>().SetSpeed(Quaternion.AngleAxis(bulletAngle * (i - median), Vector3.forward) * direction,speed);
            }
            else
            {
                bullet.GetComponent<GunBullet>().SetSpeed(Quaternion.AngleAxis(bulletAngle * (i - median) + bulletAngle / 2, Vector3.forward) * direction,speed);
            }
        }

        GameObject shell = ObjectPool.Instance.GetObject(shellPrefab);
        shell.transform.position = shellPos.position;
        shell.transform.rotation = shellPos.rotation;
    }
}
