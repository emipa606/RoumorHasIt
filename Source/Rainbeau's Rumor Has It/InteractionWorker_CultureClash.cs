using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Rumor_Code;

public class InteractionWorker_CultureClash : InteractionWorker
{
    public override float RandomSelectionWeight(Pawn initiator, Pawn recipient)
    {
        if (recipient.Faction.HostileTo(Faction.OfPlayer) && !recipient.IsPrisonerOfColony)
        {
            return 0f;
        }

        var adultCulturalAdjective = ThirdPartyManager.GetAdultCulturalAdjective(initiator);
        var adultCulturalAdjective2 = ThirdPartyManager.GetAdultCulturalAdjective(recipient);
        var childhoodCulturalAdjective = ThirdPartyManager.GetChildhoodCulturalAdjective(initiator);
        var childhoodCulturalAdjective2 = ThirdPartyManager.GetChildhoodCulturalAdjective(recipient);
        if ((adultCulturalAdjective == adultCulturalAdjective2 ||
             childhoodCulturalAdjective == adultCulturalAdjective2) &&
            (adultCulturalAdjective == childhoodCulturalAdjective2 ||
             childhoodCulturalAdjective == childhoodCulturalAdjective2))
        {
            return 0f;
        }

        var num = 0.02f;
        return num * NegativeInteractionUtility.NegativeInteractionChanceFactor(initiator, recipient);
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
        switch (a)
        {
            case "Glitterworld":
                item = RumorsRulePackDefOf.Sentence_CC_Glitterworld;
                break;
            case "Urbworld":
                item = RumorsRulePackDefOf.Sentence_CC_Urbworld;
                break;
            case "Tribal":
                item = RumorsRulePackDefOf.Sentence_CC_Tribal;
                break;
            case "Midworld":
                item = RumorsRulePackDefOf.Sentence_CC_Midworld;
                break;
            case "Imperial":
                item = RumorsRulePackDefOf.Sentence_CC_Imperial;
                break;
        }

        extraSentencePacks.Add(item);
    }
}