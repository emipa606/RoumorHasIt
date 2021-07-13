using System.Linq;
using RimWorld;
using Verse;

namespace Rumor_Code
{
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
            var enumerable = from x in ThirdPartyManager.GetAllFreeColonistsAlive
                where ThirdPartyManager.DoesEveryoneLocallyHate(x)
                select x;
            bool result;
            if (!enumerable.Any())
            {
                result = false;
            }
            else
            {
                var pawn = enumerable.RandomElement();
                if (pawn == null)
                {
                    result = false;
                }
                else if (!pawn.RaceProps.Humanlike)
                {
                    result = false;
                }
                else
                {
                    HaveEpisode(pawn);
                    result = true;
                }
            }

            return result;
        }
    }
}