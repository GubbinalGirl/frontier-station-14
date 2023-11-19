using Robust.Client.Graphics;
using Robust.Shared.Map;

namespace Content.Client.UserInterface.Systems.DragSelect.Overlays;

public sealed class DragSelectOverlay : Overlay
{
    private ScreenCoordinates _start;
    private ScreenCoordinates _end;

    private bool _enabled = false;

    protected override void Draw(in OverlayDrawArgs args)
    {
        //Don't draw if there isn't a valid rectangle.
        if (!_enabled)
            return;

        Box2 rect = new Box2(_start.Position, _end.Position);
        Color color = new Color(0, 0, 200, 100);
        args.DrawingHandle.DrawCircle(_start.Position, 100, color);
    }

    //The overlay needs to be updated at three times. When the LB is pressed, when its released,
    //and when the mouse is moved with LB down
    public void UpdateCoords(ScreenCoordinates? start, ScreenCoordinates? end)
    {
        //This feels clunky, like it should be handled by the UiController
        if (start == null || end == null)
        {
            Disable();
            return;
        }

        _start = (ScreenCoordinates) start;
        _end = (ScreenCoordinates) end;

        _enabled = true;
    }

    public void Disable()
    {
        _enabled = false;
    }
}
