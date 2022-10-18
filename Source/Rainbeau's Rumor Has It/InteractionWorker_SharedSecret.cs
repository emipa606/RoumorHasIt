using RimWorld;
using Verse;

namespace Rumor_Code;

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
        if (recipient.Faction.HostileTo(Faction.OfPlayer) && !recipient.IsPrisonerOfColony)
        {
            return 0f;
        }

        var num = 0.02f;
        if (initiator.story.traits.HasTrait(RumorsTraitDefOf.Gushing))
        {
            return num * SharersCompatibilityCurve.Evaluate(initiator.relations.CompatibilityWith(recipient));
        }

        return num * NormalCompatibilityCurve.Evaluate(initiator.relations.CompatibilityWith(recipient));
    }
}