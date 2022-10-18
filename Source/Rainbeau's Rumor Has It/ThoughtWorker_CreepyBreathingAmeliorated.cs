using RimWorld;
using Verse;

namespace Rumor_Code;

public class ThoughtWorker_CreepyBreathingAmeliorated : ThoughtWorker
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

        return other.story.traits.HasTrait(TraitDefOf.CreepyBreathing) && pawn.UnderstandsDisability();
    }
}