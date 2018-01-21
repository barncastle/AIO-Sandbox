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

            switch (character.Race)
            {
                case 1:
                    return (uint)(male ? 0x31 : 0x32);
                case 2:
                    return (uint)(male ? 0x33 : 0x34);
                case 3:
                    return (uint)(male ? 0x35 : 0x36);
                case 4:
                    return (uint)(male ? 0x37 : 0x38);
                case 5:
                    return (uint)(male ? 0x39 : 0x3A);
                case 6:
                    return (uint)(male ? 0x3B : 0x3C);
                case 7:
                    return (uint)(male ? 0x61B : 0x61C);
                case 8:
                    return (uint)(male ? 0x5C6 : 0x5C7);
                case 10:
                    return (uint)(male ? 0x3C74 : 0x3C73);
                case 11:
                    return (uint)(male ? 0x3EFD : 0x3EFE);
                default:
                    return (uint)(male ? 0x31 : 0x32); //Default to human
            }
        }

        public static void SetPowerType(this ICharacter character, bool hunterFocus = false)
        {
            switch ((Classes)character.Class)
            {
                case Classes.WARRIOR:
                    character.PowerType = (byte)PowerTypes.RAGE;
                    break;
                case Classes.ROGUE:
                    character.PowerType = (byte)PowerTypes.ENERGY;
                    break;
                default:
                    character.PowerType = (byte)PowerTypes.MANA;
                    break;
            }

            if (hunterFocus && (Classes)character.Class == Classes.HUNTER)
                character.PowerType = (byte)PowerTypes.FOCUS;
        }

        public static void SetField(this ICharacter character, int field, object value, ref SortedDictionary<int, byte[]> fieldData, ref byte[] maskArray)
        {
            switch (Type.GetTypeCode(value.GetType()))
            {
                case TypeCode.Byte:
                    fieldData.Add(field, new byte[] { (byte)value });
                    break;
                case TypeCode.Single:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                    fieldData.Add(field, BitConverter.GetBytes((dynamic)value));
                    break;
                default:
                    throw new NotSupportedException();
            }

            for (int i = 0; i < (fieldData[field].Length / 4); i++)
                maskArray[field / 8] |= (byte)(1 << ((field + i) % 8));
        }

        public static IPacketWriter BuildMessage(this ICharacter character, IPacketWriter message, string text)
        {
            message.WriteUInt8(9);  //System Message
            message.WriteUInt32(0); //Language: General
			message.WriteUInt64(0);
			if (character.Build >= 4062)
				message.WriteInt32(text.Length + 1);

            message.WriteString(text);
            message.WriteUInt8(0);
            return message;
        }

        public static IPacketWriter BuildForceSpeed(this ICharacter character, IPacketWriter writer, float modifier)
        {
            float maxmod = 8f;
            modifier *= (10f / maxmod);
            modifier = Math.Min(Math.Max(modifier, 1f), maxmod); //Min 1 Max 8

            writer.WriteFloat(modifier * 7f);
            return writer;
        }

		public static void Teleport(this ICharacter character, Location loc, ref IWorldManager manager)
		{
			character.Teleport(loc.X, loc.Y, loc.Z, loc.O, loc.Map, ref manager);
		}

		public static byte[] GetPackedGUID(this ICharacter character)
		{
			ulong guid = character.Guid;
			byte[] packed = new byte[9];
			int count = 0;

			while(guid != 0)
			{
				byte bit = (byte)(guid & 0xFF);
				if(bit != 0)
				{
					packed[0] |= (byte)(1 << count);
					packed[++count] = bit;
				}

				guid >>= 8;
			}

			return packed;
		}
	}
}
