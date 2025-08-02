using System.Linq;
using RimWorld;
using Verse;

namespace Rumor_Code;

public class ThoughtWorker_DividedColony : ThoughtWorker
{
    protected override ThoughtState CurrentStateInternal(Pawn p)
    {
        if (Controller.Settings.allowBrawls.Equals(true) || p.Map?.GetIsolatedCliques(-3) == null)
        {
            return ThoughtState.Inactive;
        }

        if (p.Map.GetIsolatedCliques(-3).Any() && p.Faction == Faction.OfPlayer)
        {
            return ThoughtState.ActiveAtStage(0);
        }

        return ThoughtState.Inactive;
    }
}