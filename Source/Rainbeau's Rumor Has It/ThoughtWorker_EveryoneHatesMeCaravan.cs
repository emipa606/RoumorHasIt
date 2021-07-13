using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace Rumor_Code
{
    public class ThoughtWorker_EveryoneHatesMeCaravan : ThoughtWorker
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
            else if (ThirdPartyManager.GetAllColonistsLocalTo(p).Count() < 2)
            {
                result = ThoughtState.Inactive;
            }
            else if (!ThirdPartyManager.DoesEveryoneLocallyHate(p))
            {
                result = ThoughtState.Inactive;
            }
            else if (p.GetCaravan() != null)
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