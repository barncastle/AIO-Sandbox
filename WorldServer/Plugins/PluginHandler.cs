using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Common.Interfaces;
using Common.Logging;

namespace WorldServer.Plugins
{
    public class PluginHandler
    {
        public static List<SandboxHost> Sandboxes;

        public static void GetPlugins()
        {
            DirectoryInfo dInfo = new DirectoryInfo(Path.Combine(Environment.CurrentDirectory, "Plugins"));

            var plugins = dInfo.EnumerateFiles("*.dll");
            var assemblies = plugins.Select(x => Assembly.LoadFile(x.FullName));
            var availableTypes = assemblies.SelectMany(x => x.GetTypes());

            Sandboxes = availableTypes.Where(x => x.GetInterfaces().Contains(typeof(ISandbox)))
                                      .Select(x => new SandboxHost((ISandbox)Activator.CreateInstance(x)))
                                      .OrderBy(x => x.Build)
                                      .ToList();
        }

        public static SandboxHost SelectPlugin()
        {
            // Print initial plugin options
            Log.Message(LogType.INIT, "Select a plugin from the below list:");

            for (int i = 0; i < Sandboxes.Count; i++)
                Log.Message(LogType.INIT, $"{i + 1}. {Sandboxes[i].RealmName}");

            Log.Message();

            while (true)
            {
                Log.SetType(LogType.MISC);

                // Load plugin from user input
                if (int.TryParse(Console.ReadLine().Trim(), out int index))
                {
                    index--;
                    if (index >= 0 && index < Sandboxes.Count) // Out of range
                        return Sandboxes[index];
                }

                Log.Message(LogType.ERROR, "Invalid selection."); // Not an integer
            }
        }
    }
}
