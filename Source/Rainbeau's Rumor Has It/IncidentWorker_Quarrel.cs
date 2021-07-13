using System.Linq;
using RimWorld;
using Verse;

namespace Rumor_Code
{
    public class IncidentWorker_Quarrel : IncidentWorker
    {
        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            var map = (Map) parms.target;
            var list = (from p in map.mapPawns.AllPawnsSpawned
                where p.RaceProps.Humanlike && p.Faction.IsPlayer
                select p).ToList();
            if (list.Count == 0)
            {
                return false;
            }

            var pawn = list.RandomElement();
            var friendlies = (from p in map.mapPawns.AllPawnsSpawned
                where p.RaceProps.Humanlike && p.Faction.IsPlayer && p != pawn && p.relations.OpinionOf(pawn) > 20 &&
                      pawn.relations.OpinionOf(p) > 20
                select p).ToList();
            if (friendlies.Count == 0)
            {
                return false;
            }

            var friend = friendlies.RandomElement();
            pawn.needs.mood.thoughts.memories.TryGainMemory(RumorsThoughtDefOf.FriendsQuarrel, friend);
            friend.needs.mood.thoughts.memories.TryGainMemory(RumorsThoughtDefOf.FriendsQuarrel, pawn);
            Find.LetterStack.ReceiveLetter("RUMOR.Quarrel".Translate(),
                "RUMOR.QuarrelMsg".Translate(pawn.LabelShort, friend.LabelShort), LetterDefOf.NegativeEvent, pawn);
            return true;
        }
    }
}