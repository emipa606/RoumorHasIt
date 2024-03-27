using RimWorld;
using Verse;

namespace Rumor_Code;

public class Alert_Cliques : Alert
{
    public override AlertReport GetReport()
    {
        AlertReport result;
        using (var enumerator = Find.Maps.GetEnumerator())
        {
            if (enumerator.MoveNext())
            {
                var current = enumerator.Current;
                var isolatedCliques = current.GetIsolatedCliques(-5);

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
        var text = "RUMOR.CliquesFormed".Translate();
        var num = 0;
        foreach (var current in Find.Maps)
        {
            var isolatedCliques = current.GetIsolatedCliques(-3);
            if (isolatedCliques == null || isolatedCliques.Count == 0)
            {
                continue;
            }

            foreach (var current2 in isolatedCliques)
            {
                num++;
                text += $"\nClique {num}: ";
                foreach (var current3 in current2)
                {
                    text += current3.NameShortColored + ", ";
                }

                text += "\n";
            }

            if (Controller.Settings.allowSplinters.Equals(true))
            {
                text += "RUMOR.CliquesExtraWarning".Translate();
            }
        }

        return text;
    }

    public override string GetLabel()
    {
        return "RUMOR.Cliques".Translate();
    }
}