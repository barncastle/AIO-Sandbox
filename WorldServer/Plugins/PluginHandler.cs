using Common.Interfaces;
using Common.Logging;
using Common.Structs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace WorldServer.Plugins
{
    public class PluginHandler
    {
        public static List<SandboxHost> Sandboxes;

        public static void GetPlugins()
        {
            DirectoryInfo dInfo = new DirectoryInfo(Path.Combine(Environment.CurrentDirectory, "Plugins"));
            FileInfo[] files = dInfo.GetFiles("*.dll");

            var assemblies = files.Select(x => Assembly.LoadFile(x.FullName));
            var availableTypes = assemblies.SelectMany(x => x.GetTypes());

            Sandboxes = availableTypes.Where(x => x.GetInterfaces().Contains(typeof(ISandbox)))
                                      .Select(x => new SandboxHost((ISandbox)Activator.CreateInstance(x)))
                                      .OrderBy(x => x.Build)
                                      .ToList();
        }

        public static SandboxHost SelectPlugin(bool firstLoad = false)
        {
            bool retry = false;

            //Print initial plugin options
            if (firstLoad)
            {
                Log.Message(LogType.INIT, "Select a plugin from the below list:");
                for (int i = 0; i < Sandboxes.Count; i++)
                {
                    string realm = Sandboxes[i].RealmName[0] == '|' ? Sandboxes[i].RealmName.Substring(10) : Sandboxes[i].RealmName;
                    Log.Message(LogType.INIT, $"{i + 1}. {realm}");
                }
                Log.Message();
            }

            //Load plugin from user input
            Log.SetType(LogType.MISC); //Set font colour

            if (int.TryParse(Console.ReadLine().Trim(), out int index))
            {
                index--;
                if (index < 0 || index >= Sandboxes.Count) //Out of range
                    retry = true;
            }
            else
            {
                retry = true;
            }

            if (retry)
            {
                Log.Message(LogType.ERROR, "Invalid selection."); //Not an integer
                return SelectPlugin();
            }

            return Sandboxes[index];
        }
    }
}
