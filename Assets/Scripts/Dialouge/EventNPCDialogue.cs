using UnityEngine;
using DialogueEditor;
using UnityEngine.Localization.Settings;
using System.Collections.Generic;

public class EventNPCDialogue : MonoBehaviour
{
    public NPCConversation conversation;

    private bool playerInRange = false;

    void Start()
    {
        if (InputManager.Instance != null)
            InputManager.Instance.OnFKeyPressed += HandleFKeyPressed;

        // Localize all nodes and options if map is set
        if (conversation != null)
        {
            // Deserialize to get the Conversation object
            Conversation deserialized = conversation.Deserialize(); // returns Conversation
            if (deserialized != null && deserialized.Root != null)
            {
                var table = LocalizationSettings.StringDatabase.GetTable("Local");
                if (table != null)
                {
                    HashSet<ConversationNode> visited = new HashSet<ConversationNode>();
                    TraverseAndLocalize(deserialized.Root, table, visited);
                }
                // Force UI refresh if conversation is active
                if (ConversationManager.Instance != null && ConversationManager.Instance.IsConversationActive)
                {
                    var currentSpeech = GetCurrentSpeechNode(deserialized.Root);
                    if (currentSpeech != null && ConversationManager.Instance.DialogueText != null)
                    {
                        ConversationManager.Instance.DialogueText.text = currentSpeech.Text;
                    }
                }
            }
        }
    }
    // Helper to get the current speech node (first node in tree)
    private SpeechNode GetCurrentSpeechNode(ConversationNode root)
    {
        if (root is SpeechNode speech)
            return speech;
        // Traverse to find first SpeechNode
        if (root.Connections != null)
        {
            foreach (var conn in root.Connections)
            {
                ConversationNode nextNode = null;
                if (conn is SpeechConnection speechConn)
                    nextNode = speechConn.SpeechNode;
                else if (conn is OptionConnection optionConn)
                    nextNode = optionConn.OptionNode;
                var found = GetCurrentSpeechNode(nextNode);
                if (found != null)
                    return found;
            }
        }
        return null;
    }

    void TraverseAndLocalize(ConversationNode node, UnityEngine.Localization.Tables.StringTable table, HashSet<ConversationNode> visited)
    {
        if (node == null || visited.Contains(node)) return;
        visited.Add(node);

        // Use the node's Text field directly as the localization key
        string localizationKey = node.Text;
        if (!string.IsNullOrEmpty(localizationKey))
        {
            var entry = table.GetEntry(localizationKey);
            if (entry != null)
            {
                node.Text = entry.GetLocalizedString();
                // If this node is currently displayed, update the UI text as well
                if (ConversationManager.Instance != null && ConversationManager.Instance.IsConversationActive)
                {
                    if (node is SpeechNode && ConversationManager.Instance.DialogueText != null)
                    {
                        ConversationManager.Instance.DialogueText.text = node.Text;
                    }
                    // For options, update option buttons
                    if (node is OptionNode)
                    {
                        var uiOptionsField = ConversationManager.Instance.GetType().GetField("m_uiOptions", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        if (uiOptionsField != null)
                        {
                            var uiOptions = uiOptionsField.GetValue(ConversationManager.Instance) as System.Collections.IList;
                            if (uiOptions != null)
                            {
                                foreach (var btnObj in uiOptions)
                                {
                                    var btnType = btnObj.GetType();
                                    var textField = btnType.GetField("Text", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                                    if (textField != null)
                                    {
                                        textField.SetValue(btnObj, node.Text);
                                    }
                                    else
                                    {
                                        // Try to find a TextMeshProUGUI field
                                        var tmpField = btnType.GetField("TextMesh", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                                        if (tmpField != null)
                                        {
                                            var tmp = tmpField.GetValue(btnObj) as TMPro.TextMeshProUGUI;
                                            if (tmp != null)
                                                tmp.text = node.Text;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        // Traverse child connections
        if (node.Connections != null)
        {
            foreach (var conn in node.Connections)
            {
                ConversationNode nextNode = null;
                // Check connection type and get the correct node
                if (conn is SpeechConnection speechConn)
                {
                    nextNode = speechConn.SpeechNode;
                }
                else if (conn is OptionConnection optionConn)
                {
                    nextNode = optionConn.OptionNode;
                }
                if (nextNode != null)
                {
                    TraverseAndLocalize(nextNode, table, visited);
                }
            }
        }
    }

    void OnDisable()
    {
        if (InputManager.Instance != null)
            InputManager.Instance.OnFKeyPressed -= HandleFKeyPressed;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }

    private void HandleFKeyPressed()
    {
        if (playerInRange && !ConversationManager.Instance.IsConversationActive)
        {
            ConversationManager.Instance.StartConversation(conversation);
        }
    }


    // Optionally, add more event actions here
    public void RoomCleared()
    {
        gameObject.SetActive(false); // Hide NPC after room cleared
        GameEvents.instance.LevelCleared(); // Trigger level cleared event
    }
}
