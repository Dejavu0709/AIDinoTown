using System.Collections.Generic;
using UnityEngine;
using System;
using com.ootii.Messages;
using RobotGame.Persistence;
using LitJson;
namespace RobotGame
{
    /// <summary>
    /// High?level API for persistent player data. All calls are routed
    /// through an <see cref="ILocalSaveProvider"/> that handles encryption.
    /// </summary>
    public static class PlayerSave
    {
        private static ILocalSaveProvider provider = new PlayerPrefsSaveProvider();

        /* ---------- Fixed Keys / Prefixes ---------- */
        private const string NickKey = "PLAYER_NICK";
        private const string UserKey = "PLAYER_DATA";
      
        private const string ChatHistoryKey = "CHAT_HISTORY";
        private const string UnlockedCharactersKey = "UNLOCKED_CHARACTERS";
        private const string UnlockedSkillBooksKey = "UNLOCKED_SKILLBOOKS";
        private const string CurrentCharacterKey = "CURRENT_CHARACTER";
        private const string ChatNameKey = "CHAT_NAME";

        #region Generic Wrappers -----------------------------------------------

        public static void SetInt(string key, int value) { provider.SetInt(key, value); provider.Save(); }
        public static int GetInt(string key, int def = 0) => provider.GetInt(key, def);

        public static void SetFloat(string key, float value) { provider.SetFloat(key, value); provider.Save(); }
        public static float GetFloat(string key, float def = 0f) => provider.GetFloat(key, def);

        public static void SetString(string key, string value) { provider.SetString(key, value); provider.Save(); }
        public static string GetString(string key, string def = "") => provider.GetString(key, def);

        public static void ResetAccount() => provider.ResetAccount();

        public static event Action<string, int> OnCurrencyChanged;

        public static event Action<string, int> OnStatsChanged;

        public static event Action OnExpChanged;

        private static Dictionary<string, int> itemCache = new();
        #endregion

        #region Profile (Nick / Level / Exp) -----------------------------------

        public static void SetNickname(string nick) => SetString(NickKey, nick);
        public static string GetNickname() => GetString(NickKey, "");

        public static void SaveUserData(UserData userData)
        {
            provider.SetString(UserKey, JsonMapper.ToJson(userData));
            provider.Save();
        }

        public static UserData LoadUserData()
        {
            string userDataJson = provider.GetString(UserKey, "");
            Debug.Log($"userDataJson: {userDataJson}");
            if (string.IsNullOrEmpty(userDataJson))
            {
                UserData userData = new UserData();
                SaveUserData(userData);
                return userData;
            }
            return JsonMapper.ToObject<UserData>(userDataJson);
        }



     /*
                 public static void AddExp(int amount)
                 {
                     if (amount <= 0 || GameInstance.Instance == null)
                         return;

                     int[] expTable = GameInstance.Instance.expPerLevel;
                     int level = GetLevel();
                     int exp = GetCurrentExp();
                     int maxLevel = Mathf.Clamp(GameInstance.Instance.maxLevel, 1, expTable.Length);
                     exp += amount;

                     while (level < maxLevel)
                     {
                         int required = expTable[level - 1];

                         if (exp >= required)
                         {
                             exp -= required;
                             level++;
                             //todo: add level up animation
                             MessageDispatcher.SendMessage(null, EventDefine.LevelUp, level, 0);

                         }
                         else
                         {
                             break;
                         }
                     }

                     if (level >= maxLevel)
                     {
                         level = maxLevel;
                         exp = 0;
                     }

                     SetLevel(level);
                     SetCurrentExp(exp);
                     OnExpChanged?.Invoke();
                 }


                 #endregion

                 #region Stats (dynamic) -------------------------------------------------

                 public static void SetStat(string statId, int delta, int max)
                 {
                     int current = GetInt(StatPrefix + statId, max);
                     int newValue = Mathf.Clamp(current + delta, 0, max);
                     SetInt(StatPrefix + statId, newValue);
                     OnStatsChanged?.Invoke(statId, newValue);
                 }

                 public static int GetStat(string statName, int def = 0)
                 {
                     return GetInt(StatPrefix + statName, def);
                 }

                 #endregion

                 #region Coins -----------------------------------------------------------
                 public static void AddCoin(string coinId, int amount)
                 {
                     if (amount <= 0) return;

                     int current = GetCoin(coinId);
                     int newVal = Mathf.Max(0, current + amount);
                     if (newVal == current) return; 

                     SetInt(CoinPrefix + coinId, newVal);
                     OnCurrencyChanged?.Invoke(coinId, newVal);
                 }



                 public static void RemoveCoin(string coinId, int amount)
                 {
                     int newVal = Mathf.Max(0, GetCoin(coinId) - amount);
                     SetInt(CoinPrefix + coinId, newVal);
                     OnCurrencyChanged?.Invoke(coinId, newVal);
                 }

                 public static int GetCoin(string coinId) =>
                     GetInt(CoinPrefix + coinId, 0);

                 #endregion

                 #region Scores ----------------------------------------------------------

                 public static void SetBestScore(string mode, int score)
                 {
                     if (score > GetBestScore(mode))
                         SetInt(ScorePrefix + mode, score);
                 }

                 public static int GetBestScore(string mode) =>
                     GetInt(ScorePrefix + mode, 0);

                 #endregion

                 #region Items -----------------------------------------------------------

                 #region Items (with quantity) -------------------------------------------

                 public static void AddItems(Dictionary<string, int> itemsToAdd)
                 {
                     LoadItemsToCache();

                     foreach (var kvp in itemsToAdd)
                     {
                         if (itemCache.ContainsKey(kvp.Key))
                             itemCache[kvp.Key] += kvp.Value;
                         else
                             itemCache[kvp.Key] = kvp.Value;
                     }

                     SaveItemCache();
                 }

                 public static void RemoveItems(Dictionary<string, int> itemsToRemove)
                 {
                     LoadItemsToCache();

                     foreach (var kvp in itemsToRemove)
                     {
                         if (itemCache.ContainsKey(kvp.Key))
                         {
                             itemCache[kvp.Key] -= kvp.Value;
                             if (itemCache[kvp.Key] <= 0)
                                 itemCache.Remove(kvp.Key);
                         }
                     }

                     SaveItemCache();
                 }

                 public static int GetItemCount(string itemId)
                 {
                     LoadItemsToCache();
                     return itemCache.TryGetValue(itemId, out int qty) ? qty : 0;
                 }

                 public static Dictionary<string, int> GetAllItems()
                 {
                     LoadItemsToCache();
                     return new Dictionary<string, int>(itemCache);
                 }

                 private static void LoadItemsToCache()
                 {
                     if (itemCache.Count > 0) return;

                     string json = GetString(ItemsKey, "");
                     itemCache.Clear();

                     if (string.IsNullOrEmpty(json)) return;

                     try
                     {
                         ItemListWrapper wrapper = JsonUtility.FromJson<ItemListWrapper>(json);
                         foreach (var entry in wrapper.items)
                         {
                             if (!string.IsNullOrEmpty(entry.id))
                                 itemCache[entry.id] = entry.amount;
                         }
                     }
                     catch
                     {
                         itemCache.Clear();
                     }
                 }

                 private static void SaveItemCache()
                 {
                     ItemListWrapper wrapper = new();
                     foreach (var pair in itemCache)
                     {
                         wrapper.items.Add(new ItemEntry { id = pair.Key, amount = pair.Value });
                     }

                     string json = JsonUtility.ToJson(wrapper);
                     SetString(ItemsKey, json);
                 }

                 private static ItemDictWrapper LoadItemDict()
                 {
                     string json = GetString(ItemsKey, "");
                     if (string.IsNullOrEmpty(json)) return new ItemDictWrapper();
                     try
                     {
                         return JsonUtility.FromJson<ItemDictWrapper>(json);
                     }
                     catch
                     {
                         return new ItemDictWrapper();
                     }
                 }

                 private static void SaveItemDict(ItemDictWrapper data)
                 {
                     string json = JsonUtility.ToJson(data);
                     SetString(ItemsKey, json);
                 }

                 #endregion

                 #region Selected Food ---------------------------------------------------------

                 /// <summary>Sets the selected food ID if player owns at least one.</summary>
                 public static void SetSelectedFood(string foodId)
                 {
                     if (GetItemCount(foodId) > 0)
                         SetString(SelectedFoodKey, foodId);
                     else
                         Debug.LogWarning($"Tried to select food '{foodId}' but quantity is 0.");
                 }

                 /// <summary>Gets the currently selected food ID.</summary>
                 public static string GetSelectedFood()
                 {
                     return GetString(SelectedFoodKey, "");
                 }

                 /// <summary>Clears the selected food.</summary>
                 public static void ClearSelectedFood()
                 {
                     SetString(SelectedFoodKey, "");
                 }

                 #endregion

                 #region Selected Clothes ---------------------------------------------------------

                 public static void SetSelectedClothes(string slotType, string ClothId)
                 {
                     SetString(SelectedClothesKey + slotType, ClothId);
                 }

                 public static string GetSelectedClothes(string slotType)
                 {
                     return GetString(SelectedClothesKey + slotType, "");
                 }

                 public static void ClearSelectedClothes(string slotType)
                 {
                     SetString(SelectedClothesKey + slotType, "");
                 }

                 public static void SetPetColor(Color color)
                 {
                     string colorHex = ColorUtility.ToHtmlStringRGBA(color);
                     SetString(PetColorKey, colorHex);
                 }

                 public static Color GetPetColor()
                 {
                     Color fallback = Color.white;
                     string hex = GetString(PetColorKey, "");
                     if (ColorUtility.TryParseHtmlString("#" + hex, out Color result))
                         return result;
                     return fallback;
                 }

                 public static void ClearPetColor()
                 {
                     SetString(PetColorKey, "");
                 }
                 #endregion

                 #region Stat Decay System ---------------------------------------------------------
                 /// <summary>Saves an UTC timestamp so offline decay can be rebuilt later.</summary>
                 public static void SaveLastTimestamp(DateTime utcTime)
                 {
                     SetString(LastStatUpdateKey, utcTime.Ticks.ToString());
                 }

                 public static string GetLastTimestamp()
                 {
                    return GetString(LastStatUpdateKey, "0");
                 }

                 // Sleep system constants
                 private const string PetStateKey = "PET_STATE";
                 private const string SleepStartTimeKey = "SLEEP_START_TIME";
                 private const string SleepDurationKey = "SLEEP_DURATION";

                 /// <summary>Sets the current state of the pet (normal, sleeping, etc.)</summary>
                 public static void SetPetState(string state)
                 {
                     SetString(PetStateKey, state);
                 }

                 /// <summary>Gets the current state of the pet</summary>
                 public static string GetPetState()
                 {
                     return GetString(PetStateKey, "normal");
                 }

                 /// <summary>Sets the UTC timestamp when the pet started sleeping</summary>
                 public static void SetSleepStartTime(DateTime startTime)
                 {
                     SetString(SleepStartTimeKey, startTime.Ticks.ToString());
                 }

                 /// <summary>Gets the UTC timestamp when the pet started sleeping</summary>
                 public static DateTime GetSleepStartTime()
                 {
                     string ticks = GetString(SleepStartTimeKey, "0");
                     return new DateTime(long.Parse(ticks), DateTimeKind.Utc);
                 }

                 /// <summary>Sets the sleep duration in seconds</summary>
                 public static void SetSleepDuration(int durationSeconds)
                 {
                     SetInt(SleepDurationKey, durationSeconds);
                 }

                 /// <summary>Gets the sleep duration in seconds</summary>
                 public static int GetSleepDuration()
                 {
                     return GetInt(SleepDurationKey, 0);
                 }

                 /// <summary>Checks if the pet is currently sleeping</summary>
                 public static bool IsPetSleeping()
                 {
                     if (GetPetState() != "sleeping") return false;

                     DateTime sleepStart = GetSleepStartTime();
                     int duration = GetSleepDuration();

                     // Check if sleep duration has elapsed
                     if ((DateTime.UtcNow - sleepStart).TotalSeconds >= duration)
                     {
                         // Sleep time is over, reset to normal state
                         SetPetState("normal");
                         return false;
                     }

                     return true;
                 }

                 /// <summary>Gets the remaining sleep time in seconds</summary>
                 public static int GetRemainingSleepTime()
                 {
                     if (GetPetState() != "sleeping") return 0;

                     DateTime sleepStart = GetSleepStartTime();
                     int duration = GetSleepDuration();
                     int elapsed = (int)(DateTime.UtcNow - sleepStart).TotalSeconds;

                     return Math.Max(0, duration - elapsed);
                 }

                 /// <summary>Adds delta to a stat, clamped between 0 and max value</summary>
                 public static void ApplyStatDelta(string statId, int delta)
                 {
                     int selectedPetId = GetPet();
                     int max = GameInstance.Instance.GetMaxStatValueById(selectedPetId, statId);
                     SetStat(statId, delta, max);
                 }

                 #endregion
         */

        public static void SaveChatHistory(List<string> chatHistory)
        {
            SetString(ChatHistoryKey, JsonMapper.ToJson(chatHistory));
            Debug.Log("ChatHistory saved: " + JsonMapper.ToJson(chatHistory));
        }

        public static List<string> GetChatHistory()
        {
            string json = GetString(ChatHistoryKey, "[]");
            Debug.Log("ChatHistory loaded: " + json);
            if(json == "[]") return new List<string>();
            return JsonMapper.ToObject<List<string>>(json);
        }

        // ChatDeepSeek - Character Management
        public static void SaveUnlockedCharacters(List<string> characters)
        {
            SetString(UnlockedCharactersKey, JsonMapper.ToJson(characters));
            Debug.Log("Unlocked characters saved: " + JsonMapper.ToJson(characters));
        }

        public static List<string> GetUnlockedCharacters()
        {
            string json = GetString(UnlockedCharactersKey, "[]");
            if(json == "[]") return new List<string>() { "ÓÄÄ¬" }; // Default character is always unlocked
            return JsonMapper.ToObject<List<string>>(json);
        }

        public static bool IsCharacterUnlocked(string character)
        {
            List<string> unlockedCharacters = GetUnlockedCharacters();
            return unlockedCharacters.Contains(character);
        }

        public static void UnlockCharacter(string character)
        {
            List<string> unlockedCharacters = GetUnlockedCharacters();
            if (!unlockedCharacters.Contains(character))
            {
                unlockedCharacters.Add(character);
                SaveUnlockedCharacters(unlockedCharacters);
            }
        }

        // ChatDeepSeek - Skill Book Management
        public static void SaveUnlockedSkillBooks(List<string> skillBooks)
        {
            SetString(UnlockedSkillBooksKey, JsonMapper.ToJson(skillBooks));
            Debug.Log("Unlocked skill books saved: " + JsonMapper.ToJson(skillBooks));
        }

        public static List<string> GetUnlockedSkillBooks()
        {
            string json = GetString(UnlockedSkillBooksKey, "[]");
            return JsonMapper.ToObject<List<string>>(json);
        }

        public static bool IsSkillBookUnlocked(string skillBook)
        {
            List<string> unlockedSkillBooks = GetUnlockedSkillBooks();
            return unlockedSkillBooks.Contains(skillBook);
        }

        public static void UnlockSkillBook(string skillBook)
        {
            List<string> unlockedSkillBooks = GetUnlockedSkillBooks();
            if (!unlockedSkillBooks.Contains(skillBook))
            {
                unlockedSkillBooks.Add(skillBook);
                SaveUnlockedSkillBooks(unlockedSkillBooks);
            }
        }

        // ChatDeepSeek - Current Character and Name
        public static void SetCurrentCharacter(string character)
        {
            SetString(CurrentCharacterKey, character);
        }

        public static string GetCurrentCharacter()
        {
            return GetString(CurrentCharacterKey, "ÓÄÄ¬"); // Default to ÓÄÄ¬ if not set
        }

        public static void SetChatName(string name)
        {
            SetString(ChatNameKey, name);
        }

        public static string GetChatName()
        {
            return GetString(ChatNameKey, "July"); // Default to July if not set
        }


        [System.Serializable] private class ItemsWrapper { public List<string> items; }
        [System.Serializable]
        private class ItemDictWrapper
        {
            public Dictionary<string, int> items = new();
        }
        #endregion
        [System.Serializable]
        private class ItemEntry
        {
            public string id;
            public int amount;
        }

        [System.Serializable]
        private class ItemListWrapper
        {
            public List<ItemEntry> items = new();
        }

        #region Provider Swap ---------------------------------------------------

        /// <summary>Replaces the storage backend (e.g., cloud provider).</summary>
        public static void SetProvider(ILocalSaveProvider custom) => provider = custom;

        #endregion
    }
}
