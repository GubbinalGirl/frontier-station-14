using Robust.Client.ComponentTrees;
using Robust.Client.Graphics;
using Robust.Client.Input;
using Robust.Client.Player;
using Robust.Client.UserInterface.Controllers;
using Robust.Shared.Input;
using Robust.Shared.Input.Binding;
using Robust.Shared.Map;
using Robust.Shared.Timing;
using System.Linq;

namespace Content.Client.UserInterface.Systems.DragSelect;

public sealed class DragSelectUiController : UIController
{
    [Dependency] private readonly IOverlayManager _overlayManager = default!;
    [Dependency] private readonly IInputManager _inputManager = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly IEyeManager _eyeManager = default!;
    [Dependency] private readonly IMapManager _mapManager = default!;
    private SharedMapSystem MapSystem => _entityManager.System<SharedMapSystem>();

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

    public override void FrameUpdate(FrameEventArgs args)
    {
        //This is set when the Use action is pressed. If its null then we aren't drag selecting.
        if (_curStartCoords == null)
            return;

        _overlay.UpdateCoords(_curStartCoords, _inputManager.MouseScreenPosition);
    }

    private void GetSelectedObjects(ScreenCoordinates start, ScreenCoordinates end)
    {
        // Find all the entities intersecting our click

        //Covert to MapCoordinates
        MapCoordinates startMC = _eyeManager.ScreenToMap(start);
        MapCoordinates endMC = _eyeManager.ScreenToMap(end);

        //Create a bounding box from these.
        Box2 rect = new Box2(startMC.Position, endMC.Position);

        //From GameplayStateBase::GetClickableEntities
        var spriteTree = _entityManager.EntitySysManager.GetEntitySystem<SpriteTreeSystem>();
        var entities = spriteTree.QueryAabb(startMC.MapId, rect);

        Logger.Debug(string.Format("Count: {0}", entities.Count));

        var metadataQuery = _entityManager.GetEntityQuery<MetaDataComponent>();

        foreach (var e in entities)
        {
            if (metadataQuery.TryGetComponent(e.Uid, out var component))
            {
                Logger.Debug(string.Format("Entity name: {0}", component.EntityName));
            }
        }

        GetSelectedTiles(startMC, endMC);
    }

    private void GetSelectedTiles(MapCoordinates start, MapCoordinates end)
    {
        //TODO: This is not working when the selection is not done from top left to bottom right
        if (_mapManager.TryFindGridAt(start.MapId, start.Position, out var gridUid, out var mapGrid))
        {
            Box2 box = new Box2(start.Position, end.Position);
            //We do not want to ignore empty tiles
            var tileRefs = MapSystem.GetLocalTilesIntersecting(gridUid, mapGrid, box, false);
            Logger.Debug(string.Format("Count of tileRefs: {0}", tileRefs.Count()));

            foreach (var t in tileRefs)
            {
                Logger.Debug(string.Format("{0}", t.ToString()));
            }
        }
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
