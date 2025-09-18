using JetBrains.Annotations;
using Newtonsoft.Json;
using NexgenDragon;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEngine;
[System.Serializable]
public class UserData
{
    public string Name;
    public string ID;
    public RobotData CurRobot;
    private int coin;
    private int diamond;
    private List<InventoryItemData> inventoryDatas;
    public List<InventoryItemData> InventoryDatas { get => inventoryDatas; set => inventoryDatas = value; }
    //public Career currentCareer;
   // public List<Career> careerHistoryList;
   // public int CurCareerId;
    public UserData()
    {
        Name = "";
        ID = Guid.NewGuid().ToString();
        CurRobot = new RobotData();
        Coin = 0;
        Diamond = 0;
        inventoryDatas = new List<InventoryItemData>();
        //currentCareer = null;
    }

    public int Diamond { get => diamond; set => diamond = value; }
    public int Coin { get => coin;set => coin = value;}
}

[System.Serializable]
public class RobotData
{
    public int ID;
    public string Name;
    public int Energy;
    public int Exp;
    public int lucky;
    public int strength;
    public int agility;
    public int constitution;
    public int intelligence;
    /// <summary>
    /// 所有激活的MiniGame
    /// </summary>
    public List<int> MiniGameModule;

    public RobotData()
    {
        ID = 1;
        Name = "Robot";
        Energy = 0;
        Exp = 0;
        MiniGameModule = new List<int>();
    }
}
[System.Serializable]
public class InventoryItemData
{
    public int ID;
    public int Quantity;
}
