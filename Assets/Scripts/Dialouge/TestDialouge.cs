using UnityEngine;
using DialogueEditor;

public class TestDialouge : MonoBehaviour
{

    public NPCConversation Conversation;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            ConversationManager.Instance.StartConversation(Conversation);
        }
    }
}
