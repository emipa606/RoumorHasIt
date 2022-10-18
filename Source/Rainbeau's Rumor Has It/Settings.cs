using UnityEngine;
using Verse;

namespace Rumor_Code;

public class Settings : ModSettings
{
    public bool allowBrawls = true;
    public bool allowDefections = true;
    public bool allowSplinters = true;

    public void DoWindowContents(Rect canvas)
    {
        var list = new Listing_Standard
        {
            ColumnWidth = canvas.width
        };
        list.Begin(canvas);
        list.Gap();
        list.CheckboxLabeled("RUMOR.AllowBrawls".Translate(), ref allowBrawls);
        list.Gap();
        list.CheckboxLabeled("RUMOR.AllowDefections".Translate(), ref allowDefections);
        list.Gap();
        list.CheckboxLabeled("RUMOR.AllowSplinters".Translate(), ref allowSplinters);
        if (Controller.currentVersion != null)
        {
            list.Gap();
            GUI.contentColor = Color.gray;
            list.Label("RUMOR.CurrentModVersion".Translate(Controller.currentVersion));
            GUI.contentColor = Color.white;
        }

        list.End();
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref allowBrawls, "allowBrawls", true);
        Scribe_Values.Look(ref allowDefections, "allowDefections", true);
        Scribe_Values.Look(ref allowSplinters, "allowSplinters", true);
    }
}