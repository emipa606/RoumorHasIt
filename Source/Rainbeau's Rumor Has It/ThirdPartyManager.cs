using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace Rumor_Code
{
    public static class ThirdPartyManager
    {
        public static int iLevel = 0;
        public static bool first = true;

        public static Dictionary<Map, List<ICollection<Pawn>>> cliqueDict =
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
                    where col.RaceProps.Humanlike && !col.Dead && col.Faction == (object) Faction.OfPlayer
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
                    where col.RaceProps.Humanlike && !col.Dead && col.Faction == (object) Faction.OfPlayer
                    select col;
                return getAllMapsContainingFreeSpawnedColonists;
            }
        }

        public static IEnumerable<Pawn> GetAllFreeColonistsAlive =>
            GetAllFreeSpawnedColonistsOnMaps.Concat(GetAllColonistsInCaravans);

        public static IEnumerable<Pawn> GetAllColonistsLocalTo(Pawn p)
        {
            return from x in GetAllFreeColonistsAlive
                where x.RaceProps.Humanlike && x.Faction == Faction.OfPlayer && x != p &&
                      (x.Map != null && x.Map == p.Map || x.GetCaravan() != null && x.GetCaravan() == p.GetCaravan())
                select x;
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
            var allColonistsLocalTo = GetAllColonistsLocalTo(p);
            bool result;
            if (!allColonistsLocalTo.Any())
            {
                result = false;
            }
            else
            {
                foreach (var current in allColonistsLocalTo)
                {
                    if (current.relations.OpinionOf(p) <= -8 || current == p)
                    {
                        continue;
                    }

                    return false;
                }

                result = true;
            }

            return result;
        }

        public static void FindCliques()
        {
            cliqueDict.Clear();
            foreach (var m in Find.Maps)
            {
                var list = m.mapPawns.FreeColonistsSpawned.ToList();
                var list2 = new List<ICollection<Pawn>>();
                var unused = 0.667f * list.Count;
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
                        if (!dictionary.ContainsKey(current))
                        {
                            continue;
                        }

                        if (!Equals(current2, dictionary[current]) && current2.Contains(current))
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
            ICollection<ICollection<Pawn>> result;
            if (cliqueDict.TryGetValue(self, out var source))
            {
                IEnumerable<Pawn> allColonists = self.mapPawns.FreeColonistsSpawned;
                result = (from clique in source
                    where GetGroupAverageRelationshipWithGroup(clique, allColonists.Except(clique)) <= threshold
                    select clique).ToList();
            }
            else
            {
                result = null;
            }

            return result;
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
            }
            else
            {
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
            if (p.story.childhood == null)
            {
                return result;
            }

            if (p.story.childhood.spawnCategories.Contains("Tribal"))
            {
                result = "Tribal";
            }
            else if (p.story.childhood.title.Contains("medieval") ||
                     p.story.childhood.baseDesc.IndexOf("Medieval", StringComparison.OrdinalIgnoreCase) >= 0 ||
                     p.story.childhood.baseDesc.IndexOf("Village", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                result = "Medieval";
            }
            else if (p.story.childhood.title.Contains("glitterworld") ||
                     p.story.childhood.baseDesc.IndexOf("Glitterworld", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                if (p.story.childhood.title != "discarded youth" && p.story.childhood.title != "corporate slave")
                {
                    result = "Glitterworld";
                }
            }
            else if (p.story.childhood.title.Contains("urbworld") || p.story.childhood.title.Contains("vatgrown") ||
                     p.story.childhood.baseDesc.IndexOf("Urbworld", StringComparison.OrdinalIgnoreCase) >= 0 ||
                     p.story.childhood.baseDesc.IndexOf("Industrial", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                result = "Urbworld";
            }
            else if (p.story.childhood.title.Contains("midworld") ||
                     p.story.childhood.baseDesc.IndexOf("Midworld", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                result = "Midworld";
            }
            else if (p.story.childhood.baseDesc.IndexOf("Tribe", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                result = "Tribal";
            }
            else if (p.story.childhood.title.Contains("imperial") ||
                     p.story.childhood.baseDesc.IndexOf("Imperial", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                result = "Imperial";
            }

            return result;
        }

        public static string GetAdultCulturalAdjective(Pawn p)
        {
            var result = "Colonial";
            if (p.story.adulthood == null)
            {
                return result;
            }

            if (p.story.adulthood.spawnCategories.Contains("Tribal"))
            {
                result = "Tribal";
            }
            else if (p.story.adulthood.title.Contains("medieval") ||
                     p.story.adulthood.baseDesc.IndexOf("Medieval", StringComparison.OrdinalIgnoreCase) >= 0 ||
                     p.story.adulthood.baseDesc.IndexOf("Village", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                result = "Medieval";
            }
            else if (p.story.adulthood.title.Contains("glitterworld") ||
                     p.story.adulthood.baseDesc.IndexOf("Glitterworld", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                if (p.story.adulthood.title != "adventurer")
                {
                    result = "Glitterworld";
                }
            }
            else if (p.story.adulthood.title.Contains("urbworld") || p.story.adulthood.title.Contains("vatgrown") ||
                     p.story.adulthood.baseDesc.IndexOf("Urbworld", StringComparison.OrdinalIgnoreCase) >= 0 ||
                     p.story.adulthood.baseDesc.IndexOf("Urbworld", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                result = "Urbworld";
            }
            else if (p.story.adulthood.title.Contains("midworld") ||
                     p.story.adulthood.baseDesc.IndexOf("Midworld", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                result = "Midworld";
            }
            else if (p.story.adulthood.baseDesc.IndexOf("Tribe", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                result = "Tribal";
            }
            else if (p.story.adulthood.title.Contains("imperial") ||
                     p.story.adulthood.baseDesc.IndexOf("Imperial", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                result = "Imperial";
            }

            return result;
        }
    }
}