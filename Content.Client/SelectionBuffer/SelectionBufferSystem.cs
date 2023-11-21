using Content.Shared.Friction;
using Content.Shared.SelectionBuffer;
using Robust.Client.GameObjects;
using Robust.Shared.Map;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Content.Client.SelectionBuffer;

public sealed class SelectionBufferSystem : SharedSelectionBufferSystem
{
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly SharedTransformSystem _transSystem = default!;

    private HashSet<EntityUid> _selectedEntities = new HashSet<EntityUid>();
    private HashSet<TileRef> _selectedTiles = new HashSet<TileRef>();

    public override bool TranslateSelection(Vector2 direction)
    {
        return TranslateSelection(_selectedEntities, direction);
    }

    public void PrettyPrintBuffer()
    {
        var metadataQuery = _entityManager.GetEntityQuery<MetaDataComponent>();
        var transformQuery = _entityManager.GetEntityQuery<TransformComponent>();

        Logger.Debug("Entities");

        //The parent of most objects is the station.

        foreach (var e in _selectedEntities)
        {
            if (metadataQuery.TryGetComponent(e, out var meta) &&
                transformQuery.TryGetComponent(e, out var trans))
            {
                Logger.Debug(string.Format("---EUID: {0} | Entity name: {1} | Coords {2}",
                    e, meta.EntityName, trans.Coordinates));

                if (metadataQuery.TryGetComponent(trans.ParentUid, out var metaparent))
                {
                    Logger.Debug(string.Format("---===PARENT: {0}", metaparent.EntityName));
                }
            }
        }
    }

    public void ClearSelection()
    {
        _selectedEntities.Clear();
    }

    #region addremove
    public void AddToSelection(HashSet<EntityUid> entities)
    {
        _selectedEntities.UnionWith(entities);
    }
    public void AddToSelection(EntityUid entity)
    {
        _selectedEntities.Add(entity);
    }
    public void AddToSelection(TileRef tile)
    {
        _selectedTiles.Add(tile);
    }

    public void RemoveFromSelection(HashSet<EntityUid> entities)
    {
        _selectedEntities.ExceptWith(entities);
    }
    public void RemoveFromSelection(EntityUid entity)
    {
        _selectedEntities.Remove(entity);
    }
    public void RemoveFromSelection(TileRef tile)
    {
        _selectedTiles.Remove(tile);
    }
    #endregion
}
