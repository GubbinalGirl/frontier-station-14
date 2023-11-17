using Robust.Client.Graphics;
using Robust.Client.Input;
using Robust.Client.Player;
using Robust.Client.UserInterface.Controllers;

namespace Content.Client.UserInterface.Systems.DragSelect;

public sealed class DragSelectUiController : UIController
{
    [Dependency] private readonly IOverlayManager _overlayManager = default!;
    [Dependency] private readonly IEyeManager _eyeManager = default!;

    private Overlays.DragSelectOverlay _overlay = default!;
    public override void Initialize()
    {
        _overlay = new Overlays.DragSelectOverlay();
        SubscribeLocalEvent<LocalPlayerAttachedEvent>(OnPlayerAttach);
        SubscribeLocalEvent<LocalPlayerDetachedEvent>(OnPlayerDetached);
        SubscribeLocalEvent<MouseButtonEventArgs>(OnMouseMove);
        SubscribeLocalEvent<MouseButtonEventArgs>(OnMousePress);
    }

    private void OnMousePress(MouseButtonEventArgs ev)
    {
        throw new NotImplementedException();
    }

    private void OnMouseMove(MouseButtonEventArgs ev)
    {
        throw new NotImplementedException();
    }

    private void OnPlayerDetached(LocalPlayerDetachedEvent ev)
    {
        throw new NotImplementedException();
    }

    private void OnPlayerAttach(LocalPlayerAttachedEvent ev)
    {
        throw new NotImplementedException();
    }
}
