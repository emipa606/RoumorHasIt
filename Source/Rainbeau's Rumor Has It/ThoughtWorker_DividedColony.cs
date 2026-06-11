using System.Linq;
using RimWorld;
using Verse;

namespace Rumor_Code;

public class ThoughtWorker_DividedColony : ThoughtWorker
{
    protected override ThoughtState CurrentStateInternal(Pawn p)
    {
        if (Controller.Settings.allowBrawls.Equals(true) || p.Faction != Faction.OfPlayer)
        {
            return ThoughtState.Inactive;
        }

        var isolatedCliques = p.Map?.GetIsolatedCliques(-3);
        if (isolatedCliques == null)
        {
            return ThoughtState.Inactive;
        }

        return isolatedCliques.Any() ? ThoughtState.ActiveAtStage(0) : ThoughtState.Inactive;
    }
}