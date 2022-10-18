using RimWorld;
using Verse;

namespace Rumor_Code;

public class ThoughtWorker_DisfiguredAmeliorated : ThoughtWorker
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

        return RelationsUtility.IsDisfigured(other) && pawn.UnderstandsDisability();
    }
}