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

    //Need to store the ScreenCoords as well
    private ScreenCoordinates? _curScreenStartCoords;
    private ScreenCoordinates? _curScreenEndCoords;

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
        SubscribeLocalEvent<MouseEnterLeaveEventArgs>(OnMouseEnterLeave);
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
            _curScreenStartCoords = ev.Position;

            _curEndCoords = null;
            _curScreenEndCoords = null;
            //Clear the selected objects
        }
        else
        {
            //It was released.
            if (_curStartCoords == null)
                return;

            _curEndCoords = _eyeManager.ScreenToMap(ev.Position);
            _curScreenEndCoords = ev.Position;

            GetSelectedObjects();
        }

        _overlay.UpdateCoords(_curScreenStartCoords, ev.Position);
    }

    private void GetSelectedObjects()
    {
        //Do we select tiles or just entities?

    }

    private void Clear()
    {
        _curStartCoords = null;
        _curScreenStartCoords = null;
        _curEndCoords = null;
        _curScreenEndCoords = null;

        _overlay.Disable();
    }

    private void OnMouseMove(MouseButtonEventArgs ev)
    {
        //If the Left Button is not pressed then we don't care.
        if (!_inputManager.IsKeyDown(Keyboard.Key.MouseLeft))
            return;

        _overlay.UpdateCoords(_curScreenStartCoords, ev.Position);
    }

    private void OnPlayerAttach(LocalPlayerAttachedEvent ev)
    {
        Clear();
        _overlayManager.AddOverlay(_overlay);
    }
    private void OnPlayerDetached(LocalPlayerDetachedEvent ev)
    {
        _overlayManager.RemoveOverlay(_overlay);
        Clear();
    }
    private void OnMouseEnterLeave(MouseEnterLeaveEventArgs ev)
    {
        //For now we just clear,
        //but it will be important for UX reasons to handle this more gracefully.
        Clear();
    }

    private void UpdateOverlay()
    {
        //The overlay needs to know screencoords, not mapcoords
        //Its also not interested in the end position, only where the mouse is now.
    }
}
