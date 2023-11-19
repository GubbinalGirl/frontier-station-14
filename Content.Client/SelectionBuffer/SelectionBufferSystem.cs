using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Content.Client.SelectionBuffer;

public sealed class SelectionBufferSystem : EntitySystem
{
    private HashSet<EntityUid> _selectedEntities = new HashSet<EntityUid>();

    public void ClearSelection()
    {
        _selectedEntities.Clear();
    }

    public void AddToSelection(HashSet<EntityUid> entities)
    {
        _selectedEntities.UnionWith(entities);
    }
    public void AddToSelection(EntityUid entity)
    {
        _selectedEntities.Add(entity);
    }

    public void RemoveFromSelection(HashSet<EntityUid> entities)
    {
        _selectedEntities.ExceptWith(entities);
    }
    public void RemoveFromSelection(EntityUid entity)
    {
        _selectedEntities.Remove(entity);
    }
}
