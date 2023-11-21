using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using Robust.Shared.Serialization;
using Robust.Shared.Network;

namespace Content.Shared.SelectionBuffer;

public abstract class SharedSelectionBufferSystem : EntitySystem
{
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly SharedTransformSystem _transSystem = default!;
    [Dependency] private readonly INetManager _netMan = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeNetworkEvent<SelectionTranslateEvent>(OnTranslateSelection);
    }

    private void OnTranslateSelection(SelectionTranslateEvent ev)
    {
        if (!_netMan.IsServer)
            return;

        Logger.Debug("OnTranslateSelection running on server.");

        foreach (var e in ev.SelectedObjects)
        {
            var euid = _entityManager.GetEntity(e);
            var trans = Transform(euid);
            var translated = trans.Coordinates.Offset(ev.Direction);

            //_transSystem.AttachToGridOrMap(e, trans);
            _transSystem.SetCoordinates(euid, translated);
        }
    }
}

[Serializable, NetSerializable]
public abstract class SelectionEvent : EntityEventArgs
{
    //Need to re-work this with the correct way to address entities over network
    public List<NetEntity> SelectedObjects;

    /// <summary>
    /// Allows us to more easily create these events.
    /// </summary>
    /// <param name="selectedObjects"></param>
    public SelectionEvent(HashSet<EntityUid> selectedObjects)
    {
        var entManager = IoCManager.Resolve<IEntityManager>();

        SelectedObjects = new List<NetEntity>(selectedObjects.Count());

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
