using System.Collections;
using System.Collections.Generic;
using NexgenDragon;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using MEC;
using RobotGame;
using TMPro;
public class ChatSample : MonoSingleton<ChatSample>
{
    /// <summary>
    /// 
    /// ????????
    /// </summary>
    [SerializeField] private ChatSetting m_ChatSettings;
    #region UI????
    /// <summary>
    /// ????UI??
    /// </summary>
    [SerializeField] private GameObject m_ChatPanel;
    /// <summary>
    /// ?????????
    /// </summary>
    [SerializeField] public  TMP_InputField m_InputWord;
    /// <summary>
    /// ????????
    /// </summary>
    [SerializeField] private Text m_TextBack;
    /// <summary>
    /// ????????
    /// </summary>
    [SerializeField] private AudioSource m_AudioSource;
    /// <summary>
    /// ??????????
    /// </summary>
    [SerializeField] private Button m_CommitMsgBtn;


    //[SerializeField] private Button m_CloseBtn;

   // [SerializeField] private InterfaceAnimManager interfaceAnimManager;


    #endregion

    #region ????????
    /// <summary>
    /// ??????????
    /// </summary>
    [SerializeField] private Animator m_Animator;
    /// <summary>
    /// ?????????????false,????????????
    /// </summary>
    [Header("???????????????????????")]
    [SerializeField] private bool m_IsVoiceMode = true;
    [Header("?????????LLM???????????????")]
    [SerializeField] private bool m_CreateVoiceMode = false;

    #endregion

    private void Start()
    {
        m_CommitMsgBtn.onClick.AddListener(delegate { SendData(); });
        //m_CloseBtn.onClick.AddListener(delegate { CloseChat(); });
        RegistButtonEvent();
        InputSettingWhenWebgl();
        ChatHistory = PlayerSave.GetChatHistory();
    }

    #region ???????

    /// <summary>
    /// webgl??????????????????
    /// </summary>
    private void InputSettingWhenWebgl()
    {
#if UNITY_WEBGL
        // m_InputWord.gameObject.AddComponent<WebGLSupport.WebGLInput>();
#endif
    }


    /// <summary>
    /// ???????
    /// </summary>
    public void SendData()
    {
        Debug.Log("SendData" + m_InputWord.text);
        if (m_InputWord.text.Equals(""))
            return;

        if (m_CreateVoiceMode)//????????????
        {
            CallBack(m_InputWord.text);
            m_InputWord.text = "";
            return;
        }


        //??????????
        ChatHistory.Add(m_InputWord.text);
        PlayerSave.SaveChatHistory(ChatHistory);
        //?????
        string _msg = m_InputWord.text;

        //????????
        m_ChatSettings.m_ChatModel.PostMsg(_msg, CallBack);

        m_InputWord.text = "";
        m_TextBack.text = "...";

        //?��????????
        SetAnimator("state", 1);
    }
    /// <summary>
    /// ?????????
    /// </summary>
    /// <param name="_postWord"></param>
    public void SendData(string _postWord)
    {
        if (_postWord.Equals(""))
            return;

        if (m_CreateVoiceMode)//????????????
        {
            CallBack(_postWord);
            m_InputWord.text = "";
            return;
        }


        //??????????
        ChatHistory.Add(_postWord);
        PlayerSave.SaveChatHistory(ChatHistory);
        //?????
        string _msg = _postWord;

        //????????
        m_ChatSettings.m_ChatModel.PostMsg(_msg, CallBack);

        m_InputWord.text = "";
        m_TextBack.text = "...";

        //?��????????
        SetAnimator("state", 1);
    }

    /// <summary>
    /// AI????????????
    /// </summary>
    /// <param name="_response"></param>
    private void CallBack(string _response)
    {
        _response = _response.Trim();
        m_TextBack.text = "";


        Debug.Log("callback: " + _response);

        //???????
        ChatHistory.Add(_response);
        PlayerSave.SaveChatHistory(ChatHistory);
        //if (!m_IsVoiceMode || m_ChatSettings.m_TextToSpeech == null)
        {
            //??????????????????
            StartTypeWords(_response);
            return;
        }


        //m_ChatSettings.m_TextToSpeech.Speak(_response, PlayVoice);
    }

    #endregion

    #region ????????
    /// <summary>
    /// ????????????????????????LLM
    /// </summary>
    [SerializeField] private bool m_AutoSend = true;
    /// <summary>
    /// ????????????
    /// </summary>
    [SerializeField] private Button m_VoiceInputBotton;
    /// <summary>
    /// ???????????
    /// </summary>
    [SerializeField] private Text m_VoiceBottonText;
    /// <summary>
    /// ???????????
    /// </summary>
    [SerializeField] private Text m_RecordTips;
    /// <summary>
    /// ????????????
    /// </summary>
    [SerializeField] private VoiceInputs m_VoiceInputs;
    /// <summary>
    /// ???????
    /// </summary>
    private void RegistButtonEvent()
    {
        if (m_VoiceInputBotton == null || m_VoiceInputBotton.GetComponent<EventTrigger>())
            return;

        EventTrigger _trigger = m_VoiceInputBotton.gameObject.AddComponent<EventTrigger>();

        //?????????��????
        EventTrigger.Entry _pointDown_entry = new EventTrigger.Entry();
        _pointDown_entry.eventID = EventTriggerType.PointerDown;
        _pointDown_entry.callback = new EventTrigger.TriggerEvent();

        //????????????
        EventTrigger.Entry _pointUp_entry = new EventTrigger.Entry();
        _pointUp_entry.eventID = EventTriggerType.PointerUp;
        _pointUp_entry.callback = new EventTrigger.TriggerEvent();

        //??????????
        _pointDown_entry.callback.AddListener(delegate { StartRecord(); });
        _pointUp_entry.callback.AddListener(delegate { StopRecord(); });

        _trigger.triggers.Add(_pointDown_entry);
        _trigger.triggers.Add(_pointUp_entry);
    }

    /// <summary>
    /// ??????
    /// </summary>
    public void StartRecord()
    {
        m_VoiceBottonText.text = "?????????...";
        m_VoiceInputs.StartRecordAudio();
    }
    /// <summary>
    /// ???????
    /// </summary>
    public void StopRecord()
    {
        m_VoiceBottonText.text = "??????????????";
        m_RecordTips.text = "????????????????...";
        m_VoiceInputs.StopRecordAudio(AcceptClip);
    }

    /// <summary>
    /// ???????????????
    /// </summary>
    /// <param name="_data"></param>
    private void AcceptData(byte[] _data)
    {
        // if (m_ChatSettings.m_SpeechToText == null)
        return;

        //m_ChatSettings.m_SpeechToText.SpeechToText(_data, DealingTextCallback);
    }

    /// <summary>
    /// ???????????????
    /// </summary>
    /// <param name="_data"></param>
    private void AcceptClip(AudioClip _audioClip)
    {
        // if (m_ChatSettings.m_SpeechToText == null)
        return;

        //m_ChatSettings.m_SpeechToText.SpeechToText(_audioClip, DealingTextCallback);
    }
    /// <summary>
    /// ????????????
    /// </summary>
    /// <param name="_msg"></param>
    private void DealingTextCallback(string _msg)
    {
        m_RecordTips.text = _msg;
        //StartCoroutine(SetTextVisible(m_RecordTips));
        //???????
        if (m_AutoSend)
        {
            SendData(_msg);
            return;
        }

        m_InputWord.text = _msg;
    }

    private IEnumerator SetTextVisible(Text _textbox)
    {
        yield return new WaitForSeconds(3f);
        _textbox.text = "";
    }

    #endregion

    #region ???????

    private void PlayVoice(AudioClip _clip, string _response)
    {
        m_AudioSource.clip = _clip;
        m_AudioSource.Play();
        Debug.Log("????????" + _clip.length);
        //??????????????????
        StartTypeWords(_response);
        //?��??????????
        SetAnimator("state", 2);
    }

    #endregion

    #region ???????????
    //??????????????
    [SerializeField] private float m_WordWaitTime = 0.2f;
    //??????????
    [SerializeField] private bool m_WriteState = false;

    /// <summary>
    /// ??????????
    /// </summary>
    /// <param name="_msg"></param>
    private void StartTypeWords(string _msg)
    {
        if (_msg == "")
            return;

        m_WriteState = true;
        m_TextBack.transform.parent.gameObject.SetActive(true);
        StartCoroutine(SetTextPerWord(_msg));
    }

    private IEnumerator SetTextPerWord(string _msg)
    {
        int currentPos = 0;
        while (m_WriteState)
        {
            yield return new WaitForSeconds(m_WordWaitTime);
            currentPos++;
            //?��???��???
            m_TextBack.text = _msg.Substring(0, currentPos);

            m_WriteState = currentPos < _msg.Length;

        }

        //��??��?��?y?��?
        SetAnimator("state", 0);
    }
    private IEnumerator SetTextPerWordOneByOne(string _msg)
    {
        int currentPos = 0;
        Debug.Log("_msg.Length" + _msg.Length);
        while (m_WriteState)
        {
            yield return new WaitForSeconds(m_WordWaitTime);
            currentPos++;
            //?????????????
            //if(currentPos % 10 == 0)
            //m_TextBack.text = _msg.Substring(0, currentPos);
            // Calculate valid start and length parameters for Substring
            int startIndex = Mathf.Max(0, currentPos - currentPos % 1);
            int length = 5;

            // Ensure we don't try to access beyond the string's length
            if (length > 0 && startIndex + length <= _msg.Length)
            {
                m_TextBack.text = _msg.Substring(startIndex, length);
            }
            else
            {
                m_TextBack.text = _msg.Substring(_msg.Length - length, length);
            }

            m_WriteState = currentPos < _msg.Length;

        }
        m_TextBack.transform.parent.gameObject.SetActive(false);
        //?��??????????
        SetAnimator("state", 0);
    }
    #endregion

    #region ???????
    //???????????
    [SerializeField] private static List<string> m_ChatHistory;
    //???????????????????
    [SerializeField] private List<GameObject> m_TempChatBox;
    //????????????
    [SerializeField] private GameObject m_HistoryPanel;
    //?????????????
    [SerializeField] private RectTransform m_rootTrans;
    //????????????
    [SerializeField] private ChatPrefab m_PostChatPrefab;
    //?????????????
    [SerializeField] private ChatPrefab m_RobotChatPrefab;
    //??????
    [SerializeField] private ScrollRect m_ScroTectObject;

    public static List<string> ChatHistory { get => m_ChatHistory; set => m_ChatHistory = value; }

    //??????????
    public void OpenAndGetHistory()
    {
        m_ChatPanel.SetActive(false);
        m_HistoryPanel.SetActive(true);

        ClearChatBox();
        StartCoroutine(GetHistoryChatInfo());
    }
    //????
    public void BackChatMode()
    {
        m_ChatPanel.SetActive(true);
        m_HistoryPanel.SetActive(false);
    }

    //???????????????
    private void ClearChatBox()
    {
        while (m_TempChatBox.Count != 0)
        {
            if (m_TempChatBox[0])
            {
                Destroy(m_TempChatBox[0].gameObject);
                m_TempChatBox.RemoveAt(0);
            }
        }
        m_TempChatBox.Clear();
    }

    //??????????��??
    private IEnumerator GetHistoryChatInfo()
    {

        yield return new WaitForEndOfFrame();

        for (int i = 0; i < ChatHistory.Count; i++)
        {
            if (i % 2 == 0)
            {
                ChatPrefab _sendChat = Instantiate(m_PostChatPrefab, m_rootTrans.transform);
                _sendChat.SetText(ChatHistory[i]);
                m_TempChatBox.Add(_sendChat.gameObject);
                continue;
            }

            ChatPrefab _reChat = Instantiate(m_RobotChatPrefab, m_rootTrans.transform);
            _reChat.SetText(ChatHistory[i]);
            m_TempChatBox.Add(_reChat.gameObject);
        }

        //??????????????
        LayoutRebuilder.ForceRebuildLayoutImmediate(m_rootTrans);
        StartCoroutine(TurnToLastLine());
    }

    private IEnumerator TurnToLastLine()
    {
        yield return new WaitForEndOfFrame();
        //???????????????
        m_ScroTectObject.verticalNormalizedPosition = 0;
    }


    #endregion

    private void SetAnimator(string _para, int _value)
    {
        if (m_Animator == null)
            return;

        m_Animator.SetInteger(_para, _value);
    }


    private void CloseChat()
    {
        DoDisappear();
        Timing.CallDelayed(2.2f, () =>
        {
           // RobotManager.Instance.CloseChat();
        });
        //m_ChatPanel.SetActive(false);
    }
    public void DoAppear()
    {
        //interfaceAnimManager.gameObject.SetActive(true);
        //interfaceAnimManager.startAppear();
        //playSound();
    }
    public void DoDisappear()
    {
        //if (interfaceAnimManager)
        {
        //    interfaceAnimManager.startDisappear();
        }
        //playSound();
    }
}
