using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Common.Extensions;
using Common.Interfaces;
using Common.Structs;

namespace Common.Constants
{
    public static class Worldports
    {
        public static readonly IList<Location> Locations;

        static Worldports()
        {
            Locations = new List<Location>();
        }

        public static void Initialize(ISandbox sandbox)
        {
            var properties = typeof(Location).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var resource = Properties.Resources.Worldports;
            var entries = resource.Split(new[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);

            int j;
            for (int i = 0; i < entries.Length; i++)
            {
                string[] data = entries[i].Split(',');

                var location = new Location();
                for (j = 0; j < properties.Length; j++)
                    properties[j].SetValueEx(location, data[j]);

                if (location.Expansion <= sandbox.Expansion)
                    Locations.Add(location);
            }
        }

        public static IEnumerable<Location> FindLocation(string needle, Expansions expansion)
        {
            needle = needle.Sanitize();

            var locations = Locations.Where(x => x.IsMatch(needle, expansion));
            var exact = locations.FirstOrDefault(x => x.IsMatch(needle, expansion, true));

            return exact?.Yield() ?? locations;
        }
    }
}
