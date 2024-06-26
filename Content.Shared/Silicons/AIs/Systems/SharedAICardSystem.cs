using Content.Shared.Actions;
using Content.Shared.Silicons.AIs.Components;
using Content.Shared.Mind.Components;
using Content.Shared.Examine;
using Robust.Shared.Serialization;
using Content.Shared.DoAfter;

namespace Content.Shared.Silicons.AIs
{
    public abstract class SharedAICardSystem : EntitySystem
    {
        [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;
        [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<AICardComponent, ComponentStartup>(OnStartup);
            SubscribeLocalEvent<AICardComponent, MindAddedMessage>(OnMindAdded);
            SubscribeLocalEvent<AICardComponent, MindRemovedMessage>(OnMindRemoved);
            SubscribeLocalEvent<AICardComponent, ExaminedEvent>(OnExamined);

            SubscribeLocalEvent<AICardComponent, MapInitEvent>(OnMapInit);
            SubscribeLocalEvent<AICardComponent, ComponentShutdown>(OnShutdown);
        }

        private void OnStartup(EntityUid uid, AICardComponent component, ComponentStartup args)
        {
            if (!TryComp(uid, out AppearanceComponent? appearance))
                return;

            if (TryComp<MindContainerComponent>(uid, out var mind) && mind.HasMind)
            {
                _appearance.SetData(uid, AICardVisuals.NoMind, false, appearance);
            }
        }

        private void OnExamined(EntityUid uid, AICardComponent component, ExaminedEvent args)
        {
            if (!args.IsInDetailsRange)
                return;

            if (TryComp<MindContainerComponent>(uid, out var mind) && mind.HasMind)
            {
                args.PushMarkup(Loc.GetString("ai-has-mind"));
            }
            else
            {
                args.PushMarkup(Loc.GetString("ai-no-mind"));
            }
        }

        private void OnMindAdded(EntityUid uid, AICardComponent component, MindAddedMessage args)
        {
            if (!TryComp(uid, out AppearanceComponent? appearance))
                return;

            _appearance.SetData(uid, AICardVisuals.NoMind, false, appearance);
        }

        private void OnMindRemoved(EntityUid uid, AICardComponent component, MindRemovedMessage args)
        {
            if (!TryComp(uid, out AppearanceComponent? appearance))
                return;

            _appearance.SetData(uid, AICardVisuals.NoMind, true, appearance);
        }

        private void OnMapInit(EntityUid uid, AICardComponent component, MapInitEvent args)
        {
            _actionsSystem.AddAction(uid, ref component.MidiAction, component.MidiActionId);
        }
        private void OnShutdown(EntityUid uid, AICardComponent component, ComponentShutdown args)
        {
            _actionsSystem.RemoveAction(uid, component.MidiAction);
        }
    }
}

[Serializable, NetSerializable]
public sealed partial class AIWipedEvent : SimpleDoAfterEvent
{
}
