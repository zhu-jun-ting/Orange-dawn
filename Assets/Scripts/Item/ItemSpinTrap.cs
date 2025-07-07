using System.Collections;
using UnityEngine;

public class ItemSpinTrap : ItemMaster
{
    [Header("Spin Trap Settings")]
    public float spinDuration = 3f;
    public float aoeInterval = 0.5f;
    public float aoeDamage = 10f;
    public float aoeForce = 20f;
    public float aoeDuration = 0.3f;
    public float aoeMaxRadius = 2f;
    public GameObject impulseAOECentrifugalPrefab; // Assign in inspector
    public string spinFxName = "FxSpin";
    private bool isSpinning = false;
    private Coroutine spinRoutine;

    public override void OnHit(Collision2D collision)
    {
        if (isSpinning) return;
        if (collision.gameObject.GetComponent<GunBullet>() == null) return;
        spinRoutine = StartCoroutine(SpinAndAOE());
    }

    private IEnumerator SpinAndAOE()
    {
        isSpinning = true;
        // Play looping spin FX
        CombatManager.PlayFx(spinFxName, transform.position, aoeMaxRadius * 2, spinDuration, true);
        float elapsed = 0f;
        while (elapsed < spinDuration)
        {
            SpawnAOE();
            yield return new WaitForSeconds(aoeInterval);
            elapsed += aoeInterval;
        }
        isSpinning = false;
    }

    private void SpawnAOE()
    {
        if (impulseAOECentrifugalPrefab == null) return;
        GameObject aoe = Instantiate(impulseAOECentrifugalPrefab, transform.position, Quaternion.identity);
        var aoeComp = aoe.GetComponent<ImpulseAOECentrifugal>();
        if (aoeComp != null)
        {
            aoeComp.maxRadius = aoeMaxRadius;
            aoeComp.duration = aoeDuration;
            aoeComp.maxDamage = aoeDamage;
            aoeComp.centrifugalForce = aoeForce;
        }
    }
}
