using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace Rumor_Code;

public class IncidentWorker_Defection : IncidentWorker
{
    private void RunAway(Pawn p, Faction defection)
    {
        p.SetFaction(defection);
        p.jobs.ClearQueuedJobs();
        var list = new List<Pawn>
        {
            p
        };
        var lordJob_ExitMapBest = new LordJob_ExitMapBest(LocomotionUrgency.Walk);
        LordMaker.MakeNewLord(defection, lordJob_ExitMapBest, p.Map, list);
    }

    protected override bool TryExecuteWorker(IncidentParms parms)
    {
        if (Controller.Settings.allowDefections.Equals(false))
        {
            return false;
        }

        var map = (Map)parms.target;
        IEnumerable<Pawn> freeColonistsSpawned = map.mapPawns.FreeColonistsSpawned;
        if (!freeColonistsSpawned.Any())
        {
            return false;
        }

        var enumerable = from x in freeColonistsSpawned
            where ThirdPartyManager.DoesEveryoneLocallyHate(x)
            select x;
        if (!enumerable.Any())
        {
            return false;
        }

        var pawn = enumerable.RandomElement();
        if (pawn == null)
        {
            return false;
        }

        if (!pawn.RaceProps.Humanlike)
        {
            return false;
        }

        if (!pawn.health.capacities.CapableOf(PawnCapacityDefOf.Consciousness) ||
            !pawn.health.capacities.CapableOf(PawnCapacityDefOf.Moving))
        {
            return false;
        }

        var enumerable2 = Find.FactionManager.AllFactions;
        enumerable2 = from f in enumerable2
            where !f.IsPlayer && f.PlayerGoodwill > 10f
            select f;
        if (!enumerable2.Any())
        {
            return false;
        }

        var faction = enumerable2.RandomElement();
        Find.LetterStack.ReceiveLetter("RUMOR.Defection".Translate(),
            pawn.Name.ToStringShort + "RUMOR.DefectedToString".Translate() + faction.Name +
            "RUMOR.DueToIsolationString".Translate(), LetterDefOf.NegativeEvent, pawn);
        RunAway(pawn, faction);
        return true;
    }
}