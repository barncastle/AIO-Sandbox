using System;
using System.Collections.Generic;
using System.Linq;
using Common.Interfaces;

namespace Common.Commands
{
    public class CommandManager
    {
        private delegate void CommandHandler(IWorldManager manager, string[] args);

        private static Dictionary<string, CommandHandler> CommandHandlers;

        static CommandManager()
        {
            CommandHandlers = new Dictionary<string, CommandHandler>(StringComparer.OrdinalIgnoreCase)
            {
                { "gps",     Commands.Gps },
                { "help",    Commands.Help },
                { "speed",   Commands.Speed },
                { "go",      Commands.Go },
                { "nudge",   Commands.Nudge },
                { "morph",   Commands.Morph },
                { "demorph", Commands.Demorph },
                { "fly",     Commands.Fly }
            };
        }


        public static bool InvokeHandler(string message, IWorldManager manager)
        {
            if (string.IsNullOrEmpty(message) || message[0] != '.')
                return false;

            string[] parts = message.Split(' ');

            string command = parts[0].TrimStart('.').Trim(); // remove command "." prefix and format
            string[] args = parts.Skip(1).Select(x => x.Trim().ToLower()).ToArray();

            if (CommandHandlers.TryGetValue(command, out var handle))
            {
                handle.Invoke(manager, args);
                return true;
            }

            return false;
        }
    }
}
