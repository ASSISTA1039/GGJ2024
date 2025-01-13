using QxFramework.Utilities;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

namespace QxFramework.Core
{
    public class QXData : MonoSingleton<QXData>
    {
        #region 游戏数据存取接口

        private readonly GameDataContainer _gameDataContainer = new GameDataContainer();

        /// <summary>
        /// 取得游戏数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public T Get<T>(string key = "Default") where T : GameDataBase, new()
        {
            return _gameDataContainer.Get<T>(key);
        }

        /// <summary>
        /// 初始化游戏数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool InitData<T>(out T data, string key = "Default") where T : GameDataBase, new()
        {
            return _gameDataContainer.InitData<T>(out data, key);
        }

        /// <summary>
        /// 设置被修改
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="action"></param>
        /// <param name="key"></param>
        public void SetModify<T>(T t, object modifier, string key = "Default")
            where T : GameDataBase, new()
        {
            _gameDataContainer.SetModify<T>(t, modifier, key);
        }

        /// <summary>
        /// 注册更新监听
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="action"></param>
        /// <param name="key"></param>
        public void RegisterUpdateListener<T>(Action<GameDataBase> action, string key = "Default")
            where T : GameDataBase, new()
        {
            _gameDataContainer.RegisterUpdateListener<T>(action, key);
        }

        /// <summary>
        /// 消除有关某个对象的所有监听
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="obj">添加过监听的对象</param>
        public void RemoveAbout(object obj)
        {
            _gameDataContainer.RemoveAbout(obj);
        }

        [HideInInspector]
        public int CurrentMap = 0;

        /// <summary>
        /// 获取所有注册的数据
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, Dictionary<Type, GameDataContainer.SavedGameData>> GetAllData()
        {
            return _gameDataContainer.GetAll();
        }

        public void SaveToFile(string FileName)
        {
            if (FileName == "")
            {
                return;
            }
            var json = _gameDataContainer.ToSaveJson();
            File.WriteAllText(Application.persistentDataPath+"/" + FileName, json);
        }

        public bool LoadFromFile(string FileName)
        {
            try
            {
                var json = File.ReadAllText(Application.persistentDataPath + "/" + FileName);
                _gameDataContainer.FromSaveJson(json);
                //LoadLevel(FileName);
                return true;
            }
            catch (Exception)
            {
                /*
                UIManager.Instance.Open("DialogWindowUI", args: new DialogWindowUI.DialogWindowUIArg
                ("警告", "存档已损坏或不兼容最新版本", null, "确定", () => { }));*/
                Debug.LogError("存档" + FileName + "已损坏或不兼容最新版本，或者压根就没有╮(╯▽╰)╭");
                return false;
            }
        }

        public void DeleteFile(string FileName)
        {
            File.Delete(Application.persistentDataPath + "/" + FileName);
        }

        /*
        /// <summary>
        /// 加载关卡的逻辑放在这里
        /// </summary>
        /// <returns></returns>
        public void LoadLevel(string FileName)
        {
            ProcedureManager.Instance.ChangeTo(Launcher.Instance.StartProcedure);
        }
        */

        #endregion 游戏数据存取接口

        #region 变量的声明和初始化
        #endregion 变量的声明和初始化

        #region 读表类函数,基层轮子

        private readonly TableAgent _tableAgent = new TableAgent();

        public TableAgent TableAgent
        {
            get { return _tableAgent; }
        }

        public void SetTableAgent()
        {
            var list = ResourceManager.Instance.LoadAll<TextAsset>("Text/Table/");
            for (int i = 0; i < list.Length; i++)
            {
                _tableAgent.Add(list[i].text);
            }
        }

        #endregion 读表类函数,基层轮子

        #region 胜利失败的监听

        #endregion 胜利失败的监听

        /// <summary>
        /// 临时翻译，后面转为表格
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Translate(string item, out string des)
        {
            try
            {
                des = TableAgent.GetString("Description", item, "Name");
                return !des.Equals("");
            }
            catch (Exception)
            {
                des = item;
                return false;
            }
        }
    }
}