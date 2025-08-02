using System.Linq;
using RimWorld;
using Verse;

namespace Rumor_Code;

public class ThoughtWorker_EveryoneHatesMeColony : ThoughtWorker
{
    protected override ThoughtState CurrentStateInternal(Pawn p)
    {
        if (!p.Spawned || !p.RaceProps.Humanlike || ThirdPartyManager.GetAllColonistsLocalTo(p).Count() < 3 ||
            !ThirdPartyManager.DoesEveryoneLocallyHate(p))
        {
            return ThoughtState.Inactive;
        }

        if (p.Map != null && p.Map.ParentFaction == p.Faction)
        {
            return ThoughtState.ActiveAtStage(0);
        }

        return ThoughtState.Inactive;
    }
}