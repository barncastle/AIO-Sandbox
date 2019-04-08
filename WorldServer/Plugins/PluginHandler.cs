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
        private List<SandboxHost> Sandboxes;
        private List<Expansions> Expansions;

        public PluginHandler() => GetPlugins();

        public SandboxHost SandboxSelector()
        {
            // expansion selector
            Expansions expansion = UserInput(Expansions, "Select an Expansion");

            // sandbox selector
            var sandboxes = Sandboxes.FindAll(x => x.Expansion == expansion);
            return UserInput(sandboxes, "Select a Sandbox", true) ?? SandboxSelector();
        }

        private void GetPlugins()
        {
            string pluginDirectory = Path.Combine(Environment.CurrentDirectory, "Plugins");

            if (Directory.Exists(pluginDirectory))
            {
                // load all of the dlls that contain a ISandbox interface
                var plugins = Directory.EnumerateFiles(pluginDirectory, "*.dll");
                var assemblies = plugins.Select(x => Assembly.LoadFile(x));
                var sandboxes = assemblies.SelectMany(x => x.GetTypes()).Where(x => x.GetInterfaces().Contains(typeof(ISandbox)));

                // create new sandbox instances
                Sandboxes = sandboxes.Select(x => new SandboxHost((ISandbox)Activator.CreateInstance(x)))
                                     .OrderBy(x => x.Expansion)
                                     .ThenBy(x => x.Build)
                                     .ToList();

                // store the list of available expansions
                Expansions = Sandboxes.Select(x => x.Expansion).Distinct().ToList();
            }

            // prevent continuing if there are no sandbox plugins
            if (Sandboxes == null || Sandboxes.Count == 0)
            {
                Log.Message(LogType.ERROR, "No plugins found. Press any key to exit");
                Console.ReadKey();
                Environment.Exit(0);
            }
        }

        private T UserInput<T>(List<T> options, string message, bool allowBack = false)
        {
            if (options.Count == 1 && !allowBack)
                return options[0]; // return only option

            // print options
            Log.Message(LogType.INIT, message);
            for (int i = 0; i < options.Count; i++)
                Log.Message(LogType.INIT, $"{i + 1}. {options[i]}");

            // print back option if applicable
            if (allowBack)
                Log.Message(LogType.INIT, $"{options.Count + 1}. Back...");

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

                    if (allowBack && index == options.Count) // back option
                        return default;
                }

                Log.Message(LogType.ERROR, "Invalid selection");
            }
        }

    }
}
