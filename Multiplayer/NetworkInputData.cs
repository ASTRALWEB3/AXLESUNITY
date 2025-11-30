using Fusion;
using UnityEngine;

/// <summary>
/// This struct defines the "packets" of input sent from Client to Server.
/// We send "Intent" (keys pressed), not position.
/// </summary>
public struct NetworkInputData : INetworkInput
{
    public Vector2 direction; // WASD or Joystick
    public NetworkButtons buttons;
}

// Helper to track specific button presses
public enum InputButtons
{
    Jump = 0,
    Fire = 1,
}