using Content.Shared.Silicons.AIs;
using Content.Shared.Silicons.AIs.Components;
using Content.Server.SurveillanceCamera;

namespace Content.Server.Silicons.AIs;

public sealed class AICoreSystem : SharedAICoreSystem
{
    [Dependency] private readonly SurveillanceCameraSystem _surveillanceCameraSystem = default!;
    [Dependency] private readonly SharedEyeSystem _sharedEyeSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AICoreComponent, SurveillanceCameraDeactivateEvent>(OnSurveillanceCameraDeactivate);
    }

    private void OnSurveillanceCameraDeactivate(EntityUid uid, AICoreComponent component, SurveillanceCameraDeactivateEvent args)
    {
        if (component.ActiveCamera == null)
            return;

        if (component.ActiveCamera != args.Camera)
            return;

        ViewCore(uid);
    }

    private void ViewCamera(EntityUid uid, EntityUid camera)
    {
        if (TryComp<AICoreComponent>(uid, out var core))
        {
            _surveillanceCameraSystem.AddActiveViewer(camera, uid);
            _sharedEyeSystem.SetTarget(uid, camera);

            if (core.ActiveCamera != null)
            {
                _surveillanceCameraSystem.RemoveActiveViewer((EntityUid) core.ActiveCamera, uid);
            }

            core.ActiveCamera = camera;
        }
    }

    private void ViewCore(EntityUid uid)
    {
        if (TryComp<AICoreComponent>(uid, out var core))
        {
            if (core.ActiveCamera == null)
                return;

            _sharedEyeSystem.SetTarget(uid, uid);
            _surveillanceCameraSystem.RemoveActiveViewer((EntityUid) core.ActiveCamera, uid);
            core.ActiveCamera = null;
        }
    }
}
