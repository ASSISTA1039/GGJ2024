#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Animations;

using UnityEngine;

namespace ERAnimation
{
    public class AnimatorControllerPanel : EditorWindow
    {
        public class ERAnimationEventSelection
        {
            public class EventRectPair
            {
                public ERAnimationEvent eve;
                public Rect rect;
            }
            public List<EventRectPair> events;
            public List<EventRectPair> Events {
                get => events;
                set
                {
                    GUIUtility.keyboardControl = 0;
                    events = value;
                }
            }
            float clipLength;

            public ERAnimationEventSelection(float clipLength)
            {
                this.clipLength = clipLength;
                events = new List<EventRectPair>();
            }

            public List<ERAnimationEvent> eventList
            {
                get
                {
                    List<ERAnimationEvent> list = new List<ERAnimationEvent>();
                    foreach (var e in Events)
                    {
                        list.Add(e.eve);
                    }
                    return list;
                }
            }

            public float startTime
            {
                get
                {
                    float minTime = float.MaxValue;
                    foreach (var e in Events)
                    {
                        if (e.eve.startTime < minTime)
                        {
                            minTime = e.eve.startTime;
                        }
                    }
                    return minTime;
                }
                set
                {
                    float diff = value - startTime;
                    foreach (var e in Events)
                    {
                        e.eve.startTime += diff;
                        if (e.eve.startTime < 0)
                        {
                            e.eve.startTime = 0;
                        }
                        else if (e.eve.startTime > e.eve.endTime - 1 / 60f)
                        {
                            e.eve.startTime = e.eve.endTime - 1 / 60f;
                        }
                    }
                }
            }

            public float endTime
            {
                get
                {
                    float maxTime = 0;
                    foreach (var e in Events)
                    {
                        if (e.eve.endTime > maxTime)
                        {
                            maxTime = e.eve.endTime;
                        }
                    }
                    return maxTime;
                }
                set
                {
                    float diff = value - endTime;
                    foreach (var e in Events)
                    {
                        e.eve.endTime += diff;
                        if (e.eve.endTime > clipLength)
                        {
                            e.eve.endTime = clipLength;
                        }
                        else if (e.eve.endTime < e.eve.startTime + 1 / 60f)
                        {
                            e.eve.endTime = e.eve.startTime + 1 / 60f;
                        }
                    }
                }
            }

            public int line
            {
                get
                {
                    int min = int.MaxValue;
                    foreach (var e in Events)
                    {
                        if (e.eve.line < min)
                        {
                            min = e.eve.line;
                        }
                    }
                    return min;
                }
                set
                {
                    int diff = value - line;
                    foreach (var e in Events)
                    {
                        e.eve.line += diff;
                        if (e.eve.line < 0)
                        {
                            e.eve.line = 0;
                        }
                    }
                }
            }

            public int lineCount
            {
                get
                {
                    int max = 0;
                    foreach (var e in Events)
                    {
                        if (e.eve.line > max)
                        {
                            max = e.eve.line;
                        }
                    }
                    return max - line + 1;
                }
            }
        }

        #region PanelArg
        public const float height = 20;
        public const float saveButtonWidth = 40;
        public const float openButtonWidth = 40;
        public const float replayButtonWidth = 40;
        public const float playButtonWidth = 40;
        public const float addButtonWidth = 40;
        public const float returnButtonWidth = 50;
        public const float deleteButtonWidth = 50;
        public const float timeFieldWidth = 40;
        public const float sliderWidth = 13;
        public const float ArrowHeight = 32;
        public const int lineCountPerGroup = 5;
        public const float timeTextWidth = 35;
        public const float splitterWidth = 5;
        public const float setEventWidth = 8;
        public const float blankWidth = 10;
        public const float maxPixelsPerFrame = 100;
        public const float minPixelsPerFrame = 0.015625f;
        #endregion

        #region Varibles
        Rect splitterRect;
        Rect leftPanelRect;
        Rect leftRect;
        Rect rightPanelRect;
        Rect rightRect;
        Rect rightWithTimelineRect;
        Rect rightFullRect;
        Rect timeline;
        Rect timelineConst;
        Vector2 startSelectPos;
        Vector2 startMoveEventPos;
        float splitterPos;
        float leftSliderPos;
        float rightRightSliderPos;
        float rightBottomSliderPos;
        float playbackTime;
        float pixelsPerFrame = 5;
        float startTime;
        float duration;
        float lastTime;
        bool dragging;
        bool isSelecting;
        bool settingTime;
        bool movePanel;
        bool isPlaying;
        bool moveEvent;
        bool setStartTime;
        bool setEndTime;
        int maxEventLineCount;
        int maxArgLineCount;
        int maxArgRowCount;
        List<ERAnimationArg> defaultArgs;
        ERAnimationEventSelection selectedEvent;
        ERAnimationState editState;
        ERAnimatorController controller;
        #endregion

        #region Command
        static string CommandBuffer;
        Commands commands = new Commands();
        float setStartTimeAmount;
        float setEndTimeAmount;
        Vector2 moveEventAmount;
        #endregion

        #region Textures
        Texture replay;
        Texture play;
        Texture pause;
        Texture2D pure;
        Texture2D splitter;
        Texture2D selected;
        Texture2D transparent;
        Texture2D border;
        Material line;
        #endregion

        #region GUIStyles
        GUIStyle boxStyle;
        GUIStyle buttonStyle;
        GUIStyle labelStyle;
        GUIStyle transparentStyle;
        #endregion

        #region Properties
        static string _texturePath = "";

        static string texturePath
        {
            get
            {
                if (string.IsNullOrEmpty(_texturePath) || !Directory.Exists(_texturePath))
                {
                    _texturePath = "";
                    foreach (string path in Directory.EnumerateDirectories(Directory.GetCurrentDirectory(), "ERAnimation", SearchOption.AllDirectories))
                    {
                        _texturePath = path + "\\Editor\\Texture";
                    }
                }
                return _texturePath;
            }
        }

        int scaleLevel
        {
            get
            {
                int i = 0;
                for (; ;i++)
                {
                    if (Mathf.Pow(lineCountPerGroup, i + 1) * pixelsPerFrame <= timeTextWidth)
                    {
                        continue;
                    }
                    else
                    {
                        return i;
                    }
                }
            }
        }

        float minSplitterPos => saveButtonWidth + replayButtonWidth + playButtonWidth + addButtonWidth + returnButtonWidth + timeFieldWidth;

        Motion clip
        {
            get
            {
                if (IsPlayable())
                {
                    foreach (var ac in ((AnimatorController)controller.Animator.runtimeAnimatorController).layers[editState.layerIndex].stateMachine.states)
                    {
                        if (ac.state.name == editState.linkedState)
                        {
                            return ac.state.motion;
                        }
                    }
                }
                return null;
            }
        }

        float timelineLength
        {
            get
            {
                if (clip != null)
                {
                    return clip.averageDuration * 60 * pixelsPerFrame;
                }
                return 100;
            }
        }
        #endregion

        public static void OpenWindow(ERAnimationState animationState, ERAnimatorController animatorControler)
        {
            if (HasOpenInstances<EditConflictWindow>())
            {
                GetWindow<EditConflictWindow>().Close();
            }
            if (HasOpenInstances<SelectAnimationControllerPanel>())
            {
                GetWindow<SelectAnimationControllerPanel>().Close();
            }
            AnimatorControllerPanel window = GetWindow<AnimatorControllerPanel>(animationState.linkedState);
            string data = PlayerPrefs.GetString("Editor_AnimatorControllerPanel_position", "");
            if (string.IsNullOrEmpty(data))
            {
                window.position = new Rect(200, 200, 800, 400);
            }
            else
            {
                string[] datas = data.Split(',');
                window.position = new Rect(Convert.ToSingle(datas[0]), Convert.ToSingle(datas[1]), Convert.ToSingle(datas[2]), Convert.ToSingle(datas[3]));
            }
            //window.position = new Rect(200, 200, 800, 400);
            window.minSize = new Vector2(400, 100);
            window.splitterPos = PlayerPrefs.GetFloat("Editor_AnimatorControllerPanel_splitterPos", 300);
            if (Directory.Exists(texturePath))
            {
                string path = texturePath;
                path = Regex.Match(path, "Asset.*").Value + "\\";
                window.play = AssetDatabase.LoadAssetAtPath<Texture>(path + "Play.png");
                window.pause = AssetDatabase.LoadAssetAtPath<Texture>(path + "Pause.png");
                window.replay = AssetDatabase.LoadAssetAtPath<Texture>(path + "Replay.png");
                window.pure = AssetDatabase.LoadAssetAtPath<Texture2D>(path + "Pure.png");
                window.splitter = AssetDatabase.LoadAssetAtPath<Texture2D>(path + "Splitter.png");
                window.selected = AssetDatabase.LoadAssetAtPath<Texture2D>(path + "Timeline.png");
                window.transparent = AssetDatabase.LoadAssetAtPath<Texture2D>(path + "Selected.png");
                window.border = AssetDatabase.LoadAssetAtPath<Texture2D>(path + "Border.png");
            }
            else
            {
                Debug.Log("材质加载失败！");
            }
            window.line = new Material(Shader.Find("GUI/Text Shader"));
            window.pixelsPerFrame = 5;
            window.editState = animationState;
            window.controller = animatorControler;
            animationState.animationTime = window.clip.averageDuration;
            window.selectedEvent = new ERAnimationEventSelection(window.clip.averageDuration);
            EditorUtility.SetDirty(window.editState);
            Undo.ClearAll();
        }

        [UnityEditor.Callbacks.DidReloadScripts]
        public static void CloseThis()
        {
            if (HasOpenInstances<AnimatorControllerPanel>())
            {
                GetWindow<AnimatorControllerPanel>().Close();
            }
        }

        //Preview
        private void Update()
        {
            if (Application.isPlaying)
            {
                if (Selection.activeGameObject != null)
                {
                    ERAnimatorController controller = Selection.activeGameObject.GetComponentInChildren<ERAnimatorController>(true);
                    if (controller != null)
                    {
                        object obj = typeof(ERAnimatorController).GetField("_updaters", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(controller);
                        playbackTime = (float)obj.GetType().GetField("realTime").GetValue(obj);
                    }
                }
            }
            else
            {
                if (IsPlayable())
                {
                    if (isPlaying)
                    {
                        if (lastTime != 0)
                        {
                            playbackTime += Time.realtimeSinceStartup - lastTime;
                        }
                        float time = clip.averageDuration;
                        if (playbackTime > time)
                        {
                            playbackTime -= time;
                        }
                        if (UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage() != null)
                        {
                            SampleAnimation(clip, UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage().prefabContentsRoot.GetComponentInChildren<ERAnimatorController>().Animator.gameObject, playbackTime);
                        }
                        lastTime = Time.realtimeSinceStartup;
                    }
                }
            }
        }

        private void OnDestroy()
        {
            string data = position.x.ToString() + "," + position.y.ToString() + "," + position.width.ToString() + "," + position.height.ToString();
            PlayerPrefs.SetString("Editor_AnimatorControllerPanel_position", data);
            PlayerPrefs.SetFloat("Editor_AnimatorControllerPanel_splitterPos", splitterPos);

            commands.DeleteAll();
            AssetDatabase.SaveAssets();
        }

        void OnGUI()
        {
            #region GUIStyles
            boxStyle = new GUIStyle(GUI.skin.box);
            transparentStyle = new GUIStyle(GUI.skin.box);
            buttonStyle = new GUIStyle(GUI.skin.button);
            labelStyle = new GUIStyle(GUI.skin.label);
            #endregion
            #region UpperLeft
            GUI.BeginGroup(new Rect(0, 0, splitterPos, height));
            if (GUI.Button(new Rect(0, 0, saveButtonWidth, height), "Save"))
            {
                controller.Animator.WriteDefaultValues();
                AssetDatabase.SaveAssets();
            }
            if (GUI.Button(new Rect(saveButtonWidth, 0, replayButtonWidth, height), new GUIContent(replay, "Play Animation from begining")))
            {
                playbackTime = 0;
            }
            if (isPlaying)
            {
                if (GUI.Button(new Rect(saveButtonWidth + replayButtonWidth, 0, playButtonWidth, height), new GUIContent(play, "Play")))
                {
                    isPlaying = false;
                    lastTime = 0;
                }
            }
            else
            {
                if (GUI.Button(new Rect(saveButtonWidth + replayButtonWidth, 0, playButtonWidth, height), new GUIContent(pause, "Pause")))
                {
                    isPlaying = true;
                }
            }
            if (GUI.Button(new Rect(saveButtonWidth + replayButtonWidth + playButtonWidth, 0, addButtonWidth, height), "Add"))
            {
                SelectEventPanel.OpenWindow(AddDefaultEvent);
            }
            if (GUI.Button(new Rect(saveButtonWidth + replayButtonWidth + playButtonWidth + addButtonWidth, 0, returnButtonWidth, height), "Return"))
            {
                SelectAnimationControllerPanel.OpenWindow(controller);
            }
            GUI.Box(new Rect(splitterRect.x - timeFieldWidth, 0, timeFieldWidth, height), new GUIContent(((int)(playbackTime * 60)).ToString(), "Current time"));
            GUI.EndGroup();
            #endregion
            #region LeftPanel
            leftPanelRect = new Rect(0, height, splitterRect.x, position.height - height);
            GUI.BeginGroup(leftPanelRect);
            GUI.BeginGroup(leftRect);
            DrawArgs();
            leftRect = new Rect(0, -leftSliderPos, leftPanelRect.width, height * maxArgLineCount);
            GUI.EndGroup();
            if (leftRect.height > position.height - height)
            {
                leftRect = new Rect(0, -leftSliderPos, leftPanelRect.width - sliderWidth, height * maxArgLineCount);
                leftSliderPos = GUI.VerticalScrollbar(new Rect(splitterPos - sliderWidth, 0, sliderWidth, position.height - height), leftSliderPos, (leftRect.height - (position.height - height)) > 0 ? position.height - height : 0, 0f, (leftRect.height - (position.height - height)) > 0 ? leftRect.height : 0);
            }
            GUI.EndGroup();
            #endregion
            #region Splitter
            splitterRect = new Rect(splitterPos, 0, splitterWidth, position.height);
            boxStyle.normal.background = splitter;
            GUI.Box(splitterRect, "", boxStyle);
            EditorGUIUtility.AddCursorRect(splitterRect, MouseCursor.ResizeHorizontal);
            #endregion
            #region RightPanel
            rightFullRect = new Rect(splitterPos + splitterWidth + blankWidth, 0, position.width - splitterPos - splitterWidth - blankWidth, position.height);
            rightWithTimelineRect = new Rect(0, 0, rightFullRect.width - sliderWidth, rightFullRect.height - sliderWidth);
            rightPanelRect = new Rect(0, height, rightFullRect.width - sliderWidth, rightFullRect.height - sliderWidth - height);
            GUI.BeginGroup(rightFullRect);
            float x = 0;
            DrawLine(new Vector2(x, 0), new Vector2(x, position.height - sliderWidth), new Rect(rightFullRect.x, 0, rightFullRect.width - sliderWidth, rightFullRect.height - sliderWidth), new Color(0, 0, 0, 0.1f));
            rightBottomSliderPos = GUI.HorizontalScrollbar(new Rect(0, position.height - sliderWidth, rightFullRect.width - sliderWidth, sliderWidth), rightBottomSliderPos, rightRect.width - rightPanelRect.width > 0 ? rightPanelRect.width : 0, 0, rightRect.width - rightPanelRect.width > 0 ? rightRect.width * 2 : 0);
            rightRightSliderPos = GUI.VerticalScrollbar(new Rect(rightFullRect.width - sliderWidth, height, sliderWidth, rightFullRect.height - sliderWidth - height), rightRightSliderPos, rightRect.height - rightPanelRect.height > 0 ? rightPanelRect.height : 0, 0, rightRect.height - rightPanelRect.height > 0 ? rightRect.height : 0);
            GUI.BeginGroup(rightWithTimelineRect);
            boxStyle.normal.background = pure;
            timelineConst = new Rect(0, 0, rightPanelRect.width, height);
            GUI.Box(timelineConst, "", boxStyle);
            #region TimeLine
            timeline = new Rect(-rightBottomSliderPos, 0, timelineLength, height);
            boxStyle.normal.background = selected;
            GUI.Box(timeline, "", boxStyle);
            for (int i = 0; ;i++)
            {
                x = Mathf.Pow(lineCountPerGroup, scaleLevel) * pixelsPerFrame * i - rightBottomSliderPos;

                if (x > rightPanelRect.width)
                {
                    break;
                }
                if (x >= 0)
                {
                    if (i % lineCountPerGroup == 0)
                    {
                        DrawLine(new Vector2(x, height / 2), new Vector2(x, height), new Rect(rightFullRect.x, 0, rightFullRect.width - sliderWidth, rightFullRect.height - sliderWidth), Color.black);
                        labelStyle.fontSize = 10;
                        GUI.Label(new Rect(x, 0, timeTextWidth, height / 2), (i * Mathf.Pow(lineCountPerGroup, scaleLevel)).ToString(), labelStyle);
                        DrawLine(new Vector2(x, height), new Vector2(x, position.height - sliderWidth), new Rect(rightFullRect.x, 0, rightFullRect.width - sliderWidth, rightFullRect.height - sliderWidth), new Color(0, 0, 0, 0.1f));
                    }
                    else
                    {
                        DrawLine(new Vector2(x, 3 * height / 4), new Vector2(x, height), new Rect(rightFullRect.x, 0, rightFullRect.width - sliderWidth, rightFullRect.height - sliderWidth), Color.black);
                        DrawLine(new Vector2(x, height), new Vector2(x, position.height - sliderWidth), new Rect(rightFullRect.x, 0, rightFullRect.width - sliderWidth, rightFullRect.height - sliderWidth), new Color(0, 0, 0, 0.1f));
                    }
                }
            }
            for (int i = 0; ; i++)
            {
                x = -rightRightSliderPos + i * height;
                if (x > rightFullRect.height - sliderWidth)
                {
                    break;
                }
                if (x >= height)
                {
                    DrawLine(new Vector2(0, x), new Vector2(rightFullRect.width - sliderWidth, x), new Rect(rightFullRect.x, 0, rightFullRect.width - sliderWidth, rightFullRect.height - sliderWidth), new Color(0, 0, 0, 0.1f));
                }
            }
            #endregion
            GUI.BeginGroup(rightPanelRect);
            rightRect = new Rect(-rightBottomSliderPos, -rightRightSliderPos, timelineLength, maxEventLineCount * height);
            GUI.BeginGroup(rightRect);
            DrawEvents();
            GUI.EndGroup();
            if (isSelecting && editState != null)
            {
                Rect rect = new Rect(startSelectPos - new Vector2(rightBottomSliderPos, rightRightSliderPos), Event.current.mousePosition - startSelectPos + new Vector2(rightBottomSliderPos, rightRightSliderPos));
                if (rect.width < 0)
                {
                    rect.width *= -1;
                    rect.x -= rect.width;
                }
                if (rect.height < 0)
                {
                    rect.height *= -1;
                    rect.y -= rect.height;
                }
                List<ERAnimationEventSelection.EventRectPair> list = new List<ERAnimationEventSelection.EventRectPair>();
                foreach (var e in editState.events)
                {
                    Rect rec = new Rect(new Vector2(e.startTime / clip.averageDuration * rightRect.width, e.line * height) - new Vector2(rightBottomSliderPos, rightRightSliderPos), new Vector2((e.endTime - e.startTime) / clip.averageDuration * rightRect.width, height));
                    if (rect.Overlaps(rec, true))
                    {
                        list.Add(new ERAnimationEventSelection.EventRectPair() { eve = e });
                    }
                }
                selectedEvent.Events = list;
                transparentStyle.normal.background = transparent;
                GUI.Box(rect, "", transparentStyle);
            }
            GUI.EndGroup();
            x = playbackTime / clip.averageDuration * timelineLength - rightBottomSliderPos;
            if (x >= 0 && x < rightPanelRect.width)
            {
                DrawLine(new Vector2(x, 0), new Vector2(x, position.height - sliderWidth), new Rect(rightFullRect.x, 0, rightFullRect.width - sliderWidth, rightFullRect.height - sliderWidth), Color.white);
            }
            GUI.EndGroup();
            GUI.EndGroup();
            //Repaint();
            #endregion
            #region Events
            if (Event.current != null)
            {
                switch (Event.current.rawType)
                {
                    case EventType.MouseDown:
                        if (Event.current.button == 0)
                        {
                            if (splitterRect.Contains(Event.current.mousePosition))
                            {
                                dragging = true;
                            }
                            if (new Rect(rightFullRect.x, 0, timelineConst.width, timelineConst.height).Contains(Event.current.mousePosition))
                            {
                                settingTime = true;
                            }
                        }
                        else if (Event.current.button == 1 || Event.current.button == 2)
                        {
                            if (new Rect(rightFullRect.x, rightPanelRect.y, rightPanelRect.width, rightPanelRect.height).Contains(Event.current.mousePosition))
                            {
                                movePanel = true;
                            }
                        }
                        break;
                    case EventType.MouseDrag:
                        if (dragging)
                        {
                            if (Event.current.delta.x > 0)
                            {
                                if (Event.current.mousePosition.x > splitterPos && Event.current.mousePosition.x < this.position.width)
                                {
                                    splitterPos += Event.current.delta.x;
                                }
                            }
                            else
                            {
                                if (Event.current.mousePosition.x < splitterPos && Event.current.mousePosition.x > minSplitterPos)
                                {
                                    splitterPos += Event.current.delta.x;
                                }
                            }
                            Repaint();
                        }
                        if (settingTime && !isPlaying)
                        {
                            float t = (Event.current.mousePosition.x - splitterPos - splitter.width - blankWidth + rightBottomSliderPos) / timelineLength * clip.averageDuration % clip.averageDuration;
                            if (t < 0)
                            {
                                t = 0;
                            }
                            playbackTime = t;

                            if (IsPlayable())
                            {
                                if (UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage() != null)
                                {
                                    SampleAnimation(clip, UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage().prefabContentsRoot.GetComponentInChildren<ERAnimatorController>().Animator.gameObject, playbackTime);
                                }
                            }
                        }
                        if (movePanel)
                        {
                            rightBottomSliderPos -= Event.current.delta.x;
                            rightRightSliderPos -= Event.current.delta.y;
                        }
                        break;
                    case EventType.MouseUp:
                        if (Event.current.button == 0)
                        {
                            if (dragging)
                            {
                                dragging = false;
                            }
                            if (settingTime)
                            {
                                settingTime = false;
                            }
                        }
                        else if (Event.current.button == 1 || Event.current.button == 2)
                        {
                            if (movePanel)
                            {
                                movePanel = false;
                            }
                        }
                        break;
                    case EventType.ScrollWheel:
                        if (new Rect(splitterPos, 0, position.width - splitterPos, position.height).Contains(Event.current.mousePosition) && focusedWindow == this)
                        {
                            float currentPos = (Event.current.mousePosition.x - splitterPos - splitter.width - blankWidth + rightBottomSliderPos) / timelineLength;

                            pixelsPerFrame /= Mathf.Pow(1.2f, Event.current.delta.y / 3);

                            if (pixelsPerFrame > maxPixelsPerFrame)
                            {
                                pixelsPerFrame = maxPixelsPerFrame;
                            }
                            else if (pixelsPerFrame < minPixelsPerFrame)
                            {
                                pixelsPerFrame = minPixelsPerFrame;
                            }
                            if (rightRect.width > rightPanelRect.width)
                            {
                                rightBottomSliderPos += (currentPos * timelineLength - rightBottomSliderPos) - (Event.current.mousePosition.x - splitterPos - splitter.width);
                            }
                            Repaint();
                        }
                        if (leftPanelRect.Contains(Event.current.mousePosition) && focusedWindow == this)
                        {
                            if (leftRect.height - (position.height - height) > 0)
                            {
                                leftSliderPos += Event.current.delta.y * 5;
                                Mathf.Clamp(leftSliderPos, 0, leftRect.height - (position.height - height));
                            }
                            Repaint();
                        }
                        break;
                    case EventType.KeyDown:
                        if (Event.current.control)
                        {
                            switch (Event.current.keyCode)
                            {
                                case KeyCode.A:
                                    selectedEvent.Events = new List<ERAnimationEventSelection.EventRectPair>();
                                    foreach (var e in editState.events)
                                    {
                                        selectedEvent.Events.Add(new ERAnimationEventSelection.EventRectPair() { eve = e });
                                    }
                                    break;
                                case KeyCode.Z:
                                    commands.Undo();
                                    CheckSelection();
                                    break;
                                case KeyCode.Y:
                                    commands.Redo();
                                    CheckSelection();
                                    break;
                                case KeyCode.C:
                                    Copy();
                                    break;
                                case KeyCode.V:
                                    Paste();
                                    break;
                                case KeyCode.X:
                                    Copy();
                                    commands.AddCommand(new DeleteEvents(selectedEvent.eventList));
                                    DeleteEvent(selectedEvent.eventList);
                                    selectedEvent.Events = new List<ERAnimationEventSelection.EventRectPair>();
                                    break;
                            }
                        }
                        break;
                }
            }
            Repaint();
            #endregion
        }

        void CheckSelection()
        {
            if (editState != null)
            {
                foreach (var e in selectedEvent.eventList)
                {
                    if (!editState.events.Contains(e))
                    {
                        selectedEvent.Events = new List<ERAnimationEventSelection.EventRectPair>();
                        break;
                    }
                }
            }
        }

        void Copy()
        {
            string result = "ERAnimCopy#";
            foreach (var e in selectedEvent.Events)
            {
                result += e.eve.GetType().FullName + "|" + e.eve.line.ToString() + "|" + e.eve.startTime.ToString() + "|" + e.eve.endTime.ToString() + "|";
                foreach (var arg in e.eve.args)
                {
                    result += arg.argName + "/" + arg.type + "/" + arg.value + ":";
                }
                result += ";";
            }
            CommandBuffer = result;
        }

        void Paste()
        {
            string data = CommandBuffer;
            if (!string.IsNullOrEmpty(data) && data.StartsWith("ERAnimCopy#"))
            {
                List<ERAnimationEvent> events = new List<ERAnimationEvent>();
                string[] eventDatas = data.Split('#')[1].Split(';');
                foreach (var ed in eventDatas)
                {
                    if (!string.IsNullOrEmpty(ed))
                    {
                        string[] eventData = ed.Split('|');
                        ERAnimationEvent animationEvent = (ERAnimationEvent)CreateInstance(typeof(ERAnimationEvent).Assembly.GetType(eventData[0]));
                        animationEvent.line = Convert.ToInt32(eventData[1]);
                        animationEvent.startTime = Convert.ToSingle(eventData[2]);
                        animationEvent.endTime = Convert.ToSingle(eventData[3]);
                        string[] args = eventData[4].Split(':');
                        List<ERAnimationArg> erargs = new List<ERAnimationArg>();
                        foreach (var arg in args)
                        {
                            if (!string.IsNullOrEmpty(arg))
                            {
                                string[] a = arg.Split('/');
                                erargs.Add(ERAnimationArg.CreateInstance<ERAnimationArg>().Init(a[0], a[1], a[2]));
                            }
                        }
                        animationEvent.args = erargs;
                        events.Add(animationEvent);
                    }
                }
                commands.AddCommand(new AddEvents(events));
                AddEvent(events);
            }
        }

        bool IsPlayable()
        {
            return controller != null && controller.Animator != null && controller.Animator.runtimeAnimatorController.animationClips.Length != 0 && editState;
        }

        void SampleAnimation(Motion motion, GameObject gameObject, float time)
        {
            if (motion is AnimationClip ac1)
            {
                ac1.SampleAnimation(gameObject, time);
            }
            else if (motion is BlendTree bt)
            {
                if (bt.children.Length > 0 && bt.children[0].motion is AnimationClip ac2)
                {
                    ac2.SampleAnimation(gameObject, time);
                }
            }
        }

        void AddObjectToAsset(UnityEngine.Object obj, UnityEngine.Object asset)
        {
            try
            {
                AssetDatabase.AddObjectToAsset(obj, asset);
            }
            catch(Exception e)
            {
                Debug.LogWarning(e);
            }
        }

        void DrawEvents()
        {
            maxEventLineCount = 0;
            boxStyle.normal.background = border;
            bool flag = false;
            bool bypass = false;
            if (selectedEvent.Events.Count > 0)
            {
                Rect left = new Rect(selectedEvent.startTime / clip.averageDuration * rightRect.width - setEventWidth / 2, selectedEvent.line * height, setEventWidth, selectedEvent.lineCount * height);
                Rect right = new Rect(selectedEvent.endTime / clip.averageDuration * rightRect.width - setEventWidth / 2, selectedEvent.line * height, setEventWidth, selectedEvent.lineCount * height);
                EditorGUIUtility.AddCursorRect(left, MouseCursor.ResizeHorizontal);
                EditorGUIUtility.AddCursorRect(right, MouseCursor.ResizeHorizontal);
                if (Event.current != null)
                {
                    switch (Event.current.rawType)
                    {
                        case EventType.MouseDown:
                            if (Event.current.button == 0)
                            {
                                if (new Rect(rightBottomSliderPos - setEventWidth / 2, rightRightSliderPos, rightPanelRect.width + setEventWidth / 2, rightPanelRect.height).Contains(Event.current.mousePosition))
                                {
                                    if (left.Contains(Event.current.mousePosition))
                                    {
                                        bypass = true;
                                        setStartTime = true;
                                        setStartTimeAmount = selectedEvent.startTime;
                                        break;
                                    }
                                    if (right.Contains(Event.current.mousePosition))
                                    {
                                        bypass = true;
                                        setEndTime = true;
                                        setEndTimeAmount = selectedEvent.endTime;
                                        break;
                                    }
                                }
                                Vector2 min = selectedEvent.Events[0].rect.min;
                                Vector2 max = selectedEvent.Events[0].rect.max;
                                foreach (var eve in selectedEvent.Events)
                                {
                                    if (eve.rect.min.x < min.x)
                                    {
                                        min.x = eve.rect.min.x;
                                    }
                                    if (eve.rect.min.y < min.y)
                                    {
                                        min.y = eve.rect.min.y;
                                    }
                                    if (eve.rect.max.x > max.x)
                                    {
                                        max.x = eve.rect.max.x;
                                    }
                                    if (eve.rect.max.y > max.y)
                                    {
                                        max.y = eve.rect.max.y;
                                    }
                                }
                                if (new Rect(min, max - min).Contains(Event.current.mousePosition) && new Rect(rightBottomSliderPos, 0, rightPanelRect.width, rightPanelRect.height).Contains(Event.current.mousePosition))
                                {
                                    bypass = true;
                                    moveEvent = true;
                                    moveEventAmount = new Vector2(selectedEvent.startTime, selectedEvent.line);
                                    startMoveEventPos = Event.current.mousePosition;
                                    startTime = selectedEvent.startTime;
                                    duration = selectedEvent.endTime - selectedEvent.startTime;
                                }
                            }
                            break;
                        case EventType.MouseDrag:
                            if (moveEvent)
                            {
                                selectedEvent.line = Mathf.Clamp((int)(moveEventAmount.y) + ((int)(Event.current.mousePosition.y / height) - (int)(startMoveEventPos.y / height)), 0, int.MaxValue);
                                float delta = (Event.current.mousePosition.x - startMoveEventPos.x) / timelineLength * clip.averageDuration;
                                delta = (int)(delta * 60) / 60f;
                                if (startTime + delta < 0)
                                {
                                    selectedEvent.startTime = 0;
                                    selectedEvent.endTime = duration;
                                }
                                else if (startTime + duration + delta > clip.averageDuration)
                                {
                                    selectedEvent.endTime = clip.averageDuration;
                                    selectedEvent.startTime = clip.averageDuration - duration;
                                }
                                else
                                {
                                    selectedEvent.startTime = startTime + delta;
                                    selectedEvent.endTime = startTime + duration + delta;
                                }
                            }
                            if (setStartTime)
                            {
                                float value = Event.current.mousePosition.x / timelineLength * clip.averageDuration;
                                value = (int)(value * 60) / 60f;
                                if (value > selectedEvent.endTime - (1f / 60))
                                {
                                    selectedEvent.startTime = selectedEvent.endTime - (1f / 60);
                                }
                                else if (value < 0)
                                {
                                    selectedEvent.startTime = 0;
                                }
                                else
                                {
                                    selectedEvent.startTime = value;
                                }
                            }
                            if (setEndTime)
                            {
                                float value = Event.current.mousePosition.x / timelineLength * clip.averageDuration;
                                value = (int)(value * 60) / 60f;
                                if (value < selectedEvent.startTime + (1f / 60))
                                {
                                    selectedEvent.endTime = selectedEvent.startTime + (1f / 60);
                                }
                                else if (value > clip.averageDuration)
                                {
                                    selectedEvent.endTime = clip.averageDuration;
                                }
                                else
                                {
                                    selectedEvent.endTime = value;
                                }
                            }
                            break;
                        case EventType.MouseUp:
                            if (Event.current.button == 0)
                            {
                                if (moveEvent)
                                {
                                    moveEvent = false;
                                    commands.AddCommand(new MoveEvent(selectedEvent.eventList, new Vector2(selectedEvent.startTime - moveEventAmount.x, selectedEvent.line - moveEventAmount.y)));
                                }
                                if (setStartTime)
                                {
                                    setStartTime = false;
                                    commands.AddCommand(new EditEvent(selectedEvent.eventList, new Vector2(selectedEvent.startTime - setStartTimeAmount, 0)));
                                }
                                if (setEndTime)
                                {
                                    setEndTime = false;
                                    commands.AddCommand(new EditEvent(selectedEvent.eventList, new Vector2(0, selectedEvent.endTime - setEndTimeAmount)));
                                }
                            }
                            break;
                    }
                }
                foreach (var e in selectedEvent.Events)
                {
                    EditorUtility.SetDirty(e.eve);
                }
            }
            for (int i = 0; i < editState.events.Count; i++)
            {
                if (editState.events[i].startTime >= clip.averageDuration)
                {
                    DeleteEvent(new List<ERAnimationEvent>() { editState.events[i] });
                    i--;
                    continue;
                }
                if (editState.events[i].line >= maxEventLineCount)
                {
                    maxEventLineCount = editState.events[i].line + 1;
                }
                if (editState.events[i].endTime > clip.averageDuration)
                {
                    editState.events[i].endTime = clip.averageDuration;
                }
                Rect rect = new Rect(editState.events[i].startTime / clip.averageDuration * rightRect.width, editState.events[i].line * height, (editState.events[i].endTime - editState.events[i].startTime) / clip.averageDuration * rightRect.width, height);
                if (!selectedEvent.Events.Contains(selectedEvent.Events.Find((e) => { return e.eve == editState.events[i]; })))
                {
                    GUI.Box(rect, editState.events[i].GetType().Name, buttonStyle);
                    if (Event.current != null && new Rect(rightBottomSliderPos, rightRightSliderPos, rightPanelRect.width, rightPanelRect.height).Contains(Event.current.mousePosition))
                    {
                        switch (Event.current.rawType)
                        {
                            case EventType.MouseDown:
                                if (Event.current.button == 0 && !bypass)
                                {
                                    if (rect.Contains(Event.current.mousePosition))
                                    {
                                        leftSliderPos = 0;
                                        selectedEvent.Events = new List<ERAnimationEventSelection.EventRectPair>() { new ERAnimationEventSelection.EventRectPair() { eve = editState.events[i] } };
                                        defaultArgs = null;
                                        flag = true;
                                    }
                                }
                                break;
                        }
                    }
                }
                else
                {
                    selectedEvent.Events.Find((e) => { return e.eve == editState.events[i]; }).rect = rect;
                    if (rect.Contains(Event.current.mousePosition))
                    {
                        flag = true;
                    }
                }
            }
            if (!bypass && !flag && Event.current != null && new Rect(rightBottomSliderPos, rightRightSliderPos, rightPanelRect.width, rightPanelRect.height).Contains(Event.current.mousePosition))
            {
                if (Event.current.button == 0)
                {
                    if (Event.current.rawType == EventType.MouseDown)
                    {
                        startSelectPos = Event.current.mousePosition;
                        isSelecting = true;
                        selectedEvent.Events = new List<ERAnimationEventSelection.EventRectPair>();
                        defaultArgs = null;
                        //selectedEventRect = default;
                    }
                }
            }
            if (selectedEvent.Events.Count > 0)
            {
                //Debug.Log(selectedEvent.events.Count);
                foreach(var e in selectedEvent.Events)
                {
                    GUI.Box(e.rect, e.eve.GetType().Name, boxStyle);
                }
                switch (Event.current.rawType)
                {
                    case EventType.KeyDown:
                        if (Event.current.keyCode == KeyCode.Delete)
                        {
                            commands.AddCommand(new DeleteEvents(selectedEvent.eventList));
                            DeleteEvent(selectedEvent.eventList);
                            selectedEvent.Events = new List<ERAnimationEventSelection.EventRectPair>();
                        }
                        break;
                }
            }
            if (Event.current != null)
            {
                switch (Event.current.rawType)
                {
                    case EventType.MouseUp:
                        if (Event.current.button == 0)
                        {
                            if (isSelecting)
                            {
                                isSelecting = false;
                            }
                        }
                        break;
                }
            }
        }

        void DrawArgs()
        {
            maxArgLineCount = 0;
            maxArgRowCount = 0;
            if (selectedEvent.Events.Count == 0)
            {
                GUI.Label(new Rect(0, maxArgLineCount * height, leftRect.width, height), "Select a event first.");
                maxArgLineCount++;
            }
            else if (selectedEvent.Events.Count == 1)
            {
                if (defaultArgs == null)
                {
                    defaultArgs = selectedEvent.Events[0].eve.Args;
                }
                if (selectedEvent.Events[0].eve.args == null)
                {
                    Debug.LogError("如果你看到了这个报错，请立刻联系PJ");
                    selectedEvent.Events[0].eve.args = defaultArgs;
                    foreach (var arg in selectedEvent.Events[0].eve.args)
                    {
                        AddObjectToAsset(arg, selectedEvent.Events[0].eve);
                    }
                    EditorUtility.SetDirty(selectedEvent.Events[0].eve);
                    AssetDatabase.Refresh();
                }
                else
                {
                    bool flag = false;
                    if (selectedEvent.Events[0].eve.args.Count != defaultArgs.Count)
                    {
                        flag = true;
                    }
                    foreach (var arg1 in defaultArgs)
                    {
                        bool flag2 = false;
                        foreach (var arg2 in selectedEvent.Events[0].eve.args)
                        {
                            if (arg1.type == arg2.type && arg1.argName == arg2.argName)
                            {
                                arg1.value = arg2.value; 
                                flag2 = true;
                            }
                        }
                        if (!flag2)
                        {
                            flag = true;
                        }
                    }
                    if (flag)
                    {
                        foreach (var arg in selectedEvent.Events[0].eve.args)
                        {
                            AssetDatabase.RemoveObjectFromAsset(arg);
                        }
                        selectedEvent.Events[0].eve.args = defaultArgs;
                        foreach (var arg in selectedEvent.Events[0].eve.args)
                        {
                            AddObjectToAsset(arg, selectedEvent.Events[0].eve);
                        }
                        EditorUtility.SetDirty(selectedEvent.Events[0].eve);
                        AssetDatabase.Refresh();
                    }
                }
                GUI.Label(new Rect(0, maxArgLineCount * height, leftRect.width - deleteButtonWidth - openButtonWidth, height), selectedEvent.Events[0].eve.GetType().Name);
                if (GUI.Button(new Rect(leftRect.width - deleteButtonWidth - openButtonWidth, maxArgLineCount * height, openButtonWidth, height), "Open"))
                {
                    var assets = AssetDatabase.GetAllAssetPaths();
                    foreach (var asset in assets)
                    {
                        var split1 = asset.Split('/');
                        var split2 = split1[split1.Length - 1].Split('.');
                        if (split2.Length != 2)
                        {
                            continue;
                        }
                        if (split1[0] == "Assets" && split1[split1.Length - 1].Split('.')[1] == "cs" && split1[split1.Length - 1].Split('.')[0] == selectedEvent.Events[0].eve.GetType().Name)
                        {
                            AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath<MonoScript>(asset));
                            break;
                        }
                    }
                }
                if (GUI.Button(new Rect(leftRect.width - deleteButtonWidth, maxArgLineCount * height, deleteButtonWidth, height), "Delete"))
                {
                    commands.AddCommand(new DeleteEvents(selectedEvent.eventList));
                    DeleteEvent(new List<ERAnimationEvent>() { selectedEvent.Events[0].eve });
                    selectedEvent.Events = new List<ERAnimationEventSelection.EventRectPair>();
                }
                maxArgLineCount++;
                if (selectedEvent.Events.Count == 1)
                {
                    foreach (var arg in selectedEvent.Events[0].eve.args)
                    {
                        GUI.Box(new Rect(0, maxArgLineCount * height, leftRect.width, height), arg.argName + "(" + arg.type + ")");
                        maxArgLineCount++;
                        string value = arg.value;
                        switch (arg.type)
                        {
                            case "System.Boolean":
                                value = BoolField(value);
                                break;
                            case "System.Int32":
                                value = IntField(value);
                                break;
                            case "System.Single":
                                value = FloatField(value);
                                break;
                            case "UnityEngine.Vector2":
                                value = Vector2Field(value);
                                break;
                            case "UnityEngine.Vector3":
                                value = Vector3Field(value);
                                break;
                            default:
                                try
                                {
                                    if (typeof(ERAnimationEvent).Assembly.GetType(arg.type).BaseType.Name == "Enum")
                                    {
                                        value = EnumField(arg.type, value);
                                    }
                                    else
                                    {
                                        value = EditorGUI.TextField(new Rect(maxArgRowCount * height, maxArgLineCount * height, leftRect.width - maxArgRowCount * height, height), value);
                                        maxArgLineCount++;
                                    }
                                }
                                catch
                                {
                                    try
                                    {
                                        if (arg.type.Split('`')[0] == "System.Collections.Generic.List")
                                        {
                                            //string _value = "[[1,1,],[1,1,],[1,1,],[1,1,],[1,1,],]";
                                            //Debug.Log(Regex.Matches(Regex.Match(_value, "(?<=\\[).*(?=\\])").Value, "\\[.*?\\](?=,)").Count);
                                            value = ListField(arg.type, value);
                                        }
                                        else
                                        {
                                            value = EditorGUI.TextField(new Rect(maxArgRowCount * height, maxArgLineCount * height, leftRect.width - maxArgRowCount * height, height), value);
                                            maxArgLineCount++;
                                        }
                                    }
                                    catch
                                    {
                                        value = EditorGUI.TextField(new Rect(maxArgRowCount * height, maxArgLineCount * height, leftRect.width - maxArgRowCount * height, height), value);
                                        maxArgLineCount++;
                                    }
                                }
                                break;
                        }
                        if (value != arg.value)
                        {
                            commands.AddCommand(new EditArgs(arg, arg.value, value));
                            arg.value = value;
                            EditorUtility.SetDirty(selectedEvent.Events[0].eve);
                        }
                    }
                    maxArgLineCount++;
                }
            }
            else
            {
                GUI.Label(new Rect(0, maxArgLineCount * height, leftRect.width, height), "Multiple event edit is not support.");
                maxArgLineCount++;
            }
        }

        #region BasicFields
        string BoolField(string value)
        {
            bool _bool = false;
            if (value == "true")
            {
                _bool = true;
            }
            _bool = EditorGUI.Toggle(new Rect(maxArgRowCount * height, maxArgLineCount * height, leftRect.width - maxArgRowCount * height, height), _bool);
            maxArgLineCount++;
            if (_bool)
            {
                value = "true";
            }
            else
            {
                value = "false";
            }
            return value;
        }

        string IntField(string value)
        {
            int _int = 0;
            try
            {
                _int = Convert.ToInt32(value);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            _int = EditorGUI.IntField(new Rect(maxArgRowCount * height, maxArgLineCount * height, leftRect.width - maxArgRowCount * height, height), _int);
            maxArgLineCount++;
            value = _int.ToString();
            return value;
        }

        string FloatField(string value)
        {
            float _float = 0;
            try
            {
                _float = Convert.ToSingle(value);
            }
            catch (Exception e)
            {
                Debug.LogWarning(e);
            }
            _float = EditorGUI.FloatField(new Rect(maxArgRowCount * height, maxArgLineCount * height, leftRect.width - maxArgRowCount * height, height), _float);
            maxArgLineCount++;
            value = _float.ToString();
            return value;
        }

        string Vector2Field(string value)
        {
            Vector2 vector2 = new Vector2();
            try
            {
                string[] elements = value.Trim().Replace("(", "").Replace(")", "").Split(',');
                vector2 = new Vector2(Convert.ToSingle(elements[0]), Convert.ToSingle(elements[1]));
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            vector2 = EditorGUI.Vector2Field(new Rect(maxArgRowCount * height, maxArgLineCount * height, leftRect.width - maxArgRowCount * height, height), "", vector2);
            maxArgLineCount++;
            value = vector2.ToString();
            return value;
        }

        string Vector3Field(string value)
        {
            Vector3 vector3 = new Vector2();
            try
            {
                string[] elements = value.Trim().Replace("(", "").Replace(")", "").Split(',');
                vector3 = new Vector3(Convert.ToSingle(elements[0]), Convert.ToSingle(elements[1]), Convert.ToSingle(elements[2]));
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            vector3 = EditorGUI.Vector3Field(new Rect(maxArgRowCount * height, maxArgLineCount * height, leftRect.width - maxArgRowCount * height, height), "", vector3);
            maxArgLineCount++;
            value = vector3.ToString();
            return value;
        }

        string EnumField(string enumType, string value)
        {
            int _enum = 0;
            var names = Enum.GetNames(typeof(ERAnimationEvent).Assembly.GetType(enumType));
            try
            {
                for (int i = 0; i < names.Length; i++)
                {
                    if (names[i] == value)
                    {
                        _enum = i;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            _enum = EditorGUI.Popup(new Rect(maxArgRowCount * height, maxArgLineCount * height, leftRect.width - maxArgRowCount * height, height), _enum, names);
            maxArgLineCount++;
            value = names[_enum].ToString();
            return value;
        }

        string ListField(string type, string _value)
        {
            if (Regex.IsMatch(type, "(?<=\\[).*(?=\\])"))
            {
                type = Regex.Match(type, "(?<=\\[).*(?=\\])").Value;
                if (Regex.IsMatch(type, "(?<=\\[).*(?=\\])"))
                {
                    if (Regex.IsMatch(type, "(?<=\\[).*(?<=\\])(?=,)"))
                    {
                        type = Regex.Match(type, "(?<=\\[).*(?<=\\])(?=,)").Value;
                    }
                    else
                    {
                        type = Regex.Match(type, "(?<=\\[).*?(?=,)").Value;
                    }
                }
            }
            List<string> values = new List<string>();
            if (GUI.Button(new Rect(maxArgRowCount * height, maxArgLineCount * height, (leftRect.width - maxArgRowCount) / 2, height), "+"))
            {
                string v;
                switch (type)
                {
                    case "System.Boolean":
                        v = "false";
                        break;
                    case "System.Int32":
                        v = "0";
                        break;
                    case "System.Single":
                        v = "0";
                        break;
                    case "UnityEngine.Vector2":
                        v = "(0,0)";
                        break;
                    case "UnityEngine.Vector3":
                        v = "(0,0,0)";
                        break;
                    default:
                        try
                        {
                            if (typeof(ERAnimationEvent).Assembly.GetType(type).BaseType.Name == "Enum")
                            {
                                v = Enum.GetNames(typeof(ERAnimationEvent).Assembly.GetType(type))[0];
                            }
                            else
                            {
                                v = "";
                            }
                        }
                        catch
                        {
                            try
                            {
                                if (type.Split('`')[0] == "System.Collections.Generic.List")
                                {
                                    v = "[]";
                                }
                                else
                                {
                                    v = "";
                                }
                            }
                            catch
                            {
                                v = "";
                            }
                        }
                        break;
                }
                values.Add(v);
            }
            _value = Regex.Match(_value, "(?<=\\[).*(?=\\])").Value;
            if (Regex.IsMatch(_value, "(?<=\\[).*(?=\\])"))
            {
                var collections = Regex.Matches(_value, "\\[.*?\\](?=,)");
                for (int i = 0; i < collections.Count; i++)
                {
                    values.Add(collections[i].Value);
                }
            }
            else
            {
                if (Regex.IsMatch(_value, "(?<=\\().*(?=\\))"))
                {
                    var collections = Regex.Matches(_value, "\\(.*?\\)(?=,)");
                    for (int i = 0; i < collections.Count; i++)
                    {
                        values.Add(collections[i].Value);
                    }
                }
                else
                {
                    var collections = _value.Split(',');
                    for (int i = 0; i < collections.Length - 1; i++)
                    {
                        values.Add(collections[i]);
                    }
                }
            }
            if (GUI.Button(new Rect((leftRect.width - maxArgRowCount) / 2 + maxArgRowCount * height, maxArgLineCount * height, leftRect.width / 2, height), "-"))
            {
                values.RemoveAt(values.Count - 1);
            }
            maxArgLineCount++;
            _value = "";
            for (int i = 0; i < values.Count; i++)
            {
                switch (type)
                {
                    case "System.Boolean":
                        values[i] = BoolField(values[i]);
                        break;
                    case "System.Int32":
                        values[i] = IntField(values[i]);
                        break;
                    case "System.Single":
                        values[i] = FloatField(values[i]);
                        break;
                    case "UnityEngine.Vector2":
                        values[i] = Vector2Field(values[i]);
                        break;
                    case "UnityEngine.Vector3":
                        values[i] = Vector3Field(values[i]);
                        break;
                    default:
                        try
                        {
                            if (typeof(ERAnimationEvent).Assembly.GetType(type).BaseType.Name == "Enum")
                            {
                                values[i] = EnumField(type, values[i]);
                            }
                            else
                            {
                                values[i] = EditorGUI.TextField(new Rect(maxArgRowCount * height, maxArgLineCount * height, leftRect.width - maxArgRowCount * height, height), values[i]);
                                maxArgLineCount++;
                            }
                        }
                        catch
                        {
                            try
                            {
                                if (type.Split('`')[0] == "System.Collections.Generic.List")
                                {
                                    maxArgRowCount++;
                                    values[i] = ListField(type, values[i]);
                                    maxArgRowCount--;
                                }
                                else
                                {
                                    values[i] = EditorGUI.TextField(new Rect(maxArgRowCount * height, maxArgLineCount * height, leftRect.width - maxArgRowCount * height, height), values[i]);
                                    maxArgLineCount++;
                                }
                            }
                            catch
                            {
                                values[i] = EditorGUI.TextField(new Rect(maxArgRowCount * height, maxArgLineCount * height, leftRect.width - maxArgRowCount * height, height), values[i]);
                                maxArgLineCount++;
                            }
                        }
                        break;
                }
                _value += values[i] + ",";
            }
            _value = "[" + _value + "]";
            return _value;
        }
        #endregion

        public void AddEvent(List<ERAnimationEvent> events)
        {
            if (events.Count > 0 && editState != null)
            {
                int minLine = events[0].line;
                foreach (var eve in events)
                {
                    if (eve.line < minLine)
                    {
                        minLine = eve.line;
                    }
                }
                foreach (var eve in events)
                {
                    foreach (var arg in eve.args)
                    {
                        AddObjectToAsset(arg, editState);
                    }
                    eve.linkedState = editState;
                    eve.line += maxEventLineCount - minLine;
                    AddObjectToAsset(eve, editState);
                    editState.events.Add(eve);
                }
                EditorUtility.SetDirty(editState);
                AssetDatabase.Refresh();
            }
        }

        public void AddDefaultEvent(List<ERAnimationEvent> events)
        {
            if (editState != null)
            {
                commands.AddCommand(new AddEvents(events));
                foreach (var e in events)
                {
                    e.args = e.Args;
                    foreach (var arg in e.args)
                    {
                        AddObjectToAsset(arg, editState);
                    }
                    e.linkedState = editState;
                    e.startTime = 0;
                    e.endTime = clip.averageDuration;
                    e.line = maxEventLineCount;
                    AddObjectToAsset(e, editState);
                    editState.events.Add(e);
                }
                EditorUtility.SetDirty(editState);
                AssetDatabase.Refresh();
            }
        }

        public void DeleteEvent(List<ERAnimationEvent> events)
        {
            if (editState != null)
            {
                foreach (var e in events)
                {
                    editState.events.Remove(e);
                    foreach (var arg in e.args)
                    {
                        AssetDatabase.RemoveObjectFromAsset(arg);
                    }
                    AssetDatabase.RemoveObjectFromAsset(e);
                }
                EditorUtility.SetDirty(editState);
                AssetDatabase.Refresh();
            }
        }

        void DrawLine(Vector2 startPos, Vector2 endPos, Rect limitArea, Color color)
        {
            line.SetPass(0);
            GL.LoadPixelMatrix();
            GL.PushMatrix();
            GL.Begin(1);
            GL.Color(color);
            GL.Vertex(startPos);
            GL.Vertex(endPos);
            GL.End();
            GL.PopMatrix();
        }

        public static List<ERAnimationState> GetConflict(ERAnimatorController controller)
        {
            List<ERAnimationState> result = new List<ERAnimationState>();
            if (controller == null || controller.Animator == null || controller.Animator.runtimeAnimatorController == null)
            {
                Debug.LogWarning("GetConflict传入参数为null");
                return result;
            }
            foreach (ERAnimationState state in controller.States)
            {
                bool flag = true;
                foreach (var ac in ((AnimatorController)controller.Animator.runtimeAnimatorController).layers[state.layerIndex].stateMachine.states)
                {
                    if (ac.state.name == state.linkedState)
                    {
                        flag = false;
                    }
                }
                if (flag)
                {
                    result.Add(state);
                }
            }
            return result;
        }
    }

    #region 快捷指令
    public class Commands
    {
        const float MaxCommandCount = 10;
        int curser = 0;
        List<Command> commands = new List<Command>();

        public void AddCommand(Command command)
        {
            //Debug.Log(command.GetType().Name);
            if (curser == commands.Count)
            {
                commands.Add(command);
                curser++;
            }
            else
            {
                for (; curser < commands.Count;)
                {
                    commands.RemoveAt(curser);
                }
                commands.Add(command);
                curser++;
            }
            command.OnAdd(commands, ref curser);
            if (commands.Count > MaxCommandCount)
            {
                commands[0].OnDelete();
                commands.RemoveAt(0);
                curser--;
            }
        }

        public void Redo()
        {
            if (curser < commands.Count)
            {
                commands[curser].Redo();
                curser++;
            }
        }

        public void Undo()
        {
            if (curser > 0)
            {
                commands[curser - 1].Undo();
                curser--;
            }
        }

        public void DeleteAll()
        {
            for (; 0 < commands.Count;)
            {
                commands[0].OnDelete();
                commands.RemoveAt(0);
            }
        }
    }

    public abstract class Command
    {
        public virtual void OnAdd(List<Command> commands, ref int curser) { }
        public virtual void OnDelete() { }
        public abstract void Redo();
        public abstract void Undo();
    }

    public class EditEvent : Command
    {
        List<ERAnimationEvent> events;
        Vector2 moveAmount;

        public EditEvent(List<ERAnimationEvent> events, Vector2 moveAmount)
        {
            this.events = events;
            this.moveAmount = moveAmount;
        }

        public override void Redo()
        {
            foreach (var e in events)
            {
                e.startTime += moveAmount.x;
                e.endTime += moveAmount.y;
            }
        }

        public override void Undo()
        {
            foreach (var e in events)
            {
                e.startTime -= moveAmount.x;
                e.endTime -= moveAmount.y;
            }
        }
    }

    public class MoveEvent : Command
    {
        List<ERAnimationEvent> events;
        Vector2 moveAmount;

        public MoveEvent(List<ERAnimationEvent> events, Vector2 moveAmount)
        {
            this.events = events;
            this.moveAmount = moveAmount;
        }

        public override void Redo()
        {
            foreach (var e in events)
            {
                e.startTime += moveAmount.x;
                e.endTime += moveAmount.x;
                e.line += (int)moveAmount.y;
            }
        }

        public override void Undo()
        {
            foreach (var e in events)
            {
                e.startTime -= moveAmount.x;
                e.endTime -= moveAmount.x;
                e.line -= (int)moveAmount.y;
            }
        }
    }

    public class EditArgs : Command
    {
        ERAnimationArg arg;
        string previousValue;
        string currentValue;

        public EditArgs(ERAnimationArg arg, string previousValue, string currentValue)
        {
            this.arg = arg;
            this.previousValue = previousValue;
            this.currentValue = currentValue;
        }

        public override void OnAdd(List<Command> commands, ref int curser)
        {
            base.OnAdd(commands, ref curser);
            if (commands.Count > 1 && commands[commands.Count - 2] is EditArgs arg && arg.arg == this.arg)
            {
                arg.currentValue = this.currentValue;
                commands.RemoveAt(commands.Count - 1);
                curser--;
            }
        }

        public override void Redo()
        {
            arg.value = currentValue;
        }

        public override void Undo()
        {
            arg.value = previousValue;
        }
    }

    public class DeleteEvents : Command
    {
        List<ERAnimationEvent> events;
        bool deleted;

        public DeleteEvents(List<ERAnimationEvent> events)
        {
            this.events = events;
            deleted = true;
        }

        public override void Undo()
        {
            EditorWindow.GetWindow<AnimatorControllerPanel>().AddEvent(events);

            deleted = false;
        }

        public override void Redo()
        {
            EditorWindow.GetWindow<AnimatorControllerPanel>().DeleteEvent(events);

            deleted = true;
        }

        public override void OnDelete()
        {
            base.OnDelete();

            foreach (var e in events)
            {
                if (deleted)
                {
                    AssetDatabase.GetAssetPath(e);
                    AssetDatabase.RemoveObjectFromAsset(e);
                    AssetDatabase.GetAssetPath(e);
                }
            }
        }
    }

    public class AddEvents : Command
    {
        List<ERAnimationEvent> events;
        bool deleted;

        public AddEvents(List<ERAnimationEvent> events)
        {
            this.events = events;
            deleted = false;
        }

        public override void Redo()
        {
            EditorWindow.GetWindow<AnimatorControllerPanel>().AddEvent(events);

            deleted = false;
        }

        public override void Undo()
        {
            EditorWindow.GetWindow<AnimatorControllerPanel>().DeleteEvent(events);

            deleted = true;
        }

        public override void OnDelete()
        {
            base.OnDelete();

            foreach (var e in events)
            {
                if (deleted)
                {
                    AssetDatabase.RemoveObjectFromAsset(e);
                }
            }
        }
    }
    #endregion
}
#endif