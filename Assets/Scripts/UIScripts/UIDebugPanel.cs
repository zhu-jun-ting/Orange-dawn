using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Reflection;
using System.Collections.Generic;

public class UIDebugPanel : MonoBehaviour
{

    [Header("Dynamic Method Execution")]
    public TMP_InputField methodInputField; // Assign in inspector
    public TMP_InputField paramInputField; // Assign in inspector
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

    public void LevelStart()
    {
        GameEvents.instance.LevelStart();
    }

    public void LC()
    {
        GameEvents.instance.LevelCleared();
    }

    public void LS()
    {
        GameEvents.instance.LevelStart();
    }

    public void LoadLevel()
    {
        if (int.TryParse(paramInputField.text, out int levelIndex)) GameEvents.instance.LoadLevel(levelIndex);
    }

    public void AC()
    {
        if (int.TryParse(paramInputField.text, out int cardId))
        {
            CardManager.instance.QueueAddCardObjects(new List<GameObject> { CardDatabase.GetCard(cardId) });
        }
    }

    public void AddCoin()
    {
        if (int.TryParse(paramInputField.text, out int coinAmount))
        {
            GameEvents.instance.UpdateCoins(coinAmount);
        }
        else
        {
            GameEvents.instance.UpdateCoins(100);
        }
    }

    public void ACs()
    {
        CardManager.instance.QueueSelectCardObjects(new List<GameObject> { debugCardMasterPrefab1, debugCardMasterPrefab2, debugCardMasterPrefab3 }); 
        CardManager.instance.QueueSelectCardObjects(new List<GameObject> { debugCardMasterPrefab1, debugCardMasterPrefab2, debugCardMasterPrefab3 }); 
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

    public void Lose()
    {
        GameEvents.instance.GameEnd(false);
    }

    public void Win()
    {
        GameEvents.instance.GameEnd(true);
    } 

    public void SetEnemyHealth()
    {
        if (float.TryParse(paramInputField.text, out float health))
        {
            GameSettings.instance.enemyHealthModifier = health;
        }
    }

    public void SetEnemyDamage()
    {
        if (float.TryParse(paramInputField.text, out float damage))
        {
            GameSettings.instance.enemyDamageModifier = damage;
        }
    }

    public void KillAllEnemies()
    {
        foreach (var enemy in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            if (enemy != null) Destroy(enemy);
        }
    }

    public void SetMaxHealth()
    {
        if (int.TryParse(paramInputField.text, out int maxHealth))
        {
            HealthBar.HealthGlobalModifier = maxHealth;
        }
    }

    public void AddExp()
    {
        if (int.TryParse(paramInputField.text, out int expAmount))
        {
            ExpBar.GainExp(expAmount);
        }
        else
        {
            ExpBar.GainExp(100);
        }
    }

    float DoubleDamage(float dmg) => dmg * 2f;
}