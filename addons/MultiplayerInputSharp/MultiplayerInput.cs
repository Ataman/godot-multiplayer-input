using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class MultiplayerInput : Node
{
    static StringName[] coreActions { get; set; } = new StringName[0];
    static Dictionary<int, Dictionary<StringName, StringName>> deviceActions { get; set; } = new Dictionary<int, Dictionary<StringName, StringName>>();
    static string[] ignoredGuids { get; set; } = new string[0];

    static MultiplayerInput()
    {
        Input.JoyConnectionChanged += OnJoyConnectionChanged;
        Reset();
    }

    public static void Reset()
    {
        InputMap.LoadFromProjectSettings();
        coreActions = InputMap.GetActions().ToArray();
        foreach(StringName action in coreActions)
        {
            foreach(InputEvent e in InputMap.ActionGetEvents(action))
            {
                if(IsJoypadEvent(e) && !IsUiAction(action))
                {
                    e.Device = 8;
                }
            }
        }
    }

    static void OnJoyConnectionChanged(long device, bool connected)
    {
        if (device >= int.MinValue && device <= int.MaxValue)
        {
            if (connected)
            {
                CreateActionsForDevice((int)device);
            }
            else
            {
                DeleteActionsForDevice((int)device);
            }
        }
        throw new ArgumentOutOfRangeException(nameof(device));
    }

    static void CreateActionsForDevice(int device)
    {
        if(ignoredGuids.Contains(Input.GetJoyGuid(device))) {
            return;
        }
        deviceActions[device] = new Dictionary<StringName, StringName>();
        foreach(StringName coreAction in coreActions)
        {
            StringName newAction = $"{device}{coreAction}";
            float deadzone = InputMap.ActionGetDeadzone(coreAction);

            InputEvent[] events = InputMap.ActionGetEvents(coreAction).Where(IsJoypadEvent).ToArray();

            if(events.Length > 0)
            {
                InputMap.AddAction(newAction, deadzone);
                deviceActions[device][coreAction] = newAction;
                foreach(InputEvent e in events)
                {
                    InputEvent newEvent = e.Duplicate() as InputEvent;
                    newEvent.Device = device;

                    InputMap.ActionAddEvent(newAction, newEvent);
                }
            }
        }
    }

    static void DeleteActionsForDevice(int device)
    {
        if(deviceActions.ContainsKey(device))
        {
            deviceActions.Remove(device);
        }
        List<StringName> actionsToErase = new List<StringName>();
        string deviceNumStr = device.ToString();

        foreach(StringName action in InputMap.GetActions())
        {
            string actionStr = action;
            var maybeDevice = actionStr.Substr(0, deviceNumStr.Length);
            if (maybeDevice.Equals(deviceNumStr))
                actionsToErase.Add(action);
        }

        foreach(StringName action in actionsToErase)
        {
            InputMap.EraseAction(action);
        }
    }

    static public float GetActionRawStrength(int device, StringName action, bool exactMatch = false)
    {
        if(device >= 0)
        {
            action = GetActionName(device, action);
        }
        return Input.GetActionRawStrength(action, exactMatch);
    }

    static public float GetActionStrength(int device, StringName action, bool exactMatch= false)
    {
        if (device >= 0)
        {
            action = GetActionName(device, action);
        }
        return Input.GetActionStrength(action, exactMatch);
    }

    static public float GetAxis(int device, StringName negativeAction, StringName positiveAction)
    {
        if (device >= 0)
        {
            negativeAction = GetActionName(device, negativeAction);
            positiveAction = GetActionName(device, positiveAction);
        }
        return Input.GetAxis(negativeAction, positiveAction);
    }

    static public Vector2 GetVector(int device, StringName negativeX, StringName positiveX, StringName negativeY, StringName positiveY, float deadzone = -1.0f)
    {
        if (device >= 0)
        {
            negativeX = GetActionName(device, negativeX);
            positiveX = GetActionName(device, positiveX);
            negativeY = GetActionName(device, negativeY);
            positiveY = GetActionName(device, positiveY);
        }
        return Input.GetVector(negativeX, positiveX, negativeY, positiveY);
    }

    static public bool IsActionJustPressed(int device, StringName action, bool exactMatch = false)
    {
        if (device >= 0)
        {
            action = GetActionName(device, action);
        }
        return Input.IsActionJustPressed(action, exactMatch);
    }

    static public bool IsActionJustReleased(int device, StringName action, bool exactMatch = false)
    {
        if (device >= 0)
        {
            action = GetActionName(device, action);
        }
        return Input.IsActionJustReleased(action, exactMatch);
    }

    static public bool IsActionPressed(int device, StringName action, bool exactMatch = false)
    {
        if (device >= 0)
        {
            action = GetActionName(device, action);
        }
        return Input.IsActionPressed(action, exactMatch);
    }

    static public StringName GetActionName(int device, StringName action)
    {
        if (device >= 0)
        {
            Assert(deviceActions.ContainsKey(device), $"Device {device} has no actions. Maybe the joypad is disconnected.");
            return deviceActions[device][action];
        }
        return action;
    }

    static public void SetUiActionDevice(int device)
    {
        Reset();
        if(device == -2)
        {
            return;
        }

        foreach(StringName action in InputMap.GetActions())
        {
            if (!IsUiAction(action))
            {
                continue;
            }

            if(device == -1)
            {
                foreach(InputEvent e in InputMap.ActionGetEvents(action))
                {
                    if(IsJoypadEvent(e))
                    {
                        InputMap.ActionEraseEvent(action, e);
                    }
                }
            }
            else
            {
                foreach(InputEvent e in InputMap.ActionGetEvents(action))
                {
                    if(IsJoypadEvent(e))
                    {
                        e.Device = device;
                    }
                    else
                    {
                        InputMap.ActionEraseEvent(action, e);
                    }
                }
            }
        }
    }

    static private bool IsJoypadEvent(InputEvent e) 
    {
        return e is InputEventJoypadButton || e is InputEventJoypadMotion;
    }

    static public bool IsUiAction(StringName action)
    {
        return action.ToString().StartsWith("ui_");
    }

    private static void Assert(bool cond, string msg)
    {
#if DEBUG
        if (cond) return;
        GD.PrintErr(msg);
        throw new ApplicationException($"Assert Failed: {msg}");
#endif
    }

}
