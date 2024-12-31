using UnityEngine;

public class WaitUntilNotExists : CustomYieldInstruction
{
    private readonly System.Object objectRef;
    public WaitUntilNotExists(System.Object obj)
    {
        this.objectRef = obj;
    }
    public override bool keepWaiting
    {
        get { return this.objectRef == null; }
    }
}