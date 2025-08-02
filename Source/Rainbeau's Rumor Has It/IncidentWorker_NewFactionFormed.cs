using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace Rumor_Code;

public class IncidentWorker_NewFactionFormed : IncidentWorker
{
    private static void BreakUpPawns(Pawn p1, Pawn p2)
    {
        if (p1.relations.DirectRelationExists(PawnRelationDefOf.Spouse, p2))
        {
            p1.relations.RemoveDirectRelation(PawnRelationDefOf.Spouse, p2);
            p1.relations.AddDirectRelation(PawnRelationDefOf.ExSpouse, p2);
            p2.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.DivorcedMe, p1);
            p1.needs.mood.thoughts.memories.RemoveMemoriesOfDef(ThoughtDefOf.GotMarried);
            p2.needs.mood.thoughts.memories.RemoveMemoriesOfDef(ThoughtDefOf.GotMarried);
            p1.needs.mood.thoughts.memories.RemoveMemoriesOfDefWhereOtherPawnIs(ThoughtDefOf.HoneymoonPhase, p2);
            p2.needs.mood.thoughts.memories.RemoveMemoriesOfDefWhereOtherPawnIs(ThoughtDefOf.HoneymoonPhase, p1);
        }
        else
        {
            p1.relations.TryRemoveDirectRelation(PawnRelationDefOf.Lover, p2);
            p1.relations.TryRemoveDirectRelation(PawnRelationDefOf.Fiance, p2);
            p1.relations.AddDirectRelation(PawnRelationDefOf.ExLover, p2);
            p2.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.BrokeUpWithMe, p1);
        }

        p1.ownership.UnclaimBed();
        TaleRecorder.RecordTale(TaleDefOf.Breakup, p1, p2);
        if (PawnUtility.ShouldSendNotificationAbout(p1) || PawnUtility.ShouldSendNotificationAbout(p2))
        {
            Find.LetterStack.ReceiveLetter("LetterLabelBreakup".Translate(),
                "LetterNoLongerLovers".Translate(p1.LabelShort, p2.LabelShort), LetterDefOf.NegativeEvent, p1);
        }
    }

    private static float NewRandomColorFromSpectrum(Faction faction)
    {
        var num = -1f;
        var result = 0f;
        for (var i = 0; i < 10; i++)
        {
            var value = Rand.Value;
            var num2 = 1f;
            var allFactionsListForReading = Find.FactionManager.AllFactionsListForReading;
            foreach (var faction2 in allFactionsListForReading)
            {
                if (faction2 == faction || faction2.def != faction.def)
                {
                    continue;
                }

                var num3 = Mathf.Abs(value - faction2.colorFromSpectrum);
                if (num3 < num2)
                {
                    num2 = num3;
                }
            }

            if (!(num2 > num))
            {
                continue;
            }

            num = num2;
            result = value;
        }

        return result;
    }

    private static Faction BuildNewFaction(Map sourceMap)
    {
        var faction = new Faction
        {
            def = RumorsFactionDefOf.SplinterColony,
            loadID = Find.UniqueIDsManager.GetNextFactionID()
        };
        faction.colorFromSpectrum = NewRandomColorFromSpectrum(faction);
        if (!faction.def.isPlayer)
        {
            if (faction.def.fixedName != null)
            {
                faction.Name = faction.def.fixedName;
            }
            else
            {
                faction.Name = NameGenerator.GenerateName(faction.def.factionNameMaker,
                    from fac in Find.FactionManager.AllFactionsVisible
                    select fac.Name);
            }
        }

        foreach (var current in Find.FactionManager.AllFactionsListForReading)
        {
            faction.TryMakeInitialRelationsWith(current);
        }

        if (!faction.def.hidden && !faction.def.isPlayer)
        {
            var factionBase = (Settlement)WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.Settlement);
            factionBase.SetFaction(faction);
            TileFinder.TryFindPassableTileWithTraversalDistance(sourceMap.Tile, 4, 10, out var tile);
            factionBase.Tile = tile;
            factionBase.Name = SettlementNameGenerator.GenerateSettlementName(factionBase);
            Find.WorldObjects.Add(factionBase);
        }

        Find.World.factionManager.Add(faction);
        sourceMap.pawnDestinationReservationManager.GetPawnDestinationSetFor(faction);
        return faction;
    }

    protected override bool TryExecuteWorker(IncidentParms parms)
    {
        if (Controller.Settings.allowSplinters.Equals(false))
        {
            return false;
        }

        var map = (Map)parms.target;
        var isolatedCliques = map.GetIsolatedCliques(-6);
        if (isolatedCliques == null || isolatedCliques.Count < 1)
        {
            return false;
        }

        var collection = isolatedCliques.RandomElement().ToList();
        for (var i = 0; i < collection.Count; i++)
        {
            var current = collection[i];
            if (current.Downed || !current.Spawned || current.Dead)
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

        var faction = BuildNewFaction(map);
        if (faction == null)
        {
            return false;
        }

        foreach (var current2 in collection)
        {
            current2.SetFactionDirect(faction);
        }

        foreach (var current3 in collection)
        {
            foreach (var current4 in collection2)
            {
                if (current3.Faction == current4.Faction)
                {
                    continue;
                }

                if (LovePartnerRelationUtility.LovePartnerRelationExists(current3, current4)
                   )
                {
                    BreakUpPawns(current3, current4);
                }
            }
        }

        var pawn = collection.RandomElement();
        faction.leader = pawn;
        var text = string.Format("RUMOR.SplitMsg".Translate(), faction.Name);
        foreach (var current5 in collection)
        {
            text = $"{text}{current5.Name.ToStringShort},   ";
        }

        Find.LetterStack.ReceiveLetter("RUMOR.Split".Translate(), text,
            LetterDefOf.NegativeEvent, collection.RandomElement());
        if (pawn.Map == null)
        {
            return true;
        }

        var lordJob_Steal = new LordJob_Steal();
        LordMaker.MakeNewLord(faction, lordJob_Steal, pawn.Map, collection);

        return true;
    }
}