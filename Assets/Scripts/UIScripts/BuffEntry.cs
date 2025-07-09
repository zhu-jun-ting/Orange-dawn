using UnityEngine;
using TMPro;

public class BuffEntry : MonoBehaviour
{
    [SerializeField] private TMP_Text buffName;
    [SerializeField] private TMP_Text buffDescription;
    public int stackCount = 0;
    public int order = 0;
    public string description;
    public string name;

    // Assigns the buff name text
    public void SetBuffName(string newName = null)
    {
        // If newName is not null or empty, update the local name variable
        if (!string.IsNullOrEmpty(newName))
        {
            name = newName;
        }
        // Otherwise, use the existing name value

        if (buffName != null)
        {
            if (stackCount >= 2)
                buffName.text = $"{name} x {stackCount}";
            else
                buffName.text = name;
        }
    }

    // Assigns the buff description text
    public void SetBuffDescription(string newDescription = null)
    {
        // If newDescription is not null or empty, update the local description variable
        if (!string.IsNullOrEmpty(newDescription))
        {
            description = newDescription;
        }
        // Otherwise, use the existing description value

        if (buffDescription != null)
            buffDescription.text = description;
    }

    // Call to stack a buff incrementally
    public void StackBuff()
    {
        stackCount++;
        SetBuffName(name); // Update the name display with the new stack count
    }
}