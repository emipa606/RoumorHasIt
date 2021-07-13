using RimWorld;
using Verse;

namespace Rumor_Code
{
    public class ThoughtWorker_UglyAmeliorated : ThoughtWorker
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
            else if (pawn.UnderstandsDisability())
            {
                var num = other.story.traits.DegreeOfTrait(TraitDefOf.Beauty);
                switch (num)
                {
                    case -1:
                        thoughtState = ThoughtState.ActiveAtStage(0);
                        break;
                    case -2:
                        thoughtState = ThoughtState.ActiveAtStage(1);
                        break;
                    default:
                        thoughtState = false;
                        break;
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