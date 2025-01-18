using UnityEngine;
using UnityEngine.UI;
using QxFramework;
using QxFramework.Core;

public class ButtonController : MonoBehaviour
{
    public Button[] buttons;  // 存储所有按钮

    void OnEnable()
    {
        // 初始化所有按钮的状态
        UpdateButtonStates();
    }

    void UpdateButtonStates()
    {
        // 遍历所有按钮，根据其名字和data值来决定是否可点击
        foreach (Button button in buttons)
        {
            // 获取按钮的名字（假设按钮名字为 "level0", "level1", ...）
            string buttonName = button.name.ToLower();  // 避免大小写问题
            int levelIndex = int.Parse(buttonName.Replace("level", ""));

            // 根据data值决定按钮是否可点击
            button.interactable = (QXData.Instance.Get<PlayerData>().PLevel >= levelIndex);
            button.gameObject.SetActive(QXData.Instance.Get<PlayerData>().PLevel >= levelIndex);
        }
    }
}

