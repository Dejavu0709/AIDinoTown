using System.Collections;
using System.Collections.Generic;
using com.ootii.Messages;
using UnityEngine;

public class MainView : MonoBehaviour
{
    public ChatMsgPanel ChatMsgPanel;

    public UserAvatar UserAvatar;

    public GameObject NewsTipsPrefab;
    
    private Vector3 _startPos = new Vector3(-253, 300, 0);
    public Transform ToastCavas;
    void Start()
    {
        UserAvatar = GameObject.FindAnyObjectByType<UserAvatar>();
    }
    void OnEnable()
    {
        MessageDispatcher.AddListener("GetChatResponseSuccess", OnGetChatResponseSuccess);
        MessageDispatcher.AddListener("SendChatResponseRequest", OnSendChatResponseRequest);
        // Crypto messages
        MessageDispatcher.AddListener("DailyCryptoPrices", OnDailyCryptoPrices);
        MessageDispatcher.AddListener("CryptoAlert", OnCryptoAlert);
        MessageDispatcher.AddListener("CryptoTick", OnCryptoTick);
    }

    void OnDisable()
    {
        MessageDispatcher.RemoveListener("GetChatResponseSuccess", OnGetChatResponseSuccess);
        MessageDispatcher.RemoveListener("SendChatResponseRequest", OnSendChatResponseRequest);
        // Crypto messages
        MessageDispatcher.RemoveListener("DailyCryptoPrices", OnDailyCryptoPrices);
        MessageDispatcher.RemoveListener("CryptoAlert", OnCryptoAlert);
        MessageDispatcher.RemoveListener("CryptoTick", OnCryptoTick);
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

    private void OnDailyCryptoPrices(IMessage message)
    {
        string content = message.Data as string;
        Debug.Log("OnDailyCryptoPrices: " + content);
        if (!string.IsNullOrEmpty(content))
        {
            ShowNewsTips(content);
        }
    }

    private void OnCryptoAlert(IMessage message)
    {
        string content = message.Data as string;
        Debug.Log("OnCryptoAlert: " + content);
        if (!string.IsNullOrEmpty(content))
        {
            ShowNewsTips(content);
        }
    }

    private void OnCryptoTick(IMessage message)
    {
        string content = message.Data as string;
        Debug.Log("OnCryptoTick: " + content);
        if (!string.IsNullOrEmpty(content))
        {
            ShowNewsTips(content);
        }
    }
    
    private void ShowNewsTips(string content)
    {
        GameObject newsTips = Instantiate(NewsTipsPrefab, transform);
        newsTips.transform.SetParent(ToastCavas);
        newsTips.transform.localPosition = _startPos;
        newsTips.GetComponent<NewsTips>().Show(content);
    }
}
