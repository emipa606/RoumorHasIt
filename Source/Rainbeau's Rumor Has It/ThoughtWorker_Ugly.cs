using RimWorld;
using Verse;

namespace Rumor_Code;

public class ThoughtWorker_Ugly : ThoughtWorker
{
    protected override ThoughtState CurrentSocialStateInternal(Pawn pawn, Pawn other)
    {
        if (!other.RaceProps.Humanlike || !RelationsUtility.PawnsKnowEachOther(pawn, other))
        {
            return false;
        }

        if (!pawn.health.capacities.CapableOf(PawnCapacityDefOf.Sight))
        {
            return false;
        }

        if (pawn.UnderstandsDisability())
        {
            return false;
        }

        var num = other.story.traits.DegreeOfTrait(TraitDef.Named("Beauty"));
        switch (num)
        {
            case -1:
                return ThoughtState.ActiveAtStage(0);
            case -2:
                return ThoughtState.ActiveAtStage(1);
            default:
                return false;
        }
    }
}