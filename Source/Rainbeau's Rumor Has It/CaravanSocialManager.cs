﻿using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace Rumor_Code;

public static class CaravanSocialManager
{
    public static void MakeCaravansInteract()
    {
        foreach (var current in ThirdPartyManager.GetAllPlayerCaravans)
        {
            MakeCaravanInteract(current);
        }
    }

    public static void MakeCaravanInteract(Caravan c)
    {
        var pawnsListForReading = c.PawnsListForReading;
        if (pawnsListForReading.Count < 3)
        {
            return;
        }

        foreach (var current in pawnsListForReading)
        {
            if (Rand.Value < 0.02)
            {
                TryInteractRandomly(current);
            }
        }
    }

    public static bool TryInteractWith(Pawn initiator, Pawn recipient, InteractionDef intDef)
    {
        if (initiator == recipient)
        {
            Log.Warning($"{initiator} tried to interact with self, interaction={intDef.defName}");
            return false;
        }

        var list = new List<RulePackDef>();
        if (intDef.initiatorThought != null)
        {
            AddInteractionThought(initiator, recipient, intDef.initiatorThought);
        }

        if (intDef.recipientThought != null && recipient.needs.mood != null)
        {
            AddInteractionThought(recipient, initiator, intDef.recipientThought);
        }

        if (intDef.initiatorXpGainSkill != null)
        {
            initiator.skills.Learn(intDef.initiatorXpGainSkill, intDef.initiatorXpGainAmount);
        }

        if (intDef.recipientXpGainSkill != null && recipient.RaceProps.Humanlike)
        {
            recipient.skills.Learn(intDef.recipientXpGainSkill, intDef.recipientXpGainAmount);
        }

        if (recipient.RaceProps.Humanlike)
        {
        }

        intDef.Worker.Interacted(initiator, recipient, list, out _, out _, out _, out _);

        Find.PlayLog.Add(new PlayLogEntry_Interaction(intDef, initiator, recipient, list));

        return true;
    }

    private static void TryInteractRandomly(Pawn p)
    {
        if (p == null)
        {
            Log.Message("Pawn is null!");
        }
        else if (p.GetCaravan() == null)
        {
        }
        else
        {
            if (p.interactions == null)
            {
            }

            if (p.RaceProps.Humanlike)
            {
                var list = ThirdPartyManager.GetAllColonistsLocalTo(p).ToList();
                if (list.Count == 0)
                {
                    return;
                }

                list.Shuffle();
                var allDefsListForReading = DefDatabase<InteractionDef>.AllDefsListForReading;
                foreach (var p2 in list)
                {
                    if (p2 == null)
                    {
                        return;
                    }

                    var p3 = p2;
                    if (!allDefsListForReading.TryRandomElementByWeight(
                            x => ParsedRandomInteractionWeight(x, p, p3),
                            out var intDef))
                    {
                        continue;
                    }

                    if (TryInteractWith(p, p2, intDef))
                    {
                        return;
                    }

                    Log.Error($"{p} failed to interact with {p}");
                }
            }
            else if (p.RaceProps.Animal && Rand.Value < 0.05f)
            {
                var nuzzle = InteractionDefOf.Nuzzle;
                var list2 = ThirdPartyManager.GetAllColonistsLocalTo(p).ToList();
                if (list2.Count == 0)
                {
                    return;
                }

                list2.Shuffle();
                foreach (var pawn in list2)
                {
                    if (pawn == null)
                    {
                        return;
                    }

                    if (TryInteractWith(p, pawn, nuzzle))
                    {
                        return;
                    }

                    Log.Error($"{p} failed to interact with {p}");
                }
            }
        }
    }

    private static float ParsedRandomInteractionWeight(InteractionDef def, Pawn p1, Pawn p2)
    {
        float result;
        try
        {
            result = def.Worker.RandomSelectionWeight(p1, p2);
        }
        catch
        {
            result = 0.025f;
        }

        return result;
    }

    private static void AddInteractionThought(Pawn pawn, Pawn otherPawn, ThoughtDef thoughtDef)
    {
        var statValue = otherPawn.GetStatValue(StatDefOf.SocialImpact);
        var thought_Memory = (Thought_Memory)ThoughtMaker.MakeThought(thoughtDef);
        thought_Memory.moodPowerFactor = statValue;
        if (thought_Memory is Thought_MemorySocial thought_MemorySocial)
        {
            thought_MemorySocial.opinionOffset *= statValue;
        }

        pawn.needs.mood.thoughts.memories.TryGainMemory(thought_Memory, otherPawn);
    }
}