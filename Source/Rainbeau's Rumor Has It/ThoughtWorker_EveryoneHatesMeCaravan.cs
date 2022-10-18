using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace Rumor_Code;

public class ThoughtWorker_EveryoneHatesMeCaravan : ThoughtWorker
{
    protected override ThoughtState CurrentStateInternal(Pawn p)
    {
        if (!p.Spawned)
        {
            return ThoughtState.Inactive;
        }

        if (!p.RaceProps.Humanlike)
        {
            return ThoughtState.Inactive;
        }

        if (ThirdPartyManager.GetAllColonistsLocalTo(p).Count() < 2)
        {
            return ThoughtState.Inactive;
        }

        if (!ThirdPartyManager.DoesEveryoneLocallyHate(p))
        {
            return ThoughtState.Inactive;
        }

        return p.GetCaravan() != null ? ThoughtState.ActiveAtStage(0) : ThoughtState.Inactive;
    }
}