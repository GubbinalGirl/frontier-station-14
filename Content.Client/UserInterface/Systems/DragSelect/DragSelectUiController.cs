using Content.Shared.Input;
using Robust.Client.Graphics;
using Robust.Client.Input;
using Robust.Client.Player;
using Robust.Client.UserInterface.Controllers;
using Robust.Shared.Input;
using Robust.Shared.Input.Binding;
using Robust.Shared.Map;
using Robust.Shared.Player;

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
        base.Initialize();
        _overlay = new Overlays.DragSelectOverlay();
        SubscribeLocalEvent<LocalPlayerAttachedEvent>(OnPlayerAttach);
        SubscribeLocalEvent<LocalPlayerDetachedEvent>(OnPlayerDetached);
        SubscribeLocalEvent<MouseMoveEventArgs>(OnMouseMove);
        SubscribeLocalEvent<MouseButtonEventArgs>(OnMouseButtonEvent);
        SubscribeLocalEvent<MouseEnterLeaveEventArgs>(OnMouseEnterLeave);
    }

    private bool HandleDragSelect(in PointerInputCmdHandler.PointerInputCmdArgs args)
    {
        Logger.Debug("YAY@!");

        return false;
    }

    private void OnMouseButtonEvent(MouseButtonEventArgs ev)
    {
        Logger.Debug("OnMouseButtonEvent");
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

    private void OnMouseMove(MouseMoveEventArgs ev)
    {
        Logger.Debug("OnMoiseMove");
        //If the Left Button is not pressed then we don't care.
        if (!_inputManager.IsKeyDown(Keyboard.Key.MouseLeft))
            return;

        _overlay.UpdateCoords(_curScreenStartCoords, ev.Position);
    }

    private void OnPlayerAttach(LocalPlayerAttachedEvent ev)
    {
        Logger.Debug("OnPlayerAttach");
        Clear();
        _overlayManager.AddOverlay(_overlay);

        CommandBinds.Builder.Bind(EngineKeyFunctions.Use, new PointerInputCmdHandler(HandleDragSelect)).Register<DragSelectUiController>();
    }
    private void OnPlayerDetached(LocalPlayerDetachedEvent ev)
    {
        _overlayManager.RemoveOverlay(_overlay);
        Clear();
        CommandBinds.Unregister<DragSelectUiController>();
    }
    private void OnMouseEnterLeave(MouseEnterLeaveEventArgs ev)
    {
        //For now we just clear,
        //but it will be important for UX reasons to handle this more gracefully.
        Clear();
    }
}
