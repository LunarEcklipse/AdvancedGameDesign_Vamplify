using System;
using UnityEngine;
using UnityEngine.Events;

public class WaitForEvent : CustomYieldInstruction, IDisposable
{
    private bool eventRaised = false;
    private UnityEvent unityEvent;

    public WaitForEvent(UnityEvent newEvent)
    {
        this.unityEvent = newEvent;
        unityEvent.AddListener(OnEventRaised);
    }

    private void OnEventRaised()
    {
        eventRaised = true;
    }

    public override bool keepWaiting
    {
        get { return !eventRaised; }
    }

    public void Dispose()
    {
        if (unityEvent != null)
        {
            unityEvent.RemoveListener(OnEventRaised);
            unityEvent = null;
        }
    }
}