using System.Linq;
using RimWorld;
using Verse;

namespace Rumor_Code;

public class ThoughtWorker_EveryoneGetsOn : ThoughtWorker
{
    protected override ThoughtState CurrentStateInternal(Pawn p)
    {
        if (!p.Spawned)
        {
            return ThoughtState.Inactive;
        }

        if (!p.RaceProps.Humanlike)
        {
            return ThoughtState.Inactive;
        }

        if (ThirdPartyManager.GetAllColonistsLocalTo(p).Count() < 2)
        {
            return ThoughtState.Inactive;
        }

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

        return num == -1 ? ThoughtState.Inactive : ThoughtState.ActiveAtStage(num);
    }
}