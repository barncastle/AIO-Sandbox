using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Common.Constants;
using Common.Interfaces;
using Common.Logging;

namespace WorldServer.Plugins
{
    public class PluginHandler
    {
        public static List<SandboxHost> Sandboxes;

        public static void GetPlugins()
        {
            string pluginDirectory = Path.Combine(Environment.CurrentDirectory, "Plugins");

            if (Directory.Exists(pluginDirectory))
            {
                var plugins = Directory.EnumerateFiles(pluginDirectory, "*.dll");
                var assemblies = plugins.Select(x => Assembly.LoadFile(x));
                var sandboxes = assemblies.SelectMany(x => x.GetTypes()).Where(x => x.GetInterfaces().Contains(typeof(ISandbox)));

                Sandboxes = sandboxes.Select(x => new SandboxHost((ISandbox)Activator.CreateInstance(x)))
                                     .OrderBy(x => x.Expansion)
                                     .ThenBy(x => x.Build)
                                     .ToList();
            }

            if (Sandboxes == null || Sandboxes.Count == 0)
            {
                Log.Message(LogType.ERROR, "No plugins found. Press any key to exit");
                Console.ReadKey();
                Environment.Exit(0);
            }
        }

        public static SandboxHost SandboxSelector()
        {
            // expansion selector
            var expansions = Sandboxes.Select(x => x.Expansion).Distinct().ToList();
            Expansions expansion = UserInput(expansions, "Select an Expansion", x => x);

            // sandbox selector
            Sandboxes.RemoveAll(x => x.Expansion != expansion);
            return UserInput(Sandboxes, "Select a Sandbox", x => x.RealmName);
        }

        private static T UserInput<T>(List<T> options, string message, Func<T, object> formatter)
        {
            if (options.Count == 1)
                return options[0]; // return only option

            // print options
            Log.Message(LogType.INIT, message);
            for (int i = 0; i < options.Count; i++)
                Log.Message(LogType.INIT, $"{i + 1}. {formatter.Invoke(options[i])}");

            while (true)
            {
                Log.Message();
                Log.SetType(LogType.MISC);

                // validate user input
                if (int.TryParse(Console.ReadLine().Trim(), out int index))
                {
                    index--;
                    if (index >= 0 && index < options.Count) // out of range
                        return options[index];
                }

                Log.Message(LogType.ERROR, "Invalid selection");
            }
        }

    }
}
