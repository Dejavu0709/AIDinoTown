using System;
using System.Collections;
using System.Collections.Generic;
using I2.Loc;
using UnityEngine;
using UnityEngine.Networking;
using LitJson;
public class ChatDeepSeek : LLM
{

	public ChatDeepSeek()
	{
		url = "https://api.deepseek.com/v1";


		//url = "https://api.deepseek.com/v1/chat/completions";
	}

	/// <summary>
	/// api key
	/// </summary>
	[SerializeField] private string api_key;
	/// <summary>
	/// AI Settings
	/// </summary>
	[SerializeField]
	private string m_SystemSetting = ""
;
	/// <summary>
	/// Model Name
	/// </summary>
	public string m_ModelName = "deepseek-reasoner";
	static string SessionId = ""; // Initial session ID


	public List<string> m_SkillBook = new List<string>() { "Mathematics", "English", "Physics", "Chemistry", "Biology", "Geography", "History", "Politics", "Music", "Art", "Physical Education", "Information Technology", "Astronomy", "Medicine", "Programming" };

	public List<string> m_Characters = new List<string>() { "Humorous", "Gentle", "Sarcastic", "Funny", "Cool", "Cute" };
	public List<string> m_Motions = new List<string>() { "Happy", "Sad", "Surprised", "Neutral", "Playful" };

	public string m_CurCharacter = "Humorous";
	public string m_CurMotion = "Neutral";
	public string m_Name = "July";
	public List<string> m_SkillBookLearned = new List<string>();

	// Product IDs for IAP
	private const string CHARACTER_PRODUCT_PREFIX = "character_";
	private const string SKILLBOOK_PRODUCT_PREFIX = "skillbook_";

	private void Start()
	{
		//At runtime, add AI settings
		//m_DataList.Add(new SendData("system", m_SystemSetting));
		
		// Load saved data
		LoadSavedData();
	}

	/// <summary>
	/// Load saved data from PlayerSave
	/// </summary>
	private void LoadSavedData()
	{
		// Load current character
		//m_CurCharacter = PetCareGame.PlayerSave.GetCurrentCharacter();
		
		// Load name
		//m_Name = PetCareGame.PlayerSave.GetChatName();
		
		// Load unlocked skill books
		//m_SkillBookLearned = PetCareGame.PlayerSave.GetUnlockedSkillBooks();
	}

	/// <summary>
	/// Purchase and unlock a character
	/// </summary>
	/// <param name="characterIndex">Index of character in m_Characters list</param>
	/// <param name="callback">Callback when purchase completes</param>
	public void PurchaseAndUnlockCharacter(int characterIndex, System.Action<bool> callback = null)
	{
		/*
		// Check if index is valid
		if (characterIndex < 0 || characterIndex >= m_Characters.Count)
		{
			Debug.LogError($"Character index {characterIndex} is out of range");
			callback?.Invoke(false);
			return;
		}

		string character = m_Characters[characterIndex];

		// Check if already unlocked
		if (PetCareGame.PlayerSave.IsCharacterUnlocked(character))
		{
			Debug.Log($"Character {character} is already unlocked");
			callback?.Invoke(true);
			return;
		}

		// Create product ID for IAP
		string productId = CHARACTER_PRODUCT_PREFIX + character;

		// Setup purchase callback
		System.Action purchaseCallback = () => {
			// Unlock the character
			PetCareGame.PlayerSave.UnlockCharacter(character);
			Debug.Log($"Character {character} unlocked successfully");
			callback?.Invoke(true);
		};

		// Initiate purchase through MonetizationManager
		PetCareGame.Monetization.MonetizationManager.Instance.Buy(productId);
		
		// For testing/development, simulate successful purchase
		#if UNITY_EDITOR
		purchaseCallback();
		#endif
		*/
	}

	/// <summary>
	/// Set current character (must be unlocked)
	/// </summary>
	/// <param name="characterIndex">Index of character in m_Characters list</param>
	/// <returns>True if successful, false if character is not unlocked</returns>
	public bool SetCurrentCharacter(int characterIndex)
	{
		/*
		// Check if index is valid
		if (characterIndex < 0 || characterIndex >= m_Characters.Count)
		{
			Debug.LogError($"Character index {characterIndex} is out of range");
			return false;
		}

		string character = m_Characters[characterIndex];

		// Check if character is unlocked
		if (!PetCareGame.PlayerSave.IsCharacterUnlocked(character))
		{
			Debug.LogWarning($"Character {character} is not unlocked yet");
			return false;
		}

		// Set current character
		m_CurCharacter = character;
		PetCareGame.PlayerSave.SetCurrentCharacter(character);
		Debug.Log($"Current character set to {character}");
		*/
		return true;
	}

	/// <summary>
	/// Purchase and unlock a skill book
	/// </summary>
	/// <param name="skillBookIndex">Index of skill book in m_SkillBook list</param>
	/// <param name="callback">Callback when purchase completes</param>
	public void PurchaseAndUnlockSkillBook(int skillBookIndex, System.Action<bool> callback = null)
	{
		/*
		// Check if index is valid
		if (skillBookIndex < 0 || skillBookIndex >= m_SkillBook.Count)
		{
			Debug.LogError($"Skill book index {skillBookIndex} is out of range");
			callback?.Invoke(false);
			return;
		}

		string skillBook = m_SkillBook[skillBookIndex];

		// Check if already unlocked
		if (PetCareGame.PlayerSave.IsSkillBookUnlocked(skillBook))
		{
			Debug.Log($"Skill book {skillBook} is already unlocked");
			callback?.Invoke(true);
			return;
		}

		// Create product ID for IAP
		string productId = SKILLBOOK_PRODUCT_PREFIX + skillBook;

		// Setup purchase callback
		System.Action purchaseCallback = () => {
			// Unlock the skill book
			PetCareGame.PlayerSave.UnlockSkillBook(skillBook);
			
			// Add to learned skill books
			if (!m_SkillBookLearned.Contains(skillBook))
			{
				m_SkillBookLearned.Add(skillBook);
			}
			
			Debug.Log($"Skill book {skillBook} unlocked successfully");
			callback?.Invoke(true);
		};

		// Initiate purchase through MonetizationManager
		PetCareGame.Monetization.MonetizationManager.Instance.Buy(productId);
		
		// For testing/development, simulate successful purchase
		#if UNITY_EDITOR
		purchaseCallback();
		#endif
		*/
	}

	/// <summary>
	/// Set the AI assistant's name
	/// </summary>
	/// <param name="name">Name to set</param>
	public void SetName(string name)
	{
		if (string.IsNullOrEmpty(name))
		{
			Debug.LogWarning("Name cannot be empty");
			return;
		}

		m_Name = name;
		//PetCareGame.PlayerSave.SetChatName(name);
		Debug.Log($"Name set to {name}");
	}

	private bool CreateSessionId()
	{
		if (string.IsNullOrEmpty(SessionId))
		{
			// 首次请求生成新会话ID
			SessionId = Guid.NewGuid().ToString("N");
			return true;
		}
		return false;
	}
	/// <summary>
	/// Send Message
	/// </summary>
	/// <returns></returns>
	public override void PostMsg(string _msg,Action<string> _callback) {


		//bool isFirst = CreateSessionId();
		if(m_DataList.Count > 0)
			m_DataList.RemoveAt(0);
			

		{
			m_DataList.Insert(0, new SendData("system", string.Format(m_SystemSetting, m_Name, JsonMapper.ToJson(m_Characters), m_CurCharacter, JsonMapper.ToJson(m_SkillBook), JsonMapper.ToJson(m_SkillBookLearned), JsonMapper.ToJson(m_Motions), m_CurMotion)));
			//上下文条数设�????
			CheckHistory();
		}

		//提示词处�????
		//string message = "当前回答的语言�????" + LanguageManager.LanguageManager.Instance.GetCurrentLanguage() +
		//		" 接下来是我的提问�????" + _msg;
		Debug.Log("Current language: " + LocalizationManager.CurrentLanguage);
		string message = "当前回答的语言?" + LocalizationManager.CurrentLanguage +
				" 接下来是我的提问?" + _msg;
		//string message = " 接下来是我的提问�????" + _msg;
		//缓存发送的信息列表
		m_DataList.Add(new SendData("user", message));
		StartCoroutine(Request(_msg, _callback));
    }


	/// <summary>
	/// Call Interface
	/// </summary>
	/// <param name="_postWord"></param>
	/// <param name="_callback"></param>
	/// <returns></returns>
	public override IEnumerator Request(string _postWord, System.Action<string> _callback)
	{
		stopwatch.Restart();
		using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
		{

			
			PostData _postData;
		
			
			_postData = new PostData
			{
					model = m_ModelName,
					messages = m_DataList
			};
			

			string _jsonText = JsonUtility.ToJson(_postData);
			byte[] data = System.Text.Encoding.UTF8.GetBytes(_jsonText);
			request.uploadHandler = (UploadHandler)new UploadHandlerRaw(data);
			request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
			Debug.Log("sessionId:" + SessionId);
			request.SetRequestHeader("Content-Type", "application/json");
			request.SetRequestHeader("Authorization", string.Format("Bearer {0}", api_key));
			//request.SetRequestHeader("X-Session-Id", SessionId);
			//request.SetRequestHeader("Session-Id", SessionId);
			//request.SetRequestHeader("DeepSeek_SessionId", SessionId);
			//request.SetRequestHeader("x-ds-trace-id", SessionId);
			yield return request.SendWebRequest();

			if (request.responseCode == 200)
			{
				string _msgBack = request.downloadHandler.text;
				MessageBack _textback = JsonUtility.FromJson<MessageBack>(_msgBack);
				if (_textback != null && _textback.choices.Count > 0)
				{

					string _backMsg = _textback.choices[0].message.content;

					// Parse the response to extract content and emotion
					string content = "";
					string emotion = "";
					ParseResponse(_backMsg, out content, out emotion);

					// If emotion is valid, update current motion
					if (!string.IsNullOrEmpty(emotion) && m_Motions.Contains(emotion))
					{
						m_CurMotion = emotion;
						/*
						var anim = UIMainMenu.Instance.GetPetModel();
						if(anim != null)
						anim.PlayByMotion(m_CurMotion);
						*/
						Debug.Log($"Emotion detected: {emotion}, setting as current motion");
					}

					// Use the parsed content as the response
					_backMsg = content;

					//Add record
					m_DataList.Add(new SendData("assistant", _backMsg));
					Debug.Log("chatDeepSeek callback: " + _backMsg);
					_callback(_backMsg);
				}
				/*
				var newSessionId = request.GetResponseHeader("x-ds-trace-id");
				Debug.Log("newSessionId:" + newSessionId);
				
				var newSessionId2 = request.GetResponseHeader("DeepSeek_SessionId");
				Debug.Log("newSessionId2:" + newSessionId2);
				var Header = request.GetResponseHeaders();
				Debug.Log("Header:" + Header);
				foreach(var item in Header)
				{
					Debug.Log("item:" + item.Key + ":" + item.Value);
				}


				if (!string.IsNullOrEmpty(newSessionId))
				{
					//SessionId = newSessionId;
				}
				*/
			}	
			else
			{
				string _msgBack = request.downloadHandler.text;
				Debug.LogError(_msgBack);
			}

			stopwatch.Stop();
			Debug.Log("DeepSeek time cost: " + stopwatch.Elapsed.TotalSeconds);
		}
	}
// Add this method to your ChatDeepSeek class
private void ParseResponse(string response, out string content, out string emotion)
{
    content = response;
    emotion = "";
    
    try
    {
        Debug.Log("response:" + response);
        
        // First, remove thinking sections if they exist
        string cleanedResponse = RemoveThinkingSections(response);
        
        // Find the last (actual) <reply> tag to avoid thinking content
        int lastReplyStartIndex = cleanedResponse.LastIndexOf("<reply>");
        
        if (lastReplyStartIndex >= 0)
        {
            int replyEndIndex = cleanedResponse.IndexOf("</reply>", lastReplyStartIndex + 7);
            
            if (replyEndIndex >= 0)
            {
                content = cleanedResponse.Substring(lastReplyStartIndex + 7, replyEndIndex - lastReplyStartIndex - 7).Trim();
                Debug.Log("Extracted content: " + content);
            }
        }

        // Extract emotion between <motion> and </motion>
        int motionStartIndex = cleanedResponse.LastIndexOf("<motion>");
        
        if (motionStartIndex >= 0)
        {
            int motionEndIndex = cleanedResponse.IndexOf("</motion>", motionStartIndex + 8);
            
            if (motionEndIndex >= 0)
            {
                emotion = cleanedResponse.Substring(motionStartIndex + 8, motionEndIndex - motionStartIndex - 8).Trim();
                Debug.Log("Extracted emotion: " + emotion);
            }
        }
    }
    catch (System.Exception e)
    {
        Debug.LogError("Error parsing response: " + e.Message);
        content = response; // Fallback to original response
        emotion = "";
    }
}

private string RemoveThinkingSections(string response)
{
    string result = response;
    
    try
    {
        // Remove all <thinking>...</thinking> sections
        while (true)
        {
            int thinkingStart = result.IndexOf("<thinking>");
            if (thinkingStart == -1) break;
            
            int thinkingEnd = result.IndexOf("</thinking>", thinkingStart);
            if (thinkingEnd == -1) break;
            
            // Remove the entire thinking section including tags
            result = result.Remove(thinkingStart, thinkingEnd - thinkingStart + 11);
        }
        
        Debug.Log("Cleaned response (thinking removed): " + result);
    }
    catch (System.Exception e)
    {
        Debug.LogError("Error removing thinking sections: " + e.Message);
        return response; // Return original if error occurs
    }
    
    return result.Trim();
}
	#region Data Packets

	[Serializable]
	public class PostData
	{
		public string model;
		public List<SendData> messages;
		public bool stream=false;
	}


	[Serializable]
	public class MessageBack
	{
		public string id;
		public string created;
		public string model;
		public List<MessageBody> choices;
	}
	[Serializable]
	public class MessageBody
	{
		public Message message;
		public string finish_reason;
		public string index;
	}
	[Serializable]
	public class Message
	{
		public string role;
		public string content;
	}

	#endregion


}
