using System;
using Unity.VisualScripting;

// This file declare custom events for the game. When you write your own event, don't forget to Regenerate Units through
// the "Regenerate Unit" button in the Visual Script section of the Project Settings
// For how this event is triggered, check TurnManager.cs

[UnitCategory("Events/Tutorial")]
[UnitTitle("Turn Tick")]
public sealed class TurnEvent :  GameObjectEventUnit<EmptyEventArgs>
{
    public static string EventHook = "TurnEvent";

    protected override string hookName => EventHook;
    
    public override Type MessageListenerType { get; }
}
