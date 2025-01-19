using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QxFramework;
using QxFramework.Core;


public class DataManager : LogicModuleBase,IDataManager
{
    #region 变量的初始化
    private PlayerData _playerData;
    private readonly string saveName = "PlayerSave";

    #endregion

    #region PlayerData的定义（属性）
    public PlayerData PlayerData
    {
        get
        {
            _playerData = QXData.Instance.Get<PlayerData>();
            if (_playerData == null)
            {
                // 处理 _playerData 为 null 的情况
                Debug.LogError("DataManager:_playerData is NULL");
                _playerData = new PlayerData(); 
            }
            return _playerData;
        }
    }
    #endregion


    #region 存档的初始化、加载

    /// <summary>
    ///存档初始化,会在流程被初始化的时候调用
    /// </summary>
    public override void Init()
    {
        //继承的Init
        base.Init();

        if (!RegisterData(out _playerData))
        {
            if (PlayerData == null)
            {
                Debug.LogError("DataManager:PlayerData is NULL");
            }
            PlayerData.PID = 1;
            PlayerData.PMoney = 0;
            PlayerData.PHealth = 1;
            PlayerData.PMaxHealth = 1;
            PlayerData.PLevel = 0;


            Debug.Log("DataManager:未找到Player存档，已启动初始化部分。");


        }

    }

    /// <summary>
    /// 存档方法
    /// </summary>
    public void SaveData()
    {
        QXData.Instance.SaveToFile(saveName);
    }

    /// <summary>
    /// 存档加载方法，记得需要在实例化playerData类之前创建
    /// </summary>
    /// <returns></returns>
    public bool isLoad()
    {

        return QXData.Instance.LoadFromFile(saveName);
    }



    #endregion

    #region 数据修改方法
    

    public void ChangeMaxHp(int _maxHp)
    { 
        PlayerData.PHealth = _maxHp;
        if (PlayerData.PHealth > PlayerData.PMaxHealth)
        {
            PlayerData.PHealth = PlayerData.PMaxHealth;
        }
    }

    public void ChangePlayerLevel(int _level)
    {
        PlayerData.PLevel = _level;
        Debug.Log("Player enter level"+_level);
    }

    public void ChangeHp(int _hp)
    {
        PlayerData.PHealth = _hp;
    }

    public void ChangeMoney(int money)
    { 
        PlayerData.PMoney=money;
    }

    #endregion

    #region 基础数据处理
    /// <summary>
    /// 读取csv并转化为链表，接受参数CSV
    /// </summary>
    /// <param name="cardStore"></param>
    /// <returns></returns>
    protected Dictionary<string, Dictionary<string, string>> ReadCsvToList(TextAsset CSV)
    {
        string[] RowData = CSV.text.Split("\n");
        string[] ID = RowData[0].Split(',');
        Dictionary<string, Dictionary<string, string>> List = new Dictionary<string, Dictionary<string, string>>();
        for (int i = 1; i < RowData.Length - 1; i++)
        {
            string[] RowArray = RowData[i].Split(',');
            Dictionary<string, string> Card = new Dictionary<string, string>();
            for (int n = 1; n < RowArray.Length; n++)
            {
                Card.Add(ID[n], RowArray[n]);
            }
            List.Add(RowArray[0], Card);
        }
        return List;
    }

    #endregion

    #region 外部接口



    #endregion

    /// <summary>
    /// 备用测试方法
    /// </summary>
    void IDataManager.Init()
    {
        Debug.Log("DataManager:Test success");
    }
}

interface IDataManager
{
    void Init();
    #region Player 接口

    void ChangeMaxHp(int hp);
    void ChangeHp(int hp);
    void ChangePlayerLevel(int _level);

    void ChangeMoney(int money);

    #endregion


    bool isLoad();
    void SaveData();

    #region 外部卡牌接口
   
 
    #endregion
    //void LoadData();
}


