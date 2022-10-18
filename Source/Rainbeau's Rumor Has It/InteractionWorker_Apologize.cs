using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Rumor_Code;

public class InteractionWorker_Apologize : InteractionWorker
{
    public override float RandomSelectionWeight(Pawn initiator, Pawn recipient)
    {
        if (recipient.Faction.HostileTo(Faction.OfPlayer) && !recipient.IsPrisonerOfColony)
        {
            return 0f;
        }

        var num = 10f;
        var enumerable = ThirdPartyManager.GetMemoriesWithDef(recipient, ThoughtDefOf.Insulted);
        enumerable = enumerable.Concat(ThirdPartyManager.GetMemoriesWithDef(recipient, ThoughtDefOf.HarmedMe));
        enumerable =
            enumerable.Concat(ThirdPartyManager.GetMemoriesWithDef(recipient, ThoughtDefOf.HadAngeringFight));
        enumerable =
            enumerable.Concat(
                ThirdPartyManager.GetMemoriesWithDef(recipient, RumorsThoughtDefOf.HadLiesToldAboutMe));
        enumerable = from x in enumerable
            where ((Thought_MemorySocial)x).OtherPawn() == initiator
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
        var enumerable = ThirdPartyManager.GetMemoriesWithDef(recipient, ThoughtDefOf.Insulted);
        enumerable = enumerable.Concat(ThirdPartyManager.GetMemoriesWithDef(recipient, ThoughtDefOf.HarmedMe));
        enumerable =
            enumerable.Concat(ThirdPartyManager.GetMemoriesWithDef(recipient, ThoughtDefOf.HadAngeringFight));
        enumerable =
            enumerable.Concat(
                ThirdPartyManager.GetMemoriesWithDef(recipient, RumorsThoughtDefOf.HadLiesToldAboutMe));
        enumerable = from x in enumerable
            where ((Thought_MemorySocial)x).OtherPawn() == initiator
            select x;
        if (!enumerable.Any())
        {
            return;
        }

        var thought_MemorySocial = (Thought_MemorySocial)enumerable.RandomElement();
        float num = Rand.Range(1, 100) + initiator.skills.GetSkill(SkillDefOf.Social).Level;
        switch (num)
        {
            case < 40f:
                extraSentencePacks.Add(RumorsRulePackDefOf.Sentence_ApologyFailed);
                break;
            case < 90f:
                extraSentencePacks.Add(RumorsRulePackDefOf.Sentence_ApologySucceeded);
                recipient.needs.mood.thoughts.memories.TryGainMemory(RumorsThoughtDefOf.ApologizedTo, initiator);
                break;
            default:
                extraSentencePacks.Add(RumorsRulePackDefOf.Sentence_ApologySucceededBig);
                recipient.needs.mood.thoughts.memories.RemoveMemory(thought_MemorySocial);
                recipient.needs.mood.thoughts.memories.TryGainMemory(RumorsThoughtDefOf.ApologizedToBig, initiator);
                initiator.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.RapportBuilt, recipient);
                break;
        }
    }
}