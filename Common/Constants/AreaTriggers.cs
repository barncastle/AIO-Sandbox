using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Common.Extensions;
using Common.Interfaces;
using Common.Structs;

namespace Common.Constants
{
    public static class AreaTriggers
    {
        public static readonly IDictionary<uint, Location> Triggers;

        private static readonly uint[] Continents = new uint[] { 0, 1, 530, 571 };

        static AreaTriggers()
        {
            Triggers = new Dictionary<uint, Location>();
        }

        public static void Initialize(ISandbox sandbox)
        {
            var properties = typeof(Location).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var resource = Properties.Resources.AreaTriggers;
            var entries = resource.Split(new[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);

            int j;
            for (int i = 0; i < entries.Length; i++)
            {
                string[] data = entries[i].Split(',');

                var location = new Location();
                for (j = 0; j < properties.Length; j++)
                    properties[j].SetValueEx(location, data[j + 1]);

                // skip invalid expansions
                if (location.Expansion > sandbox.Expansion)
                    continue;

                // take the most recent variant
                var id = uint.Parse(data[0]);
                if (!Triggers.ContainsKey(id) || Triggers[id].Expansion < location.Expansion)
                    Triggers[id] = location;
            }

            ApplyOverrides(sandbox.Build);
        }

        public static IEnumerable<Location> FindTrigger(string needle, Expansions expansion)
        {
            needle = needle.Sanitize();

            var triggers = Triggers.Values.Where(x => x.IsMatch(needle, expansion) && !Continents.Contains(x.Map));
            var exact = triggers.FirstOrDefault(x => x.IsMatch(needle, expansion, true));

            return exact?.Yield() ?? triggers;
        }

        private static void ApplyOverrides(int build)
        {
            if (build <= 3494)
            {
                Triggers[45] = new Location(77f, -1f, 20f, 0, 44, "Scarlet Monastery");
            }                
        }
    }
}
