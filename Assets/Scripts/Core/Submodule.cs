using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace QxFramework.Core
{
    /// <summary>
    /// 子类模块
    /// </summary>
    public class Submodule 
    {
        /// <summary>
        /// 关联流程
        /// </summary>
        public ProcedureBase ProcedureRoot;

        public List<UIBase> OpenUIs = new List<UIBase>();

        /// <summary>
        /// 设置Root
        /// </summary>
        /// <param name="procedureRoot"></param>
        public void SetRootProcedure(ProcedureBase procedureRoot)
        {
            ProcedureRoot = procedureRoot;
        }

        /// <summary>
        /// 打开UI
        /// </summary>
        /// <param name="uiName"></param>
        /// <param name="layer"></param>
        /// <param name="name"></param>
        /// <param name="args"></param>
        protected UIBase OpenUI(string uiName, string name = "", object args = null)
        {
            var uibase = UIManager.Instance.Open(uiName, name:name, args:args);
            OpenUIs.UniqueAdd(uibase);
            return uibase;
        }

        /// <summary>
        ///  关闭指定名称的UI，是从后往前查找。
        /// </summary>
        /// <param name="uiName">UI名称。</param>
        protected void CloseUI(string uiName, string objName = "")
        {
            UIManager.Instance.Close(uiName, objName);

            //以防万一移除掉
            OpenUIs.RemoveAll((ui) => { return !ui.enabled; });         
        }

        /// <summary>
        /// 关闭打开的所有UI
        /// </summary>
        protected void CloseAllUI()
        {
            for (int i = 0; i < OpenUIs.Count; i++)
            {
                UIManager.Instance.Close(OpenUIs[i]);
            }
        }

        public void Init()
        {
            OnInit();
        }

        public void Update()
        {
            OnUpdate();
        }

        public void FixedUpdate()
        {
            OnFixedUpdate();
        }

        /// <summary>
        /// 销毁自身
        /// </summary>
        public void Destory()
        {
            OnDestroy();
            MessageManager.Instance.RemoveAbout(this);
            CloseAllUI();
        }

        protected virtual void OnInit()
        {

        }

        protected virtual void OnUpdate()
        {

        }

        protected virtual void OnFixedUpdate()
        {

        }

        protected virtual void OnDestroy()
        {

        }        
    }
}