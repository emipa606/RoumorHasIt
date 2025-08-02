using System.Linq;
using RimWorld;
using Verse;

namespace Rumor_Code;

public class IncidentWorker_MadWithLoneliness : IncidentWorker
{
    private static void HaveEpisode(Pawn p)
    {
        var value = Rand.Value;
        if (value < 0.15)
        {
            p.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Berserk);
            return;
        }

        if (value < 0.45)
        {
            p.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Wander_Psychotic);
            return;
        }

        p.mindState.mentalStateHandler.TryStartMentalState(
            p.story.traits.HasTrait(TraitDefOf.DrugDesire)
                ? DefDatabase<MentalStateDef>.GetNamedSilentFail("Binging_DrugExtreme")
                : MentalStateDefOf.Wander_Sad);
    }

    protected override bool TryExecuteWorker(IncidentParms parms)
    {
        var possiblePawns = ThirdPartyManager.GetAllFreeColonistsAlive;
        var pawn = possiblePawns?.InRandomOrder().FirstOrDefault(ThirdPartyManager.DoesEveryoneLocallyHate);
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