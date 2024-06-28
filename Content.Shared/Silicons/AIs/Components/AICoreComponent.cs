using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.Silicons.AIs.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class AICoreComponent : Component
{
    [DataField(serverOnly: true)]
    public EntProtoId? MidiActionId = "ActionPAIPlayMidi";

    [DataField(serverOnly: true)] // server only, as it uses a server-BUI event !type
    public EntityUid? MidiAction;

    [DataField("screenState")]
    public string ScreenState = "Blue";

    [ViewVariables]
    public EntityUid? ActiveCamera { get; set; }
}

[Serializable, NetSerializable]
public enum AICoreLayers : byte
{
    Screen,
    NoMind,
    Dead
}

[Serializable, NetSerializable]
public enum AICoreVisuals : byte
{
    Screen,
    Dead,
    NoMind
}
