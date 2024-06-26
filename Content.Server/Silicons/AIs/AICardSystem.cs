using Content.Shared.Interaction;
using Content.Shared.Timing;
using Content.Shared.Silicons.AIs.Components;
using Content.Server.Instruments;
using Content.Server.Popups;
using Content.Shared.Popups;
using Robust.Shared.Player;
using Content.Shared.Mind;
using Content.Shared.Mobs.Systems;
using Content.Shared.Silicons.AIs;
using Content.Server.Silicons.Laws;
using Content.Shared.Silicons.Laws;
using Content.Shared.Silicons.Laws.Components;
using Content.Shared.Verbs;
using Content.Shared.Mind.Components;
using Content.Shared.Mind;
using Content.Shared.DoAfter;
using Robust.Server.Audio;
using Robust.Shared.Audio;

namespace Content.Server.Silicons.AIs;

public sealed class AICardSystem : SharedAICardSystem
{
    [Dependency] private readonly InstrumentSystem _instrumentSystem = default!;
    [Dependency] private readonly MetaDataSystem _metaData = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly MobStateSystem _mobStateSystem = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SiliconLawSystem _siliconLaw = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly AudioSystem _audioSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AICardComponent, AfterInteractEvent>(OnAfterInteract);
        SubscribeLocalEvent<AICardComponent, GetVerbsEvent<ActivationVerb>>(AddWipeVerb);
        SubscribeLocalEvent<AICardComponent, AIWipedEvent>(WipeAI);
    }

    private void AddWipeVerb(EntityUid uid, AICardComponent component, GetVerbsEvent<ActivationVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract)
            return;

        if (TryComp<MindContainerComponent>(uid, out var mind) && mind.HasMind)
        {
            ActivationVerb verb = new()
            {
                Text = Loc.GetString("ai-wipe-verb"),
                Act = () =>
                {
                    var actionargs = new DoAfterArgs(EntityManager, args.User, (float) 30, new AIWipedEvent(), uid, target: uid, used: uid)
                    {
                        BreakOnUserMove = false,
                        BreakOnTargetMove = true,
                        BreakOnDamage = true,
                        NeedHand = true,
                    };

                    if (!_doAfter.TryStartDoAfter(actionargs))
                        return;

                    _popup.PopupEntity(Loc.GetString("ai-wiping"), uid, type: PopupType.LargeCaution);
                }
            };
            args.Verbs.Add(verb);
        }
    }

    private void WipeAI(EntityUid uid, AICardComponent component, AIWipedEvent args)
    {
        if (args.Cancelled || args.Handled || args.Target == null || args.Used == null)
            return;

        if (!_mind.TryGetMind(uid, out var mindId, out var mind))
            return;

        _mind.TransferTo(mindId, null, mind: mind);
        _audioSystem.PlayPvs((SoundSpecifier) new SoundPathSpecifier("/Audio/Machines/borg_deathsound.ogg"), uid);
        _popup.PopupEntity(Loc.GetString("ai-wiped"), uid, type: PopupType.Large);

        AITurningOff(uid);

        args.Handled = true;
    }

    private void OnAfterInteract(EntityUid uid, AICardComponent component, AfterInteractEvent args)
    {
        if (!args.CanReach || args.Target == null || args.Handled)
            return;

        MindComponent? aiMind = null;
        MindComponent? targetMind = null;

        if (HasComp<AICoreComponent>(args.Target))
        {
            if(!_mind.TryGetMind(uid, out var aiMindId, out aiMind)){
                aiMind = null;
            };

            if(!_mind.TryGetMind((EntityUid) args.Target, out var targetMindId, out targetMind)){
                targetMind = null;
            };

            if (aiMind != null)
            {
                if (_mobStateSystem.IsDead((EntityUid) args.Target))
                {
                    _popup.PopupEntity(Loc.GetString("ai-core-dead"), uid);
                }
                else
                {
                    if (targetMind != null)
                    {
                        _popup.PopupEntity(Loc.GetString("ai-core-has-mind"), uid);
                    }
                    else
                    {
                        TransferAI(uid, (EntityUid) args.Target, aiMind, aiMindId);
                        _popup.PopupEntity(Loc.GetString("ai-uploaded"), uid);
                    }
                }
            }
            else
            {
                if (targetMind != null)
                {
                    TransferAI((EntityUid) args.Target, uid, targetMind, targetMindId);
                    _popup.PopupEntity(Loc.GetString("ai-downloaded"), uid);
                }
                else
                {
                    _popup.PopupEntity(Loc.GetString("ai-core-not-found"), uid);
                }
            }
        }
        args.Handled = true;
    }

    public void TransferAI(EntityUid uid, EntityUid target, MindComponent mind, EntityUid mindId)
    {
        // Transfer Mind
        _mind.TransferTo(mindId, null);
        _mind.TransferTo(mindId, target, ghostCheckOverride: true, false, mind);

        // Transfer Name
        if (TryComp<MetaDataComponent>(target, out var metadata))
        {
            if (TryComp<MetaDataComponent>(uid, out var metadata2))
            {
                var proto = metadata.EntityPrototype;
                if (proto != null)
                {
                    _metaData.SetEntityName(target, metadata2.EntityName);
                }
            }
        }

        AITurningOff(uid);

        // Transfer ScreenState
        if (TryComp(target, out AppearanceComponent? appearance))
        {
            if (TryComp<AICoreComponent>(uid, out var corecomponent))
            {
                _appearance.SetData(target, AICardVisuals.Screen, corecomponent.ScreenState, appearance);

                if (TryComp<AICardComponent>(target, out var cardcomponent2))
                    cardcomponent2.ScreenState = corecomponent.ScreenState;
            }

            if (TryComp<AICardComponent>(uid, out var cardcomponent))
            {
                _appearance.SetData(target, AICoreVisuals.Screen, cardcomponent.ScreenState, appearance);

                if (TryComp<AICardComponent>(target, out var corecomponent2))
                    corecomponent2.ScreenState = cardcomponent.ScreenState;
            }
        }

        // TODO Transfer Laws
        // if (TryComp<SiliconLawBoundComponent>(uid, out var lawBound))
        // {
        //     if (TryComp<SiliconLawBoundComponent>(target, out var lawBoundTarget))
        //     {
        //         var laws = _siliconLaw.GetLaws(uid, lawBound);
        //         var lawsTarget = _siliconLaw.GetLaws(target, lawBoundTarget);
        //         laws = lawsTarget.Clone();
        //     }
        // }


        // TODO Admin log the action.
    }

    public void AITurningOff(EntityUid uid)
    {
        //  Close the instrument interface if it was open
        //  before closing
        if (HasComp<ActiveInstrumentComponent>(uid) && TryComp<ActorComponent>(uid, out var actor))
        {
            _instrumentSystem.ToggleInstrumentUi(uid, actor.PlayerSession);
        }

        //  Stop instrument
        if (TryComp<InstrumentComponent>(uid, out var instrument)) _instrumentSystem.Clean(uid, instrument);
        if (TryComp<MetaDataComponent>(uid, out var metadata))
        {
            var proto = metadata.EntityPrototype;
            if (proto != null)
                _metaData.SetEntityName(uid, proto.Name);
        }
    }
}
