using UnityEngine;

public class WaitUntilExists : CustomYieldInstruction
{
    private readonly System.Object objectRef;

    public WaitUntilExists(System.Object obj)
    {
        this.objectRef = obj;
    }

    public override bool keepWaiting
    {
        get { return this.objectRef != null; }
    }
}