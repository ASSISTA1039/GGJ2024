using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AnimationUtil : MonoBehaviour
{

    public void SetBool(string name, bool value, Action OnFinish)
    {
        GetComponent<Animator>().SetBool(name, value);
        StartCoroutine(CheckFinish(OnFinish));
    }

    private void OnEnable(){
    }
    private void OnDisable()
    {        
        StopAllCoroutines();
    }

    private IEnumerator CheckFinish(Action OnFinish)
    {
        Action action = OnFinish;
        yield return 0;
        while (GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).normalizedTime > 1.0f)
        {
            yield return 0;
        }
        while (GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f)
        {
            yield return 0;
        }
        if (action != null)
        {
            action();
        }
    }
}
