using Verse;

namespace Rumor_Code;

public class Watcher(Map map) : MapComponent(map)
{
    public override void MapComponentTick()
    {
        base.MapComponentTick();
        if (map.IsHashIntervalTick(100))
        {
            CaravanSocialManager.MakeCaravansInteract();
        }

        if (map.IsHashIntervalTick(15000))
        {
            ThirdPartyManager.FindCliques();
        }
    }
}