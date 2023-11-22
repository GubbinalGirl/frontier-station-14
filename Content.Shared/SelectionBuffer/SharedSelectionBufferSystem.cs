using System.Linq;
using System.Numerics;
using Robust.Shared.Serialization;
using Robust.Shared.Network;

namespace Content.Shared.SelectionBuffer;

/// <summary>
/// Handles selecting multiple objects and acting upon the group. 
/// </summary>
public abstract class SharedSelectionBufferSystem : EntitySystem
{
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly SharedTransformSystem _transSystem = default!;
    [Dependency] private readonly INetManager _netMan = default!;
    private SharedMapSystem MapSystem => _entityManager.System<SharedMapSystem>();
    public override void Initialize()
    {
        base.Initialize();

        SubscribeNetworkEvent<SelectionTranslateEvent>(OnTranslateSelection);
        SubscribeNetworkEvent<SelectionDeleteEvent>(OnDeleteSelection);
        SubscribeNetworkEvent<SelectionCopyEvent>(OnCopySelection);
    }

    private void OnCopySelection(SelectionCopyEvent ev)
    {
        throw new NotImplementedException();
    }

    private void OnDeleteSelection(SelectionDeleteEvent ev)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Translates (moves) the entire group.
    ///
    /// How do we handle tiles, fixtures, anchoring and connections?
    /// What happens when I copy/paste a bunch of walls into space?
    /// </summary>
    /// <param name="ev"></param>
    private void OnTranslateSelection(SelectionTranslateEvent ev)
    {
        if (!_netMan.IsServer)
            return;

        /*
         * This still has the issue that tiles are not copied so they just float in space.
         * Need to recreate the tiles if necessary. The server could figure that out so no need to send tilerefs.
         * Also need to account for anchoring.
         */

        foreach (var e in ev.SelectedObjects)
        {
            //Remember to translate the NetEntities back to EUIDs
            var euid = _entityManager.GetEntity(e);

            var trans = Transform(euid);

            //If the parent of the trans component is 0 then its in space?
            var parent = trans.ParentUid;

            //Check what type of tile is at the destination

            //Instead of translating we could respawn things?

            //Get the tileref of the tile currently under object e
            //var tileRef = MapSystem.GetTileRef();
            var translated = trans.Coordinates.Offset(ev.Direction);

            _transSystem.SetCoordinates(euid, translated);
        }
    }
}

[Serializable, NetSerializable]
public abstract class SelectionEvent : EntityEventArgs
{
    //Need to re-work this with the correct way to address entities over network
    public HashSet<NetEntity> SelectedObjects;

    /// <summary>
    /// Allows us to more easily create these events.
    /// </summary>
    /// <param name="selectedObjects"></param>
    public SelectionEvent(HashSet<EntityUid> selectedObjects)
    {
        var entManager = IoCManager.Resolve<IEntityManager>();

        SelectedObjects = new HashSet<NetEntity>(selectedObjects.Count());

        foreach (var e in selectedObjects)
        {
            SelectedObjects.Add(entManager.GetNetEntity(e));
        }
    }
}

[Serializable, NetSerializable]
public sealed class SelectionDeleteEvent : SelectionEvent
{
    public SelectionDeleteEvent(HashSet<EntityUid> selectedObjects) : base(selectedObjects)
    {
    }
}

[Serializable, NetSerializable]
public sealed class SelectionTranslateEvent : SelectionEvent
{
    public Vector2 Direction;
    public SelectionTranslateEvent(HashSet<EntityUid> selectedObjects, Vector2 direction) : base(selectedObjects)
    {
        Direction = direction;
    }
}

[Serializable, NetSerializable]
public sealed class SelectionCopyEvent : SelectionEvent
{
    public SelectionCopyEvent(HashSet<EntityUid> selectedObjects) : base(selectedObjects)
    {
    }
}
