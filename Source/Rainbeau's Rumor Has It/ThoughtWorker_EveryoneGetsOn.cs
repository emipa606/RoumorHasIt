using System.Linq;
using RimWorld;
using Verse;

namespace Rumor_Code
{
    public class ThoughtWorker_EveryoneGetsOn : ThoughtWorker
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
            else
            {
                var num = 1;
                foreach (var current in ThirdPartyManager.GetAllColonistsLocalTo(p))
                {
                    if (p == current || !current.IsColonist || current.Dead || p.relations.OpinionOf(current) >= 40 &&
                        current.relations.OpinionOf(p) >= 40)
                    {
                        continue;
                    }

                    num = 0;
                    break;
                }

                foreach (var current2 in ThirdPartyManager.GetAllColonistsLocalTo(p))
                {
                    if (p == current2 || !current2.IsColonist || current2.Dead ||
                        p.relations.OpinionOf(current2) >= 15 && current2.relations.OpinionOf(p) >= 15)
                    {
                        continue;
                    }

                    num = -1;
                    break;
                }

                result = num == -1 ? ThoughtState.Inactive : ThoughtState.ActiveAtStage(num);
            }

            return result;
        }
    }
}