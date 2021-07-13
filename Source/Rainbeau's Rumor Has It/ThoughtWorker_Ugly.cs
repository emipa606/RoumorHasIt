using RimWorld;
using Verse;

namespace Rumor_Code
{
    public class ThoughtWorker_Ugly : ThoughtWorker
    {
        protected override ThoughtState CurrentSocialStateInternal(Pawn pawn, Pawn other)
        {
            ThoughtState thoughtState;
            if (!other.RaceProps.Humanlike || !RelationsUtility.PawnsKnowEachOther(pawn, other))
            {
                thoughtState = false;
            }
            else if (!pawn.health.capacities.CapableOf(PawnCapacityDefOf.Sight))
            {
                thoughtState = false;
            }
            else if (!pawn.UnderstandsDisability())
            {
                var num = other.story.traits.DegreeOfTrait(TraitDefOf.Beauty);
                if (num == -1)
                {
                    thoughtState = ThoughtState.ActiveAtStage(0);
                }
                else if (num == -2)
                {
                    thoughtState = ThoughtState.ActiveAtStage(1);
                }
                else
                {
                    thoughtState = false;
                }
            }
            else
            {
                thoughtState = false;
            }

            return thoughtState;
        }
    }
}