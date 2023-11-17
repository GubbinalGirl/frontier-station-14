using Robust.Client.Graphics;
using Robust.Client.Input;
using Robust.Client.Player;
using Robust.Client.UserInterface.Controllers;
using Robust.Shared.Map;

namespace Content.Client.UserInterface.Systems.DragSelect;

public sealed class DragSelectUiController : UIController
{
    [Dependency] private readonly IOverlayManager _overlayManager = default!;
    [Dependency] private readonly IEyeManager _eyeManager = default!;
    [Dependency] private readonly IInputManager _inputManager = default!;


    private MapCoordinates? _curStartCoords;
    private MapCoordinates? _curEndCoords;

    private Overlays.DragSelectOverlay _overlay = default!;
    public override void Initialize()
    {
        _overlay = new Overlays.DragSelectOverlay();
        SubscribeLocalEvent<LocalPlayerAttachedEvent>(OnPlayerAttach);
        SubscribeLocalEvent<LocalPlayerDetachedEvent>(OnPlayerDetached);
        SubscribeLocalEvent<MouseButtonEventArgs>(OnMouseMove);
        SubscribeLocalEvent<MouseButtonEventArgs>(OnMouseButtonEvent);
    }

    private void OnMouseButtonEvent(MouseButtonEventArgs ev)
    {
        if (ev.Button != Mouse.Button.Left)
            return;

        //Is it pressed or released? Check Keyboard.Key.MouseLeft
        if (_inputManager.IsKeyDown(Keyboard.Key.MouseLeft))
        {
            //It was pressed.
            _curStartCoords = _eyeManager.ScreenToMap(ev.Position);
            _curEndCoords = null;
            //Clear the selected objects
        }
        else
        {
            //It was released.
            if (_curStartCoords == null)
                return;

            _curEndCoords = _eyeManager.ScreenToMap(ev.Position);

            GetSelectedObjects();
        }
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

    private void GetSelectedObjects()
    {

    }
}
