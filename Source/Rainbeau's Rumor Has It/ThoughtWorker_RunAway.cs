using RimWorld;
using Verse;

namespace Rumor_Code
{
    public class ThoughtWorker_RunAway : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            return ThoughtState.Inactive;
        }
    }
}