using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Rumor_Code;

public class IncidentWorker_Brawl : IncidentWorker
{
    protected override bool TryExecuteWorker(IncidentParms parms)
    {
        if (Controller.Settings.allowBrawls.Equals(false))
        {
            return false;
        }

        var map = (Map)parms.target;
        var isolatedCliques = map.GetIsolatedCliques(-3);
        if (isolatedCliques == null || isolatedCliques.Count < 1)
        {
            return false;
        }

        var collection = isolatedCliques.RandomElement().ToList();
        for (var i = 0; i < collection.Count; i++)
        {
            var current = collection[i];
            if (current.Downed || !current.Spawned || current.Dead || current.Map != map)
            {
                collection.Remove(current);
            }
        }

        if (collection.Count < 3)
        {
            return false;
        }

        ICollection<Pawn> collection2 = map.mapPawns.FreeColonistsSpawned.Except(collection).ToList();
        if (collection2.Count < 2)
        {
            return false;
        }

        var socialFight = false;
        foreach (var current2 in collection)
        {
            if (current2 == null)
            {
                continue;
            }

            var pawn = collection2.RandomElement();
            if (pawn == null)
            {
                continue;
            }

            if (!current2.Awake() || !pawn.Awake())
            {
                continue;
            }

            current2.interactions.StartSocialFight(pawn);
            collection2.Remove(pawn);
            socialFight = true;
        }

        if (!socialFight)
        {
            return false;
        }

        string text = "RUMOR.BrawlMsg".Translate();
        foreach (var current3 in collection)
        {
            text = $"{text}\n    {current3.Name.ToStringShort}";
        }

        Find.LetterStack.ReceiveLetter("RUMOR.Brawl".Translate(), text,
            LetterDefOf.NegativeEvent, collection.RandomElement());
        return true;
    }
}