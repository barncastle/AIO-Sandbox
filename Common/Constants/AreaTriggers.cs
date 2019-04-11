using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Common.Extensions;
using Common.Structs;

namespace Common.Constants
{
    public static class AreaTriggers
    {
        public static readonly IDictionary<uint, Location> Triggers;

        static AreaTriggers()
        {
            Triggers = new Dictionary<uint, Location>();

            var properties = typeof(Location).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var resource = Properties.Resources.ResourceManager.GetString("AreaTriggers");
            var entries = resource.Split(new[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);

            int j;
            for (int i = 0; i < entries.Length; i++)
            {
                string[] data = entries[i].Split(',');

                var location = new Location();
                for (j = 0; j < properties.Length; j++)
                    properties[j].SetValueEx(location, data[j + 1]);

                Triggers[uint.Parse(data[0])] = location;
            }
        }

        public static IEnumerable<(string Desc, Location Loc)> FindTrigger(string needle, Expansions expansion)
        {
            needle = needle.Replace(" ", "").Replace("'", "").Trim();

            var exact = Triggers.Where(x => x.Value.IsValid(needle, true, expansion) && x.Value.Map > 1);
            if (exact.Any())
            {
                var trigger = exact.First();
                yield return ($"{trigger.Value.Description} : {trigger.Key}", trigger.Value);
                yield break;
            }

            foreach (var trigger in Triggers)
                if (trigger.Value.IsValid(needle, false, expansion) && trigger.Value.Map > 1)
                    yield return ($"{trigger.Value.Description} : {trigger.Key}", trigger.Value);
        }
    }
}
