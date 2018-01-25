using Common.Constants;
using Common.Interfaces;
using Common.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Extensions
{
	public static class CharacterExtensions
	{
		public static uint GetDisplayId(this ICharacter character)
		{
			bool male = (character.Gender == 0);

			switch ((Races)character.Race)
			{
				case Races.HUMAN:
					return (uint)(male ? 0x31 : 0x32);
				case Races.ORC:
					return (uint)(male ? 0x33 : 0x34);
				case Races.DWARF:
					return (uint)(male ? 0x35 : 0x36);
				case Races.NIGHT_ELF:
					return (uint)(male ? 0x37 : 0x38);
				case Races.UNDEAD:
					return (uint)(male ? 0x39 : 0x3A);
				case Races.TAUREN:
					return (uint)(male ? 0x3B : 0x3C);
				case Races.GNOME:
					return (uint)(male ? 0x61B : 0x61C);
				case Races.TROLL:
					return (uint)(male ? 0x5C6 : 0x5C7);
				case Races.BLOODELF:
					return (uint)(male ? 0x3C74 : 0x3C73);
				case Races.DRAENEI:
					return (uint)(male ? 0x3EFD : 0x3EFE);
				default:
					return (uint)(male ? 0x31 : 0x32); //Default to human
			}
		}

		public static void SetDefaultValues(this ICharacter character, bool hunterFocus = false)
		{
			bool male = (character.Gender == 0);

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



		public static void SetField(this ICharacter character, int field, object value, ref SortedDictionary<int, byte[]> fieldData, ref byte[] maskArray)
		{
			switch (Type.GetTypeCode(value.GetType()))
			{
				case TypeCode.Byte:
					fieldData.Add(field, new byte[] { (byte)value });
					break;
				case TypeCode.Single:
					fieldData.Add(field, BitConverter.GetBytes((float)value));
					break;
				case TypeCode.UInt16:
					fieldData.Add(field, BitConverter.GetBytes((ushort)value));
					break;
				case TypeCode.UInt32:
					fieldData.Add(field, BitConverter.GetBytes((uint)value));
					break;
				case TypeCode.UInt64:
					fieldData.Add(field, BitConverter.GetBytes((ulong)value));
					break;
				case TypeCode.Int16:
					fieldData.Add(field, BitConverter.GetBytes((short)value));
					break;
				case TypeCode.Int32:
					fieldData.Add(field, BitConverter.GetBytes((int)value));
					break;
				case TypeCode.Int64:
					fieldData.Add(field, BitConverter.GetBytes((long)value));
					break;
				default:
					throw new NotSupportedException();
			}

			for (int i = 0; i < (fieldData[field].Length / 4); i++)
				maskArray[field / 8] |= (byte)(1 << ((field + i) % 8));
		}

		public static IPacketWriter BuildMessage(this ICharacter character, IPacketWriter message, string text, int build)
		{
			message.WriteUInt8(9);  // System Message
			message.WriteUInt32(0); // Language: General
			message.WriteUInt64(0);

			if (build >= 4062)
				message.WriteInt32(text.Length + 1); // string length

			message.WriteString(text);
			message.WriteUInt8(0);
			return message;
		}

		public static IPacketWriter BuildForceSpeed(this ICharacter character, IPacketWriter writer, float modifier)
		{
			const float maxmod = 8f;
			modifier *= (10f / maxmod);
			modifier = Math.Min(Math.Max(modifier, 1f), maxmod); //Min 1 Max 8

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
