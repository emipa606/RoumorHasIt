using RimWorld;
using Verse;

namespace Rumor_Code;

public class ThoughtWorker_CreepyBreathing : ThoughtWorker
{
    protected override ThoughtState CurrentSocialStateInternal(Pawn pawn, Pawn other)
    {
        if (!other.RaceProps.Humanlike || !RelationsUtility.PawnsKnowEachOther(pawn, other))
        {
            return false;
        }

        if (!pawn.health.capacities.CapableOf(PawnCapacityDefOf.Hearing))
        {
            return false;
        }

        if (other.story.traits.HasTrait(TraitDefOf.CreepyBreathing))
        {
            return !pawn.UnderstandsDisability();
        }

        return false;
    }
}