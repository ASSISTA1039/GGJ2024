using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QxFramework.Core;
using UnityEngine.SceneManagement;
using System;
using QxFramework.Utilities;

public class LevelManager : Singleton<LevelManager>, ISystemModule
{
    //加载进度
    public float LoadProgress
    {
        get
        {
            if (_asyncOperation == null)
            {
                return _asyncOperation.progress;
            }
            else
            {
                return 1f;
            }
        }
    }

    public string CurrentLevel
    {
        get
        {
            return _currentLevel;
        }

        set
        {
            _currentLevel = value;
        }
    }

    public bool FirstEnter
    {
        get
        {
            return firstEnter;
        }
    }

    private string _currentLevel = string.Empty;

    private Action<string> _onCompleted = null;
    private Action<string> _onClose = null;

    private AsyncOperation _asyncOperation = null;

    private bool firstEnter = true;

    public void OpenLevel(string levelName, Action<string> onCompleted)
    {
        if (_onCompleted != null)
        {
            Debug.LogError("[LevelManager] 已经正在加载一个关卡中......");
        }

        if (string.IsNullOrEmpty(levelName))
        {
            levelName = CurrentLevel;
            CloseLevel();
        }
        //Debug.Log("Open "+levelName);
        CurrentLevel = levelName;
        _onCompleted = onCompleted;
        _asyncOperation = SceneManager.LoadSceneAsync(CurrentLevel, LoadSceneMode.Additive);
    }

    public void CloseLevel(Action<string> onClose = null)
    {
        if (!string.IsNullOrEmpty(CurrentLevel))
        {
            Debug.Log("Close "+ CurrentLevel);
            _onClose = onClose;
            SceneManager.UnloadSceneAsync(CurrentLevel);
            CurrentLevel = string.Empty;
        }
    }

    public override void Initialize()
    {
        SceneManager.sceneLoaded += LoadCompleted;
        SceneManager.sceneUnloaded += SceneUnloaded;
    }

    private void LoadCompleted(Scene scene, LoadSceneMode loadSceneMode)
    {
        SceneManager.SetActiveScene(scene);
        MessageManager.Instance.Get<LevelLoadEvent>().DispatchMessage(LevelLoadEvent.Load, scene.name);
        if (_onCompleted != null)
        {
            _onCompleted(scene.name);
            _onCompleted = null;
        }
    }

    private void SceneUnloaded(Scene scene)
    {
        MessageManager.Instance.Get<LevelLoadEvent>().DispatchMessage(LevelLoadEvent.UnLoad, scene.name);
        if (_onClose != null)
        {
            _onClose(scene.name);
            _onClose = null;
        }
    }

    public void Update(float deltaTime)
    {

    }

    public void FixedUpdate(float deltaTime)
    {

    }

    public void Dispose()
    {
        SceneManager.sceneLoaded -= LoadCompleted;
        SceneManager.sceneUnloaded -= SceneUnloaded;
    }

    public enum LevelLoadEvent
    {
        Error,
        Load,
        UnLoad,
    }
}

