using System;
using System.Linq;
using Common.Constants;
using Common.Cryptography;
using Common.Extensions;
using Common.Interfaces;
using Common.Structs;

namespace Common.Commands
{
    public class Commands
    {
        #region Coordinates

        [CommandHelp(".gps")]
        public static void Gps(IWorldManager manager, string[] args)
        {
            var character = manager.Account.ActiveCharacter;
            manager.Send(character.BuildMessage(character.Location.ToString()));
        }

        #endregion Coordinates

        #region Teleport

        [CommandHelp(".go {x} {y} {z} Optional: {mapid}")]
        [CommandHelp(".go {name}")]
        [CommandHelp(".go instance {name | id}")]
        public static void Go(IWorldManager manager, string[] args)
        {
            if (args.Length == 0)
                return;

            if (Read<float>(args, 0, out _)) // Co-ordinate port
                GoLocation(manager, args);
            else if (args[0].ToLower().Trim() == "instance") // Area Trigger
                GoTrigger(manager, args);
            else
                GoNamedArea(manager, true, args); // Worldport
        }

        private static void GoNamedArea(IWorldManager manager, bool worldport, string[] args)
        {
            int skip = args[0] == "area" || args[0] == "instance" ? 1 : 0;
            string needle = string.Join(" ", args.Skip(skip)); // Replace "area" and "instance"

            var locations = worldport ? Worldports.FindLocation(needle) : AreaTriggers.FindTrigger(needle);

            switch (locations.Count())
            {
                case 0: // No matches
                    manager.Send(manager.Account.ActiveCharacter.BuildMessage("No matching locations found"));
                    break;

                case 1: // Single match
                    manager.Account.ActiveCharacter.Teleport(locations.First().Loc, ref manager);
                    break;

                default: // Multiple possible matches
                    manager.Send(manager.Account.ActiveCharacter.BuildMessage("Multiple matches:"));

                    foreach (var (Desc, Loc) in locations)
                        manager.Send(manager.Account.ActiveCharacter.BuildMessage(" " + Desc));

                    break;
            }
        }

        private static void GoLocation(IWorldManager manager, string[] args)
        {
            if (args.Length < 3 || args.Length > 4)
                return;

            var character = manager.Account.ActiveCharacter;
            uint map = character.Location.Map;

            bool teleport = Read(args, 0, out float x);
            teleport &= Read(args, 1, out float y);
            teleport &= Read(args, 2, out float z);
            if (args.Length > 3)
                teleport &= Read(args, 3, out map);

            if (teleport)
                character.Teleport(x, y, z, character.Location.O, map, ref manager);
        }

        private static void GoTrigger(IWorldManager manager, string[] args)
        {
            if (args.Length < 2)
                return;

            var character = manager.Account.ActiveCharacter;

            if (uint.TryParse(args[1], out uint areaid)) // Area Id check
            {
                if (!AreaTriggers.Triggers.TryGetValue(areaid, out var loc))
                {
                    manager.Send(character.BuildMessage($"Area Id {areaid} does not exist"));
                    return;
                }

                character.Teleport(loc, ref manager);
            }
            else
            {
                GoNamedArea(manager, false, args); // Area name check
            }
        }

        #endregion Teleport

        #region Nudge

        [CommandHelp(".nudge Optional: [0 - 100] {z offset}")]
        public static void Nudge(IWorldManager manager, string[] args)
        {
            var character = manager.Account.ActiveCharacter;
            var loc = (Location)character.Location.Clone();

            if (Read(args, 0, out float force))
                force = Math.Min(Math.Max((int)force, 0), 100); // min 0 max 100
            else if (args.Length == 0)
                force = 1;
            else
                return;

            loc.X += (float)Math.Cos(loc.O) * force;
            loc.Y += (float)Math.Sin(loc.O) * force;
            if (args.Length > 1 && Read(args, 1, out float z)) // adjust Z position
                loc.Z += z;

            character.Teleport(loc, ref manager);
        }

        #endregion Nudge

        #region Speed

        [CommandHelp(".speed [0.1 - 1000] Optional: {run | swim | fly | all} ")]
        public static void Speed(IWorldManager manager, string[] args)
        {
            if (args.Length < 1)
                return;

            Read(args, 0, out float speed);
            speed = Math.Min(Math.Max(speed, 0.1f), 1000f); // Min 0.1 Max 1000.0

            string type = (args.Length > 1 ? args[1] : "all").ToLower().Trim();
            bool canfly = ClientAuth.ClientBuild >= 5965;

            var character = manager.Account.ActiveCharacter;

            switch (type)
            {
                case "swim":
                    manager.Send(character.BuildForceSpeed(speed, SpeedType.Swim));
                    break;

                case "run":
                    manager.Send(character.BuildForceSpeed(speed, SpeedType.Run));
                    break;

                case "fly" when canfly:
                    manager.Send(character.BuildForceSpeed(speed, SpeedType.Fly));
                    break;

                case "all" when canfly:
                    manager.Send(character.BuildForceSpeed(speed, SpeedType.Run));
                    manager.Send(character.BuildForceSpeed(speed, SpeedType.Swim));
                    manager.Send(character.BuildForceSpeed(speed, SpeedType.Fly));
                    break;

                case "all":
                    manager.Send(character.BuildForceSpeed(speed, SpeedType.Run));
                    manager.Send(character.BuildForceSpeed(speed, SpeedType.Swim));
                    break;

                default:
                    return;
            }

            manager.Send(character.BuildMessage($"{type.ToUpperFirst()} speed changed to {speed}x normal"));
        }

        #endregion Speed

        #region Morph

        [CommandHelp(".morph {id}")]
        public static void Morph(IWorldManager manager, string[] args)
        {
            if (args.Length < 1)
                return;

            if (Read(args, 0, out uint id))
            {
                var character = manager.Account.ActiveCharacter;
                character.DisplayId = id;

                manager.Send(character.BuildUpdate());
            }
        }

        [CommandHelp(".demorph")]
        public static void Demorph(IWorldManager manager, string[] args)
        {
            var character = manager.Account.ActiveCharacter;
            character.Demorph();
            manager.Send(character.BuildUpdate());
        }

        #endregion Morph

        #region Fly

        [CommandHelp(".fly {on | off}")]
        public static void Fly(IWorldManager manager, string[] args)
        {
            // check client build
            if (ClientAuth.ClientBuild < 5965)
                return;

            // validate args
            if (args.Length < 1)
                return;

            var character = manager.Account.ActiveCharacter;
            bool enabled = false;

            IPacketWriter packet = null;
            switch (args[0].ToLower().Trim())
            {
                case "on":
                    packet = character.BuildFly(true);
                    enabled = true;
                    break;
                case "off":
                    packet = character.BuildFly(false);
                    break;
            }

            if (packet != null)
            {
                manager.Send(packet);
                manager.Send(character.BuildMessage($"Flight {(enabled ? "enabled" : "disabled")}"));
            }
        }

        #endregion


        public static void Help(IWorldManager manager, string[] args)
        {
            var character = manager.Account.ActiveCharacter;
            var attrs = typeof(Commands).GetMethods()
                                        .Where(x => x.IsDefined(typeof(CommandHelpAttribute), false))
                                        .SelectMany(x => x.GetCustomAttributes(typeof(CommandHelpAttribute), false) as CommandHelpAttribute[])
                                        .OrderBy(x => x.HelpText);

            if (attrs.Any())
            {
                manager.Send(character.BuildMessage("Commands: "));
                foreach (var attr in attrs)
                    manager.Send(character.BuildMessage("    " + attr.HelpText));
            }
        }

        private static bool Read<T>(string[] args, uint index, out T result) where T : unmanaged
        {
            if (index < args.Length)
            {
                var converter = System.ComponentModel.TypeDescriptor.GetConverter(typeof(T));
                if (converter.IsValid(args[index]))
                {
                    result = (T)converter.ConvertFromString(args[index]);
                    return true;
                }
            }

            result = default;
            return false;
        }
    }
}
