using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace Rumor_Code;

public static class ThirdPartyManager
{
    public static int iLevel = 0;
    public static bool first = true;

    public static readonly Dictionary<Map, List<ICollection<Pawn>>> cliqueDict =
        new Dictionary<Map, List<ICollection<Pawn>>>();

    public static IEnumerable<Map> GetAllMapsContainingFreeSpawnedColonists =>
        from map in Find.Maps
        where map.mapPawns.FreeColonistsSpawnedCount > 0
        select map;

    public static IEnumerable<Caravan> GetAllPlayerCaravans =>
        from car in Find.WorldObjects.Caravans
        where car.Faction == Faction.OfPlayer
        select car;

    public static IEnumerable<Pawn> GetAllColonistsInCaravans
    {
        get
        {
            var getAllPlayerCaravans =
                from car in GetAllPlayerCaravans
                from col in car.PawnsListForReading
                where col.RaceProps.Humanlike && !col.Dead && col.Faction == Faction.OfPlayer
                select col;
            return getAllPlayerCaravans;
        }
    }

    public static IEnumerable<Pawn> GetAllFreeSpawnedColonistsOnMaps
    {
        get
        {
            var getAllMapsContainingFreeSpawnedColonists =
                from map in GetAllMapsContainingFreeSpawnedColonists
                from col in map.mapPawns.FreeColonistsSpawned
                where col.RaceProps.Humanlike && !col.Dead && col.Faction == Faction.OfPlayer
                select col;
            return getAllMapsContainingFreeSpawnedColonists;
        }
    }

    public static IEnumerable<Pawn> GetAllFreeColonistsAlive =>
        GetAllFreeSpawnedColonistsOnMaps.Concat(GetAllColonistsInCaravans);

    public static IEnumerable<Pawn> GetAllColonistsLocalTo(Pawn p)
    {
        var possiblePawns = GetAllFreeColonistsAlive.ToList();
        foreach (var x in possiblePawns)
        {
            if (x.RaceProps.Humanlike && x.Faction == Faction.OfPlayer && x != p && (x.Map != null && x.Map == p.Map ||
                    x.GetCaravan() != null && x.GetCaravan() == p.GetCaravan()))
            {
                yield return x;
            }
        }
    }

    public static IEnumerable<Pawn> GetKnownPeople(Pawn p1, Pawn p2)
    {
        var source = GetAllFreeColonistsAlive;
        source = from p in source
            where p.RaceProps.Humanlike
            select p;
        return from p3 in source
            where p3 != p1 && p3 != p2 && RelationsUtility.PawnsKnowEachOther(p1, p3) &&
                  RelationsUtility.PawnsKnowEachOther(p2, p3)
            select p3;
    }

    public static IEnumerable<Thought_Memory> GetMemoriesWithDef(Pawn p, ThoughtDef tdef)
    {
        var memories = p.needs.mood.thoughts.memories.Memories;
        return from x in memories
            where x.def == tdef
            select x;
    }

    public static bool DoesEveryoneLocallyHate(Pawn p)
    {
        var allColonistsLocalTo = GetAllColonistsLocalTo(p).ToList();
        if (!allColonistsLocalTo.Any())
        {
            return false;
        }

        foreach (var current in allColonistsLocalTo)
        {
            if (!(current.relations?.OpinionOf(p) <= -8) && current != p)
            {
                return false;
            }
        }

        return true;
    }

    public static void FindCliques()
    {
        cliqueDict.Clear();
        foreach (var m in Find.Maps)
        {
            var list = m.mapPawns.FreeColonistsSpawned.ToList();
            var list2 = new List<ICollection<Pawn>>();
            _ = 0.667f * list.Count;
            BKA(list2, new List<Pawn>(), list, new List<Pawn>());
            if (list2.Count == 0)
            {
                break;
            }

            list2 = (from x in list2
                where x.Count >= 3
                select x).ToList();
            if (list2.Count == 0)
            {
                break;
            }

            IDictionary<Pawn, ICollection<Pawn>> dictionary = new Dictionary<Pawn, ICollection<Pawn>>();
            IEnumerable<Pawn> freeColonistsSpawned = m.mapPawns.FreeColonistsSpawned;
            foreach (var p in freeColonistsSpawned)
            {
                var list3 = (from x in list2
                    where x.Contains(p)
                    select x).ToList();
                if (list3.Count < 2)
                {
                    continue;
                }

                list3.Sort((x1, x2) =>
                    GetPawnAverageRelationshipWithGroup(p, x1)
                        .CompareTo(GetPawnAverageRelationshipWithGroup(p, x2)));
                list3.Reverse();
                dictionary.Add(p, list3.FirstOrDefault());
            }

            foreach (var current in list)
            {
                foreach (var current2 in list2)
                {
                    if (!dictionary.TryGetValue(current, out var pawns))
                    {
                        continue;
                    }

                    if (!Equals(current2, pawns) && current2.Contains(current))
                    {
                        current2.Remove(current);
                    }
                }
            }

            list2 = (from x in list2
                where x.Count >= 3 && x.Count < 0.667f * m.mapPawns.FreeColonistsSpawnedCount
                select x).ToList();
            if (!list2.Any())
            {
                break;
            }

            var value = list2.Distinct().ToList();
            first = false;
            cliqueDict.Add(m, value);
        }
    }

    public static ICollection<ICollection<Pawn>> GetIsolatedCliques(this Map self, int threshold)
    {
        if (!cliqueDict.TryGetValue(self, out var source))
        {
            return null;
        }

        IEnumerable<Pawn> allColonists = self.mapPawns.FreeColonistsSpawned;
        return (from clique in source
            where GetGroupAverageRelationshipWithGroup(clique, allColonists.Except(clique)) <= threshold
            select clique).ToList();
    }

    public static float GetPawnAverageRelationshipWithGroup(Pawn p, IEnumerable<Pawn> g)
    {
        var num = 0f;
        foreach (var current in g)
        {
            num += p.relations.OpinionOf(current);
        }

        return num / g.Count();
    }

    public static float GetGroupAverageRelationshipWithGroup(IEnumerable<Pawn> g1, IEnumerable<Pawn> g2)
    {
        var num = 0f;
        foreach (var current in g1)
        {
            num += GetPawnAverageRelationshipWithGroup(current, g2);
        }

        return num / g1.Count();
    }

    public static void BKA(ICollection<ICollection<Pawn>> cliques, ICollection<Pawn> R, ICollection<Pawn> P,
        ICollection<Pawn> X)
    {
        if (P.Count == 0 && X.Count == 0)
        {
            if (R.Count != 0)
            {
                cliques.Add(R);
            }

            return;
        }

        var list = P.Union(X).ToList();
        list.Sort((p1, p2) => Neighbours(p1, P).Count().CompareTo(Neighbours(p2, P).Count()));
        list.Reverse();
        var p = list.FirstOrDefault();
        var second = Neighbours(p, P).ToList();
        var list2 = new List<Pawn>();
        var list3 = new List<Pawn>();
        var list4 = new List<Pawn>();
        var list5 = new List<Pawn>(P).Except(second).ToList();
        foreach (var current in list5)
        {
            list2.Clear();
            list2.AddRange(R);
            if (!list2.Contains(current))
            {
                list2.Add(current);
            }

            list3.Clear();
            list3.AddRange(P);
            list3 = list3.Intersect(Neighbours(current, list)).ToList();
            list4.Clear();
            list4.AddRange(X);
            list4 = list4.Intersect(Neighbours(current, list)).ToList();
            BKA(cliques, list2, list3, list4);
            P.Remove(current);
            X.Add(current);
        }
    }

    private static IEnumerable<Pawn> Neighbours(Pawn p1, IEnumerable<Pawn> pawns)
    {
        return from x in pawns
            where p1 != x && p1.relations.OpinionOf(x) >= 10 && x.relations.OpinionOf(p1) >= 10
            select x;
    }

    public static string GetChildhoodCulturalAdjective(Pawn p)
    {
        var result = "Colonial";
        if (p.story?.Childhood == null)
        {
            return result;
        }

        if (p.story.Childhood.spawnCategories?.Contains("Tribal") == true)
        {
            return "Tribal";
        }

        var title = p.story.Childhood.title;
        var description = p.story.Childhood.description;

        if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(description))
        {
            return result;
        }

        if (title.Contains("medieval") ||
            description.IndexOf("Medieval", StringComparison.OrdinalIgnoreCase) >= 0 ||
            description.IndexOf("Village", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return "Medieval";
        }

        if (title.Contains("glitterworld") ||
            description.IndexOf("Glitterworld", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            if (title != "discarded youth" && title != "corporate slave")
            {
                return "Glitterworld";
            }
        }
        else if (title.Contains("urbworld") || title.Contains("vatgrown") ||
                 description.IndexOf("Urbworld", StringComparison.OrdinalIgnoreCase) >= 0 ||
                 description.IndexOf("Industrial", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return "Urbworld";
        }
        else if (title.Contains("midworld") ||
                 description.IndexOf("Midworld", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return "Midworld";
        }
        else if (description.IndexOf("Tribe", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return "Tribal";
        }
        else if (title.Contains("imperial") ||
                 description.IndexOf("Imperial", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return "Imperial";
        }

        return result;
    }

    public static string GetAdultCulturalAdjective(Pawn p)
    {
        var result = "Colonial";
        if (p.story?.Adulthood == null)
        {
            return result;
        }

        if (p.story.Adulthood.spawnCategories?.Contains("Tribal") == true)
        {
            return "Tribal";
        }

        var title = p.story.Adulthood.title;
        var description = p.story.Adulthood.description;

        if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(description))
        {
            return result;
        }

        if (title.Contains("medieval") ||
            description.IndexOf("Medieval", StringComparison.OrdinalIgnoreCase) >= 0 ||
            description.IndexOf("Village", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return "Medieval";
        }

        if (title.Contains("glitterworld") ||
            description.IndexOf("Glitterworld", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            if (title != "adventurer")
            {
                return "Glitterworld";
            }
        }
        else if (title.Contains("urbworld") || title.Contains("vatgrown") ||
                 description.IndexOf("Urbworld", StringComparison.OrdinalIgnoreCase) >= 0 ||
                 description.IndexOf("Urbworld", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return "Urbworld";
        }
        else if (title.Contains("midworld") ||
                 description.IndexOf("Midworld", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return "Midworld";
        }
        else if (description.IndexOf("Tribe", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return "Tribal";
        }
        else if (title.Contains("imperial") ||
                 description.IndexOf("Imperial", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return "Imperial";
        }

        return result;
    }
}