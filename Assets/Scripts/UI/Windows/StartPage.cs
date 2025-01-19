using System.Collections;
using System.Collections.Generic;
using QxFramework.Core;
using QxFramework;
using UnityEngine;
using Unity.UI;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using Sirenix.OdinInspector;
using UnityEditor;

public class StartPage : UIBase
{
    [HideInEditorMode] public Image creatorImage;
    [HideInEditorMode] public Image startImage;
    [HideInEditorMode] public Image exitImage;

    private Coroutine creatorFadeCoroutine;
    private Coroutine startFadeCoroutine;
    private Coroutine exitFadeCoroutine;

    private void Start()
    {
    }

    public override void OnDisplay(object args)
    {
        RegisterTrigger("CreatorButton").onPointerEnter = EnterCreatorButton;
        RegisterTrigger("ClickToStartButton").onPointerEnter = EnterClickToStartButton;
        RegisterTrigger("ExitButton").onPointerEnter = EnterExitButton;

        RegisterTrigger("CreatorButton").onClick = OnCreatorButton;
        RegisterTrigger("ClickToStartButton").onClick = OnClickToStartButton;
        RegisterTrigger("ExitButton").onClick = OnExitButton;

        creatorImage = transform.Find("Content/creatorContent").GetComponent<Image>();
        startImage = transform.Find("Content/startContent").GetComponent<Image>();
        exitImage = transform.Find("Content/exitContent").GetComponent<Image>();

        creatorImage.color = new Color(creatorImage.color.r, creatorImage.color.g, creatorImage.color.b, 0);
        startImage.color = new Color(startImage.color.r, startImage.color.g, startImage.color.b, 0);
        exitImage.color = new Color(exitImage.color.r, exitImage.color.g, exitImage.color.b, 0);
        StopAllCoroutines();
        exitFadeCoroutine = null;
        startFadeCoroutine = null;
        creatorFadeCoroutine = null;
        
    }
    //public void OnEnable()
    //{
    //    StopAllCoroutines();
    //}

    private void OnExitButton(GameObject @object, PointerEventData data)
    {
#if UNITY_EDITOR
    EditorApplication.isPlaying = false;
#else
    Application.Quit();
#endif

    }

    private void OnClickToStartButton(GameObject @object, PointerEventData data)
    {
        UIManager.Instance.StartBlackoutAndReveal(1f, null, "LevelUI", null);
        UIManager.Instance.Close("StartPage");
    }

    private void OnCreatorButton(GameObject @object, PointerEventData data)
    {
        UIManager.Instance.Open("CreatorPage", 4, "CreatorPage");
        UIManager.Instance.Close("StartPage");
    }

    private void ExitExitButton(GameObject @object, PointerEventData data)
    {
        // Stop the current exit fade coroutine and start a new one
        if (exitFadeCoroutine == null) //StopCoroutine(exitFadeCoroutine);
        exitFadeCoroutine = StartCoroutine(FadeOut(exitImage));
    }

    private void ExitClickToStartButton(GameObject @object, PointerEventData data)
    {
        // Stop the current start fade coroutine and start a new one
        if (startFadeCoroutine == null) //StopCoroutine(startFadeCoroutine);
        startFadeCoroutine = StartCoroutine(FadeOut(startImage));
    }

    private void ExitCreatorButton(GameObject @object, PointerEventData data)
    {
        // Stop the current creator fade coroutine and start a new one
        if (creatorFadeCoroutine != null) StopCoroutine(creatorFadeCoroutine);
        creatorFadeCoroutine = StartCoroutine(FadeOut(creatorImage));
    }

    private void EnterExitButton(GameObject @object, PointerEventData data)
    {
        // Stop the current exit fade coroutine and start a new one
        if (exitFadeCoroutine == null) //StopCoroutine(exitFadeCoroutine);
        exitFadeCoroutine = StartCoroutine(FadeIn(exitImage));
    }

    private void EnterClickToStartButton(GameObject @object, PointerEventData data)
    {
        // Stop the current start fade coroutine and start a new one
        if (startFadeCoroutine == null) //StopCoroutine(startFadeCoroutine);
        startFadeCoroutine = StartCoroutine(FadeIn(startImage));
    }

    private void EnterCreatorButton(GameObject @object, PointerEventData data)
    {
        // Stop the current creator fade coroutine and start a new one
        if (creatorFadeCoroutine == null) //StopCoroutine(creatorFadeCoroutine);
        creatorFadeCoroutine = StartCoroutine(FadeIn(creatorImage));
    }

    private IEnumerator FadeIn(Image image)
    {
        Debug.Log("FadeIn started for " + image.name);
        while (image.color.a < 1f)
        {
            Color color = image.color;
            color.a += Time.deltaTime / 0.5f;
            image.color = color;
            yield return null;
        }
        Debug.Log("FadeIn finished for " + image.name);
    }

    private IEnumerator FadeOut(Image image)
    {
        Debug.Log("FadeOut started for " + image.name);
        while (image.color.a > 0f)
        {
            Color color = image.color;
            color.a -= Time.deltaTime / 0.5f;
            image.color = color;
            yield return null;
        }
        Debug.Log("FadeOut finished for " + image.name);
    }
}