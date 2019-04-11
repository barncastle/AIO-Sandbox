using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Common.Extensions;
using Common.Structs;

namespace Common.Constants
{
    public static class Worldports
    {
        public static readonly IDictionary<string, Location> Locations;

        static Worldports()
        {
            Locations = new Dictionary<string, Location>(StringComparer.OrdinalIgnoreCase);

            var properties = typeof(Location).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var resource = Properties.Resources.ResourceManager.GetString("Worldports");
            var entries = resource.Split(new[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);

            int j;
            for (int i = 0; i < entries.Length; i++)
            {
                string[] data = entries[i].Split(',');

                var location = new Location();
                for (j = 0; j < properties.Length; j++)
                    properties[j].SetValueEx(location, data[j]);

                Locations[location.Description] = location;
            }
        }

        public static IEnumerable<(string Desc, Location Loc)> FindLocation(string needle, Expansions expansion)
        {
            needle = needle.Replace(" ", "").Replace("'", "").Trim();

            var exact = Locations.Where(x => x.Value.IsValid(needle, true, expansion));
            if (exact.Any())
            {
                var location = exact.First();
                yield return (location.Key, location.Value);
                yield break;
            }

            foreach (var location in Locations)
                if (location.Value.IsValid(needle, false, expansion))
                    yield return (location.Key, location.Value);
        }
    }
}
