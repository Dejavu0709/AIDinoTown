using System.Collections;
using System.Collections.Generic;
using com.ootii.Messages;
using UnityEngine;

public class MainView : MonoBehaviour
{
    public ChatMsgPanel ChatMsgPanel;
    public UserAvatar UserAvatar;
    void Start()
    {
        UserAvatar = GameObject.FindAnyObjectByType<UserAvatar>();
    }
    void OnEnable()
    {
        MessageDispatcher.AddListener("GetChatResponseSuccess", OnGetChatResponseSuccess);
        MessageDispatcher.AddListener("SendChatResponseRequest", OnSendChatResponseRequest);
    }

    void OnDisable()
    {
        MessageDispatcher.RemoveListener("GetChatResponseSuccess", OnGetChatResponseSuccess);
        MessageDispatcher.RemoveListener("SendChatResponseRequest", OnSendChatResponseRequest);
    }
    
    private void OnGetChatResponseSuccess(IMessage message)
    {
        // Show the regular chat panel
        ChatMsgPanel.Show();
        UserAvatar.HideInfo();
         
        // If chat bubbles are enabled and the component exists, it will handle showing the bubble
        // The ChatBubblePanel is already listening for the same message event
    }
    
    private void OnSendChatResponseRequest(IMessage message)
    {
       UserAvatar.ShowInfo("...");
    }
}
