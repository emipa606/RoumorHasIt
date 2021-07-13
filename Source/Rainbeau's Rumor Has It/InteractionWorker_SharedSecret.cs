using RimWorld;
using Verse;

namespace Rumor_Code
{
    public class InteractionWorker_SharedSecret : InteractionWorker
    {
        private readonly SimpleCurve NormalCompatibilityCurve;
        private readonly SimpleCurve SharersCompatibilityCurve;

        public InteractionWorker_SharedSecret()
        {
            var simpleCurve = new SimpleCurve
            {
                new CurvePoint(-1.5f, 0f),
                new CurvePoint(-0.5f, 0.1f),
                new CurvePoint(0.5f, 1f),
                new CurvePoint(1f, 1.8f),
                new CurvePoint(2f, 3f)
            };
            NormalCompatibilityCurve = simpleCurve;
            simpleCurve = new SimpleCurve
            {
                new CurvePoint(-1.5f, 1.1f),
                new CurvePoint(-0.5f, 1.5f),
                new CurvePoint(0.5f, 1.8f),
                new CurvePoint(1f, 2f),
                new CurvePoint(2f, 3f)
            };
            SharersCompatibilityCurve = simpleCurve;
        }

        public override float RandomSelectionWeight(Pawn initiator, Pawn recipient)
        {
            float result;
            if (recipient.Faction.HostileTo(Faction.OfPlayer) && !recipient.IsPrisonerOfColony)
            {
                result = 0f;
            }
            else
            {
                var num = 0.02f;
                if (initiator.story.traits.HasTrait(RumorsTraitDefOf.Gushing))
                {
                    result = num * SharersCompatibilityCurve.Evaluate(initiator.relations.CompatibilityWith(recipient));
                }
                else
                {
                    result = num * NormalCompatibilityCurve.Evaluate(initiator.relations.CompatibilityWith(recipient));
                }
            }

            return result;
        }
    }
}