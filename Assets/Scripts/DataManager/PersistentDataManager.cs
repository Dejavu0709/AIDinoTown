using System.Collections;
using System.Collections.Generic;
using com.ootii.Messages;
using RobotGame;
using UnityEngine;

public class PersistentDataManager
{
    public static UserData userData;

    public static void Init()
    {
        userData = PlayerSave.LoadUserData();
       
        userData.Coin = 100;
        PersistentDataManager.InitializeStartingResources();
        //MessageDispatcher.AddListener("ActionFinish", ActionFinish, true);
    }

    public static void Save()
    {
        PlayerSave.SaveUserData(userData);
    }


    public static void ChangeDiamond(int value)
    {
        userData.Diamond += value;
        if(userData.Diamond < 0)
        {
            userData.Diamond = 0;
        }
        Save();
    }
    
    public static void AddDiamond(int value)
    {
        if (value > 0)
        {
            userData.Diamond += value;
            Save();
        }
    }
    
    // Initialize user with some starting diamonds if they have none
    public static void InitializeStartingResources()
    {

        userData.Diamond = 111500; // Give 100 starting diamonds
        Save();

    }
}
