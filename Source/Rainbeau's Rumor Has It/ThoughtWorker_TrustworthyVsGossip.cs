using RimWorld;
using Verse;

namespace Rumor_Code
{
    public class ThoughtWorker_TrustworthyVsGossip : ThoughtWorker
    {
        protected override ThoughtState CurrentSocialStateInternal(Pawn p, Pawn other)
        {
            ThoughtState result;
            if (!RelationsUtility.PawnsKnowEachOther(p, other))
            {
                result = false;
            }
            else if (!p.RaceProps.Humanlike || !other.RaceProps.Humanlike)
            {
                result = false;
            }
            else if (p.story.traits.HasTrait(RumorsTraitDefOf.Trustworthy) &&
                     other.story.traits.HasTrait(RumorsTraitDefOf.Gossip))
            {
                result = true;
            }
            else
            {
                result = false;
            }

            return result;
        }
    }
}