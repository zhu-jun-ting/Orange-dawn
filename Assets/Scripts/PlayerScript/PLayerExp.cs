using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PLayerExp : MonoBehaviour
{
    public float Exp;
    

    // Start is called before the first frame update
    void Start()
    {
        Exp = 100;

        ExpBar.Level = 1;
        ExpBar.ExpMax = Exp;
        ExpBar.ExpCurrent = 0;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void GainExp(float Experience)
    {
        if (Experience + ExpBar.ExpCurrent >= ExpBar.ExpMax)
        {
            ExpBar.Level += 1;
            ExpBar.ExpCurrent = ExpBar.ExpCurrent + Experience - ExpBar.ExpMax;
            ExpBar.ExpMax += 50;
            HealthBar.HealthCurrent = HealthBar.HealthMax + 10;
            HealthBar.HealthMax += 10;
        }
        else
        {
            ExpBar.ExpCurrent += Experience;
        }

    }

    
    private void OnTriggerEnter2D(Collider2D collision)
    {

        if (collision.gameObject.CompareTag("Experience") == true)
        {
           // GainExp(30);
        }
    }
    


}
