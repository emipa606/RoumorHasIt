using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Rumor_Code;

public class InteractionWorker_ChattedAboutSomeone : InteractionWorker
{
    private Pawn p3;

    public override float RandomSelectionWeight(Pawn initiator, Pawn recipient)
    {
        if (recipient.Faction.HostileTo(Faction.OfPlayer) && !recipient.IsPrisonerOfColony)
        {
            return 0f;
        }

        var num = 0.7f;
        if (initiator.story.traits.HasTrait(RumorsTraitDefOf.Gossip))
        {
            num *= 2f;
        }

        p3 = ChooseChattedAbout(initiator, recipient);

        return p3 == null ? 0f : num;
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

        float num = initiator.relations.OpinionOf(p3);
        float num2 = initiator.relations.OpinionOf(p3);
        if (Rand.Range(0, 100) < (initiator.relations.OpinionOf(recipient) + 20f +
                                  (1.5f * recipient.skills.GetSkill(SkillDefOf.Social).Level)) / 1.5f &&
            !initiator.story.traits.HasTrait(RumorsTraitDefOf.Manipulative))
        {
            ThoughtDef thoughtDef = null;
            switch (num2)
            {
                case > 40f:
                    thoughtDef = RumorsThoughtDefOf.HeardGreatThings;
                    break;
                case > 10f:
                    thoughtDef = RumorsThoughtDefOf.HeardGoodThings;
                    break;
                case < -40f:
                    thoughtDef = RumorsThoughtDefOf.HeardAwfulThings;
                    break;
                case < -10f:
                    thoughtDef = RumorsThoughtDefOf.HeardBadThings;
                    break;
            }

            if (thoughtDef != null)
            {
                initiator.needs.mood.thoughts.memories.TryGainMemory(thoughtDef, p3);
            }
        }

        if (!(Rand.Range(0, 100) < (recipient.relations.OpinionOf(initiator) + 20f +
                                    (1.5f * initiator.skills.GetSkill(SkillDefOf.Social).Level)) / 1.5f) ||
            recipient.story.traits.HasTrait(RumorsTraitDefOf.Manipulative))
        {
            return;
        }

        ThoughtDef thoughtDef2 = null;
        switch (num)
        {
            case > 40f:
                thoughtDef2 = RumorsThoughtDefOf.HeardGreatThings;
                break;
            case > 10f:
                thoughtDef2 = RumorsThoughtDefOf.HeardGoodThings;
                break;
            case < -40f:
                thoughtDef2 = RumorsThoughtDefOf.HeardAwfulThings;
                break;
            case < -10f:
                thoughtDef2 = RumorsThoughtDefOf.HeardBadThings;
                break;
        }

        if (thoughtDef2 != null)
        {
            recipient.needs.mood.thoughts.memories.TryGainMemory(thoughtDef2, p3);
        }
    }

    private Pawn ChooseChattedAbout(Pawn p1, Pawn p2)
    {
        var enumerable = ThirdPartyManager.GetKnownPeople(p1, p2);
        enumerable = enumerable.Concat(ThirdPartyManager.GetKnownPeople(p1, p2)
            .Intersect(ThirdPartyManager.GetAllColonistsLocalTo(p1)));
        Pawn result;
        if (!enumerable.Any())
        {
            result = null;
        }
        else if (p1.story.traits.HasTrait(RumorsTraitDefOf.Manipulative))
        {
            var enumerable2 = from x in enumerable
                where p1.relations.OpinionOf(x) < -10 && p2.relations.OpinionOf(x) > 10 ||
                      p1.relations.OpinionOf(x) > 10 && p2.relations.OpinionOf(x) < -10
                select x;
            result = !enumerable2.Any() ? null : enumerable2.RandomElement();
        }
        else
        {
            result = enumerable.RandomElement();
        }

        return result;
    }
}