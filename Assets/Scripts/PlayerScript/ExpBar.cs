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
        //HealthCurrent = HealthMax;
    }

    // Update is called once per frame
    void Update()
    {
        ExperienceBar.fillAmount = (float)ExpCurrent / (float)ExpMax;
        ExpText.text = ExpCurrent.ToString() + "/" + ExpMax.ToString();
        LevelText.text = "LV. " + Level.ToString();
    }

     
}
