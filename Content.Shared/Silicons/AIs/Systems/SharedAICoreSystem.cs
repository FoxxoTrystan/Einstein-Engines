using Content.Shared.Silicons.AIs.Components;
using Content.Shared.Mobs;
using Content.Shared.Mind.Components;
using Content.Shared.Examine;
using Content.Shared.Actions;
using Content.Shared.Mind;

namespace Content.Shared.Silicons.AIs
{
    public abstract class SharedAICoreSystem : EntitySystem
    {
        [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;
        [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<AICoreComponent, ComponentStartup>(OnStartup);
            SubscribeLocalEvent<AICoreComponent, MindAddedMessage>(OnMindAdded);
            SubscribeLocalEvent<AICoreComponent, MindRemovedMessage>(OnMindRemoved);
            SubscribeLocalEvent<AICoreComponent, ExaminedEvent>(OnExamined);
            SubscribeLocalEvent<AICoreComponent, MobStateChangedEvent>(OnMobState);
            SubscribeLocalEvent<AICoreComponent, GetCharactedDeadIcEvent>(OnGetDeadIC);

            SubscribeLocalEvent<AICoreComponent, MapInitEvent>(OnMapInit);
            SubscribeLocalEvent<AICoreComponent, ComponentShutdown>(OnShutdown);
        }

        private void OnStartup(EntityUid uid, AICoreComponent component, ComponentStartup args)
        {
            if (!TryComp(uid, out AppearanceComponent? appearance))
                return;

            if (TryComp<MindContainerComponent>(uid, out var mind) && mind.HasMind)
            {
                _appearance.SetData(uid, AICoreVisuals.NoMind, false, appearance);
            }
        }

        private void OnExamined(EntityUid uid, AICoreComponent component, ExaminedEvent args)
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

        private void OnMobState(EntityUid uid, AICoreComponent component, MobStateChangedEvent args)
        {
            if (!TryComp(uid, out AppearanceComponent? appearance))
                return;

            if (args.NewMobState == MobState.Dead)
            {
                _appearance.SetData(uid, AICoreVisuals.Dead, true, appearance);
            }
            else
            {
                _appearance.SetData(uid, AICoreVisuals.Dead, false, appearance);
            }
        }

        private void OnMindAdded(EntityUid uid, AICoreComponent component, MindAddedMessage args)
        {
            if (!TryComp(uid, out AppearanceComponent? appearance))
                return;

            _appearance.SetData(uid, AICoreVisuals.NoMind, false, appearance);
        }

        private void OnMindRemoved(EntityUid uid, AICoreComponent component, MindRemovedMessage args)
        {
            if (!TryComp(uid, out AppearanceComponent? appearance))
                return;

            _appearance.SetData(uid, AICoreVisuals.NoMind, true, appearance);
        }

        private void OnMapInit(EntityUid uid, AICoreComponent component, MapInitEvent args)
        {
            _actionsSystem.AddAction(uid, ref component.MidiAction, component.MidiActionId);
        }
        private void OnShutdown(EntityUid uid, AICoreComponent component, ComponentShutdown args)
        {
            _actionsSystem.RemoveAction(uid, component.MidiAction);
        }

        private void OnGetDeadIC(EntityUid uid, AICoreComponent component, ref GetCharactedDeadIcEvent args)
        {
            args.Dead = true;
        }
    }

}
