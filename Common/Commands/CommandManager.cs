using Common.Interfaces;
using Common.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Common.Commands
{
    public class CommandManager
    {
        public static Dictionary<string, HandleCommand> CommandHandlers = new Dictionary<string, HandleCommand>();
        public delegate void HandleCommand(IWorldManager manager, string[] args);

        static CommandManager()
        {
            DefineCommand("gps", Commands.Gps);
            DefineCommand("help", Commands.Help);
            DefineCommand("speed", Commands.Speed);
            DefineCommand("go", Commands.Go);
            DefineCommand("nudge", Commands.Nudge);
            DefineCommand("morph", Commands.Morph);
            DefineCommand("demorph", Commands.Demorph);
        }

        public static void DefineCommand(string command, HandleCommand handler) => CommandHandlers[command.ToLower()] = handler;

        public static bool InvokeHandler(string command, IWorldManager manager)
        {
            if (string.IsNullOrEmpty(command))
                return false;

            if (command[0] != '.')
                return false;

            string[] lines = command.Split(' ');
            return InvokeHandler(lines[0], manager, lines.Skip(1).ToArray());
        }

        public static bool InvokeHandler(string command, IWorldManager manager, params string[] args)
        {
            command = command.TrimStart('.').Trim().ToLower(); //Remove command "." prefix and format
            if (CommandHandlers.ContainsKey(command))
            {
                CommandHandlers[command].Invoke(manager, args);
                return true;
            }

            return false;
        }
    }
}
