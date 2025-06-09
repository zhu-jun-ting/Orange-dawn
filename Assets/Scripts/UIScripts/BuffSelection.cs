using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class BuffSelection : View
{
    [SerializeField] private Button option1;
    [SerializeField] private Button option2;
    [SerializeField] private Button option3;
    public List<StatMaster> buff_list;


    private void OnEnable()
    {
        if (InputManager.Instance != null)
            InputManager.Instance.OnBuffSelectionToggle += Toggle;
    }

    private void OnDestroy() {
        if (InputManager.Instance != null)
            InputManager.Instance.OnBuffSelectionToggle -= Toggle;
    }

    public override void Update() { /* Input is now handled via InputManager event. */ }

    private void Toggle()
    {
        CanvasManager.Show<BuffSelection>();
        // Debug.Log("BuffSelection toggled.");
    }

    public override void Initialize() { 
        buff_list = BuffStatList.instance.buffstats;

        option1.onClick.AddListener(Select1); 
        option2.onClick.AddListener(Select2);
        option3.onClick.AddListener(Select3);
    }

    // TODO: randomize everything and update buff pool
    private void Select1() {
        CanvasManager.Hide<BuffSelection>();
        StatMaster buff_stat = buff_list[0];

        Buff buff = new Buff(buff_stat);

        // Debug.Log(buff.buff_name);
        GameObject.FindGameObjectWithTag("Player").GetComponent<IBuffable>().ApplyBuff(buff);
    }

    private void Select2() {
        CanvasManager.Hide<BuffSelection>();
        StatMaster buff_stat = buff_list[1];

        Buff buff = new Buff(buff_stat);

        // Debug.Log(buff.buff_name);
        GameObject.FindGameObjectWithTag("Player").GetComponent<IBuffable>().ApplyBuff(buff);
    }

    private void Select3() {
        CanvasManager.Hide<BuffSelection>();
        StatMaster buff_stat = buff_list[2];

        Buff buff = new Buff(buff_stat);

        // Debug.Log(buff.buff_name);
        GameObject.FindGameObjectWithTag("Player").GetComponent<IBuffable>().ApplyBuff(buff);
    }

    public override void Show() {
        base.Show();
        Time.timeScale = 0f;


    
        // GameObject.FindWithTag("Player").transform.Find("SwordHolder").gameObject.GetComponent<Attack>().enabled = false;
    }

    public override void Hide() {
        base.Hide();
        Time.timeScale = 1f;
    
        // GameObject.FindWithTag("Player").transform.Find("SwordHolder").gameObject.GetComponent<Attack>().enabled = true;
    }
}
