using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExpBar : MonoBehaviour

{
    public Text ExpText;
    public Text LevelText;
    public static float ExpCurrent;
    public static float ExpMax;
    public static float Level;

    private Image ExperienceBar;
    // Start is called before the first frame update
    void Start()
    {
        ExperienceBar = GetComponent<Image>();

        ExpMax = 100;
        ExpBar.Level = 1;
        ExpBar.ExpCurrent = 0;
    }

    // Update is called once per frame
    void Update()
    {
        ExperienceBar.fillAmount = (float)ExpCurrent / (float)ExpMax;
        ExpText.text = ExpCurrent.ToString() + "/" + ExpMax.ToString();
        LevelText.text = "LV. " + Level.ToString();
    }

    public static void GainExp(float experience)
    {
        if (experience + ExpCurrent >= ExpMax)
        {
            Level += 1;
            ExpCurrent = ExpCurrent + experience - ExpMax;
            ExpMax += 50;
            HealthBar.HealthCurrent = HealthBar.HealthMax + 10;
            HealthBar.HealthMax += 10;
        }
        else
        {
            ExpCurrent += experience;
        }
    }
}
