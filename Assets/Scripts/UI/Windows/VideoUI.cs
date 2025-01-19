using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Video;
using QxFramework.Core;
using UnityEditor;
using System.Data.Common;

public class VideoUI : UIBase
{
    [HideInInspector] public VideoPlayer videoPlayer; // 视频播放器组件
    private bool isVideoPaused = false;
    private bool isVideoPlaying = false;
    public override void OnDisplay(object args)
    {
        base.OnDisplay(args);

        // 设置视频播放
        if (videoPlayer == null)
        {
            videoPlayer = transform.GetChild(0).GetComponent<VideoPlayer>();
            videoPlayer.targetCamera = Camera.main;
        }

        // 初始化视频
        if (!string.IsNullOrEmpty((string)args))
        {
            videoPlayer.url = System.IO.Path.Combine(Application.streamingAssetsPath, (string)args + ".mp4");

        }
        else
        {
            Debug.Log((string)args + "未找到");
        }

        videoPlayer.loopPointReached += OnVideoFinished;

        // 开始播放视频
        if (QXData.Instance.Get<PlayerData>().PLevel <= 4)
            videoPlayer.Play();
        else
        { Debug.Log("display close"); OnClose(); }

        isVideoPlaying = true;

        // 注册跳过按钮事件
        RegisterTrigger("Video Player").onClick = OnSkipVideo;
    }

    private void OnSkipVideo(GameObject obj, PointerEventData pData)
    {
        // 停止播放视频并关闭UI
        if (videoPlayer.isPlaying)
        {
            videoPlayer.Pause();
        }

    }
    
    public void Update()
    {
        //Debug.Log(videoPlayer.isPlaying);
        // 检查视频是否暂停
        if (videoPlayer.isPaused && !isVideoPaused)
        {
            isVideoPaused = true;
            CheckAndOpenUI();
            
        }
        else if (!videoPlayer.isPaused && isVideoPaused)
        {
            isVideoPaused = false;
        }
    }
    private void OnVideoFinished(VideoPlayer vp)
    {
        // 检查目标物体是否存在
        CheckAndOpenUI();
    }
    private void CheckAndOpenUI()
    {
        GameObject targetObject = GameObject.Find("FuncBlocks(Clone)");
        if (targetObject != null && QXData.Instance.Get<PlayerData>().PLevel != 3&& QXData.Instance.Get<PlayerData>().PLevel != 4)
        {
            UIManager.Instance.Open("GameUI", 2, "GameUI");
            Debug.Log("Check close");
            OnClose();
        }
        else
        {
            Debug.Log("FuncBlock=null");
        }
        
        
         if (QXData.Instance.Get<PlayerData>().PLevel == 3)
        {
            videoPlayer.url = System.IO.Path.Combine(Application.streamingAssetsPath, "5" + ".mp4");
            isVideoPlaying = true;
            GameMgr.Get<IDataManager>().ChangePlayerLevel(4);
        }
        else if(QXData.Instance.Get<PlayerData>().PLevel == 4)
        {
            UIManager.Instance.Open("StartPage");
            Debug.Log("stage close");
            OnClose();
        }
    }
    protected override void OnClose()
    {
        // 执行基类关闭逻辑
        base.OnClose();
    }
}