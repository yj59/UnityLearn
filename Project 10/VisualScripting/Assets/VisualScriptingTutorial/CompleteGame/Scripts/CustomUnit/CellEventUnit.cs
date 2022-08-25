using System;
using Unity.VisualScripting;
using VisualScriptingTutorial;

// This file declare custom events for the game. When you write your own event, don't forget to Regenerate Units through
// the "Regenerate Unit" button in the Visual Script section of the Project Settings
// For how those event are triggered, check Move and MoveUpdate in MovingObject.cs

[UnitCategory("Events/Tutorial")]
[UnitTitle("Enter Cell")]
public sealed class EnterCell :  GameObjectEventUnit<MovingObject>
{
    public static string EventHook = "EnterCellEvent";
    
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
[UnitTitle("Exit Cell")]
public sealed class ExitCell :  GameObjectEventUnit<MovingObject>
{
    public static string EventHook = "ExitCellEvent";

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
[UnitTitle("Want To Enter Cell")]
public sealed class WantToEnterCell :  GameObjectEventUnit<MovingObject>
{
    public static string EventHook = "WantToEnterCellEvent";

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
[UnitTitle("Want To Exit Cell")]
public sealed class WantToExitCell :  GameObjectEventUnit<MovingObject>
{
    public static string EventHook = "WantToExitCellEvent";

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
[UnitTitle("Player Entered Cell")]
public sealed class PlayerEnteredCellUnit :  GameObjectEventUnit<PlayerControl>
{
    public static string EventHook = "PlayerEnteredCellEvent";

    protected override string hookName => EventHook;

    [DoNotSerialize]
    public ValueOutput playerControl { get; private set; }

    protected override void Definition()
    {
        base.Definition();

        playerControl = ValueOutput<PlayerControl>(nameof(playerControl));
    }
    
    protected override void AssignArguments(Flow flow, PlayerControl args)
    {
        flow.SetValue(playerControl, args);
    }

    public override Type MessageListenerType { get; }
}