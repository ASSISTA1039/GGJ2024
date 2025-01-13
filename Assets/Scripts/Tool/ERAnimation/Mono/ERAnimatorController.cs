using System.Collections.Generic;
using UnityEngine;

namespace ERAnimation
{
    [DisallowMultipleComponent]
    public class ERAnimatorController : MonoBehaviour
    {
        private class ERAnimatorControllerUpdater
        {
            public bool updated;
            public int layer;
            public float realTime;
            public float nextTime;
            public float realAnimationTime;
            public float nextAnimationTime;
            public int currentNamehash;
            public int nextNamehash;
            public ERAnimationState currentState;
            public List<ERAnimationState> states;

            public ERAnimatorControllerUpdater(int layer)
            {
                this.layer = layer;
                realTime = nextTime = 0;
                currentNamehash = nextNamehash = 0;
                currentState = null;
                states = new List<ERAnimationState>();
            }

            public static List<ERAnimatorControllerUpdater> BuildUpdaters(List<ERAnimationState> states)
            {
                List<ERAnimatorControllerUpdater> result = new List<ERAnimatorControllerUpdater>();
                foreach (ERAnimationState state in states)
                {
                    bool flag = true;
                    foreach (var temp in result)
                    {
                        if (temp.layer == state.layerIndex)
                        {
                            temp.states.Add(state);
                            flag = false;
                        }
                    }

                    if (flag)
                    {
                        var updater = new ERAnimatorControllerUpdater(state.layerIndex);
                        updater.states.Add(state);
                        result.Add(updater);
                    }
                }
                return result;
            }
        }

        [SerializeField]
        private List<ERAnimationState> states = new List<ERAnimationState>();
        public List<ERAnimationState> States
        {
            get
            {
                if (states == null)
                {
                    states = new List<ERAnimationState>();
                }
                else
                {
                    for (int i = 0; i < states.Count; i++)
                    {
                        if (states[i] == null)
                        {
                            states.RemoveAt(i);
                            i--;
                        }
                    }
                }
                return states;
            }
        }
        private List<ERAnimatorControllerUpdater> _updaters;

        [SerializeField]
        private Animator _animator;

        public Animator Animator
        {
            get
            {
                if (_animator == null)
                {
                    if (!TryGetComponent(out _animator))
                    {
                        _animator = GetComponentInParent<Animator>();
                    }
                }
                return _animator;
            }
        }

        private void Awake()
        {
            //克隆原状态和事件
            List<ERAnimationState> cloneStates = new List<ERAnimationState>();
            foreach (ERAnimationState state in States)
            {
                var cloneState = Instantiate(state);
                cloneState.linkedController = this;
                List<ERAnimationEvent> cloneEvents = new List<ERAnimationEvent>();
                foreach (var e in state.events)
                {
                    var cloneEvent = Instantiate(e);
                    List<ERAnimationArg> cloneArgs = new List<ERAnimationArg>();
                    foreach (var a in e.args)
                    {
                        var cloneArg = Instantiate(a);
                        cloneArgs.Add(cloneArg);
                    }
                    cloneEvent.linkedState = cloneState;
                    cloneEvent.args = cloneArgs;
                    cloneEvents.Add(cloneEvent);
                }
                cloneState.events = cloneEvents;
                cloneState.events.Sort((a, b) => {
                    if (a.line > b.line)
                    {
                        return 1;
                    }
                    else if (a.line == b.line)
                    {
                        return 0;
                    }
                    else
                    {
                        return -1;
                    }
                });
                cloneStates.Add(cloneState);
            }
            states = cloneStates;
            _updaters = ERAnimatorControllerUpdater.BuildUpdaters(States);

            //初始化事件与状态
            foreach (ERAnimationState state in States)
            {
                foreach (ERAnimationEvent e in state.events)
                {
                    e.Init();
                }
            }
        }

        private void Update()
        {
            if (Animator != null && !Mathf.Approximately(Animator.speed, 0))
            {
                //check update mode
                if (Animator.updateMode != AnimatorUpdateMode.Normal)
                {
                    Debug.LogWarning("Animator updateMode should be Normal, otherwise function may work incorrectly");
                }

                //update events
                for (int i = 0; i < Animator.layerCount; i++)
                {
                    ERAnimatorControllerUpdater updater = _updaters.Find(item => { return item.layer == i; });
                    if (updater != null)
                    {
                        AnimatorStateInfo info = Animator.GetCurrentAnimatorStateInfo(i);
                        float length = 0;
                        ERAnimationState state = updater.states.Find(item =>
                        {
                            string name = Animator.GetLayerName(i) + "." + item.linkedState;
                            return info.IsName(name);
                        });
                        if (state != null)
                        {
                            length = state.animationTime;
                        }
                        if (info.shortNameHash != 0)
                        {
                            updater.realTime = (info.loop ? info.normalizedTime % 1 : info.normalizedTime) * length;
                            updater.currentNamehash = info.shortNameHash;
                            updater.realAnimationTime = length;
                        }
                        else
                        {
                            updater.realTime = 0;
                            updater.currentNamehash = 0;
                            updater.realAnimationTime = 0;
                        }
                        //Debug.Log(info.shortNameHash);
                        info = Animator.GetNextAnimatorStateInfo(i);
                        length = 0;
                        state = updater.states.Find(item =>
                        {
                            string name = Animator.GetLayerName(i) + "." + item.linkedState;
                            return info.IsName(name);
                        });
                        if (state != null)
                        {
                            length = state.animationTime;
                        }
                        if (info.shortNameHash != 0)
                        {
                            updater.nextTime = (info.loop ? info.normalizedTime % 1 : info.normalizedTime) * length;
                            updater.nextNamehash = info.shortNameHash;
                            updater.nextAnimationTime = length;
                        }
                        else
                        {
                            updater.nextTime = 0;
                            updater.nextNamehash = 0;
                            updater.nextAnimationTime = 0;
                        }

                        updater.updated = true;
                        //Debug.Log(info.shortNameHash);
                    }
                }
            }
            foreach(ERAnimatorControllerUpdater updater in _updaters)
            {                
                if (updater.currentState != null)
                {
                    foreach (var e in updater.currentState.events)
                    {
                        e.Update();
                    }
                }
            }
        }

        private void LateUpdate()
        {
            if (Animator != null/* && !Mathf.Approximately(Animator.speed, 0)*/)
            {
                for (int i = 0; i < Animator.layerCount; i++)
                {
                    ERAnimatorControllerUpdater updater = _updaters.Find(item => { return item.layer == i; });
                    if (updater != null)
                    {
                        if (updater.updated)
                        {
                            AnimatorStateInfo currentInfo = Animator.GetCurrentAnimatorStateInfo(i);
                            AnimatorStateInfo nextInfo = Animator.GetNextAnimatorStateInfo(i);
                            //Debug.Log(currentInfo.shortNameHash);
                            //Debug.Log(nextInfo.shortNameHash);
                            float realTime;
                            float lastTime;
                            ERAnimationState nextState;
                            if (nextInfo.shortNameHash == 0)
                            {
                                if (currentInfo.shortNameHash == updater.nextNamehash)
                                {
                                    updater.realTime = updater.nextTime;
                                    updater.realAnimationTime = updater.nextAnimationTime;
                                }
                                realTime = (currentInfo.loop ? currentInfo.normalizedTime % 1 : currentInfo.normalizedTime) * updater.realAnimationTime;
                                lastTime = updater.realTime;
                                nextState = updater.states.Find(item =>
                                {
                                    string name = Animator.GetLayerName(i) + "." + item.linkedState;
                                    return currentInfo.IsName(name);
                                });
                            }
                            else
                            {
                                if (nextInfo.shortNameHash == updater.currentNamehash)
                                {
                                    updater.nextTime = updater.realTime;
                                    updater.nextAnimationTime = updater.realAnimationTime;
                                }
                                realTime = (nextInfo.loop ? nextInfo.normalizedTime % 1 : nextInfo.normalizedTime) * updater.nextAnimationTime;
                                lastTime = updater.nextTime;
                                nextState = updater.states.Find(item =>
                                {
                                    string name = Animator.GetLayerName(i) + "." + item.linkedState;
                                    return nextInfo.IsName(name);
                                });
                            }
                            if (nextState == updater.currentState)
                            {
                                if (nextState != null)
                                {
                                    if (realTime < lastTime)
                                    {
                                        if (currentInfo.normalizedTime >= 1)
                                        {
                                            foreach (ERAnimationEvent e in nextState.events)
                                            {
                                                e.OnStateReplay();
                                                if (lastTime < e.endTime)
                                                {
                                                    if (lastTime < e.startTime)
                                                    {
                                                        e.OnEnter();
                                                        e.OnLeave();
                                                    }
                                                    else if (realTime < e.startTime)
                                                    {
                                                        e.OnLeave();
                                                    }
                                                    else
                                                    {
                                                        e.OnLeave();
                                                        e.OnEnter();
                                                    }
                                                }
                                                else
                                                {
                                                    if (realTime < e.startTime)
                                                    {
                                                        continue;
                                                    }
                                                    else if (realTime < e.endTime)
                                                    {
                                                        e.OnEnter();
                                                    }
                                                    else
                                                    {
                                                        e.OnEnter();
                                                        e.OnLeave();
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            foreach (ERAnimationEvent e in nextState.events)
                                            {
                                                if (e.isPlaying)
                                                {
                                                    e.OnLeave();
                                                }
                                            }
                                            foreach (var e in nextState.events)
                                            {
                                                if (!e.isPlaying && realTime >= e.startTime && realTime < e.endTime || Mathf.Approximately(e.startTime, 0))
                                                {
                                                    e.OnEnter();
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        foreach (ERAnimationEvent e in nextState.events)
                                        {
                                            if (lastTime < e.startTime)
                                            {
                                                if (realTime >= e.endTime)
                                                {
                                                    e.OnEnter();
                                                    e.OnLeave();
                                                }
                                                else if (realTime < e.startTime)
                                                {
                                                    continue;
                                                }
                                                else
                                                {
                                                    e.OnEnter();
                                                }
                                            }
                                            else
                                            {
                                                if (realTime < e.endTime)
                                                {
                                                    continue;
                                                }
                                                else if (lastTime < e.endTime)
                                                {
                                                    e.OnLeave();
                                                }
                                                else
                                                {
                                                    continue;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (updater.currentState != null)
                                {
                                    foreach (ERAnimationEvent e in updater.currentState.events)
                                    {
                                        if (e.isPlaying)
                                        {
                                            e.OnLeave();
                                        }
                                        e.OnStateLeave();
                                    }
                                }
                                if (nextState != null)
                                {
                                    foreach (var e in nextState.events)
                                    {
                                        e.OnStateEnter();
                                        if (!e.isPlaying && realTime >= e.startTime && realTime < e.endTime || Mathf.Approximately(e.startTime, 0))
                                        {
                                            e.OnEnter();
                                        }
                                    }
                                }
                            }
                            updater.currentState = nextState;
                        }
                        updater.updated = false;
                    }
                }
            }
            foreach (ERAnimatorControllerUpdater updater in _updaters)
            {
                if (updater.currentState != null)
                {
                    foreach (var e in updater.currentState.events)
                    {
                        e.LateUpdate();
                    }
                }
            }
        }

        private void FixedUpdate()
        {
            foreach(ERAnimatorControllerUpdater updater in _updaters)
            {
                if (updater.currentState != null)
                {
                    foreach (var e in updater.currentState.events)
                    {
                        e.FixedUpdate();
                    }
                }
            }
        }
    }
}