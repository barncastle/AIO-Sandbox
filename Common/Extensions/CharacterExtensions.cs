using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Common.Constants;
using Common.Interfaces;
using Common.Structs;

namespace Common.Extensions
{
    public static class CharacterExtensions
    {
        public static uint GetDisplayId(this ICharacter character)
        {
            bool male = character.Gender == 0;

            switch ((Races)character.Race)
            {
                case Races.HUMAN:
                    return male ? 0x31u : 0x32u;

                case Races.ORC:
                    return male ? 0x33u : 0x34u;

                case Races.DWARF:
                    return male ? 0x35u : 0x36u;

                case Races.NIGHT_ELF:
                    return male ? 0x37u : 0x38u;

                case Races.UNDEAD:
                    return male ? 0x39u : 0x3Au;

                case Races.TAUREN:
                    return male ? 0x3Bu : 0x3Cu;

                case Races.GNOME:
                    return male ? 0x61Bu : 0x61Cu;

                case Races.TROLL:
                    return male ? 0x5C6u : 0x5C7u;

                case Races.BLOODELF:
                    return male ? 0x3C74u : 0x3C73u;

                case Races.DRAENEI:
                    return male ? 0x3EFDu : 0x3EFEu;

                default:
                    return male ? 0x31u : 0x32; // Default to human
            }
        }

        public static void SetDefaultValues(this ICharacter character)
        {
            bool male = character.Gender == 0;
            bool hunterFocus = character.Build < 3807;

            // scale
            character.Scale = 1f;
            if ((Races)character.Race == Races.TAUREN)
                character.Scale = male ? 1.35f : 1.25f; // tauren male, tauren female

            // display id
            character.DisplayId = character.GetDisplayId();

            // power type
            switch ((Classes)character.Class)
            {
                case Classes.WARRIOR:
                    character.PowerType = (byte)PowerTypes.RAGE;
                    break;

                case Classes.ROGUE:
                    character.PowerType = (byte)PowerTypes.ENERGY;
                    break;

                case Classes.HUNTER:
                    character.PowerType = (byte)(hunterFocus ? PowerTypes.FOCUS : PowerTypes.MANA);
                    break;

                default:
                    character.PowerType = (byte)PowerTypes.MANA;
                    break;
            }
        }

        public static IPacketWriter BuildMessage(this ICharacter character, IPacketWriter message, string text)
        {
            int build = character.Build;
            byte messageType = (byte)(build >= 4937 ? 0xA : 0x9);

            message.WriteUInt8(messageType); // System Message
            message.WriteUInt32(0); // Language: General
            message.WriteUInt64(0);

            if (build >= 4062)
                message.WriteInt32(text.Length + 1); // string length

            message.WriteString(text);
            message.WriteUInt8(0); // status flag
            return message;
        }

        public static IPacketWriter BuildForceSpeed(this ICharacter character, IPacketWriter writer, float modifier)
        {
            const float maxmod = 8f;
            modifier *= 10f / maxmod;
            modifier = Math.Min(Math.Max(modifier, 1f), maxmod); // Min 1 Max 8

            writer.WriteFloat(modifier * 7f);
            return writer;
        }

        public static void Teleport(this ICharacter character, Location loc, ref IWorldManager manager)
        {
            character.Teleport(loc.X, loc.Y, loc.Z, loc.O, loc.Map, ref manager);
        }

        public static void Demorph(this ICharacter character)
        {
            character.DisplayId = character.GetDisplayId();
        }

        public static byte[] GetPackedGUID(this ICharacter character)
        {
            ulong guid = character.Guid;
            byte[] packed = new byte[9];
            int count = 0;

            while (guid > 0)
            {
                byte bit = (byte)guid;
                if (bit != 0)
                {
                    packed[0] |= (byte)(1 << count);
                    packed[++count] = bit;
                }

                guid >>= 8;
            }

            Array.Resize(ref packed, count + 1);
            return packed;
        }
    }
}
