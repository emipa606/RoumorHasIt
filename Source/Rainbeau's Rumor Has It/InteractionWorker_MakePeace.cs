﻿using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Rumor_Code;

public class InteractionWorker_MakePeace : InteractionWorker
{
    public override float RandomSelectionWeight(Pawn initiator, Pawn recipient)
    {
        if (recipient.Faction.HostileTo(Faction.OfPlayer) && !recipient.IsPrisonerOfColony)
        {
            return 0f;
        }

        var num = 0.12f;
        if (initiator.story.traits.HasTrait(RumorsTraitDefOf.Peacemaker))
        {
            num *= 10f;
        }

        var enumerable = ThirdPartyManager.GetMemoriesWithDef(recipient, ThoughtDef.Named("Insulted"));
        enumerable = enumerable.Concat(ThirdPartyManager.GetMemoriesWithDef(recipient, ThoughtDefOf.HarmedMe));
        enumerable =
            enumerable.Concat(ThirdPartyManager.GetMemoriesWithDef(recipient, ThoughtDefOf.HadAngeringFight));
        enumerable =
            enumerable.Concat(
                ThirdPartyManager.GetMemoriesWithDef(recipient, RumorsThoughtDefOf.HadLiesToldAboutMe));
        enumerable = from x in enumerable
            where ((Thought_MemorySocial)x).OtherPawn() != initiator
            select x;
        return !enumerable.Any() ? 0f : num;
    }

    public override void Interacted(Pawn initiator, Pawn recipient, List<RulePackDef> extraSentencePacks,
        out string letterText, out string letterLabel, out LetterDef letterDef, out LookTargets lookTargets)
    {
        letterLabel = null;
        letterText = null;
        letterDef = null;
        lookTargets = null;
        var enumerable = ThirdPartyManager.GetMemoriesWithDef(recipient, ThoughtDef.Named("Insulted"));
        enumerable = enumerable.Concat(ThirdPartyManager.GetMemoriesWithDef(recipient, ThoughtDefOf.HarmedMe));
        enumerable =
            enumerable.Concat(ThirdPartyManager.GetMemoriesWithDef(recipient, ThoughtDefOf.HadAngeringFight));
        enumerable =
            enumerable.Concat(
                ThirdPartyManager.GetMemoriesWithDef(recipient, RumorsThoughtDefOf.HadLiesToldAboutMe));
        enumerable = from x in enumerable
            where ((Thought_MemorySocial)x).OtherPawn() != initiator
            select x;
        if (!enumerable.Any())
        {
            return;
        }

        var thought_Memory = enumerable.RandomElement();
        if (Rand.Range(0, 25) > thought_Memory.MoodOffset() + 2f +
            initiator.skills.GetSkill(SkillDefOf.Social).Level)
        {
            recipient.needs.mood.thoughts.memories.RemoveMemory(thought_Memory);
            if (initiator.story.traits.HasTrait(RumorsTraitDefOf.Peacemaker))
            {
                initiator.needs.mood.thoughts.memories.TryGainMemory(RumorsThoughtDefOf.EnjoyMakingPeace);
            }

            extraSentencePacks.Add(RumorsRulePackDefOf.Sentence_MakePeaceSucceeded);
        }
        else
        {
            extraSentencePacks.Add(RumorsRulePackDefOf.Sentence_MakePeaceFailed);
        }
    }
}