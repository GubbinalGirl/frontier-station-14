using Robust.Client.Graphics;
using Robust.Shared.Map;

namespace Content.Client.UserInterface.Systems.DragSelect.Overlays;

public sealed class DragSelectOverlay : Overlay
{
    private ScreenCoordinates? _start;
    private ScreenCoordinates? _end;

    protected override void Draw(in OverlayDrawArgs args)
    {
        //Don't draw if there isn't a valid rectangle.
        if (_start == null || _end == null)
            return;
    }

    public void UpdateCoords(ScreenCoordinates? start, ScreenCoordinates? end)
    {
        _start = start;
        _end = end;
    }

    public void Disable()
    {
        _start = null;
        _end = null;
    }
}
