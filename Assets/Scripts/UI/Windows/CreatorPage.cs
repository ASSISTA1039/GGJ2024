using System.Collections;
using System.Collections.Generic;
using QxFramework.Core;
using UnityEngine;
using Unity.UI;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class CreatorPage : UIBase
{
    public Image exitImage;
    public Vector3 defaultScale = Vector3.one;
    public Vector3 enlargeScale = new Vector3(1.5f,1.5f,1.5f);
    public float transitionDuration = 0.5f;

    private void Start()
    {
        exitImage = transform.Find("ExitButton/ExitImage").GetComponent<Image>(); 
        exitImage.rectTransform.localScale = defaultScale;
        exitImage.gameObject.SetActive(true);

    }
    public override void OnDisplay(object args)
    {
        RegisterTrigger("ExitButton").onPointerEnter = EnterExitButton;
        RegisterTrigger("ExitButton").onPointerExit = ExitExitButton;
        RegisterTrigger("ExitButton").onClick = OnExitButton;
    }

    private void OnExitButton(GameObject @object, PointerEventData data)
    {
        UIManager.Instance.Open("StartPage", 0, "startPage");
        OnClose();
    }

    private void ExitExitButton(GameObject @object, PointerEventData data)
    {
        StartCoroutine(ScaleImage(exitImage.rectTransform, enlargeScale, defaultScale, transitionDuration));
    }

    private void EnterExitButton(GameObject @object, PointerEventData data)
    {
        StartCoroutine(ScaleImage(exitImage.rectTransform, defaultScale, enlargeScale, transitionDuration));
    }

    private IEnumerator ScaleImage(RectTransform imageRectTransform, Vector3 fromScale, Vector3 toScale, float duration)
    {
        float elapsedTime = 0.0f;
        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            imageRectTransform.localScale = Vector3.Lerp(fromScale, toScale, t);
            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }
        imageRectTransform.localScale = toScale;
    }
}
