using RimWorld;
using Verse;

namespace Rumor_Code
{
    public class ThoughtWorker_CreepyBreathing : ThoughtWorker
    {
        protected override ThoughtState CurrentSocialStateInternal(Pawn pawn, Pawn other)
        {
            ThoughtState thoughtState;
            if (!other.RaceProps.Humanlike || !RelationsUtility.PawnsKnowEachOther(pawn, other))
            {
                thoughtState = false;
            }
            else if (!pawn.health.capacities.CapableOf(PawnCapacityDefOf.Hearing))
            {
                thoughtState = false;
            }
            else if (other.story.traits.HasTrait(TraitDefOf.CreepyBreathing))
            {
                thoughtState = !pawn.UnderstandsDisability();
            }
            else
            {
                thoughtState = false;
            }

            return thoughtState;
        }
    }
}