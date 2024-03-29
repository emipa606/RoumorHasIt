﻿using Verse;

namespace Rumor_Code;

public class Watcher(Map map) : MapComponent(map)
{
    private int currentTick;

    public override void MapComponentTick()
    {
        base.MapComponentTick();
        currentTick = Find.TickManager.TicksGame;
        if (currentTick % 100 == 0)
        {
            CaravanSocialManager.MakeCaravansInteract();
        }

        if (currentTick % 15000 == 10)
        {
            ThirdPartyManager.FindCliques();
        }
    }
}