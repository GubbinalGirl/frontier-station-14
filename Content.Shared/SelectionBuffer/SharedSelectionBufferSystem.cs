using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace Content.Shared.SelectionBuffer;

public abstract class SharedSelectionBufferSystem : EntitySystem
{
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly SharedTransformSystem _transSystem = default!;

    public override void Initialize()
    {

    }

    public abstract bool TranslateSelection(Vector2 direction);

    /// <summary>
    /// Translates (moves) the selected objects by direction units. Returns true on success.
    /// </summary>
    /// <returns></returns>
    public bool TranslateSelection(HashSet<EntityUid> selection, Vector2 direction)
    {
        //Get the transformcomponent of each one.
        //Is there a proper way to try and move stuff?
        //What about anchored objects? APCs, wires?

        var transformQuery = _entityManager.GetEntityQuery<TransformComponent>();

        foreach (var e in selection)
        {
            if (transformQuery.TryGetComponent(e, out var trans))
            {
                var translated = trans.Coordinates.Offset(direction);

                //_transSystem.AttachToGridOrMap(e, trans);
                _transSystem.SetCoordinates(e, translated);
            }
        }

        return true;
    }
}
