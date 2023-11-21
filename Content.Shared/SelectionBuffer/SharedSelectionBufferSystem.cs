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

        SubscribeNetworkEvent<SelectionTranslateMessage>(OnTranslateSelection);
    }

    private void OnTranslateSelection(SelectionTranslateMessage ev)
    {
        if (!_netMan.IsServer)
            return;

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

public abstract class SelectionMessage : EntityEventArgs
{
    //Need to re-work this with the correct way to address entities over network
    public HashSet<NetEntity> SelectedObjects;

    public SelectionMessage(HashSet<NetEntity> selectedObjects)
    {
        SelectedObjects = selectedObjects;
    }

    /// <summary>
    /// Allows us to more easily create these events.
    /// </summary>
    /// <param name="selectedObjects"></param>
    public SelectionMessage(HashSet<EntityUid> selectedObjects)
    {
        var entManager = IoCManager.Resolve<IEntityManager>();

        SelectedObjects = new HashSet<NetEntity>(selectedObjects.Count());

        foreach (var e in selectedObjects)
        {
            SelectedObjects.Add(entManager.GetNetEntity(e));
        }
    }
}

public sealed class SelectionDeleteMessage : SelectionMessage
{
    public SelectionDeleteMessage(HashSet<EntityUid> selectedObjects) : base(selectedObjects)
    {
    }
}

public sealed class SelectionTranslateMessage : SelectionMessage
{
    public Vector2 Direction;
    public SelectionTranslateMessage(HashSet<EntityUid> selectedObjects, Vector2 direction) : base(selectedObjects)
    {
        Direction = direction;
    }
}
