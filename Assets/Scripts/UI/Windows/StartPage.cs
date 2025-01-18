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

public class StartPage : UIBase
{
    [HideInEditorMode]public Image creatorImage;
    [HideInEditorMode] public Image startImage;
    [HideInEditorMode] public Image exitImage;

    private Coroutine fadeCoroutine;

    private void Start()
    {
        creatorImage = transform.Find("Content/creatorContent").GetComponent<Image>();
        startImage = transform.Find("Content/startContent").GetComponent<Image>();
        exitImage = transform.Find("Content/exitContent").GetComponent<Image>();

        creatorImage.color = new Color(creatorImage.color.r, creatorImage.color.g, creatorImage.color.b, 0);
        startImage.color = new Color(startImage.color.r, startImage.color.g, startImage.color.b, 0);
        exitImage.color = new Color(exitImage.color.r, exitImage.color.g, exitImage.color.b, 0);
    }
    public override void OnDisplay(object args)
    {
        RegisterTrigger("CreatorButton").onPointerEnter = EnterCreatorButton;
        RegisterTrigger("ClickToStartButton").onPointerEnter = EnterClickToStartButton;
        RegisterTrigger("ExitButton").onPointerEnter = EnterExitButton;

        //RegisterTrigger("CreatorButton").onPointerExit = ExitCreatorButton;
        //RegisterTrigger("ClickToStartButton").onPointerExit = ExitClickToStartButton;
        //RegisterTrigger("ExitButton").onPointerExit = ExitExitButton;

        RegisterTrigger("CreatorButton").onClick = OnCreatorButton;
        RegisterTrigger("ClickToStartButton").onClick = OnClickToStartButton;
        RegisterTrigger("ExitButton").onClick = OnExitButton;
    }

    private void OnExitButton(GameObject @object, PointerEventData data)
    {
        Application.Quit();
    }

    private void OnClickToStartButton(GameObject @object, PointerEventData data)
    {
        UIManager.Instance.StartBlackoutAndReveal(1f, null, "LevelUI", null);
        UIManager.Instance.Close("StartPage");
    }

    private void OnCreatorButton(GameObject @object, PointerEventData data)
    {
        UIManager.Instance.Open("CreatorPage", 4, "CreatorPage");
        OnClose();
    }

    private void ExitExitButton(GameObject @object, PointerEventData data)
    {
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeOut(exitImage));
    }

    private void ExitClickToStartButton(GameObject @object, PointerEventData data)
    {
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeOut(startImage));
    }

    private void ExitCreatorButton(GameObject @object, PointerEventData data)
    {
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeOut(creatorImage));
    }

    private void EnterExitButton(GameObject @object, PointerEventData data)
    {
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeIn(exitImage));
    }

    private void EnterClickToStartButton(GameObject @object, PointerEventData data)
    {
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeIn(startImage));
    }

    private void EnterCreatorButton(GameObject @object, PointerEventData data)
    {
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeIn(creatorImage));
    }

    private IEnumerator FadeIn(Image image)
    {
        while (image.color.a < 1f)
        {
            Color color = image.color;
            color.a += Time.deltaTime /0.5f;
            image.color = color;
            yield return null;
        }
    }

    private IEnumerator FadeOut(Image image)
    {
        while (image.color.a > 0f)
        {
            Color color = image.color;
            color.a -= Time.deltaTime / 0.5f;
            image.color = color;
            yield return null;
        }
    }
}