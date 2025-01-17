using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerData : GameDataBase
{
    public int PID;
    public int PHealth ;
    public int PMaxHealth ;
    public int PHandCard ;
    public int PMoney;
    public int PLevel;
    
    public PlayerRoundData PlayerRoundData;
}


public class PlayerRoundData 
{ 
    
}

public class MonsterData 
{
    public int MHealth;
    public int MMaxHealth;
    public string args;
    public string Name="Monster";
}
public class MapData : GameDataBase
{
    public int Process;
    public string SelectMap;
    public bool isChangeMap;
}

