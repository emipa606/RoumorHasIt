using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace Rumor_Code
{

    public class Controller : Mod
    {
        public static Settings Settings;
        public override string SettingsCategory() { return "RUMOR.RumorHasIt".Translate(); }
        public override void DoSettingsWindowContents(Rect canvas) { Settings.DoWindowContents(canvas); }
        public Controller(ModContentPack content) : base(content)
        {
            Settings = GetSettings<Settings>();
        }
    }

    public class Settings : ModSettings
    {
        public bool allowBrawls = true;
        public bool allowDefections = true;
        public bool allowSplinters = true;
        public void DoWindowContents(Rect canvas)
        {
            Listing_Standard list = new Listing_Standard
            {
                ColumnWidth = canvas.width
            };
            list.Begin(canvas);
            list.Gap();
            list.CheckboxLabeled("RUMOR.AllowBrawls".Translate(), ref allowBrawls);
            list.Gap();
            list.CheckboxLabeled("RUMOR.AllowDefections".Translate(), ref allowDefections);
            list.Gap();
            list.CheckboxLabeled("RUMOR.AllowSplinters".Translate(), ref allowSplinters);
            list.End();
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref allowBrawls, "allowBrawls", true);
            Scribe_Values.Look(ref allowDefections, "allowDefections", true);
            Scribe_Values.Look(ref allowSplinters, "allowSplinters", true);
        }
    }

    [DefOf]
    public static class RumorsFactionDefOf
    {
        public static FactionDef SplinterColony;
    }

    [DefOf]
    public static class RumorsRulePackDefOf
    {
        public static RulePackDef Sentence_ApologyFailed;
        public static RulePackDef Sentence_ApologySucceeded;
        public static RulePackDef Sentence_ApologySucceededBig;
        public static RulePackDef Sentence_MakePeaceFailed;
        public static RulePackDef Sentence_MakePeaceSucceeded;
        public static RulePackDef Sentence_CC_Colonist;
        public static RulePackDef Sentence_CC_Glitterworld;
        public static RulePackDef Sentence_CC_Imperial;
        public static RulePackDef Sentence_CC_Midworld;
        public static RulePackDef Sentence_CC_Tribal;
        public static RulePackDef Sentence_CC_Urbworld;
    }

    [DefOf]
    public static class RumorsThoughtDefOf
    {
        public static ThoughtDef ApologizedTo;
        public static ThoughtDef ApologizedToBig;
        public static ThoughtDef EnjoyMakingPeace;
        public static ThoughtDef FriendsQuarrel;
        public static ThoughtDef HadLiesToldAboutMe;
        public static ThoughtDef HeardAwfulThings;
        public static ThoughtDef HeardBadThings;
        public static ThoughtDef HeardGoodThings;
        public static ThoughtDef HeardGreatThings;
        public static ThoughtDef ILied;
        public static ThoughtDef LiedTo;
        public static ThoughtDef MySecretWasRevealed;
        public static ThoughtDef MySecretWasRevealedMood;
        public static ThoughtDef ReceivedSecret;
        public static ThoughtDef SharedSecret;
    }

    [DefOf]
    public static class RumorsTraitDefOf
    {
        public static TraitDef CompulsiveLiar;
        public static TraitDef Gossip;
        public static TraitDef Gushing;
        public static TraitDef Manipulative;
        public static TraitDef Peacemaker;
        public static TraitDef Trustworthy;
    }

    public class Watcher : MapComponent
    {
        private int currentTick;
        public Watcher(Map map) : base(map) { }
        public override void MapComponentTick()
        {
            base.MapComponentTick();
            currentTick = Find.TickManager.TicksGame;
            if (currentTick % 100 == 0)
            {
                CaravanSocialManager.MakeCaravansInteract();
            }
            if (currentTick % 15000 == 10)
            {
                ThirdPartyManager.FindCliques();
            }
        }
    }

    // ========== ALERTS ==========

    public class Alert_Cliques : Alert
    {
        public override AlertReport GetReport()
        {
            AlertReport result;
            using (List<Map>.Enumerator enumerator = Find.Maps.GetEnumerator())
            {
                if (enumerator.MoveNext())
                {
                    Map current = enumerator.Current;
                    ICollection<ICollection<Pawn>> isolatedCliques = current.GetIsolatedCliques(-5);
                    if (isolatedCliques == null) { }
                    if (isolatedCliques == null || isolatedCliques.Count == 0)
                    {
                        result = false;
                        return result;
                    }
                    result = true;
                    return result;
                }
            }
            result = false;
            return result;
        }
        public override TaggedString GetExplanation()
        {
            TaggedString text = Translator.Translate("RUMOR.CliquesFormed");
            int num = 0;
            foreach (Map current in Find.Maps)
            {
                ICollection<ICollection<Pawn>> isolatedCliques = current.GetIsolatedCliques(-3);
                if (isolatedCliques != null && isolatedCliques.Count != 0)
                {
                    foreach (ICollection<Pawn> current2 in isolatedCliques)
                    {
                        num++;
                        text += string.Format("\nClique {0}: ", num);
                        foreach (Pawn current3 in current2)
                        {
                            text += current3.NameShortColored + ", ";
                        }
                        text += "\n";
                    }
                    if (Controller.Settings.allowSplinters.Equals(true))
                    {
                        text += Translator.Translate("RUMOR.CliquesExtraWarning");
                    }
                }
            }
            return text;
        }
        public override string GetLabel()
        {
            return Translator.Translate("RUMOR.Cliques");
        }
    }

    public class Alert_DefectionRisk : Alert
    {
        private IEnumerable<Pawn> DefectionRiskPawns
        {
            get
            {
                return from p in ThirdPartyManager.GetAllFreeColonistsAlive
                       where ThirdPartyManager.DoesEveryoneLocallyHate(p)
                       select p;
            }
        }
        public override AlertReport GetReport()
        {
            IEnumerable<Pawn> getAllFreeColonistsAlive = ThirdPartyManager.GetAllFreeColonistsAlive;
            AlertReport result;
            if (getAllFreeColonistsAlive.Count() == 0)
            {
                result = false;
            }
            else
            {
                Pawn pawn = getAllFreeColonistsAlive.FirstOrDefault();

                try
                {
                    foreach (Pawn current in getAllFreeColonistsAlive.ToList())
                    {
                        if (ThirdPartyManager.DoesEveryoneLocallyHate(current))
                        {
                            result = AlertReport.CulpritIs(current);
                            return result;
                        }
                    }
                }
                catch (InvalidOperationException)
                {
                    result = AlertReport.CulpritIs(pawn);
                    return result;
                }
                result = false;
            }
            return result;
        }
        public override TaggedString GetExplanation()
        {
            TaggedString text;
            if (Controller.Settings.allowDefections.Equals(true))
            {
                text = Translator.Translate("RUMOR.DefectionRiskMsg");
                try
                {
                    foreach (Pawn current in DefectionRiskPawns.ToList())
                    {
                        text = text + "\n     " + current.NameShortColored;
                    }
                }
                catch (InvalidOperationException)
                {
                }
            }
            else
            {
                text = Translator.Translate("RUMOR.SocialIsolationMsg");
                try
                {
                    foreach (Pawn current2 in DefectionRiskPawns.ToList())
                    {
                        text = text + "\n     " + current2.NameShortColored;
                    }
                }
                catch (InvalidOperationException)
                {
                }
            }
            return text;
        }
        public override string GetLabel()
        {
            string result;
            if (Controller.Settings.allowDefections.Equals(true))
            {
                result = Translator.Translate("RUMOR.DefectionRisk");
            }
            else
            {
                result = Translator.Translate("RUMOR.SocialIsolation");
            }
            return result;
        }
    }

    // ========== CARAVAN SOCIAL MANAGER ==========

    public static class CaravanSocialManager
    {
        public static void MakeCaravansInteract()
        {
            foreach (Caravan current in ThirdPartyManager.GetAllPlayerCaravans)
            {
                CaravanSocialManager.MakeCaravanInteract(current);
            }
        }
        public static void MakeCaravanInteract(Caravan c)
        {
            List<Pawn> pawnsListForReading = c.PawnsListForReading;
            if (pawnsListForReading.Count() >= 3)
            {
                foreach (Pawn current in pawnsListForReading)
                {
                    if (Rand.Value < 0.02)
                    {
                        CaravanSocialManager.TryInteractRandomly(current);
                    }
                }
            }
        }
        public static bool TryInteractWith(Pawn initiator, Pawn recipient, InteractionDef intDef)
        {
            bool result;

            if (initiator == recipient)
            {
                Log.Warning(initiator + " tried to interact with self, interaction=" + intDef.defName);
                result = false;
            }
            else
            {
                List<RulePackDef> list = new List<RulePackDef>();
                if (intDef.initiatorThought != null)
                {
                    CaravanSocialManager.AddInteractionThought(initiator, recipient, intDef.initiatorThought);
                }
                if (intDef.recipientThought != null && recipient.needs.mood != null)
                {
                    CaravanSocialManager.AddInteractionThought(recipient, initiator, intDef.recipientThought);
                }
                if (intDef.initiatorXpGainSkill != null)
                {
                    initiator.skills.Learn(intDef.initiatorXpGainSkill, intDef.initiatorXpGainAmount, false);
                }
                if (intDef.recipientXpGainSkill != null && recipient.RaceProps.Humanlike)
                {
                    recipient.skills.Learn(intDef.recipientXpGainSkill, intDef.recipientXpGainAmount, false);
                }
                bool flag = false;
                if (recipient.RaceProps.Humanlike) { }
                if (!flag)
                {
                    intDef.Worker.Interacted(initiator, recipient, list, out _, out _, out _, out _);
                }
                Find.PlayLog.Add(new PlayLogEntry_Interaction(intDef, initiator, recipient, list));
                result = true;
            }
            return result;
        }
        private static bool TryInteractRandomly(Pawn p)
        {
            bool result;
            if (p == null)
            {
                Log.Message("Pawn is null!");
                result = false;
            }
            else if (CaravanUtility.GetCaravan(p) == null)
            {
                result = false;
            }
            else
            {
                if (p.interactions == null) { }
                if (p.RaceProps.Humanlike)
                {
                    List<Pawn> list = ThirdPartyManager.GetAllColonistsLocalTo(p).ToList();
                    if (list.Count() == 0)
                    {
                        result = false;
                        return result;
                    }
                    GenList.Shuffle(list);
                    List<InteractionDef> allDefsListForReading = DefDatabase<InteractionDef>.AllDefsListForReading;
                    for (int i = 0; i < list.Count; i++)
                    {
                        Pawn p2 = list[i];
                        if (p == null || p2 == null)
                        {
                            result = false;
                            return result;
                        }
                        if (GenCollection.TryRandomElementByWeight(allDefsListForReading, (InteractionDef x) => CaravanSocialManager.ParsedRandomInteractionWeight(x, p, p2), out InteractionDef intDef))
                        {
                            if (CaravanSocialManager.TryInteractWith(p, p2, intDef))
                            {
                                result = true;
                                return result;
                            }
                            Log.Error(p + " failed to interact with " + p);
                        }
                    }
                }
                else if (p.RaceProps.Animal && Rand.Value < 0.05f)
                {
                    InteractionDef nuzzle = InteractionDefOf.Nuzzle;
                    List<Pawn> list2 = ThirdPartyManager.GetAllColonistsLocalTo(p).ToList();
                    if (list2.Count() == 0)
                    {
                        result = false;
                        return result;
                    }
                    GenList.Shuffle(list2);
                    List<InteractionDef> allDefsListForReading2 = DefDatabase<InteractionDef>.AllDefsListForReading;
                    for (int j = 0; j < list2.Count; j++)
                    {
                        Pawn pawn = list2[j];
                        if (p == null || pawn == null)
                        {
                            result = false;
                            return result;
                        }
                        if (CaravanSocialManager.TryInteractWith(p, pawn, nuzzle))
                        {
                            result = true;
                            return result;
                        }
                        Log.Error(p + " failed to interact with " + p);
                    }
                }
                result = false;
            }
            return result;
        }
        private static float ParsedRandomInteractionWeight(InteractionDef def, Pawn p1, Pawn p2)
        {
            float result;
            try
            {
                result = def.Worker.RandomSelectionWeight(p1, p2);
            }
            catch (NullReferenceException)
            {
                result = 0.025f;
            }
            return result;
        }
        private static void AddInteractionThought(Pawn pawn, Pawn otherPawn, ThoughtDef thoughtDef)
        {
            float statValue = StatExtension.GetStatValue(otherPawn, StatDefOf.SocialImpact, true);
            Thought_Memory thought_Memory = (Thought_Memory)ThoughtMaker.MakeThought(thoughtDef);
            thought_Memory.moodPowerFactor = statValue;
            if (thought_Memory is Thought_MemorySocial thought_MemorySocial)
            {
                thought_MemorySocial.opinionOffset *= statValue;
            }
            pawn.needs.mood.thoughts.memories.TryGainMemory(thought_Memory, otherPawn);
        }
    }

    // ========== INCIDENT WORKERS ==========

    public class IncidentWorker_Brawl : IncidentWorker
    {
        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            bool result;
            if (Controller.Settings.allowBrawls.Equals(false))
            {
                result = false;
            }
            else
            {
                Map map = (Map)parms.target;
                ICollection<ICollection<Pawn>> isolatedCliques = map.GetIsolatedCliques(-3);
                if (isolatedCliques == null || isolatedCliques.Count < 1)
                {
                    result = false;
                }
                else
                {
                    ICollection<Pawn> collection = GenCollection.RandomElement(isolatedCliques);
                    foreach (Pawn current in collection)
                    {
                        if (current.Downed || !current.Spawned || current.Dead || current.Map != map)
                        {
                            collection.Remove(current);
                        }
                    }
                    if (collection.Count < 3)
                    {
                        result = false;
                    }
                    else
                    {
                        ICollection<Pawn> collection2 = map.mapPawns.FreeColonistsSpawned.Except(collection).ToList();
                        if (collection2 == null || collection2.Count() < 2)
                        {
                            result = false;
                        }
                        else
                        {
                            bool flag = false;
                            foreach (Pawn current2 in collection)
                            {
                                if (current2 != null)
                                {
                                    Pawn pawn = GenCollection.RandomElement(collection2);
                                    if (pawn != null)
                                    {
                                        if (RestUtility.Awake(current2) && RestUtility.Awake(pawn))
                                        {
                                            current2.interactions.StartSocialFight(pawn);
                                            collection2.Remove(pawn);
                                            flag = true;
                                        }
                                    }
                                }
                            }
                            if (flag)
                            {
                                string text = Translator.Translate("RUMOR.BrawlMsg");
                                foreach (Pawn current3 in collection)
                                {
                                    text = text + "\n    " + current3.Name.ToStringShort;
                                }
                                Find.LetterStack.ReceiveLetter(Translator.Translate("RUMOR.Brawl"), text, LetterDefOf.NegativeEvent, GenCollection.RandomElement(collection), null);
                                result = true;
                            }
                            else
                            {
                                result = false;
                            }
                        }
                    }
                }
            }
            return result;
        }
    }

    public class IncidentWorker_Defection : IncidentWorker
    {
        private void RunAway(Pawn p, Faction defection)
        {
            p.SetFaction(defection, null);
            p.jobs.ClearQueuedJobs();
            List<Pawn> list = new List<Pawn>
            {
                p
            };
            LordJob_ExitMapBest lordJob_ExitMapBest = new LordJob_ExitMapBest(LocomotionUrgency.Walk, false);
            LordMaker.MakeNewLord(defection, lordJob_ExitMapBest, p.Map, list);
        }
        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            bool result;
            if (Controller.Settings.allowDefections.Equals(false))
            {
                result = false;
            }
            else
            {
                Map map = (Map)parms.target;
                IEnumerable<Pawn> freeColonistsSpawned = map.mapPawns.FreeColonistsSpawned;
                if (freeColonistsSpawned.Count() == 0)
                {
                    result = false;
                }
                else
                {
                    IEnumerable<Pawn> enumerable = from x in freeColonistsSpawned
                                                   where ThirdPartyManager.DoesEveryoneLocallyHate(x)
                                                   select x;
                    if (enumerable.Count() == 0)
                    {
                        result = false;
                    }
                    else
                    {
                        Pawn pawn = GenCollection.RandomElement(enumerable);
                        if (pawn == null)
                        {
                            result = false;
                        }
                        else if (!pawn.RaceProps.Humanlike)
                        {
                            result = false;
                        }
                        else if (!pawn.health.capacities.CapableOf(PawnCapacityDefOf.Consciousness) || !pawn.health.capacities.CapableOf(PawnCapacityDefOf.Moving))
                        {
                            result = false;
                        }
                        else
                        {
                            IEnumerable<Faction> enumerable2 = Find.FactionManager.AllFactions;
                            enumerable2 = from f in enumerable2
                                          where !f.IsPlayer && f.PlayerGoodwill > 10f
                                          select f;
                            if (enumerable2.Count() == 0)
                            {
                                result = false;
                            }
                            else
                            {
                                Faction faction = GenCollection.RandomElement(enumerable2);
                                Find.LetterStack.ReceiveLetter(Translator.Translate("RUMOR.Defection"), pawn.Name.ToStringShort + Translator.Translate("RUMOR.DefectedToString") + faction.Name + Translator.Translate("RUMOR.DueToIsolationString"), LetterDefOf.NegativeEvent, pawn, null);
                                this.RunAway(pawn, faction);
                                result = true;
                            }
                        }
                    }
                }
            }
            return result;
        }
    }

    public class IncidentWorker_MadWithLoneliness : IncidentWorker
    {
        private void HaveEpisode(Pawn p)
        {
            float value = Rand.Value;
            if (value < 0.15)
            {
                p.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Berserk, null, false, false, null);
            }
            else if (value < 0.45)
            {
                p.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Wander_Psychotic, null, false, false, null);
            }
            else if (p.story.traits.HasTrait(TraitDefOf.DrugDesire))
            {
                p.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Binging_DrugExtreme, null, false, false, null);
            }
            else
            {
                p.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Wander_Sad, null, false, false, null);
            }
        }
        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            IEnumerable<Pawn> enumerable = from x in ThirdPartyManager.GetAllFreeColonistsAlive
                                           where ThirdPartyManager.DoesEveryoneLocallyHate(x)
                                           select x;
            bool result;
            if (enumerable.Count() == 0)
            {
                result = false;
            }
            else
            {
                Pawn pawn = GenCollection.RandomElement(enumerable);
                if (pawn == null)
                {
                    result = false;
                }
                else if (!pawn.RaceProps.Humanlike)
                {
                    result = false;
                }
                else
                {
                    this.HaveEpisode(pawn);
                    result = true;
                }
            }
            return result;
        }
    }

    public class IncidentWorker_NewFactionFormed : IncidentWorker
    {
        private void BreakUpPawns(Pawn p1, Pawn p2)
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
            TaleRecorder.RecordTale(TaleDefOf.Breakup, new object[] {
                p1,
                p2
            });
            if (PawnUtility.ShouldSendNotificationAbout(p1) || PawnUtility.ShouldSendNotificationAbout(p2))
            {
                Find.LetterStack.ReceiveLetter(Translator.Translate("LetterLabelBreakup"), Translator.Translate("LetterNoLongerLovers", new object[] {
                    p1.LabelShort,
                    p2.LabelShort
                }), LetterDefOf.NegativeEvent, p1, null);
            }
        }
        private static float NewRandomColorFromSpectrum(Faction faction)
        {
            float num = -1f;
            float result = 0f;
            for (int i = 0; i < 10; i++)
            {
                float value = Rand.Value;
                float num2 = 1f;
                List<Faction> allFactionsListForReading = Find.FactionManager.AllFactionsListForReading;
                for (int j = 0; j < allFactionsListForReading.Count; j++)
                {
                    Faction faction2 = allFactionsListForReading[j];
                    if (faction2 != faction && faction2.def == faction.def)
                    {
                        float num3 = Mathf.Abs(value - faction2.colorFromSpectrum);
                        if (num3 < num2)
                        {
                            num2 = num3;
                        }
                    }
                }
                if (num2 > num)
                {
                    num = num2;
                    result = value;
                }
            }
            return result;
        }
        public static Faction BuildNewFaction(Map sourceMap, bool hostile)
        {
            Faction faction = new Faction
            {
                def = RumorsFactionDefOf.SplinterColony,
                loadID = Find.UniqueIDsManager.GetNextFactionID()
            };
            faction.colorFromSpectrum = IncidentWorker_NewFactionFormed.NewRandomColorFromSpectrum(faction);
            if (!faction.def.isPlayer)
            {
                if (faction.def.fixedName != null)
                {
                    faction.Name = faction.def.fixedName;
                }
                else
                {
                    faction.Name = NameGenerator.GenerateName(faction.def.factionNameMaker, from fac in Find.FactionManager.AllFactionsVisible
                                                                                            select fac.Name, false);
                }
            }
            foreach (Faction current in Find.FactionManager.AllFactionsListForReading)
            {
                faction.TryMakeInitialRelationsWith(current);
            }
            if (!faction.def.hidden && !faction.def.isPlayer)
            {
                Settlement factionBase = (Settlement)WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.Settlement);
                factionBase.SetFaction(faction);
                int tile = -1;
                TileFinder.TryFindPassableTileWithTraversalDistance(sourceMap.Tile, 4, 10, out tile, null, false);
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
            bool result;
            if (Controller.Settings.allowSplinters.Equals(false))
            {
                result = false;
            }
            else
            {
                Map map = (Map)parms.target;
                ICollection<ICollection<Pawn>> isolatedCliques = map.GetIsolatedCliques(-6);
                if (isolatedCliques == null || isolatedCliques.Count < 1)
                {
                    result = false;
                }
                else
                {
                    ICollection<Pawn> collection = GenCollection.RandomElement(isolatedCliques);
                    foreach (Pawn current in collection)
                    {
                        if (current.Downed || !current.Spawned || current.Dead)
                        {
                            collection.Remove(current);
                        }
                    }
                    if (collection.Count < 3)
                    {
                        result = false;
                    }
                    else
                    {
                        ICollection<Pawn> collection2 = map.mapPawns.FreeColonistsSpawned.Except(collection).ToList();
                        if (collection2 == null || collection2.Count() < 2)
                        {
                            result = false;
                        }
                        else
                        {
                            Faction faction = IncidentWorker_NewFactionFormed.BuildNewFaction(map, false);
                            if (faction == null)
                            {
                                result = false;
                            }
                            else
                            {
                                foreach (Pawn current2 in collection)
                                {
                                    current2.SetFactionDirect(faction);
                                }
                                foreach (Pawn current3 in collection)
                                {
                                    foreach (Pawn current4 in collection2)
                                    {
                                        if (current3.Faction != current4.Faction)
                                        {
                                            if (LovePartnerRelationUtility.LovePartnerRelationExists(current3, current4))
                                            {
                                                this.BreakUpPawns(current3, current4);
                                            }
                                        }
                                    }
                                }
                                Pawn pawn = GenCollection.RandomElement(collection);
                                faction.leader = pawn;
                                string text = string.Format(Translator.Translate("RUMOR.SplitMsg"), faction.Name);
                                foreach (Pawn current5 in collection)
                                {
                                    text = text + current5.Name.ToStringShort + ",   ";
                                }
                                Find.LetterStack.ReceiveLetter(Translator.Translate("RUMOR.Split"), text, LetterDefOf.NegativeEvent, GenCollection.RandomElement(collection), null);
                                if (pawn.Map != null)
                                {
                                    LordJob_Steal lordJob_Steal = new LordJob_Steal();
                                    LordMaker.MakeNewLord(faction, lordJob_Steal, pawn.Map, collection);
                                }
                                result = true;
                            }
                        }
                    }
                }
            }
            return result;
        }
    }

    public class IncidentWorker_Quarrel : IncidentWorker
    {
        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            List<Pawn> list = (from p in map.mapPawns.AllPawnsSpawned where p.RaceProps.Humanlike && p.Faction.IsPlayer select p).ToList();
            if (list.Count == 0)
            {
                return false;
            }
            Pawn pawn = list.RandomElement();
            List<Pawn> friendlies = (from p in map.mapPawns.AllPawnsSpawned where p.RaceProps.Humanlike && p.Faction.IsPlayer && p != pawn && p.relations.OpinionOf(pawn) > 20 && pawn.relations.OpinionOf(p) > 20 select p).ToList();
            if (friendlies.Count == 0)
            {
                return false;
            }
            Pawn friend = friendlies.RandomElement();
            pawn.needs.mood.thoughts.memories.TryGainMemory(RumorsThoughtDefOf.FriendsQuarrel, friend);
            friend.needs.mood.thoughts.memories.TryGainMemory(RumorsThoughtDefOf.FriendsQuarrel, pawn);
            Find.LetterStack.ReceiveLetter("RUMOR.Quarrel".Translate(), "RUMOR.QuarrelMsg".Translate(pawn.LabelShort, friend.LabelShort), LetterDefOf.NegativeEvent, pawn, null);
            return true;
        }
    }

    // ========== INTERACTION WORKERS ==========

    public class InteractionWorker_Apologize : InteractionWorker
    {
        public override float RandomSelectionWeight(Pawn initiator, Pawn recipient)
        {
            float result;
            if (FactionUtility.HostileTo(recipient.Faction, Faction.OfPlayer) && !recipient.IsPrisonerOfColony)
            {
                result = 0f;
            }
            else
            {
                float num = 10f;
                IEnumerable<Thought_Memory> enumerable = ThirdPartyManager.GetMemoriesWithDef(recipient, ThoughtDefOf.Insulted);
                enumerable = enumerable.Concat(ThirdPartyManager.GetMemoriesWithDef(recipient, ThoughtDefOf.HarmedMe));
                enumerable = enumerable.Concat(ThirdPartyManager.GetMemoriesWithDef(recipient, ThoughtDefOf.HadAngeringFight));
                enumerable = enumerable.Concat(ThirdPartyManager.GetMemoriesWithDef(recipient, RumorsThoughtDefOf.HadLiesToldAboutMe));
                enumerable = from x in enumerable
                             where ((Thought_MemorySocial)x).OtherPawn() == initiator
                             select x;
                if (enumerable.Count() == 0)
                {
                    result = 0f;
                }
                else
                {
                    result = num;
                }
            }
            return result;
        }
        public override void Interacted(Pawn initiator, Pawn recipient, List<RulePackDef> extraSentencePacks, out string letterText, out string letterLabel, out LetterDef letterDef, out LookTargets lookTargets)
        {
            letterLabel = null;
            letterText = null;
            letterDef = null;
            lookTargets = null;
            IEnumerable<Thought_Memory> enumerable = ThirdPartyManager.GetMemoriesWithDef(recipient, ThoughtDefOf.Insulted);
            enumerable = enumerable.Concat(ThirdPartyManager.GetMemoriesWithDef(recipient, ThoughtDefOf.HarmedMe));
            enumerable = enumerable.Concat(ThirdPartyManager.GetMemoriesWithDef(recipient, ThoughtDefOf.HadAngeringFight));
            enumerable = enumerable.Concat(ThirdPartyManager.GetMemoriesWithDef(recipient, RumorsThoughtDefOf.HadLiesToldAboutMe));
            enumerable = from x in enumerable
                         where ((Thought_MemorySocial)x).OtherPawn() == initiator
                         select x;
            if (enumerable.Count() != 0)
            {
                Thought_MemorySocial thought_MemorySocial = (Thought_MemorySocial)GenCollection.RandomElement(enumerable);
                float num = Rand.Range(1, 100) + initiator.skills.GetSkill(SkillDefOf.Social).Level;
                if (num < 40f)
                {
                    extraSentencePacks.Add(RumorsRulePackDefOf.Sentence_ApologyFailed);
                }
                else if (num < 90f)
                {
                    extraSentencePacks.Add(RumorsRulePackDefOf.Sentence_ApologySucceeded);
                    recipient.needs.mood.thoughts.memories.TryGainMemory(RumorsThoughtDefOf.ApologizedTo, initiator);
                }
                else
                {
                    extraSentencePacks.Add(RumorsRulePackDefOf.Sentence_ApologySucceededBig);
                    recipient.needs.mood.thoughts.memories.RemoveMemory(thought_MemorySocial);
                    recipient.needs.mood.thoughts.memories.TryGainMemory(RumorsThoughtDefOf.ApologizedToBig, initiator);
                    initiator.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.RapportBuilt, recipient);
                }
            }
        }
    }

    public class InteractionWorker_ChattedAboutSomeone : InteractionWorker
    {
        private Pawn p3;
        public override float RandomSelectionWeight(Pawn initiator, Pawn recipient)
        {
            float result;
            if (FactionUtility.HostileTo(recipient.Faction, Faction.OfPlayer) && !recipient.IsPrisonerOfColony)
            {
                result = 0f;
            }
            else
            {
                float num = 0.7f;
                if (initiator.story.traits.HasTrait(RumorsTraitDefOf.Gossip))
                {
                    num *= 2f;
                }
                this.p3 = this.ChooseChattedAbout(initiator, recipient);
                if (this.p3 == null)
                {
                    result = 0f;
                }
                else
                {
                    result = num;
                }
            }
            return result;
        }
        public override void Interacted(Pawn initiator, Pawn recipient, List<RulePackDef> extraSentencePacks, out string letterText, out string letterLabel, out LetterDef letterDef, out LookTargets lookTargets)
        {
            letterLabel = null;
            letterText = null;
            letterDef = null;
            lookTargets = null;
            if (this.p3 != null)
            {
                bool flag = Rand.Range(0, 100) < (initiator.relations.OpinionOf(recipient) + 20f + 1.5f * recipient.skills.GetSkill(SkillDefOf.Social).Level) / 1.5f;
                bool flag2 = Rand.Range(0, 100) < (recipient.relations.OpinionOf(initiator) + 20f + 1.5f * initiator.skills.GetSkill(SkillDefOf.Social).Level) / 1.5f;
                float num = initiator.relations.OpinionOf(this.p3);
                float num2 = initiator.relations.OpinionOf(this.p3);
                if (flag && !initiator.story.traits.HasTrait(RumorsTraitDefOf.Manipulative))
                {
                    ThoughtDef thoughtDef = null;
                    if (num2 > 40f)
                    {
                        thoughtDef = RumorsThoughtDefOf.HeardGreatThings;
                    }
                    else if (num2 > 10f)
                    {
                        thoughtDef = RumorsThoughtDefOf.HeardGoodThings;
                    }
                    else if (num2 < -40f)
                    {
                        thoughtDef = RumorsThoughtDefOf.HeardAwfulThings;
                    }
                    else if (num2 < -10f)
                    {
                        thoughtDef = RumorsThoughtDefOf.HeardBadThings;
                    }
                    if (thoughtDef != null)
                    {
                        initiator.needs.mood.thoughts.memories.TryGainMemory(thoughtDef, this.p3);
                    }
                }
                if (flag2 && !recipient.story.traits.HasTrait(RumorsTraitDefOf.Manipulative))
                {
                    ThoughtDef thoughtDef2 = null;
                    if (num > 40f)
                    {
                        thoughtDef2 = RumorsThoughtDefOf.HeardGreatThings;
                    }
                    else if (num > 10f)
                    {
                        thoughtDef2 = RumorsThoughtDefOf.HeardGoodThings;
                    }
                    else if (num < -40f)
                    {
                        thoughtDef2 = RumorsThoughtDefOf.HeardAwfulThings;
                    }
                    else if (num < -10f)
                    {
                        thoughtDef2 = RumorsThoughtDefOf.HeardBadThings;
                    }
                    if (thoughtDef2 != null)
                    {
                        recipient.needs.mood.thoughts.memories.TryGainMemory(thoughtDef2, this.p3);
                    }
                }
            }
        }
        private Pawn ChooseChattedAbout(Pawn p1, Pawn p2)
        {
            IEnumerable<Pawn> enumerable = ThirdPartyManager.GetKnownPeople(p1, p2);
            enumerable = enumerable.Concat(ThirdPartyManager.GetKnownPeople(p1, p2).Intersect(ThirdPartyManager.GetAllColonistsLocalTo(p1)));
            Pawn result;
            if (enumerable.Count() == 0)
            {
                result = null;
            }
            else if (p1.story.traits.HasTrait(RumorsTraitDefOf.Manipulative))
            {
                IEnumerable<Pawn> enumerable2 = from x in enumerable
                                                where (p1.relations.OpinionOf(x) < -10 && p2.relations.OpinionOf(x) > 10) || (p1.relations.OpinionOf(x) > 10 && p2.relations.OpinionOf(x) < -10)
                                                select x;
                if (enumerable2.Count() == 0)
                {
                    result = null;
                }
                else
                {
                    result = GenCollection.RandomElement(enumerable2);
                }
            }
            else
            {
                result = GenCollection.RandomElement(enumerable);
            }
            return result;
        }
    }

    public class InteractionWorker_CultureClash : InteractionWorker
    {
        public override float RandomSelectionWeight(Pawn initiator, Pawn recipient)
        {
            float result;
            if (FactionUtility.HostileTo(recipient.Faction, Faction.OfPlayer) && !recipient.IsPrisonerOfColony)
            {
                result = 0f;
            }
            else
            {
                string adultCulturalAdjective = ThirdPartyManager.GetAdultCulturalAdjective(initiator);
                string adultCulturalAdjective2 = ThirdPartyManager.GetAdultCulturalAdjective(recipient);
                string childhoodCulturalAdjective = ThirdPartyManager.GetChildhoodCulturalAdjective(initiator);
                string childhoodCulturalAdjective2 = ThirdPartyManager.GetChildhoodCulturalAdjective(recipient);
                bool flag = (adultCulturalAdjective == adultCulturalAdjective2 || childhoodCulturalAdjective == adultCulturalAdjective2) && (adultCulturalAdjective == childhoodCulturalAdjective2 || childhoodCulturalAdjective == childhoodCulturalAdjective2);
                if (flag)
                {
                    result = 0f;
                }
                else
                {
                    float num = 0.02f;
                    result = num * NegativeInteractionUtility.NegativeInteractionChanceFactor(initiator, recipient);
                }
            }
            return result;
        }
        public override void Interacted(Pawn initiator, Pawn recipient, List<RulePackDef> extraSentencePacks, out string letterText, out string letterLabel, out LetterDef letterDef, out LookTargets lookTargets)
        {
            letterLabel = null;
            letterText = null;
            letterDef = null;
            lookTargets = null;
            string adultCulturalAdjective = ThirdPartyManager.GetAdultCulturalAdjective(initiator);
            string adultCulturalAdjective2 = ThirdPartyManager.GetAdultCulturalAdjective(recipient);
            string childhoodCulturalAdjective = ThirdPartyManager.GetChildhoodCulturalAdjective(initiator);
            string childhoodCulturalAdjective2 = ThirdPartyManager.GetChildhoodCulturalAdjective(recipient);
            string a;
            if (!(adultCulturalAdjective == adultCulturalAdjective2) && !(childhoodCulturalAdjective == adultCulturalAdjective2))
            {
                a = ThirdPartyManager.GetAdultCulturalAdjective(recipient);
            }
            else
            {
                if (adultCulturalAdjective == childhoodCulturalAdjective2 || childhoodCulturalAdjective == childhoodCulturalAdjective2)
                {
                    return;
                }
                a = ThirdPartyManager.GetChildhoodCulturalAdjective(recipient);
            }
            RulePackDef item = RumorsRulePackDefOf.Sentence_CC_Colonist;
            if (a == "Glitterworld")
            {
                item = RumorsRulePackDefOf.Sentence_CC_Glitterworld;
            }
            if (a == "Urbworld")
            {
                item = RumorsRulePackDefOf.Sentence_CC_Urbworld;
            }
            if (a == "Tribal")
            {
                item = RumorsRulePackDefOf.Sentence_CC_Tribal;
            }
            if (a == "Midworld")
            {
                item = RumorsRulePackDefOf.Sentence_CC_Midworld;
            }
            if (a == "Imperial")
            {
                item = RumorsRulePackDefOf.Sentence_CC_Imperial;
            }
            extraSentencePacks.Add(item);
        }
    }

    public class InteractionWorker_MakePeace : InteractionWorker
    {
        public override float RandomSelectionWeight(Pawn initiator, Pawn recipient)
        {
            float result;
            if (FactionUtility.HostileTo(recipient.Faction, Faction.OfPlayer) && !recipient.IsPrisonerOfColony)
            {
                result = 0f;
            }
            else
            {
                float num = 0.12f;
                if (initiator.story.traits.HasTrait(RumorsTraitDefOf.Peacemaker))
                {
                    num *= 10f;
                }
                IEnumerable<Thought_Memory> enumerable = ThirdPartyManager.GetMemoriesWithDef(recipient, ThoughtDefOf.Insulted);
                enumerable = enumerable.Concat(ThirdPartyManager.GetMemoriesWithDef(recipient, ThoughtDefOf.HarmedMe));
                enumerable = enumerable.Concat(ThirdPartyManager.GetMemoriesWithDef(recipient, ThoughtDefOf.HadAngeringFight));
                enumerable = enumerable.Concat(ThirdPartyManager.GetMemoriesWithDef(recipient, RumorsThoughtDefOf.HadLiesToldAboutMe));
                enumerable = from x in enumerable
                             where ((Thought_MemorySocial)x).OtherPawn() != initiator
                             select x;
                if (enumerable.Count() == 0)
                {
                    result = 0f;
                }
                else
                {
                    result = num;
                }
            }
            return result;
        }
        public override void Interacted(Pawn initiator, Pawn recipient, List<RulePackDef> extraSentencePacks, out string letterText, out string letterLabel, out LetterDef letterDef, out LookTargets lookTargets)
        {
            letterLabel = null;
            letterText = null;
            letterDef = null;
            lookTargets = null;
            IEnumerable<Thought_Memory> enumerable = ThirdPartyManager.GetMemoriesWithDef(recipient, ThoughtDefOf.Insulted);
            enumerable = enumerable.Concat(ThirdPartyManager.GetMemoriesWithDef(recipient, ThoughtDefOf.HarmedMe));
            enumerable = enumerable.Concat(ThirdPartyManager.GetMemoriesWithDef(recipient, ThoughtDefOf.HadAngeringFight));
            enumerable = enumerable.Concat(ThirdPartyManager.GetMemoriesWithDef(recipient, RumorsThoughtDefOf.HadLiesToldAboutMe));
            enumerable = from x in enumerable
                         where ((Thought_MemorySocial)x).OtherPawn() != initiator
                         select x;
            if (enumerable.Count() != 0)
            {
                Thought_Memory thought_Memory = GenCollection.RandomElement(enumerable);
                bool flag = Rand.Range(0, 25) > thought_Memory.MoodOffset() + 2f + initiator.skills.GetSkill(SkillDefOf.Social).Level;
                if (flag)
                {
                    recipient.needs.mood.thoughts.memories.RemoveMemory(thought_Memory);
                    if (initiator.story.traits.HasTrait(RumorsTraitDefOf.Peacemaker))
                    {
                        initiator.needs.mood.thoughts.memories.TryGainMemory(RumorsThoughtDefOf.EnjoyMakingPeace, null);
                    }
                    extraSentencePacks.Add(RumorsRulePackDefOf.Sentence_MakePeaceSucceeded);
                }
                else
                {
                    extraSentencePacks.Add(RumorsRulePackDefOf.Sentence_MakePeaceFailed);
                }
            }
        }
    }

    public class InteractionWorker_RevealSecret : InteractionWorker
    {
        private Pawn p3;
        public override float RandomSelectionWeight(Pawn initiator, Pawn recipient)
        {
            float result;
            if (initiator == null || recipient == null)
            {
                result = 0f;
            }
            else if (FactionUtility.HostileTo(recipient.Faction, Faction.OfPlayer) && !recipient.IsPrisonerOfColony)
            {
                result = 0f;
            }
            else if (initiator.story.traits.HasTrait(RumorsTraitDefOf.Trustworthy))
            {
                result = 0f;
            }
            else
            {
                IEnumerable<Thought_Memory> enumerable = ThirdPartyManager.GetMemoriesWithDef(initiator, RumorsThoughtDefOf.ReceivedSecret);
                if (enumerable.Count() == 0)
                {
                    result = 0f;
                }
                else
                {
                    enumerable = from x in enumerable
                                 where ((Thought_MemorySocial)x).OtherPawn() != recipient
                                 select x;
                    if (enumerable.Count() == 0)
                    {
                        result = 0f;
                    }
                    else
                    {
                        Thought_MemorySocial memory = GenCollection.RandomElement(enumerable) as Thought_MemorySocial;
                        IEnumerable<Pawn> enumerable2 = from x in ThirdPartyManager.GetAllFreeColonistsAlive
                                                        where x == memory.OtherPawn()
                                                        select x;
                        if (enumerable2.Count() == 0)
                        {
                            result = 0f;
                        }
                        else
                        {
                            this.p3 = GenCollection.RandomElement(enumerable2);
                            if (this.p3 == null)
                            {
                                result = 0f;
                            }
                            else
                            {
                                float num = Rand.Value;
                                if (initiator.story.traits.HasTrait(RumorsTraitDefOf.Gossip))
                                {
                                    num /= 2f;
                                }
                                if (num > Mathf.InverseLerp(75f, -50f, initiator.relations.OpinionOf(this.p3)))
                                {
                                    result = 0f;
                                }
                                else
                                {
                                    float num2 = 0.035f;
                                    if (initiator.story.traits.HasTrait(RumorsTraitDefOf.Gossip))
                                    {
                                        num2 *= 3.5f;
                                    }
                                    num2 *= (100f + initiator.relations.OpinionOf(recipient)) / 200f;
                                    result = num2;
                                }
                            }
                        }
                    }
                }
            }
            return result;
        }
        public override void Interacted(Pawn initiator, Pawn recipient, List<RulePackDef> extraSentencePacks, out string letterText, out string letterLabel, out LetterDef letterDef, out LookTargets lookTargets)
        {
            letterLabel = null;
            letterText = null;
            letterDef = null;
            lookTargets = null;
            if (this.p3 != null)
            {
                if (this.p3 != null)
                {
                    IEnumerable<Thought_Memory> enumerable = ThirdPartyManager.GetMemoriesWithDef(this.p3, RumorsThoughtDefOf.SharedSecret);
                    enumerable = from x in enumerable
                                 where ((Thought_MemorySocial)x).OtherPawn() == initiator
                                 select x;
                    if (enumerable.Count() > 0)
                    {
                        this.p3.needs.mood.thoughts.memories.RemoveMemory(GenCollection.RandomElement(enumerable));
                    }
                    if (this.p3.Map == initiator.Map)
                    {
                        this.p3.needs.mood.thoughts.memories.TryGainMemory(RumorsThoughtDefOf.MySecretWasRevealed, initiator);
                    }
                }
            }
        }
    }

    public class InteractionWorker_SharedSecret : InteractionWorker
    {
        private readonly SimpleCurve NormalCompatibilityCurve;
        private readonly SimpleCurve SharersCompatibilityCurve;
        public InteractionWorker_SharedSecret()
        {
            SimpleCurve simpleCurve = new SimpleCurve
            {
                { new CurvePoint(-1.5f, 0f), true },
                { new CurvePoint(-0.5f, 0.1f), true },
                { new CurvePoint(0.5f, 1f), true },
                { new CurvePoint(1f, 1.8f), true },
                { new CurvePoint(2f, 3f), true }
            };
            this.NormalCompatibilityCurve = simpleCurve;
            simpleCurve = new SimpleCurve
            {
                { new CurvePoint(-1.5f, 1.1f), true },
                { new CurvePoint(-0.5f, 1.5f), true },
                { new CurvePoint(0.5f, 1.8f), true },
                { new CurvePoint(1f, 2f), true },
                { new CurvePoint(2f, 3f), true }
            };
            this.SharersCompatibilityCurve = simpleCurve;
        }
        public override float RandomSelectionWeight(Pawn initiator, Pawn recipient)
        {
            float result;
            if (FactionUtility.HostileTo(recipient.Faction, Faction.OfPlayer) && !recipient.IsPrisonerOfColony)
            {
                result = 0f;
            }
            else
            {
                float num = 0.02f;
                if (initiator.story.traits.HasTrait(RumorsTraitDefOf.Gushing))
                {
                    result = num * this.SharersCompatibilityCurve.Evaluate(initiator.relations.CompatibilityWith(recipient));
                }
                else
                {
                    result = num * this.NormalCompatibilityCurve.Evaluate(initiator.relations.CompatibilityWith(recipient));
                }
            }
            return result;
        }
    }

    public class InteractionWorker_SpreadRumors : InteractionWorker
    {
        public override float RandomSelectionWeight(Pawn initiator, Pawn recipient)
        {
            float result;
            if (FactionUtility.HostileTo(recipient.Faction, Faction.OfPlayer) && !recipient.IsPrisonerOfColony)
            {
                result = 0f;
            }
            else
            {
                float num = 0.024f;
                if (!initiator.health.capacities.CapableOf(PawnCapacityDefOf.Talking))
                {
                    result = 0f;
                }
                else
                {
                    if (initiator.story.traits.HasTrait(RumorsTraitDefOf.CompulsiveLiar))
                    {
                        num *= 4.5f;
                    }
                    else if (initiator.story.traits.HasTrait(RumorsTraitDefOf.Trustworthy))
                    {
                        num /= 10f;
                    }
                    result = num;
                }
            }
            return result;
        }
        public override void Interacted(Pawn initiator, Pawn recipient, List<RulePackDef> extraSentencePacks, out string letterText, out string letterLabel, out LetterDef letterDef, out LookTargets lookTargets)
        {
            letterLabel = null;
            letterText = null;
            letterDef = null;
            lookTargets = null;
            Pawn pawn = this.ChooseGossipTarget(initiator, recipient);
            if (pawn != null)
            {
                if (3 + initiator.skills.GetSkill(SkillDefOf.Social).Level > Rand.Range(0, 25))
                {
                    recipient.needs.mood.thoughts.memories.TryGainMemory(RumorsThoughtDefOf.HeardAwfulThings, pawn);
                    if (initiator.story.traits.HasTrait(RumorsTraitDefOf.Trustworthy))
                    {
                        initiator.needs.mood.thoughts.memories.TryGainMemory(RumorsThoughtDefOf.ILied, null);
                    }
                }
                else
                {
                    recipient.needs.mood.thoughts.memories.TryGainMemory(RumorsThoughtDefOf.LiedTo, initiator);
                    pawn.needs.mood.thoughts.memories.TryGainMemory(RumorsThoughtDefOf.HadLiesToldAboutMe, initiator);
                }
            }
        }
        private Pawn ChooseGossipTarget(Pawn p1, Pawn p2)
        {
            IEnumerable<Pawn> enumerable = ThirdPartyManager.GetKnownPeople(p1, p2);
            enumerable = enumerable.Concat(ThirdPartyManager.GetKnownPeople(p1, p2).Intersect(ThirdPartyManager.GetAllColonistsLocalTo(p1)));
            Pawn result;
            if (enumerable.Count() == 0)
            {
                result = null;
            }
            else
            {
                Pawn pawn = null;
                if (p1.story.traits.HasTrait(RumorsTraitDefOf.Manipulative))
                {
                    foreach (Pawn current in
                        from x in enumerable
                        orderby p2.relations.OpinionOf(x) descending
                        select x)
                    {
                        if (p1.relations.OpinionOf(current) < 0)
                        {
                            pawn = current;
                            break;
                        }
                    }
                }
                else if (p1.story.traits.HasTrait(RumorsTraitDefOf.CompulsiveLiar))
                {
                    pawn = GenCollection.RandomElement(enumerable);
                }
                else
                {
                    GenCollection.TryRandomElementByWeight(enumerable, (Pawn x) => 0.0025f * (100f - p1.relations.OpinionOf(x)) + 0.0025f * (100f - p2.relations.OpinionOf(x)), out pawn);
                }
                result = pawn;
            }
            return result;
        }
    }

    // ========== THIRD PARTY MANAGER ==========

    public static class ThirdPartyManager
    {
        public static int iLevel = 0;
        public static bool first = true;
        public static Dictionary<Map, List<ICollection<Pawn>>> cliqueDict = new Dictionary<Map, List<ICollection<Pawn>>>();
        public static IEnumerable<Map> GetAllMapsContainingFreeSpawnedColonists
        {
            get
            {
                return from map in Find.Maps
                       where map.mapPawns.FreeColonistsSpawnedCount > 0
                       select map;
            }
        }
        public static IEnumerable<Caravan> GetAllPlayerCaravans
        {
            get
            {
                return from car in Find.WorldObjects.Caravans
                       where car.Faction == Faction.OfPlayer
                       select car;
            }
        }
        public static IEnumerable<Pawn> GetAllColonistsInCaravans
        {
            get
            {
                IEnumerable<Pawn> getAllPlayerCaravans =
                    from car in ThirdPartyManager.GetAllPlayerCaravans
                    from col in car.PawnsListForReading
                    where (col.RaceProps.Humanlike && !col.Dead && col.Faction == (object)Faction.OfPlayer)
                    select col;
                return getAllPlayerCaravans;
            }
        }
        public static IEnumerable<Pawn> GetAllFreeSpawnedColonistsOnMaps
        {
            get
            {
                IEnumerable<Pawn> getAllMapsContainingFreeSpawnedColonists =
                    from map in ThirdPartyManager.GetAllMapsContainingFreeSpawnedColonists
                    from col in map.mapPawns.FreeColonistsSpawned
                    where (col.RaceProps.Humanlike && !col.Dead && col.Faction == (object)Faction.OfPlayer)
                    select col;
                return getAllMapsContainingFreeSpawnedColonists;
            }
        }
        public static IEnumerable<Pawn> GetAllFreeColonistsAlive
        {
            get
            {
                return ThirdPartyManager.GetAllFreeSpawnedColonistsOnMaps.Concat(ThirdPartyManager.GetAllColonistsInCaravans);
            }
        }
        public static IEnumerable<Pawn> GetAllColonistsLocalTo(Pawn p)
        {
            return from x in ThirdPartyManager.GetAllFreeColonistsAlive
                   where x.RaceProps.Humanlike && x.Faction == Faction.OfPlayer && x != p && ((x.Map != null && x.Map == p.Map) || (CaravanUtility.GetCaravan(x) != null && CaravanUtility.GetCaravan(x) == CaravanUtility.GetCaravan(p)))
                   select x;
        }
        public static IEnumerable<Pawn> GetKnownPeople(Pawn p1, Pawn p2)
        {
            IEnumerable<Pawn> source = ThirdPartyManager.GetAllFreeColonistsAlive;
            source = from p in source
                     where p.RaceProps.Humanlike
                     select p;
            return from p3 in source
                   where p3 != p1 && p3 != p2 && RelationsUtility.PawnsKnowEachOther(p1, p3) && RelationsUtility.PawnsKnowEachOther(p2, p3)
                   select p3;
        }
        public static IEnumerable<Thought_Memory> GetMemoriesWithDef(Pawn p, ThoughtDef tdef)
        {
            List<Thought_Memory> memories = p.needs.mood.thoughts.memories.Memories;
            return from x in memories
                   where x.def == tdef
                   select x;
        }
        public static bool DoesEveryoneLocallyHate(Pawn p)
        {
            IEnumerable<Pawn> allColonistsLocalTo = ThirdPartyManager.GetAllColonistsLocalTo(p);
            bool result;
            if (allColonistsLocalTo.Count() < 1)
            {
                result = false;
            }
            else
            {
                foreach (Pawn current in allColonistsLocalTo)
                {
                    if (current.relations.OpinionOf(p) > -8 && current != p)
                    {
                        result = false;
                        return result;
                    }
                }
                result = true;
            }
            return result;
        }
        public static void FindCliques()
        {
            ThirdPartyManager.cliqueDict.Clear();
            foreach (Map m in Find.Maps)
            {
                List<Pawn> list = m.mapPawns.FreeColonistsSpawned.ToList();
                List<ICollection<Pawn>> list2 = new List<ICollection<Pawn>>();
                float num = 0.667f * list.Count;
                ThirdPartyManager.BKA(list2, new List<Pawn>(), list, new List<Pawn>());
                if (list2.Count() == 0)
                {
                    break;
                }
                list2 = (from x in list2
                         where x.Count >= 3
                         select x).ToList();
                if (list2.Count() == 0)
                {
                    break;
                }
                IDictionary<Pawn, ICollection<Pawn>> dictionary = new Dictionary<Pawn, ICollection<Pawn>>();
                IEnumerable<Pawn> freeColonistsSpawned = m.mapPawns.FreeColonistsSpawned;
                foreach (Pawn p in freeColonistsSpawned)
                {
                    List<ICollection<Pawn>> list3 = (from x in list2
                                                     where x.Contains(p)
                                                     select x).ToList();
                    if (list3.Count >= 2)
                    {
                        list3.Sort((ICollection<Pawn> x1, ICollection<Pawn> x2) => ThirdPartyManager.GetPawnAverageRelationshipWithGroup(p, x1).CompareTo(ThirdPartyManager.GetPawnAverageRelationshipWithGroup(p, x2)));
                        list3.Reverse();
                        dictionary.Add(p, list3.FirstOrDefault());
                    }
                }
                foreach (Pawn current in list)
                {
                    foreach (ICollection<Pawn> current2 in list2)
                    {
                        if (dictionary.ContainsKey(current))
                        {
                            if (current2 != dictionary[current] && current2.Contains(current))
                            {
                                current2.Remove(current);
                            }
                        }
                    }
                }
                list2 = (from x in list2
                         where x.Count >= 3 && x.Count < 0.667f * m.mapPawns.FreeColonistsSpawnedCount
                         select x).ToList();
                if (list2.Count() == 0)
                {
                    break;
                }
                List<ICollection<Pawn>> value = list2.Distinct().ToList();
                ThirdPartyManager.first = false;
                ThirdPartyManager.cliqueDict.Add(m, value);
            }
        }
        public static ICollection<ICollection<Pawn>> GetIsolatedCliques(this Map self, int threshold)
        {
            ICollection<ICollection<Pawn>> result;
            if (ThirdPartyManager.cliqueDict.TryGetValue(self, out List<ICollection<Pawn>> source))
            {
                IEnumerable<Pawn> allColonists = self.mapPawns.FreeColonistsSpawned;
                result = (from clique in source
                          where ThirdPartyManager.GetGroupAverageRelationshipWithGroup(clique, allColonists.Except(clique)) <= threshold
                          select clique).ToList();
            }
            else
            {
                result = null;
            }
            return result;
        }
        public static float GetPawnAverageRelationshipWithGroup(Pawn p, IEnumerable<Pawn> g)
        {
            float num = 0f;
            foreach (Pawn current in g)
            {
                num += p.relations.OpinionOf(current);
            }
            return num / g.Count();
        }
        public static float GetGroupAverageRelationshipWithGroup(IEnumerable<Pawn> g1, IEnumerable<Pawn> g2)
        {
            float num = 0f;
            foreach (Pawn current in g1)
            {
                num += ThirdPartyManager.GetPawnAverageRelationshipWithGroup(current, g2);
            }
            return num / g1.Count();
        }
        public static void BKA(ICollection<ICollection<Pawn>> cliques, ICollection<Pawn> R, ICollection<Pawn> P, ICollection<Pawn> X)
        {
            if (P.Count == 0 && X.Count == 0)
            {
                if (R.Count != 0)
                {
                    cliques.Add(R);
                }
            }
            else
            {
                List<Pawn> list = P.Union(X).ToList();
                list.Sort((Pawn p1, Pawn p2) => ThirdPartyManager.Neighbours(p1, P).Count().CompareTo(ThirdPartyManager.Neighbours(p2, P).Count()));
                list.Reverse();
                Pawn p = list.FirstOrDefault();
                List<Pawn> second = ThirdPartyManager.Neighbours(p, P).ToList();
                List<Pawn> list2 = new List<Pawn>();
                List<Pawn> list3 = new List<Pawn>();
                List<Pawn> list4 = new List<Pawn>();
                List<Pawn> list5 = new List<Pawn>(P).Except(second).ToList();
                foreach (Pawn current in list5)
                {
                    list2.Clear();
                    list2.AddRange(R);
                    if (!list2.Contains(current))
                    {
                        list2.Add(current);
                    }
                    list3.Clear();
                    list3.AddRange(P);
                    list3 = list3.Intersect(ThirdPartyManager.Neighbours(current, list)).ToList();
                    list4.Clear();
                    list4.AddRange(X);
                    list4 = list4.Intersect(ThirdPartyManager.Neighbours(current, list)).ToList();
                    ThirdPartyManager.BKA(cliques, list2, list3, list4);
                    P.Remove(current);
                    X.Add(current);
                }
            }
        }
        private static IEnumerable<Pawn> Neighbours(Pawn p1, IEnumerable<Pawn> pawns)
        {
            return from x in pawns
                   where p1 != x && p1.relations.OpinionOf(x) >= 10 && x.relations.OpinionOf(p1) >= 10
                   select x;
        }
        public static string GetChildhoodCulturalAdjective(Pawn p)
        {
            string result = "Colonial";
            if (p.story.childhood != null)
            {
                if (p.story.childhood.spawnCategories.Contains("Tribal"))
                {
                    result = "Tribal";
                }
                else if (p.story.childhood.title.Contains("medieval") || p.story.childhood.baseDesc.IndexOf("Medieval", StringComparison.OrdinalIgnoreCase) >= 0 || p.story.childhood.baseDesc.IndexOf("Village", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    result = "Medieval";
                }
                else if (p.story.childhood.title.Contains("glitterworld") || p.story.childhood.baseDesc.IndexOf("Glitterworld", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    if (p.story.childhood.title != "discarded youth" && p.story.childhood.title != "corporate slave")
                    {
                        result = "Glitterworld";
                    }
                }
                else if (p.story.childhood.title.Contains("urbworld") || p.story.childhood.title.Contains("vatgrown") || p.story.childhood.baseDesc.IndexOf("Urbworld", StringComparison.OrdinalIgnoreCase) >= 0 || p.story.childhood.baseDesc.IndexOf("Industrial", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    result = "Urbworld";
                }
                else if (p.story.childhood.title.Contains("midworld") || p.story.childhood.baseDesc.IndexOf("Midworld", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    result = "Midworld";
                }
                else if (p.story.childhood.baseDesc.IndexOf("Tribe", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    result = "Tribal";
                }
                else if (p.story.childhood.title.Contains("imperial") || p.story.childhood.baseDesc.IndexOf("Imperial", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    result = "Imperial";
                }
            }
            return result;
        }
        public static string GetAdultCulturalAdjective(Pawn p)
        {
            string result = "Colonial";
            if (p.story.adulthood != null)
            {
                if (p.story.adulthood.spawnCategories.Contains("Tribal"))
                {
                    result = "Tribal";
                }
                else if (p.story.adulthood.title.Contains("medieval") || p.story.adulthood.baseDesc.IndexOf("Medieval", StringComparison.OrdinalIgnoreCase) >= 0 || p.story.adulthood.baseDesc.IndexOf("Village", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    result = "Medieval";
                }
                else if (p.story.adulthood.title.Contains("glitterworld") || p.story.adulthood.baseDesc.IndexOf("Glitterworld", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    if (p.story.adulthood.title != "adventurer")
                    {
                        result = "Glitterworld";
                    }
                }
                else if (p.story.adulthood.title.Contains("urbworld") || p.story.adulthood.title.Contains("vatgrown") || p.story.adulthood.baseDesc.IndexOf("Urbworld", StringComparison.OrdinalIgnoreCase) >= 0 || p.story.adulthood.baseDesc.IndexOf("Urbworld", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    result = "Urbworld";
                }
                else if (p.story.adulthood.title.Contains("midworld") || p.story.adulthood.baseDesc.IndexOf("Midworld", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    result = "Midworld";
                }
                else if (p.story.adulthood.baseDesc.IndexOf("Tribe", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    result = "Tribal";
                }
                else if (p.story.adulthood.title.Contains("imperial") || p.story.adulthood.baseDesc.IndexOf("Imperial", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    result = "Imperial";
                }
            }
            return result;
        }
    }

    // ========== THOUGHT WORKERS ==========

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
            else if (p.Map.GetIsolatedCliques(-3).Count() > 0 && p.Faction == Faction.OfPlayer)
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
                int num = 1;
                foreach (Pawn current in ThirdPartyManager.GetAllColonistsLocalTo(p))
                {
                    if (p != current && current.IsColonist && !current.Dead && (p.relations.OpinionOf(current) < 40 || current.relations.OpinionOf(p) < 40))
                    {
                        num = 0;
                        break;
                    }
                }
                foreach (Pawn current2 in ThirdPartyManager.GetAllColonistsLocalTo(p))
                {
                    if (p != current2 && current2.IsColonist && !current2.Dead && (p.relations.OpinionOf(current2) < 15 || current2.relations.OpinionOf(p) < 15))
                    {
                        num = -1;
                        break;
                    }
                }
                if (num == -1)
                {
                    result = ThoughtState.Inactive;
                }
                else
                {
                    result = ThoughtState.ActiveAtStage(num);
                }
            }
            return result;
        }
    }

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
            else if (CaravanUtility.GetCaravan(p) != null)
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

    public class ThoughtWorker_RunAway : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            return ThoughtState.Inactive;
        }
    }

    public class ThoughtWorker_TrustworthyVsGossip : ThoughtWorker
    {
        protected override ThoughtState CurrentSocialStateInternal(Pawn p, Pawn other)
        {
            ThoughtState result;
            if (!RelationsUtility.PawnsKnowEachOther(p, other))
            {
                result = false;
            }
            else if (!p.RaceProps.Humanlike || !other.RaceProps.Humanlike)
            {
                result = false;
            }
            else if (p.story.traits.HasTrait(RumorsTraitDefOf.Trustworthy) && other.story.traits.HasTrait(RumorsTraitDefOf.Gossip))
            {
                result = true;
            }
            else
            {
                result = false;
            }
            return result;
        }
    }

    public class ThoughtWorker_AnnoyingVoice : ThoughtWorker
    {
        public ThoughtWorker_AnnoyingVoice() { }
        protected override ThoughtState CurrentSocialStateInternal(Pawn pawn, Pawn other)
        {
            ThoughtState thoughtState;
            if (!other.RaceProps.Humanlike || !RelationsUtility.PawnsKnowEachOther(pawn, other))
            {
                thoughtState = false;
            }
            else if (!pawn.health.capacities.CapableOf(PawnCapacityDefOf.Hearing))
            {
                thoughtState = false;
            }
            else if (other.story.traits.HasTrait(TraitDefOf.AnnoyingVoice))
            {
                thoughtState = (!pawn.UnderstandsDisability());
            }
            else
            {
                thoughtState = false;
            }
            return thoughtState;
        }
    }

    public class ThoughtWorker_AnnoyingVoiceAmeliorated : ThoughtWorker
    {
        public ThoughtWorker_AnnoyingVoiceAmeliorated() { }
        protected override ThoughtState CurrentSocialStateInternal(Pawn pawn, Pawn other)
        {
            ThoughtState thoughtState;
            if (!other.RaceProps.Humanlike || !RelationsUtility.PawnsKnowEachOther(pawn, other))
            {
                thoughtState = false;
            }
            else if (!pawn.health.capacities.CapableOf(PawnCapacityDefOf.Hearing))
            {
                thoughtState = false;
            }
            else if (other.story.traits.HasTrait(TraitDefOf.AnnoyingVoice))
            {
                thoughtState = (pawn.UnderstandsDisability());
            }
            else
            {
                thoughtState = false;
            }
            return thoughtState;
        }
    }

    public class ThoughtWorker_CreepyBreathing : ThoughtWorker
    {
        public ThoughtWorker_CreepyBreathing() { }
        protected override ThoughtState CurrentSocialStateInternal(Pawn pawn, Pawn other)
        {
            ThoughtState thoughtState;
            if (!other.RaceProps.Humanlike || !RelationsUtility.PawnsKnowEachOther(pawn, other))
            {
                thoughtState = false;
            }
            else if (!pawn.health.capacities.CapableOf(PawnCapacityDefOf.Hearing))
            {
                thoughtState = false;
            }
            else if (other.story.traits.HasTrait(TraitDefOf.CreepyBreathing))
            {
                thoughtState = (!pawn.UnderstandsDisability());
            }
            else
            {
                thoughtState = false;
            }
            return thoughtState;
        }
    }

    public class ThoughtWorker_CreepyBreathingAmeliorated : ThoughtWorker
    {
        public ThoughtWorker_CreepyBreathingAmeliorated() { }
        protected override ThoughtState CurrentSocialStateInternal(Pawn pawn, Pawn other)
        {
            ThoughtState thoughtState;
            if (!other.RaceProps.Humanlike || !RelationsUtility.PawnsKnowEachOther(pawn, other))
            {
                thoughtState = false;
            }
            else if (!pawn.health.capacities.CapableOf(PawnCapacityDefOf.Hearing))
            {
                thoughtState = false;
            }
            else if (other.story.traits.HasTrait(TraitDefOf.CreepyBreathing))
            {
                thoughtState = (pawn.UnderstandsDisability());
            }
            else
            {
                thoughtState = false;
            }
            return thoughtState;
        }
    }

    public class ThoughtWorker_Disfigured : ThoughtWorker
    {
        public ThoughtWorker_Disfigured() { }
        protected override ThoughtState CurrentSocialStateInternal(Pawn pawn, Pawn other)
        {
            ThoughtState thoughtState;
            if (!other.RaceProps.Humanlike || !RelationsUtility.PawnsKnowEachOther(pawn, other) || other.Dead)
            {
                thoughtState = false;
            }
            else if (!pawn.health.capacities.CapableOf(PawnCapacityDefOf.Sight))
            {
                thoughtState = false;
            }
            else if (RelationsUtility.IsDisfigured(other))
            {
                thoughtState = (!pawn.UnderstandsDisability());
            }
            else
            {
                thoughtState = false;
            }
            return thoughtState;
        }
    }

    public class ThoughtWorker_DisfiguredAmeliorated : ThoughtWorker
    {
        public ThoughtWorker_DisfiguredAmeliorated() { }
        protected override ThoughtState CurrentSocialStateInternal(Pawn pawn, Pawn other)
        {
            ThoughtState thoughtState;
            if (!other.RaceProps.Humanlike || !RelationsUtility.PawnsKnowEachOther(pawn, other) || other.Dead)
            {
                thoughtState = false;
            }
            else if (!pawn.health.capacities.CapableOf(PawnCapacityDefOf.Sight))
            {
                thoughtState = false;
            }
            else if (RelationsUtility.IsDisfigured(other))
            {
                thoughtState = (pawn.UnderstandsDisability());
            }
            else
            {
                thoughtState = false;
            }
            return thoughtState;
        }
    }

    public class ThoughtWorker_Ugly : ThoughtWorker
    {
        public ThoughtWorker_Ugly() { }
        protected override ThoughtState CurrentSocialStateInternal(Pawn pawn, Pawn other)
        {
            ThoughtState thoughtState;
            if (!other.RaceProps.Humanlike || !RelationsUtility.PawnsKnowEachOther(pawn, other))
            {
                thoughtState = false;
            }
            else if (!pawn.health.capacities.CapableOf(PawnCapacityDefOf.Sight))
            {
                thoughtState = false;
            }
            else if (!pawn.UnderstandsDisability())
            {
                int num = other.story.traits.DegreeOfTrait(TraitDefOf.Beauty);
                if (num == -1)
                {
                    thoughtState = ThoughtState.ActiveAtStage(0);
                }
                else if (num == -2)
                {
                    thoughtState = ThoughtState.ActiveAtStage(1);
                }
                else
                {
                    thoughtState = false;
                }
            }
            else
            {
                thoughtState = false;
            }
            return thoughtState;
        }
    }

    public class ThoughtWorker_UglyAmeliorated : ThoughtWorker
    {
        public ThoughtWorker_UglyAmeliorated() { }
        protected override ThoughtState CurrentSocialStateInternal(Pawn pawn, Pawn other)
        {
            ThoughtState thoughtState;
            if (!other.RaceProps.Humanlike || !RelationsUtility.PawnsKnowEachOther(pawn, other))
            {
                thoughtState = false;
            }
            else if (!pawn.health.capacities.CapableOf(PawnCapacityDefOf.Sight))
            {
                thoughtState = false;
            }
            else if (pawn.UnderstandsDisability())
            {
                int num = other.story.traits.DegreeOfTrait(TraitDefOf.Beauty);
                if (num == -1)
                {
                    thoughtState = ThoughtState.ActiveAtStage(0);
                }
                else if (num == -2)
                {
                    thoughtState = ThoughtState.ActiveAtStage(1);
                }
                else
                {
                    thoughtState = false;
                }
            }
            else
            {
                thoughtState = false;
            }
            return thoughtState;
        }
    }

    public static class EmpathyUtility
    {
        public static bool UnderstandsDisability(this Pawn self)
        {
            bool flag;
            flag = (RelationsUtility.IsDisfigured(self) || self.story.traits.DegreeOfTrait(TraitDefOf.Beauty) < 0 || self.story.traits.HasTrait(TraitDefOf.CreepyBreathing) || self.story.traits.HasTrait(TraitDefOf.AnnoyingVoice));
            return flag;
        }
    }

}
