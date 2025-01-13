using QxFramework.Core;
using System.Collections.Generic;
using UnityEngine;

namespace EventLogicSystem
{
    public partial class EventContext
    {
        public TableAgent Table
        {
            get
            {
                if (_table == null)
                {
                    SetTableAgent();
                }
                return _table;
            }
        }

        public int TemplateId;

        public ConditionEventCreator EventCreator;

        public readonly List<int> Params = new List<int>();

        private TableAgent _table = null;

        public void Init()
        {
            SetTableAgent();
        }

        public void SetTableAgent()
        {
            _table = new TableAgent();
#if UNITY_EDITOR
            //编辑器模式下
            if (!Application.isPlaying)
            {
                var list = Resources.LoadAll<TextAsset>("Text/Table/");
                for (int i = 0; i < list.Length; i++)
                {
                    _table.Add(list[i].text);
                }
                return;
            }

#endif
            //如果有现成的表格
            if (QXData.Instance.TableAgent != null)
            {
                _table = QXData.Instance.TableAgent;
                return;
            }

            //如果没有，则自己调用表格读取
            var list2 = ResourceManager.Instance.LoadAll<TextAsset>("Text/Table/");
            for (int i = 0; i < list2.Length; i++)
            {
                Table.Add(list2[i].text);
            }
        }
    }
}