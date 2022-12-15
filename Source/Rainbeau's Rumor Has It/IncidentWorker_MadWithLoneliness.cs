using System.Linq;
using RimWorld;
using Verse;

namespace Rumor_Code;

public class IncidentWorker_MadWithLoneliness : IncidentWorker
{
    private void HaveEpisode(Pawn p)
    {
        var value = Rand.Value;
        if (value < 0.15)
        {
            p.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Berserk);
        }
        else if (value < 0.45)
        {
            p.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Wander_Psychotic);
        }
        else if (p.story.traits.HasTrait(TraitDefOf.DrugDesire))
        {
            p.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Binging_DrugExtreme);
        }
        else
        {
            p.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Wander_Sad);
        }
    }

    protected override bool TryExecuteWorker(IncidentParms parms)
    {
        var pawns = ThirdPartyManager.GetAllFreeColonistsAlive.Where(ThirdPartyManager.DoesEveryoneLocallyHate);
        if (!pawns.Any())
        {
            return false;
        }

        var pawn = pawns.RandomElement();

        if (pawn == null)
        {
            return false;
        }

        if (!pawn.RaceProps.Humanlike)
        {
            return false;
        }

        HaveEpisode(pawn);
        return true;
    }
}