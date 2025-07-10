using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Reflection;
using System.Collections.Generic;

public class UIDebugPanel : MonoBehaviour
{

    [Header("Dynamic Method Execution")]
    public TMP_InputField methodInputField; // Assign in inspector
    public Button executeButton; // Assign in inspector
    public Button recallButton; // Assign in inspector


    [Header("Debug Fields")]
    public GameObject debugCardMasterPrefab1;
    public GameObject debugCardMasterPrefab2;
    public GameObject debugCardMasterPrefab3;
    public GameObject implusePrefab;
    public GameObject LightningTower; // Assign in inspector
    public GameObject lightBeam; // Assign in inspector


    // private fields
    private string lastExecutedMethod = null;

    void Start()
    {
        if (executeButton != null)
            executeButton.onClick.AddListener(ExecuteMethodFromInput);
        if (recallButton != null)
            recallButton.onClick.AddListener(RecallLastMethod);
    }

    // Example methods to call
    public void LevelCleared()
    {
        GameEvents.instance.LevelCleared();
    }

    public void AddCardObject()
    {
        CardManager.instance.AddCardObject(debugCardMasterPrefab1); 
    }

    public void AddCardObjects()
    {
        CardManager.instance.SelectCardObjects(new List<GameObject> { debugCardMasterPrefab1, debugCardMasterPrefab2, debugCardMasterPrefab3 }); 
    }

    public void ExecuteMethodFromInput()
    {
        if (methodInputField == null) return;
        string methodName = methodInputField.text;
        if (string.IsNullOrWhiteSpace(methodName)) return;
        // Try to find and invoke a method on this class
        MethodInfo method = GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (method != null)
        {
            method.Invoke(this, null);
            lastExecutedMethod = methodName;
            Debug.Log($"Executed method: {methodName}");
        }
        else
        {
            Debug.LogError($"No method found with name: {methodName}");
        }
    }

    public void RecallLastMethod()
    {
        if (string.IsNullOrWhiteSpace(lastExecutedMethod))
        {
            Debug.LogWarning("No method has been executed yet.");
            return;
        }
        MethodInfo method = GetType().GetMethod(lastExecutedMethod, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (method != null)
        {
            method.Invoke(this, null);
            Debug.Log($"Recalled and executed method: {lastExecutedMethod}");
        }
        else
        {
            Debug.LogError($"No method found with name: {lastExecutedMethod}");
        }
    }

    public void SpawnImpulse()
    {
        if (implusePrefab == null)
        {
            Debug.LogError("Impulse prefab not assigned!");
            return;
        }

        // Find the player GameObject by tag (make sure your player has the "Player" tag)
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("Player object not found!");
            return;
        }

        // Instantiate the impulsePrefab at the player's position and rotation
        Instantiate(implusePrefab, player.transform.position, player.transform.rotation);
        // Debug.Log("Impulse prefab instantiated at player's location.");
    }

    public void ActivateDoubleDamage() 
    {
        GameEvents.OnModifyDamage += DoubleDamage;
    }

    public void ShowMsgInfo() 
    {
        GameEvents.instance.ShowMessage("This is a debug message", GameEvents.MessageType.FullInfo, Vector2.zero);
    }

        public void ShowMsgWarning() 
    {
        GameEvents.instance.ShowMessage("This is a debug message", GameEvents.MessageType.FullWarning, Vector2.zero);
    }

    public void ShowMsgLocal() 
    {
        GameEvents.instance.ShowMessage(
            "This is a debug message",
            GameEvents.MessageType.LocalInfo,
            new Vector2(Screen.width / 2f, Screen.height / 2f)
        );
    }

    public void Lightning()
    {
        LightningTower.GetComponent<ItemLightningTower>().PerformAttack();
    }

    public void LevelStart()
    {
        GameEvents.instance.LevelStart(0);
    }

    public void Beam()
    {
        // Find the player GameObject by tag (make sure your player has the "Player" tag)
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("Player object not found!");
            return;
        }

        GameObject beam = Instantiate(lightBeam, player.transform.position, Quaternion.identity);
    }

    public void Tip()
    {
        CanvasManager.ShowTip("Tip Title", "Tip description...");
    }

    float DoubleDamage(float dmg) => dmg * 2f;
}