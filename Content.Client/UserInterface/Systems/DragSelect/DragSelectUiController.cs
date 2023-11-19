using Content.Shared.Input;
using Robust.Client.Graphics;
using Robust.Client.Input;
using Robust.Client.Player;
using Robust.Client.UserInterface.Controllers;
using Robust.Shared.Input;
using Robust.Shared.Input.Binding;
using Robust.Shared.Map;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Client.UserInterface.Systems.DragSelect;

public sealed class DragSelectUiController : UIController
{
    [Dependency] private readonly IOverlayManager _overlayManager = default!;
    [Dependency] private readonly IEyeManager _eyeManager = default!;
    [Dependency] private readonly IInputManager _inputManager = default!;

    //Need to store the ScreenCoords as well
    private ScreenCoordinates? _curStartCoords;

    private Overlays.DragSelectOverlay _overlay = default!;
    public override void Initialize()
    {
        base.Initialize();
        _overlay = new Overlays.DragSelectOverlay();
        SubscribeLocalEvent<LocalPlayerAttachedEvent>(OnPlayerAttach);
        SubscribeLocalEvent<LocalPlayerDetachedEvent>(OnPlayerDetached);
    }

    private bool HandleDragSelect(in PointerInputCmdHandler.PointerInputCmdArgs args)
    {
        //Is it pressed or released? Check Keyboard.Key.MouseLeft
        if (args.State == BoundKeyState.Down)
        {
            //It was pressed.
            _curStartCoords = args.ScreenCoordinates;

            //Clear the selected objects
        }
        else if (args.State == BoundKeyState.Up)
        {
            //It was released.

            //If there are no start coords somehow then we bail.
            if (_curStartCoords == null)
                return false;

            //Safe to cast to non-nullable as we just checked above
            GetSelectedObjects((ScreenCoordinates) _curStartCoords, args.ScreenCoordinates);

            Clear();
        }

        return false;
    }

    public override void FrameUpdate(FrameEventArgs args)
    {
        //This is set when the Use action is pressed. If its null then we aren't drag selecting.
        if (_curStartCoords == null)
            return;

        _overlay.UpdateCoords(_curStartCoords, _inputManager.MouseScreenPosition);
    }

    private void GetSelectedObjects(ScreenCoordinates start, ScreenCoordinates end)
    {
        //Do we select tiles or just entities?

    }

    private void Clear()
    {
        _curStartCoords = null;

        _overlay.Disable();
    }

    private void OnPlayerAttach(LocalPlayerAttachedEvent ev)
    {
        Clear();
        _overlayManager.AddOverlay(_overlay);

        //We do NOT want to ignoreUp.
        CommandBinds.Builder.Bind(EngineKeyFunctions.Use, new PointerInputCmdHandler(HandleDragSelect, false))
            .Register<DragSelectUiController>();
    }
    private void OnPlayerDetached(LocalPlayerDetachedEvent ev)
    {
        _overlayManager.RemoveOverlay(_overlay);
        Clear();
        CommandBinds.Unregister<DragSelectUiController>();
    }
}
