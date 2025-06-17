using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PawnMaster : MonoBehaviour, IBuffable
{
    [Header("Pawn Master : the buff list UI panel game object and the buff icon prefab")]
    public Transform buff_icon_grid;
    public GameObject icon_prefab;

    public class BuffController
    {
        public IEnumerator timer;
        public BuffIconController icon_controller;
    }

    protected Dictionary<Buff, BuffController> buffs = new Dictionary<Buff, BuffController>();

    // this is use for logging which frame should we call the Update functions in the buff list
    // whenever the current_buff_frame_count == FRAME_PER_TICK, we should call once of all Update functions in buffs
    protected int FRAME_PER_TICK;
    protected int current_buff_frame_count;

    public DOTStat dot_stat;

    public virtual void Start()
    {
        current_buff_frame_count = 0;
        FRAME_PER_TICK = Mathf.RoundToInt(CombatManager.TICK_INTERVAL / Time.fixedDeltaTime);
    }

    public virtual void FixedUpdate()
    {
        current_buff_frame_count++;
        if (current_buff_frame_count == FRAME_PER_TICK)
        {
            if (buffs != null)
            {
                OnBuffUpdateTick();
            }
            current_buff_frame_count = 0;
        }
    }

    protected void OnBuffUpdateTick()
    {
        foreach (Buff buff in buffs.Keys)
        {
            buff.UpdateBuff(this);
            // Debug.Log("update buff tick, total buffs" + buffs.Keys.Count);
        }
    }

    public virtual void ApplyBuff(Buff buff_)
    {
        Buff to_remove = null;
        // first check if the current buff list contains the buff, if contains, refresh the timer
        foreach (Buff buff in buffs.Keys)
        {
            if (buff.buff_name == buff_.buff_name)
            {
                StopCoroutine(buffs[buff].timer);
                buffs[buff].icon_controller.RemoveIcon();
                buff.EndBuff(this);
                // buffs.Remove(buff);  
                to_remove = buff;
            }
        }

        if (to_remove != null) buffs.Remove(to_remove);

        buff_.StartBuff(this);

        BuffController buff_controller = new BuffController();
        if (icon_prefab != null)
        {
            Transform buff_icon = Instantiate(icon_prefab, Vector3.zero, Quaternion.identity).transform;
            BuffIconController buff_icon_controller = buff_icon.GetComponent<BuffIconController>();
            buff_icon.SetParent(buff_icon_grid);
            buff_icon_controller.SetIcon(buff_.icon);
            buff_controller.icon_controller = buff_icon_controller;
        }

        // start the countdown timer for handling buff duration and add a handler to the dict
        if (buff_.duration != 0f) {
            IEnumerator timer = BuffEndTimer(buff_.duration, buff_, Time.time, buff_controller.icon_controller);
            buff_controller.timer = timer;
            StartCoroutine(timer);
        }
        
        if (!buffs.TryAdd(buff_, buff_controller))
        {
            Debug.LogError("can not add buff type" + buff_.buff_type);
        }
    }

    private IEnumerator BuffEndTimer(float duration_, Buff buff_, float start_time, BuffIconController controller)
    {
        while (true)
        {
            float progress = (Time.time - start_time) / duration_;

            if (progress < 1f)
            {
                controller.SetProgress(progress);
                // Debug.Log(progress);
                yield return new WaitForSeconds(.1f);
            }
            else
            {
                // timer ends, remove buff 
                if (buffs.ContainsKey(buff_))
                {
                    buff_.EndBuff(this);
                    // TODO: update UI for ending of a buff
                    controller.RemoveIcon();
                    buffs.Remove(buff_);
                }
                break;
            }
        }
    }

    public virtual void TakeDamage(float _amount, GameEvents.DamageType damage_type_, float _hit_back_factor, Transform instigator, Gun source = null)
    {
        // This method should be overridden in derived classes to handle damage logic
    }


    public void BuffDamage(float amount_)
    {
        // ...existing code...
    }

    public virtual void UpdatePlayerContinuousAOE(ContiniousAOEStat stat)
    {
        Debug.LogError("pawn " + gameObject + " can not have an AOE buff");
    }

    public virtual void AddLifeStealPercent(float percent_)
    {
        Debug.LogError("pawn " + gameObject + " can not have an lifesteal buff");
    }
}
