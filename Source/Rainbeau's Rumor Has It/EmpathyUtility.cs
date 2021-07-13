using RimWorld;
using Verse;

namespace Rumor_Code
{
    public static class EmpathyUtility
    {
        public static bool UnderstandsDisability(this Pawn self)
        {
            var isDisfigured = RelationsUtility.IsDisfigured(self) ||
                               self.story.traits.DegreeOfTrait(TraitDefOf.Beauty) < 0 ||
                               self.story.traits.HasTrait(TraitDefOf.CreepyBreathing) ||
                               self.story.traits.HasTrait(TraitDefOf.AnnoyingVoice);
            return isDisfigured;
        }
    }
}