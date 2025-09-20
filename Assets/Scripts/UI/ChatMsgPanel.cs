using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChatMsgPanel : MonoBehaviour
{
    public Text MyMsg;
    public Text TitleText;
    // Start is called before the first frame update
    public void Show()
    {
        this.gameObject.SetActive(true);
        MyMsg.text = ChatSample.CurMsgSended;
    }
    public void Hide()
    {
        this.gameObject.SetActive(false);
    }
}
