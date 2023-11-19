using Content.Client.SelectionBuffer;
using Robust.Client.ComponentTrees;
using Robust.Client.Graphics;
using Robust.Client.Input;
using Robust.Client.Player;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controllers;
using Robust.Shared.Input;
using Robust.Shared.Input.Binding;
using Robust.Shared.Map;
using Robust.Shared.Timing;
using System.Linq;

namespace Content.Client.UserInterface.Systems.DragSelect;

/// <summary>
/// A UIController that allows players to select multiple objects and tiles by clicking and dragging the mouse.
/// </summary>
public sealed class DragSelectUiController : UIController
{
    [Dependency] private readonly IOverlayManager _overlayManager = default!;
    [Dependency] private readonly IInputManager _inputManager = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly IEyeManager _eyeManager = default!;
    [Dependency] private readonly IMapManager _mapManager = default!;
    private SharedMapSystem MapSystem => _entityManager.System<SharedMapSystem>();

    [UISystemDependency] private readonly SelectionBufferSystem _selectionBuffer = default!;

    /// <summary>
    /// The point on screen where the player first left clicked. It is not converted to MapCoordinates
    /// until the Use button is released so that the selection box can "move" with the player.
    /// </summary>
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
        }
        else if (args.State == BoundKeyState.Up)
        {
            //It was released.

            //If there are no start coords somehow then we bail. Not sure if this would ever happen
            if (_curStartCoords == null)
                return false;

            //Safe to cast to non-nullable as we just checked above
            GetSelectedObjects((ScreenCoordinates) _curStartCoords, args.ScreenCoordinates);

            //Selection is complete so we disable the overlay
            Clear();
        }

        return false;
    }

    /// <summary>
    /// Every frame we update the overlay with the current mouse position. We expect non-relative coordinates.
    /// </summary>
    /// <param name="args"></param>
    public override void FrameUpdate(FrameEventArgs args)
    {
        //This is set when the Use action is pressed. If its null then we aren't drag selecting.
        if (_curStartCoords == null)
            return;

        _overlay.UpdateCoords(_curStartCoords, _inputManager.MouseScreenPosition);
    }

    /// <summary>
    /// Get the entities (excluding tiles) under the selection box.
    /// </summary>
    /// <param name="start">The point in screen space where the LMB was pressed.</param>
    /// <param name="end">The point in screen space where the LMB was released.</param>
    private void GetSelectedObjects(ScreenCoordinates start, ScreenCoordinates end)
    {
        //Covert to MapCoordinates.
        MapCoordinates startMC = _eyeManager.ScreenToMap(start);
        MapCoordinates endMC = _eyeManager.ScreenToMap(end);

        //Create a bounding box in world space from these.
        Box2 rect = new Box2(startMC.Position, endMC.Position);

        //From GameplayStateBase::GetClickableEntities. Query the Sprite Tree for every object
        //within the selection box.
        var spriteTree = _entityManager.EntitySysManager.GetEntitySystem<SpriteTreeSystem>();
        var entities = spriteTree.QueryAabb(startMC.MapId, rect);

        //Print out the names of the selected entities for debug purposes.
        Logger.Debug(string.Format("Count: {0}", entities.Count));

        //We get the entity name from the metadata component.
        var metadataQuery = _entityManager.GetEntityQuery<MetaDataComponent>();

        foreach (var e in entities)
        {
            _selectionBuffer.AddToSelection(e.Uid);

            if (metadataQuery.TryGetComponent(e.Uid, out var component))
            {
                Logger.Debug(string.Format("Entity name: {0}", component.EntityName));
            }
        }

        GetSelectedTiles(startMC, endMC);
    }

    /// <summary>
    /// Get the tiles under the selection box formed by the parameters. We pass MapCoordinates instead
    /// of a Box2 because we need some extra data from the MapCoordinates class that wouldn't be in a Box2.
    /// </summary>
    /// <param name="start">The point where the LMB was pressed.</param>
    /// <param name="end">The point where the LMB was released.</param>
    private void GetSelectedTiles(MapCoordinates start, MapCoordinates end)
    {
        if (_mapManager.TryFindGridAt(start.MapId, start.Position, out var gridUid, out var mapGrid))
        {
            Box2 box = new Box2(start.Position, end.Position);

            //We do not want to ignore empty tiles
            //GetTilesIntersecting is used instead of GetLocalTilesIntersecting because the former
            //allows us to use boxes that aren't in "top left to bottom right" form
            var tileRefs = MapSystem.GetTilesIntersecting(gridUid, mapGrid, box, false);

            //Print some stuff for debug purposes.
            Logger.Debug(string.Format("Count of tileRefs: {0}", tileRefs.Count()));

            foreach (var t in tileRefs)
            {
                Logger.Debug(string.Format("{0}", t.ToString()));
            }
        }
    }

    /// <summary>
    /// Disables the overlay and clears the starting coordinates.
    /// </summary>
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
