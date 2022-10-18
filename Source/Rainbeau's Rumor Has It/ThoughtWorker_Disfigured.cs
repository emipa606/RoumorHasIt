using RimWorld;
using Verse;

namespace Rumor_Code;

public class ThoughtWorker_Disfigured : ThoughtWorker
{
    protected override ThoughtState CurrentSocialStateInternal(Pawn pawn, Pawn other)
    {
        if (!other.RaceProps.Humanlike || !RelationsUtility.PawnsKnowEachOther(pawn, other) || other.Dead)
        {
            return false;
        }

        if (!pawn.health.capacities.CapableOf(PawnCapacityDefOf.Sight))
        {
            return false;
        }

        if (RelationsUtility.IsDisfigured(other))
        {
            return !pawn.UnderstandsDisability();
        }

        return false;
    }
}