using Godot;

public partial class DeviceInput : RefCounted
{
    [Signal]
    public delegate void ConnectionChangedEventHandler(bool connected);

    public int Device { get; private set; } = -1;
    public bool Connected { get; private set; } = true;

    public DeviceInput(int device)
    {
        Device = device;
        Input.JoyConnectionChanged += OnJoyConnectionChanged;
    }

    public bool IsKeyboard() => Device < 0;
    public bool IsJoypad() => Device >= 0;
    public string GetGuid()
    {
        if(IsKeyboard())
        {
            return "Keyboard";
        }
        else
        {
            return Input.GetJoyGuid(Device);
        }
    }

    public string GetName()
    {
        if(IsKeyboard())
        {
            return "Keyboard";
        }
        else
        {
            return Input.GetJoyName(Device);
        }
    }

    public float GetVibrationDuration()
    {
        if (IsKeyboard()) return 0.0f;
        return Input.GetJoyVibrationDuration(Device);
    }

    public Vector2 GetVibrationStrength()
    {
        if (IsKeyboard()) return Vector2.Zero;
        return Input.GetJoyVibrationStrength(Device);
    }

    public bool IsKnown()
    {
        if (IsKeyboard()) return true;
        return Input.IsJoyKnown(Device);
    }

    public void StartVibration(float weakMagnitude, float strongMagnitude, float duration = 0.0f)
    {
        if (IsKeyboard()) return;
        Input.StartJoyVibration(Device, weakMagnitude, strongMagnitude, duration);
    }

    public void StopVibration()
    {
        if (IsKeyboard()) return;
        Input.StopJoyVibration(Device);
    }

    public float GetActionRawStrength(StringName action, bool exactMatch = false)
    {
        if (!Connected) return 0.0f;
        return MultiplayerInput.GetActionRawStrength(Device, action, exactMatch);
    }

    public float GetActionStrength(StringName action, bool exactMatch = false)
    {
        if (!Connected) return 0.0f;
        return MultiplayerInput.GetActionStrength(Device, action, exactMatch);
    }

    public float GetAxis(StringName negativeAction, StringName positiveAction)
    {
        if (!Connected) return 0.0f;
        return MultiplayerInput.GetAxis(Device, negativeAction, positiveAction);
    }

    public Vector2 GetVector(StringName negativeX, StringName positiveX, StringName negativeY, StringName positiveY, float deadzone = -1.0f)
    {
        if (!Connected) return Vector2.Zero;
        return MultiplayerInput.GetVector(Device, negativeX, positiveX, negativeY, positiveY, deadzone);
    }

    public bool IsActionJustPressed(StringName action, bool exactMatch = false)
    {
        if (!Connected) return false;
        return MultiplayerInput.IsActionJustPressed(Device, action, exactMatch);
    }

    public bool IsActionJustReleased(StringName action, bool exactMatch = false)
    {
        if (!Connected) return false;
        return MultiplayerInput.IsActionJustReleased(Device, action, exactMatch);
    }

    public bool IsActionPressed(StringName action, bool exactMatch = false)
    {
        if (!Connected) return false;
        return MultiplayerInput.IsActionPressed(Device, action, exactMatch);
    }

    public void TakeUiActions()
    {
        if (!Connected) return;
        MultiplayerInput.SetUiActionDevice(Device);
    }

    void OnJoyConnectionChanged(long device, bool connected)
    {
        if(device == Device)
        {
            EmitSignal(SignalName.ConnectionChanged, connected);
            Connected = connected;
        }
    }
}
