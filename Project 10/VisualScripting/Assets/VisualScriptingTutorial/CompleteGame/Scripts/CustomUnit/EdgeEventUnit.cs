using System;
using Unity.VisualScripting;
using VisualScriptingTutorial;

// This file declare custom events for the game. When you write your own event, don't forget to Regenerate Units through
// the "Regenerate Unit" button in the Visual Script section of the Project Settings
// For how those event are triggered, check Move and MoveUpdate in MovingObject.cs

[UnitCategory("Events/Tutorial")]
[UnitTitle("Crossing Edge")]
public sealed class CrossEdgeEvent : GameObjectEventUnit<MovingObject>
{
    public static string EventHook = "CrossEdgeEvent";

    protected override string hookName => EventHook;

    [DoNotSerialize]
    public ValueOutput movingObject { get; private set; }

    protected override void Definition()
    {
        base.Definition();

        movingObject = ValueOutput<MovingObject>(nameof(movingObject));
    }
    
    protected override void AssignArguments(Flow flow, MovingObject args)
    {
        flow.SetValue(movingObject, args);
    }

    public override Type MessageListenerType { get; }
}

[UnitCategory("Events/Tutorial")]
[UnitTitle("Want To Cross Edge")]
public sealed class WantCrossEdgeEvent : GameObjectEventUnit<MovingObject>
{
    public static string EventHook = "WantCrossEdgeEvent";

    protected override string hookName => EventHook;

    [DoNotSerialize]
    public ValueOutput movingObject { get; private set; }

    protected override void Definition()
    {
        base.Definition();

        movingObject = ValueOutput<MovingObject>(nameof(movingObject));
    }
    
    protected override void AssignArguments(Flow flow, MovingObject args)
    {
        flow.SetValue(movingObject, args);
    }

    public override Type MessageListenerType { get; }
}