using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{

    public Slider slider;
    public Gradient gradient;
    public Image fill;

    public void SetHealthPercent(float percent) {
        float p = Mathf.Clamp(percent, 0f, 1f);

        slider.value = p;
        fill.color = gradient.Evaluate(p);
    }

    public void SetHealth(float max_health, float cur_health) {
        SetHealthPercent(cur_health / max_health);
    }
}
