using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Rumor_Code
{
    public class InteractionWorker_CultureClash : InteractionWorker
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
                var adultCulturalAdjective = ThirdPartyManager.GetAdultCulturalAdjective(initiator);
                var adultCulturalAdjective2 = ThirdPartyManager.GetAdultCulturalAdjective(recipient);
                var childhoodCulturalAdjective = ThirdPartyManager.GetChildhoodCulturalAdjective(initiator);
                var childhoodCulturalAdjective2 = ThirdPartyManager.GetChildhoodCulturalAdjective(recipient);
                if ((adultCulturalAdjective == adultCulturalAdjective2 ||
                     childhoodCulturalAdjective == adultCulturalAdjective2) &&
                    (adultCulturalAdjective == childhoodCulturalAdjective2 ||
                     childhoodCulturalAdjective == childhoodCulturalAdjective2))
                {
                    result = 0f;
                }
                else
                {
                    var num = 0.02f;
                    result = num * NegativeInteractionUtility.NegativeInteractionChanceFactor(initiator, recipient);
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
            var adultCulturalAdjective = ThirdPartyManager.GetAdultCulturalAdjective(initiator);
            var adultCulturalAdjective2 = ThirdPartyManager.GetAdultCulturalAdjective(recipient);
            var childhoodCulturalAdjective = ThirdPartyManager.GetChildhoodCulturalAdjective(initiator);
            var childhoodCulturalAdjective2 = ThirdPartyManager.GetChildhoodCulturalAdjective(recipient);
            string a;
            if (adultCulturalAdjective != adultCulturalAdjective2 &&
                childhoodCulturalAdjective != adultCulturalAdjective2)
            {
                a = ThirdPartyManager.GetAdultCulturalAdjective(recipient);
            }
            else
            {
                if (adultCulturalAdjective == childhoodCulturalAdjective2 ||
                    childhoodCulturalAdjective == childhoodCulturalAdjective2)
                {
                    return;
                }

                a = ThirdPartyManager.GetChildhoodCulturalAdjective(recipient);
            }

            var item = RumorsRulePackDefOf.Sentence_CC_Colonist;
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
}