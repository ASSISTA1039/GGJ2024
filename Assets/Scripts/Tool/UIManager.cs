using System.Collections.Generic;
using QxFramework.Utilities;
using UnityEngine;
using System.Net;
using System;
using System.Collections;
using UnityEngine.UI;
using TMPro;

namespace QxFramework.Core
{
    public enum UIMsg
    {
        UpDateWareHouse
    }
    public enum CourseUI
    {
        CloseUI,
    }
    public enum HintType
    {
        CommonHint,
        ClickWindow,
        ChooseWindow,
    }

    



    /// <summary>
    /// 界面管理，提供管理界面和界面组的功能，如显示隐藏界面、激活界面。
    /// </summary>
    public class UIManager : MonoSingleton<UIManager>
    {
        /// <summary>
        /// UI预设目录，位于Assets/Resource/Prefabs/UI/
        /// </summary>
        public readonly string FoldPath = "Prefabs/UI/";

        //internal void Open(string v, object args)
        //{
        //    throw new NotImplementedException();
        //}

        /// <summary>
        /// 子面板对象列表
        /// </summary>
        private readonly List<Transform> _panels = new List<Transform>();

        //TODO 要改成可以存在同种的多个于场上
        /// <summary>
        /// 打开的UI列表
        /// </summary>
        private readonly List<KeyValuePair<string, UIBase>> _openUI = new List<KeyValuePair<string, UIBase>>();


        private GameObject shadePanel;


        #region 外加的方法

        public void RestartUI(string uiName)
        {
            if (GameObject.Find(uiName) != null)
            {
                GameObject.Find(uiName).SetActive(true);
            }
            else if (GameObject.Find(uiName + "(Clone)") != null)
            {
                GameObject.Find(uiName + "(Clone)").SetActive(true);
            }
        }

        //黑幕方法
        /// <summary>
        /// 用于使黑幕渐变到黑色。
        /// </summary>
        /// <param name="blackScreenObject">黑幕物体</param>
        /// <param name="fadeDuration">黑幕时间</param>
        /// <param name="produce">跳转到的流程</param>
        public void FadeToBlack(GameObject blackScreenObject, float fadeDuration,string proceduce = null, object args=null,string ui=null)
        {
            if (blackScreenObject == null)
            {
                Debug.LogError("黑幕物体为空");
                return;
            }
            StartCoroutine(FadeToBlackCoroutine(blackScreenObject, fadeDuration,proceduce, args,ui));
        }

        private IEnumerator FadeToBlackCoroutine(GameObject blackScreenObject, float fadeDuration,string proceduce=null, object args = null, string ui = null)
        {
            Image blackScreen = blackScreenObject.GetComponent<Image>();
            Color color = blackScreen.color;
            blackScreen.raycastTarget = false;
            // 设置初始透明度
            color.a = 0;
            blackScreen.color = color;
            blackScreenObject.SetActive(true); // 确保黑幕图片可见

            float elapsedTime = 0f;
            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                color.a = Mathf.Clamp01(elapsedTime / fadeDuration);
                blackScreen.color = color;
                yield return null;
            }

            // 确保完全黑色
            color.a = 1;
            blackScreen.color = color;

            CloseAll();//Doing:Fade会关闭一切UI
            if (proceduce != null)
                ProcedureManager.Instance.ChangeTo(proceduce, args);
            else if (ui != null)
            {
                Open(ui,3,ui);
            }
            else
                Application.Quit();
        }
        private IEnumerator FadeToOpenCoroutine(GameObject blackScreenObject, float fadeDuration,string ui)
        {
            Image blackScreen = blackScreenObject.GetComponent<Image>();
            Color color = blackScreen.color;

            // 设置初始透明度
            color.a = 1;
            blackScreen.color = color;
            blackScreenObject.SetActive(true); // 确保黑幕图片可见
            blackScreen.raycastTarget = false;

            float elapsedTime = 0f;
            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                color.a = 1f-Mathf.Clamp01(elapsedTime / fadeDuration);
                blackScreen.color = color;
                yield return null;
            }

            // 确保完全白色
            color.a = 0;
            blackScreen.color = color;

            // 退出应用
            Application.Quit();
        }
        #endregion

        #region 变白方法
        public void StartBlackoutAndReveal(float fadeDuration, string textToReveal, string ui,object args=null)
        {
            // 检查并创建Shade面板
            shadePanel = GameObject.Find("Shade");
            if (shadePanel == null)
            {
                shadePanel = new GameObject("Shade");
                var rectTransform = shadePanel.AddComponent<RectTransform>();
                rectTransform.sizeDelta = new Vector2(Screen.width, Screen.height);
                var image = shadePanel.AddComponent<Image>();
                image.color = new Color(0, 0, 0, 1); // 完全黑色
                shadePanel.transform.SetParent(Canvas.FindObjectOfType<Canvas>().transform, false);
            }
            shadePanel.SetActive(true);
            shadePanel.GetComponent<Image>().color = new Color(0, 0, 0, 1);
            // 创建子物体以显示文本
            var textObject = new GameObject("RevealedText");
            textObject.transform.SetParent(shadePanel.transform);
            var textMesh = textObject.AddComponent<TextMeshProUGUI>();
            textMesh.font = Resources.Load<TMP_FontAsset>("Fonts/pixel SDF");
            textMesh.text = "";
            textMesh.fontSize = 60;
            textMesh.color = Color.white;
            textMesh.alignment = TextAlignmentOptions.Center;
            textMesh.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0.5f); // 设置锚点为中心
            textMesh.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.5f);
            textMesh.GetComponent<RectTransform>().anchoredPosition = Vector2.zero; // 位置设置为零
            textMesh.GetComponent<RectTransform>().sizeDelta = new Vector2(600, 100);

            // 启动协程
            StartCoroutine(ExecuteBlackoutAndReveal(fadeDuration, textToReveal, ui, textMesh,args));
        }

        private IEnumerator ExecuteBlackoutAndReveal(float fadeDuration, string textToReveal, string ui, TextMeshProUGUI textMesh, object args = null)
        {
            // 渐变显示文本
            yield return StartCoroutine(RevealText(textToReveal, textMesh));
            if (ui != null)
            {
                Open(ui, 2, ui, args);
            }
            else
            {
                Debug.Log("UI=NULL");
            }
            // 渐变淡出黑色面板
            yield return StartCoroutine(FadeOutShade(fadeDuration));

        }

        private IEnumerator RevealText(string textToReveal, TextMeshProUGUI textMesh)
        {
            if (textToReveal != null)
            {
                for (int i = 0; i < textToReveal.Length; i++)
                {
                    textMesh.text += textToReveal[i]; // 添加一个字符
                    yield return new WaitForSeconds(0.1f); // 每个字符的显示延迟
                }
                yield return new WaitForSeconds(1f); // 等待一秒
            }
            textMesh.gameObject.SetActive(false);
        }

        private IEnumerator FadeOutShade(float fadeDuration)
        {
            float elapsedTime = 0f;
            Color initialColor = new Color(0, 0, 0, 1);
            shadePanel = GameObject.Find("Shade");
            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                float alpha = Mathf.Lerp(1, 0, elapsedTime / fadeDuration);
                shadePanel.GetComponent<Image>().color = new Color(0, 0, 0, alpha);
                yield return null;
            }

            // 设置为不阻挡射线
            shadePanel.GetComponent<Image>().raycastTarget = false;
        }

        #endregion





        /// <summary>
        ///  打开一个UI。
        /// </summary>
        /// <param name="uiName">UI预设的名称。</param>
        /// <param name="layer">显示在哪一个层。</param>
        /// <param name="args">附带的参数。</param>
        /// <returns></returns>
        public UIBase Open(string uiName, int layer = 2, string name = "", object args = null)
        {
            return Open(uiName, _panels[layer], name, args);
        }

        public UIBase OpenInChild(string uiName, string ChildName, string name = "", object args = null)
        {
            Transform parent = null;
            RectTransform[] allChildren = GetComponentsInChildren<RectTransform>();
            foreach(RectTransform child in allChildren)
            {
                if(child.gameObject.name == ChildName)
                {
                    parent = child;
                }
            }
            return Open(uiName, parent, name, args);
        }
        public UIBase OpenAt(string uiName, string ObjName, string name = "", object args = null)
        {
            RectTransform parent = GameObject.Find(ObjName).GetComponent<RectTransform>();
            return Open(uiName, parent, name, args);
        }

        /// <summary>
        /// 检测ui是否开启
        /// </summary>
        /// <returns></returns>
        public bool OpenUICheck(string uiName)
        {
            for (int i = _openUI.Count - 1; i >= 0; i--)
            {
                if (_openUI[i].Key == uiName)
                {
                    if (uiName != "" && _openUI[i].Value.name.Contains(uiName))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        ///  打开一个UI。
        /// </summary>
        /// <param name="uiName">UI预设的名称。</param>
        /// <param name="parent">父对象</param>
        /// <param name="args">附带的参数。</param>
        /// <returns></returns>
        public UIBase Open(string uiName, Transform parent, string name = "", object args = null)
        {
            //实例化UI
            if (ResourceManager.Instance == null)
            {
                Debug.LogError("ResourceManager instance is null.");
                return null;
            }


            GameObject ui = ResourceManager.Instance.Instantiate(FoldPath + uiName, parent);
            if (ui == null)
            {
                Debug.LogError("UI object is null. "+uiName );
                return null;
            }
            ui.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

            if (name != "")
            {
                ui.name = name;
            }

            //添加到UI键值对列表
            _openUI.Add(new KeyValuePair<string, UIBase>(uiName, ui.GetComponent<UIBase>()));

            //启动参数
            if (ui.GetComponent<UIBase>() == null)
            {
                Debug.LogError("[UIManager]" + ui.name + "未挂载继承UIBase的脚本");
            }

            else
            {
                //通过脚本覆盖掉执行的层级
                if (ui.GetComponent<UIBase>().UILayer!=-1)
                {
                    ui.transform.SetParent(_panels[ui.GetComponent<UIBase>().UILayer]);
                }
                ui.GetComponent<UIBase>().DoDisplay(args);
            }

            return ui.GetComponent<UIBase>();
        }

        /// <summary>
        ///  关闭指定名称的UI，是从后往前查找。
        /// </summary>
        /// <param name="uiName">UI名称。</param>
        public void Close(string uiName, string objName = "")
        {
            for (int i = _openUI.Count - 1; i >= 0; i--)
            {
                if (_openUI[i].Key == uiName)
                {
                    if (objName != "" && _openUI[i].Value.name != objName)
                    {
                        continue;
                    }
                    if (_openUI[i].Value != null)
                    {
                        CloseUI(_openUI[i].Value);
                        _openUI.RemoveAt(i);
                        break;
                    }
                    else
                    {
                        Debug.Log("关闭UI时，这个UI并不存在");
                    }
                }
            }
        }







        /// <summary>
        /// 关闭所有UI
        /// </summary>
        public void CloseAll()
        {
            for (int i = _openUI.Count - 1; i >= 0; i--)
            {
                var pair = _openUI[i];
                if(pair.Key == "HintUI")
                {
                    continue;
                }
                _openUI.RemoveAt(i);
                CloseUI(pair.Value);
                //break;
            }
        }

        /// <summary>
        ///  关闭指定UI对象。
        /// </summary>
        /// <param name="ui">ui对象</param>
        public void Close(UIBase ui, string objName = "")
        {
            for (int i = _openUI.Count - 1; i >= 0; i--)
            {
                if (_openUI[i].Value == ui)
                {
                    if (objName != "" && _openUI[i].Value.name != objName)
                    {
                        continue;
                    }
                    var pair = _openUI[i];
                    _openUI.Remove(pair);
                    CloseUI(pair.Value);
                    break;
                }
            }
        }

        /// <summary>
        /// 进行关闭和销毁处理。
        /// </summary>
        /// <param name="ui">UI对象</param>
        private void CloseUI(UIBase ui)
        {
            if (ui != null)
            {
                ui.DoClose();
                MessageManager.Instance.RemoveAbout(ui);
            }
        }

        public bool FindUI(string uiName, string objName = "")
        {
            bool IsFind = false;
            for (int i = _openUI.Count - 1; i >= 0; i--)
            {
                if (_openUI[i].Key == uiName)
                {
                    if (_openUI[i].Value != null)
                    {
                        IsFind = true;
                    }
                    else
                    {
                        // Debug.Log("关闭UI时，这个UI并不存在");
                    }
                }
            }
            return IsFind;
        }
        
        public bool FindUIs(string[] UIs)
        {
            for(int i = 0; i < UIs.Length; i++)
            {
                if (FindUI(UIs[i]))
                {
                    return true;
                }
            }
            return false;
        }

        public UIBase GetUI(string uiName)
        {
            foreach (KeyValuePair<string, UIBase> kvp in _openUI)
            {
                if (kvp.Key == uiName)
                {
                    if (kvp.Value != null)
                    {
                        return kvp.Value;
                    }
                }
            }
            // Debug.Log("没找到这个UI");
            return null;
        }

        // Use this for initialization
        private void Awake()
        {
            //收集所有子对象
            for (int i = 0; i < transform.childCount; i++)
            {
                _panels.Add(transform.GetChild(i));
                //销毁现有的UI预设
                for (int j = _panels[i].childCount - 1; j >= 0; j--)
                {
#if UNITY_EDITOR
                    if (_panels[i].GetChild(j).name != "DebugCommandUI")
                    {
                        Destroy(_panels[i].GetChild(j).gameObject);
                    }
#endif
                }
            }
        }
    }
} 