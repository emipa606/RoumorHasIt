using System.Linq;
using RimWorld;
using Verse;

namespace Rumor_Code
{
    public class ThoughtWorker_EveryoneHatesMeColony : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            ThoughtState result;
            if (!p.Spawned)
            {
                result = ThoughtState.Inactive;
            }
            else if (!p.RaceProps.Humanlike)
            {
                result = ThoughtState.Inactive;
            }
            else if (ThirdPartyManager.GetAllColonistsLocalTo(p).Count() < 3)
            {
                result = ThoughtState.Inactive;
            }
            else if (!ThirdPartyManager.DoesEveryoneLocallyHate(p))
            {
                result = ThoughtState.Inactive;
            }
            else if (p.Map != null && p.Map.ParentFaction == p.Faction)
            {
                result = ThoughtState.ActiveAtStage(0);
            }
            else
            {
                result = ThoughtState.Inactive;
            }

            return result;
        }
    }
}