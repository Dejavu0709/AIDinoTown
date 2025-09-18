using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PetCareGame;

/// <summary>
/// Test script for ChatDeepSeek features
/// </summary>
public class ChatDeepSeekTest : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ChatDeepSeek chatDeepSeek;
    
    [Header("Character UI")]
    [SerializeField] private Dropdown characterDropdown;
    [SerializeField] private Button unlockCharacterButton;
    [SerializeField] private Button setCharacterButton;
    [SerializeField] private Text currentCharacterText;
    
    [Header("Skill Book UI")]
    [SerializeField] private Dropdown skillBookDropdown;
    [SerializeField] private Button unlockSkillBookButton;
    [SerializeField] private Text unlockedSkillBooksText;
    
    [Header("Name UI")]
    [SerializeField] private InputField nameInputField;
    [SerializeField] private Button setNameButton;
    [SerializeField] private Text currentNameText;
    
    private void Start()
    {
        if (chatDeepSeek == null)
        {
            Debug.LogError("ChatDeepSeek reference is missing!");
            return;
        }
        
        InitializeUI();
        UpdateUI();
    }
    
    private void InitializeUI()
    {
        // Setup character dropdown
        characterDropdown.ClearOptions();
        List<string> characterOptions = new List<string>();
        foreach (string character in chatDeepSeek.m_Characters)
        {
            characterOptions.Add(character);
        }
        characterDropdown.AddOptions(characterOptions);
        
        // Setup skill book dropdown
        skillBookDropdown.ClearOptions();
        List<string> skillBookOptions = new List<string>();
        foreach (string skillBook in chatDeepSeek.m_SkillBook)
        {
            skillBookOptions.Add(skillBook);
        }
        skillBookDropdown.AddOptions(skillBookOptions);
        
        // Setup button listeners
        unlockCharacterButton.onClick.AddListener(UnlockSelectedCharacter);
        setCharacterButton.onClick.AddListener(SetSelectedCharacter);
        unlockSkillBookButton.onClick.AddListener(UnlockSelectedSkillBook);
        setNameButton.onClick.AddListener(SetName);
    }
    
    private void UpdateUI()
    {
        // Update character info
        currentCharacterText.text = "Current Character: " + chatDeepSeek.m_CurCharacter;
        
        // Update skill books info
        string skillBooksText = "Unlocked Skill Books: ";
        List<string> unlockedSkillBooks = PlayerSave.GetUnlockedSkillBooks();
        if (unlockedSkillBooks.Count == 0)
        {
            skillBooksText += "None";
        }
        else
        {
            skillBooksText += string.Join(", ", unlockedSkillBooks);
        }
        unlockedSkillBooksText.text = skillBooksText;
        
        // Update name info
        currentNameText.text = "Current Name: " + chatDeepSeek.m_Name;
    }
    
    private void UnlockSelectedCharacter()
    {
        int selectedIndex = characterDropdown.value;
        string selectedCharacter = characterDropdown.options[selectedIndex].text;
        Debug.Log("Attempting to unlock character: " + selectedCharacter);
        
        chatDeepSeek.PurchaseAndUnlockCharacter(selectedIndex, (success) => {
            if (success)
            {
                Debug.Log("Character unlocked successfully: " + selectedCharacter);
                UpdateUI();
            }
            else
            {
                Debug.LogError("Failed to unlock character: " + selectedCharacter);
            }
        });
    }
    
    private void SetSelectedCharacter()
    {
        int selectedIndex = characterDropdown.value;
        string selectedCharacter = characterDropdown.options[selectedIndex].text;
        Debug.Log("Attempting to set character: " + selectedCharacter);
        
        bool success = chatDeepSeek.SetCurrentCharacter(selectedIndex);
        if (success)
        {
            Debug.Log("Character set successfully: " + selectedCharacter);
            UpdateUI();
        }
        else
        {
            Debug.LogWarning("Failed to set character (not unlocked?): " + selectedCharacter);
        }
    }
    
    private void UnlockSelectedSkillBook()
    {
        int selectedIndex = skillBookDropdown.value;
        string selectedSkillBook = skillBookDropdown.options[selectedIndex].text;
        Debug.Log("Attempting to unlock skill book: " + selectedSkillBook);
        
        chatDeepSeek.PurchaseAndUnlockSkillBook(selectedIndex, (success) => {
            if (success)
            {
                Debug.Log("Skill book unlocked successfully: " + selectedSkillBook);
                UpdateUI();
            }
            else
            {
                Debug.LogError("Failed to unlock skill book: " + selectedSkillBook);
            }
        });
    }
    
    private void SetName()
    {
        string newName = nameInputField.text;
        if (string.IsNullOrEmpty(newName))
        {
            Debug.LogWarning("Name cannot be empty");
            return;
        }
        
        Debug.Log("Setting name to: " + newName);
        chatDeepSeek.SetName(newName);
        UpdateUI();
    }
}
