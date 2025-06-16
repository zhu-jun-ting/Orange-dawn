using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Collider2D))]
public class AoeController : MonoBehaviour
{
    private Animator animator;
    private List<Collider2D> pawns = new List<Collider2D>();

    [Header("Triggering Tags")]
    public List<string> trigger_tags;

    [Header("AOE DOT Test")]
    public DOTStat dot_stat;

    void Start()
    {
        animator = GetComponent<Animator>();
        // Register for Y key event
        if (InputManager.Instance != null)
            InputManager.Instance.OnYKeyPressed += OnYKeyPressed;
        // Register for pause or other input events here if needed in the future
    }

    private void OnDestroy()
    {
        if (InputManager.Instance != null)
            InputManager.Instance.OnYKeyPressed -= OnYKeyPressed;
        // Unregister other input events here if added in the future
    }

    private void OnYKeyPressed()
    {
        animator.SetTrigger("start_anim");
    }

    public void DealAOEDamage()
    {
        foreach (Collider2D pawn in pawns)
        {
            if (pawn != null)
            {
                IBuffable ibuffable = pawn.gameObject.GetComponent<IBuffable>();
                if (ibuffable != null)
                {
                    Buff buff = new Buff(dot_stat);
                    ibuffable.TakeDamage(10, GameEvents.DamageType.Normal, 0f, transform);
                    ibuffable.ApplyBuff(buff);
                }
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!pawns.Contains(other) && trigger_tags.Contains(other.tag))
        {
            pawns.Add(other);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (pawns.Contains(other) && trigger_tags.Contains(other.tag))
        {
            pawns.Remove(other);
        }
    }

#if UNITY_EDITOR
    public void Reset()
    {
        AudioSource source = GetComponent<AudioSource>();
        Light light = GetComponent<Light>();

        if (source == null && light == null)
        {
            if (UnityEditor.EditorUtility.DisplayDialog("Choose a Component", "You are missing one of the required components. Please choose one to add", "AudioSource", "Light"))
            {
                gameObject.AddComponent<AudioSource>();
            }
            else
            {
                gameObject.AddComponent<Light>();
            }
        }
    }
#endif

    // Placeholder for future use
    public void Nothing() { }
}
