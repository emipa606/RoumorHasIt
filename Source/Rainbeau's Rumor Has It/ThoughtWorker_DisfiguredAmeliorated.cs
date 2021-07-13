using RimWorld;
using Verse;

namespace Rumor_Code
{
    public class ThoughtWorker_DisfiguredAmeliorated : ThoughtWorker
    {
        protected override ThoughtState CurrentSocialStateInternal(Pawn pawn, Pawn other)
        {
            ThoughtState thoughtState;
            if (!other.RaceProps.Humanlike || !RelationsUtility.PawnsKnowEachOther(pawn, other) || other.Dead)
            {
                thoughtState = false;
            }
            else if (!pawn.health.capacities.CapableOf(PawnCapacityDefOf.Sight))
            {
                thoughtState = false;
            }
            else if (RelationsUtility.IsDisfigured(other))
            {
                thoughtState = pawn.UnderstandsDisability();
            }
            else
            {
                thoughtState = false;
            }

            return thoughtState;
        }
    }
}