﻿using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Rumor_Code;

public class InteractionWorker_RevealSecret : InteractionWorker
{
    private Pawn p3;

    public override float RandomSelectionWeight(Pawn initiator, Pawn recipient)
    {
        if (initiator == null || recipient == null)
        {
            return 0f;
        }

        if (recipient.Faction.HostileTo(Faction.OfPlayer) && !recipient.IsPrisonerOfColony)
        {
            return 0f;
        }

        if (initiator.story.traits.HasTrait(RumorsTraitDefOf.Trustworthy))
        {
            return 0f;
        }

        var enumerable = ThirdPartyManager.GetMemoriesWithDef(initiator, RumorsThoughtDefOf.ReceivedSecret);
        if (!enumerable.Any())
        {
            return 0f;
        }

        enumerable = from x in enumerable
            where ((Thought_MemorySocial)x).OtherPawn() != recipient
            select x;
        if (!enumerable.Any())
        {
            return 0f;
        }

        {
            var memory = enumerable.RandomElement() as Thought_MemorySocial;
            var enumerable2 = from x in ThirdPartyManager.GetAllFreeColonistsAlive
                where x == memory?.OtherPawn()
                select x;
            if (!enumerable2.Any())
            {
                return 0f;
            }

            p3 = enumerable2.RandomElement();
            if (p3 == null)
            {
                return 0f;
            }

            var num = Rand.Value;
            if (initiator.story.traits.HasTrait(RumorsTraitDefOf.Gossip))
            {
                num /= 2f;
            }

            if (num > Mathf.InverseLerp(75f, -50f, initiator.relations.OpinionOf(p3)))
            {
                return 0f;
            }

            var num2 = 0.035f;
            if (initiator.story.traits.HasTrait(RumorsTraitDefOf.Gossip))
            {
                num2 *= 3.5f;
            }

            num2 *= (100f + initiator.relations.OpinionOf(recipient)) / 200f;
            return num2;
        }
    }

    public override void Interacted(Pawn initiator, Pawn recipient, List<RulePackDef> extraSentencePacks,
        out string letterText, out string letterLabel, out LetterDef letterDef, out LookTargets lookTargets)
    {
        letterLabel = null;
        letterText = null;
        letterDef = null;
        lookTargets = null;

        if (p3 == null)
        {
            return;
        }

        var enumerable = ThirdPartyManager.GetMemoriesWithDef(p3, RumorsThoughtDefOf.SharedSecret);
        enumerable = from x in enumerable
            where ((Thought_MemorySocial)x).OtherPawn() == initiator
            select x;
        if (enumerable.Any())
        {
            p3.needs.mood.thoughts.memories.RemoveMemory(enumerable.RandomElement());
        }

        if (p3.Map == initiator.Map)
        {
            p3.needs.mood.thoughts.memories.TryGainMemory(RumorsThoughtDefOf.MySecretWasRevealed,
                initiator);
        }
    }
}