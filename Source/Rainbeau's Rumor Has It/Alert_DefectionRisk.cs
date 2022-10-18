using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Rumor_Code;

public class Alert_DefectionRisk : Alert
{
    private IEnumerable<Pawn> DefectionRiskPawns =>
        from p in ThirdPartyManager.GetAllFreeColonistsAlive
        where ThirdPartyManager.DoesEveryoneLocallyHate(p)
        select p;

    public override AlertReport GetReport()
    {
        var getAllFreeColonistsAlive = ThirdPartyManager.GetAllFreeColonistsAlive;
        AlertReport result;
        if (!getAllFreeColonistsAlive.Any())
        {
            result = false;
        }
        else
        {
            var pawn = getAllFreeColonistsAlive.FirstOrDefault();

            try
            {
                foreach (var current in getAllFreeColonistsAlive.ToList())
                {
                    if (!ThirdPartyManager.DoesEveryoneLocallyHate(current))
                    {
                        continue;
                    }

                    result = AlertReport.CulpritIs(current);
                    return result;
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
            text = "RUMOR.DefectionRiskMsg".Translate();
            try
            {
                foreach (var current in DefectionRiskPawns.ToList())
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
            text = "RUMOR.SocialIsolationMsg".Translate();
            try
            {
                foreach (var current2 in DefectionRiskPawns.ToList())
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
        string result = Controller.Settings.allowDefections.Equals(true)
            ? "RUMOR.DefectionRisk".Translate()
            : "RUMOR.SocialIsolation".Translate();

        return result;
    }
}