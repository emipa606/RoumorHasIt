using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Rumor_Code
{
    public class InteractionWorker_SpreadRumors : InteractionWorker
    {
        public override float RandomSelectionWeight(Pawn initiator, Pawn recipient)
        {
            float result;
            if (recipient.Faction.HostileTo(Faction.OfPlayer) && !recipient.IsPrisonerOfColony)
            {
                result = 0f;
            }
            else
            {
                var num = 0.024f;
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

        public override void Interacted(Pawn initiator, Pawn recipient, List<RulePackDef> extraSentencePacks,
            out string letterText, out string letterLabel, out LetterDef letterDef, out LookTargets lookTargets)
        {
            letterLabel = null;
            letterText = null;
            letterDef = null;
            lookTargets = null;
            var pawn = ChooseGossipTarget(initiator, recipient);
            if (pawn == null)
            {
                return;
            }

            if (3 + initiator.skills.GetSkill(SkillDefOf.Social).Level > Rand.Range(0, 25))
            {
                recipient.needs.mood.thoughts.memories.TryGainMemory(RumorsThoughtDefOf.HeardAwfulThings, pawn);
                if (initiator.story.traits.HasTrait(RumorsTraitDefOf.Trustworthy))
                {
                    initiator.needs.mood.thoughts.memories.TryGainMemory(RumorsThoughtDefOf.ILied);
                }
            }
            else
            {
                recipient.needs.mood.thoughts.memories.TryGainMemory(RumorsThoughtDefOf.LiedTo, initiator);
                pawn.needs.mood.thoughts.memories.TryGainMemory(RumorsThoughtDefOf.HadLiesToldAboutMe, initiator);
            }
        }

        private Pawn ChooseGossipTarget(Pawn p1, Pawn p2)
        {
            var enumerable = ThirdPartyManager.GetKnownPeople(p1, p2);
            enumerable = enumerable.Concat(ThirdPartyManager.GetKnownPeople(p1, p2)
                .Intersect(ThirdPartyManager.GetAllColonistsLocalTo(p1)));
            Pawn result;
            if (!enumerable.Any())
            {
                result = null;
            }
            else
            {
                Pawn pawn = null;
                if (p1.story.traits.HasTrait(RumorsTraitDefOf.Manipulative))
                {
                    foreach (var current in
                        from x in enumerable
                        orderby p2.relations.OpinionOf(x) descending
                        select x)
                    {
                        if (p1.relations.OpinionOf(current) >= 0)
                        {
                            continue;
                        }

                        pawn = current;
                        break;
                    }
                }
                else if (p1.story.traits.HasTrait(RumorsTraitDefOf.CompulsiveLiar))
                {
                    pawn = enumerable.RandomElement();
                }
                else
                {
                    enumerable.TryRandomElementByWeight(
                        x => (0.0025f * (100f - p1.relations.OpinionOf(x))) +
                             (0.0025f * (100f - p2.relations.OpinionOf(x))), out pawn);
                }

                result = pawn;
            }

            return result;
        }
    }
}