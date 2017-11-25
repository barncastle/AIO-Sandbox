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

		private static bool loaded = false;

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

		public static SandboxHost SelectPlugin()
		{
			bool retry = true;

			//Print initial plugin options
			if (!loaded)
			{
				Log.Message(LogType.INIT, "Select a plugin from the below list:");
				for (int i = 0; i < Sandboxes.Count; i++)
					Log.Message(LogType.INIT, $"{i + 1}. {Sandboxes[i].RealmName}");
				Log.Message();

				loaded = true;
			}

			//Load plugin from user input
			Log.SetType(LogType.MISC); //Set font colour

			if (int.TryParse(Console.ReadLine().Trim(), out int index))
			{
				index--;
				if (index >= 0 && index < Sandboxes.Count) //Out of range
					retry = false;
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
