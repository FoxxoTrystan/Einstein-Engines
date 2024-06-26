using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.Silicons.AIs.Components;

[RegisterComponent, NetworkedComponent]

public sealed partial class AICardComponent : Component
{
    [DataField(serverOnly: true)]
    public EntProtoId? MidiActionId = "ActionPAIPlayMidi";

    [DataField(serverOnly: true)] // server only, as it uses a server-BUI event !type
    public EntityUid? MidiAction;

    [DataField("screenState")]
    public string ScreenState = "Blue";
}

[Serializable, NetSerializable]
public enum AICardLayers : byte
{
    Screen,
    NoMind,
    Light
}

[Serializable, NetSerializable]
public enum AICardVisuals : byte
{
    Screen,
    NoMind
}
