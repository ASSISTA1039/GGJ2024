using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EventLogicSystem;

public interface IEventManager
{
    /// <summary>
    /// 覆盖一个模板，一般用于调试
    /// </summary>
    /// <param name="id"></param>
    /// <param name="conditionEventTemplate"></param>
    void OverwriteTemplate(ConditionEventTemplate conditionEventTemplate);

    /// <summary>
    /// 强制执行某事件
    /// </summary>
    /// <param name="templateId"></param>
    /// <param name="paramList"></param>
    void ForceEvent(int templateId, List<int> paramList = null);


}