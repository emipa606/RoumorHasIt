using System.Linq;
using RimWorld;
using Verse;

namespace Rumor_Code
{
    public class ThoughtWorker_DividedColony : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            ThoughtState result;
            if (Controller.Settings.allowBrawls.Equals(true))
            {
                result = ThoughtState.Inactive;
            }
            else if (p.Map == null)
            {
                result = ThoughtState.Inactive;
            }
            else if (p.Map.GetIsolatedCliques(-3) == null)
            {
                result = ThoughtState.Inactive;
            }
            else if (p.Map.GetIsolatedCliques(-3).Any() && p.Faction == Faction.OfPlayer)
            {
                result = ThoughtState.ActiveAtStage(0);
            }
            else if (p.Map.GetIsolatedCliques(-3) == null)
            {
                result = ThoughtState.Inactive;
            }
            else
            {
                result = ThoughtState.Inactive;
            }

            return result;
        }
    }
}