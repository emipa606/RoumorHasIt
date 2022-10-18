using RimWorld;
using Verse;

namespace Rumor_Code;

public class ThoughtWorker_TrustworthyVsGossip : ThoughtWorker
{
    protected override ThoughtState CurrentSocialStateInternal(Pawn p, Pawn other)
    {
        if (!RelationsUtility.PawnsKnowEachOther(p, other))
        {
            return false;
        }

        if (!p.RaceProps.Humanlike || !other.RaceProps.Humanlike)
        {
            return false;
        }

        return p.story.traits.HasTrait(RumorsTraitDefOf.Trustworthy) &&
               other.story.traits.HasTrait(RumorsTraitDefOf.Gossip);
    }
}