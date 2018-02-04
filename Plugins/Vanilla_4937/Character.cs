using Common.Extensions;
using Common.Interfaces;
using Common.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Constants;
using System.Collections;

namespace Vanilla_4937
{
	[Serializable]
	public class Character : ICharacter
	{
		public int Build { get; set; } = Sandbox.Instance.Build;

		public ulong Guid { get; set; }
		public string Name { get; set; }
		public byte Race { get; set; }
		public byte Class { get; set; }
		public byte Gender { get; set; }
		public byte Skin { get; set; }
		public byte Face { get; set; }
		public byte HairStyle { get; set; }
		public byte HairColor { get; set; }
		public byte FacialHair { get; set; }
		public uint Level { get; set; } = 11;
		public uint Zone { get; set; }
		public Location Location { get; set; }
		public bool IsOnline { get; set; } = false;
		public uint Health { get; set; } = 100;
		public uint Mana { get; set; } = 100;
		public uint Rage { get; set; } = 1000;
		public uint Focus { get; set; } = 100;
		public uint Energy { get; set; } = 100;
		public uint Strength { get; set; } = 10;
		public uint Agility { get; set; } = 10;
		public uint Stamina { get; set; } = 10;
		public uint Intellect { get; set; } = 10;
		public uint Spirit { get; set; } = 10;
		public byte PowerType { get; set; } = 1;
		public byte RestedState { get; set; } = 3;
		public StandState StandState { get; set; } = StandState.STANDING;
		public bool IsTeleporting { get; set; } = false;
		public uint DisplayId { get; set; }
		public uint MountDisplayId { get; set; }
		public float Scale { get; set; }

		public IPacketWriter BuildUpdate()
		{
			byte maskSize = (((int)Fields.MAX + 32) / 32);
			SortedDictionary<int, byte[]> fieldData = new SortedDictionary<int, byte[]>();
			byte[] maskArray = new byte[maskSize * 4];

			Action<Fields, object> SetField = (place, value) => this.SetField((int)place, value, ref fieldData, ref maskArray);

			PacketWriter writer = new PacketWriter(Sandbox.Instance.Opcodes[global::Opcodes.SMSG_UPDATE_OBJECT], "SMSG_UPDATE_OBJECT");
			writer.WriteUInt32(1); //Number of transactions
			writer.WriteUInt8(0);

			writer.WriteUInt8(3); //UpdateType <--- New Type for 1.9.0
			writer.Write(this.GetPackedGUID());
			writer.WriteUInt8(4); //ObjectType, 4 = Player

			writer.WriteUInt8(0x71); // UpdateFlags
			writer.WriteUInt32(0x2000);  //MovementFlagMask
			writer.WriteUInt32((uint)Environment.TickCount);

			writer.WriteFloat(Location.X);  //x
			writer.WriteFloat(Location.Y);  //y
			writer.WriteFloat(Location.Z);  //z
			writer.WriteFloat(Location.O);  //w (o)
			writer.WriteInt32(0); // falltime

			writer.WriteFloat(0);
			writer.WriteFloat(1);
			writer.WriteFloat(0);
			writer.WriteFloat(0);

			writer.WriteFloat(2.5f); // WalkSpeed
			writer.WriteFloat(7.0f); // RunSpeed
			writer.WriteFloat(2.5f); // Backwards WalkSpeed
			writer.WriteFloat(4.7222f); // SwimSpeed
			writer.WriteFloat(4.7222f); // Backwards SwimSpeed
			writer.WriteFloat(3.14f); // TurnSpeed

			writer.Write(1);
			
			SetField(Fields.OBJECT_FIELD_GUID, this.Guid);
			SetField(Fields.OBJECT_FIELD_TYPE, (uint)0x19);
			SetField(Fields.OBJECT_FIELD_ENTRY, 0);
			SetField(Fields.OBJECT_FIELD_SCALE_X, this.Scale);
			SetField(Fields.OBJECT_FIELD_PADDING, 0);
			SetField(Fields.UNIT_FIELD_TARGET, (ulong)0);
			SetField(Fields.UNIT_FIELD_HEALTH, this.Health);
			SetField(Fields.UNIT_FIELD_POWER2, 0);
			SetField(Fields.UNIT_FIELD_MAXHEALTH, this.Health);
			SetField(Fields.UNIT_FIELD_MAXPOWER2, this.Rage);
			SetField(Fields.UNIT_FIELD_LEVEL, 0);// this.Level);
			SetField(Fields.UNIT_FIELD_BYTES_0, BitConverter.ToUInt32(new byte[] { this.Race, this.Class, this.Gender, this.PowerType }, 0));
			SetField(Fields.UNIT_FIELD_STAT0, this.Strength);
			SetField(Fields.UNIT_FIELD_STAT1, this.Agility);
			SetField(Fields.UNIT_FIELD_STAT2, this.Stamina);
			SetField(Fields.UNIT_FIELD_STAT3, this.Intellect);
			SetField(Fields.UNIT_FIELD_STAT4, this.Spirit);
			SetField(Fields.UNIT_FIELD_FLAGS, 8);
			SetField(Fields.UNIT_FIELD_BASE_MANA, this.Mana);
			SetField(Fields.UNIT_FIELD_DISPLAYID, DisplayId);
			SetField(Fields.UNIT_FIELD_MOUNTDISPLAYID, MountDisplayId);
			SetField(Fields.UNIT_FIELD_BYTES_1, BitConverter.ToUInt32(new byte[] { (byte)StandState, 0, 0, 0 }, 0));
			SetField(Fields.UNIT_FIELD_BYTES_2, 0);
			SetField(Fields.PLAYER_BYTES, BitConverter.ToUInt32(new byte[] { Skin, Face, HairStyle, HairColor }, 0));
			SetField(Fields.PLAYER_BYTES_2, BitConverter.ToUInt32(new byte[] { FacialHair, 0, 0, RestedState }, 0));
			SetField(Fields.PLAYER_BYTES_3, (uint)this.Gender);
			SetField(Fields.PLAYER_XP, 47);
			SetField(Fields.PLAYER_NEXT_LEVEL_XP, 200);
			SetField(Fields.PLAYER_FIELD_WATCHED_FACTION_INDEX, -1);
			SetField(Fields.PLAYER_FLAGS, 0);

			SetField(Fields.UNIT_FIELD_ATTACK_POWER, 1);
			SetField(Fields.UNIT_FIELD_ATTACK_POWER_MODS, 0);
			SetField(Fields.UNIT_FIELD_RANGED_ATTACK_POWER, 1);
			SetField(Fields.UNIT_FIELD_RANGED_ATTACK_POWER_MODS, 0);

			for (int i = 0; i < 32; i++)
				SetField(Fields.PLAYER_EXPLORED_ZONES_1 + i, 0xFFFFFFFF);

			//FillInPartialObjectData
			writer.WriteUInt8(maskSize); //UpdateMaskBlocks
			writer.WriteBytes(maskArray);
			foreach (var kvp in fieldData)
				writer.WriteBytes(kvp.Value); //Data

			return writer;
		}

		public IPacketWriter BuildMessage(string text)
		{
			PacketWriter message = new PacketWriter(Sandbox.Instance.Opcodes[global::Opcodes.SMSG_MESSAGECHAT], "SMSG_MESSAGECHAT");
			return this.BuildMessage(message, text, Sandbox.Instance.Build);
		}

		public void Teleport(float x, float y, float z, float o, uint map, ref IWorldManager manager)
		{
			IsTeleporting = true;

			if (Location.Map == map)
			{
				PacketWriter movementStatus = new PacketWriter(Sandbox.Instance.Opcodes[global::Opcodes.MSG_MOVE_TELEPORT_ACK], "MSG_MOVE_TELEPORT_ACK");
				movementStatus.WriteUInt64(this.Guid);
				movementStatus.WriteUInt64(0); //Flags
				movementStatus.WriteFloat(x);
				movementStatus.WriteFloat(y);
				movementStatus.WriteFloat(z);
				movementStatus.WriteFloat(o);
				movementStatus.WriteFloat(0);
				manager.Send(movementStatus);
			}
			else
			{
				//Loading screen
				PacketWriter transferPending = new PacketWriter(Sandbox.Instance.Opcodes[global::Opcodes.SMSG_TRANSFER_PENDING], "SMSG_TRANSFER_PENDING");
				transferPending.WriteUInt32(map);
				manager.Send(transferPending);

				//New world transfer
				PacketWriter newWorld = new PacketWriter(Sandbox.Instance.Opcodes[global::Opcodes.SMSG_NEW_WORLD], "SMSG_NEW_WORLD");
				newWorld.WriteUInt32(map);
				newWorld.WriteFloat(x);
				newWorld.WriteFloat(y);
				newWorld.WriteFloat(z);
				newWorld.WriteFloat(o);
				manager.Send(newWorld);
			}

			System.Threading.Thread.Sleep(150); //Pause to factor unsent packets

			Location = new Location(x, y, z, o, map);
			manager.Send(BuildUpdate());

			IsTeleporting = false;
		}

		public IPacketWriter BuildForceSpeed(float modifier, bool swim = false)
		{
			var opcode = swim ? global::Opcodes.SMSG_FORCE_SWIM_SPEED_CHANGE : global::Opcodes.SMSG_FORCE_SPEED_CHANGE;
			PacketWriter writer = new PacketWriter(Sandbox.Instance.Opcodes[opcode], opcode.ToString());
			writer.WriteUInt64(this.Guid);
			writer.Write(0);
			return this.BuildForceSpeed(writer, modifier);
		}

		internal enum Fields
		{
			OBJECT_FIELD_GUID = 0,
			OBJECT_FIELD_TYPE = 2,
			OBJECT_FIELD_ENTRY = 3,
			OBJECT_FIELD_SCALE_X = 4,
			OBJECT_FIELD_PADDING = 5,
			UNIT_FIELD_CHARM = 6,
			UNIT_FIELD_SUMMON = 8,
			UNIT_FIELD_CHARMEDBY = 10,
			UNIT_FIELD_SUMMONEDBY = 12,
			UNIT_FIELD_CREATEDBY = 14,
			UNIT_FIELD_TARGET = 16,
			UNIT_FIELD_PERSUADED = 18,
			UNIT_FIELD_CHANNEL_OBJECT = 20,
			UNIT_FIELD_HEALTH = 22,
			UNIT_FIELD_POWER1 = 23,
			UNIT_FIELD_POWER2 = 24,
			UNIT_FIELD_POWER3 = 25,
			UNIT_FIELD_POWER4 = 26,
			UNIT_FIELD_POWER5 = 27,
			UNIT_FIELD_MAXHEALTH = 28,
			UNIT_FIELD_MAXPOWER1 = 29,
			UNIT_FIELD_MAXPOWER2 = 30,
			UNIT_FIELD_MAXPOWER3 = 31,
			UNIT_FIELD_MAXPOWER4 = 32,
			UNIT_FIELD_MAXPOWER5 = 33,
			UNIT_FIELD_LEVEL = 34,
			UNIT_FIELD_FACTIONTEMPLATE = 35,
			UNIT_FIELD_BYTES_0 = 36,
			UNIT_VIRTUAL_ITEM_SLOT_DISPLAY = 37,
			UNIT_VIRTUAL_ITEM_INFO = 40,
			UNIT_FIELD_FLAGS = 46,
			UNIT_FIELD_AURA = 47,
			UNIT_FIELD_AURAFLAGS = 95,
			UNIT_FIELD_AURALEVELS = 101,
			UNIT_FIELD_AURAAPPLICATIONS = 113,
			UNIT_FIELD_AURASTATE = 125,
			UNIT_FIELD_BASEATTACKTIME = 126,
			UNIT_FIELD_RANGEDATTACKTIME = 128,
			UNIT_FIELD_BOUNDINGRADIUS = 129,
			UNIT_FIELD_COMBATREACH = 130,
			UNIT_FIELD_DISPLAYID = 131,
			UNIT_FIELD_NATIVEDISPLAYID = 132,
			UNIT_FIELD_MOUNTDISPLAYID = 133,
			UNIT_FIELD_MINDAMAGE = 134,
			UNIT_FIELD_MAXDAMAGE = 135,
			UNIT_FIELD_MINOFFHANDDAMAGE = 136,
			UNIT_FIELD_MAXOFFHANDDAMAGE = 137,
			UNIT_FIELD_BYTES_1 = 138,
			UNIT_FIELD_PETNUMBER = 139,
			UNIT_FIELD_PET_NAME_TIMESTAMP = 140,
			UNIT_FIELD_PETEXPERIENCE = 141,
			UNIT_FIELD_PETNEXTLEVELEXP = 142,
			UNIT_DYNAMIC_FLAGS = 143,
			UNIT_CHANNEL_SPELL = 144,
			UNIT_MOD_CAST_SPEED = 145,
			UNIT_CREATED_BY_SPELL = 146,
			UNIT_NPC_FLAGS = 147,
			UNIT_NPC_EMOTESTATE = 148,
			UNIT_TRAINING_POINTS = 149,
			UNIT_FIELD_STAT0 = 150,
			UNIT_FIELD_STAT1 = 151,
			UNIT_FIELD_STAT2 = 152,
			UNIT_FIELD_STAT3 = 153,
			UNIT_FIELD_STAT4 = 154,
			UNIT_FIELD_RESISTANCES = 155,
			UNIT_FIELD_BASE_MANA = 162,
			UNIT_FIELD_BASE_HEALTH = 163,
			UNIT_FIELD_BYTES_2 = 164,
			UNIT_FIELD_ATTACK_POWER = 165,
			UNIT_FIELD_ATTACK_POWER_MODS = 166,
			UNIT_FIELD_ATTACK_POWER_MULTIPLIER = 167,
			UNIT_FIELD_RANGED_ATTACK_POWER = 168,
			UNIT_FIELD_RANGED_ATTACK_POWER_MODS = 169,
			UNIT_FIELD_RANGED_ATTACK_POWER_MULTIPLIER = 170,
			UNIT_FIELD_MINRANGEDDAMAGE = 171,
			UNIT_FIELD_MAXRANGEDDAMAGE = 172,
			UNIT_FIELD_POWER_COST_MODIFIER = 173,
			UNIT_FIELD_POWER_COST_MULTIPLIER = 180,
			UNIT_FIELD_PADDING = 187,
			UNIT_END = 188,
			PLAYER_DUEL_ARBITER = 188,
			PLAYER_FLAGS = 190,
			PLAYER_GUILDID = 191,
			PLAYER_GUILDRANK = 192,
			PLAYER_BYTES = 193,
			PLAYER_BYTES_2 = 194,
			PLAYER_BYTES_3 = 195,
			PLAYER_DUEL_TEAM = 196,
			PLAYER_GUILD_TIMESTAMP = 197,
			PLAYER_QUEST_LOG_1_1 = 198,
			PLAYER_QUEST_LOG_1_2 = 199,
			PLAYER_QUEST_LOG_2_1 = 201,
			PLAYER_QUEST_LOG_2_2 = 202,
			PLAYER_QUEST_LOG_3_1 = 204,
			PLAYER_QUEST_LOG_3_2 = 205,
			PLAYER_QUEST_LOG_4_1 = 207,
			PLAYER_QUEST_LOG_4_2 = 208,
			PLAYER_QUEST_LOG_5_1 = 210,
			PLAYER_QUEST_LOG_5_2 = 211,
			PLAYER_QUEST_LOG_6_1 = 213,
			PLAYER_QUEST_LOG_6_2 = 214,
			PLAYER_QUEST_LOG_7_1 = 216,
			PLAYER_QUEST_LOG_7_2 = 217,
			PLAYER_QUEST_LOG_8_1 = 219,
			PLAYER_QUEST_LOG_8_2 = 220,
			PLAYER_QUEST_LOG_9_1 = 222,
			PLAYER_QUEST_LOG_9_2 = 223,
			PLAYER_QUEST_LOG_10_1 = 225,
			PLAYER_QUEST_LOG_10_2 = 226,
			PLAYER_QUEST_LOG_11_1 = 228,
			PLAYER_QUEST_LOG_11_2 = 229,
			PLAYER_QUEST_LOG_12_1 = 231,
			PLAYER_QUEST_LOG_12_2 = 232,
			PLAYER_QUEST_LOG_13_1 = 234,
			PLAYER_QUEST_LOG_13_2 = 235,
			PLAYER_QUEST_LOG_14_1 = 237,
			PLAYER_QUEST_LOG_14_2 = 238,
			PLAYER_QUEST_LOG_15_1 = 240,
			PLAYER_QUEST_LOG_15_2 = 241,
			PLAYER_QUEST_LOG_16_1 = 243,
			PLAYER_QUEST_LOG_16_2 = 244,
			PLAYER_QUEST_LOG_17_1 = 246,
			PLAYER_QUEST_LOG_17_2 = 247,
			PLAYER_QUEST_LOG_18_1 = 249,
			PLAYER_QUEST_LOG_18_2 = 250,
			PLAYER_QUEST_LOG_19_1 = 252,
			PLAYER_QUEST_LOG_19_2 = 253,
			PLAYER_QUEST_LOG_20_1 = 255,
			PLAYER_QUEST_LOG_20_2 = 256,
			PLAYER_VISIBLE_ITEM_1_CREATOR = 258,
			PLAYER_VISIBLE_ITEM_1_0 = 260,
			PLAYER_VISIBLE_ITEM_1_PROPERTIES = 268,
			PLAYER_VISIBLE_ITEM_1_PAD = 269,
			PLAYER_VISIBLE_ITEM_2_CREATOR = 270,
			PLAYER_VISIBLE_ITEM_2_0 = 272,
			PLAYER_VISIBLE_ITEM_2_PROPERTIES = 280,
			PLAYER_VISIBLE_ITEM_2_PAD = 281,
			PLAYER_VISIBLE_ITEM_3_CREATOR = 282,
			PLAYER_VISIBLE_ITEM_3_0 = 284,
			PLAYER_VISIBLE_ITEM_3_PROPERTIES = 292,
			PLAYER_VISIBLE_ITEM_3_PAD = 293,
			PLAYER_VISIBLE_ITEM_4_CREATOR = 294,
			PLAYER_VISIBLE_ITEM_4_0 = 296,
			PLAYER_VISIBLE_ITEM_4_PROPERTIES = 304,
			PLAYER_VISIBLE_ITEM_4_PAD = 305,
			PLAYER_VISIBLE_ITEM_5_CREATOR = 306,
			PLAYER_VISIBLE_ITEM_5_0 = 308,
			PLAYER_VISIBLE_ITEM_5_PROPERTIES = 316,
			PLAYER_VISIBLE_ITEM_5_PAD = 317,
			PLAYER_VISIBLE_ITEM_6_CREATOR = 318,
			PLAYER_VISIBLE_ITEM_6_0 = 320,
			PLAYER_VISIBLE_ITEM_6_PROPERTIES = 328,
			PLAYER_VISIBLE_ITEM_6_PAD = 329,
			PLAYER_VISIBLE_ITEM_7_CREATOR = 330,
			PLAYER_VISIBLE_ITEM_7_0 = 332,
			PLAYER_VISIBLE_ITEM_7_PROPERTIES = 340,
			PLAYER_VISIBLE_ITEM_7_PAD = 341,
			PLAYER_VISIBLE_ITEM_8_CREATOR = 342,
			PLAYER_VISIBLE_ITEM_8_0 = 344,
			PLAYER_VISIBLE_ITEM_8_PROPERTIES = 352,
			PLAYER_VISIBLE_ITEM_8_PAD = 353,
			PLAYER_VISIBLE_ITEM_9_CREATOR = 354,
			PLAYER_VISIBLE_ITEM_9_0 = 356,
			PLAYER_VISIBLE_ITEM_9_PROPERTIES = 364,
			PLAYER_VISIBLE_ITEM_9_PAD = 365,
			PLAYER_VISIBLE_ITEM_10_CREATOR = 366,
			PLAYER_VISIBLE_ITEM_10_0 = 368,
			PLAYER_VISIBLE_ITEM_10_PROPERTIES = 376,
			PLAYER_VISIBLE_ITEM_10_PAD = 377,
			PLAYER_VISIBLE_ITEM_11_CREATOR = 378,
			PLAYER_VISIBLE_ITEM_11_0 = 380,
			PLAYER_VISIBLE_ITEM_11_PROPERTIES = 388,
			PLAYER_VISIBLE_ITEM_11_PAD = 389,
			PLAYER_VISIBLE_ITEM_12_CREATOR = 390,
			PLAYER_VISIBLE_ITEM_12_0 = 392,
			PLAYER_VISIBLE_ITEM_12_PROPERTIES = 400,
			PLAYER_VISIBLE_ITEM_12_PAD = 401,
			PLAYER_VISIBLE_ITEM_13_CREATOR = 402,
			PLAYER_VISIBLE_ITEM_13_0 = 404,
			PLAYER_VISIBLE_ITEM_13_PROPERTIES = 412,
			PLAYER_VISIBLE_ITEM_13_PAD = 413,
			PLAYER_VISIBLE_ITEM_14_CREATOR = 414,
			PLAYER_VISIBLE_ITEM_14_0 = 416,
			PLAYER_VISIBLE_ITEM_14_PROPERTIES = 424,
			PLAYER_VISIBLE_ITEM_14_PAD = 425,
			PLAYER_VISIBLE_ITEM_15_CREATOR = 426,
			PLAYER_VISIBLE_ITEM_15_0 = 428,
			PLAYER_VISIBLE_ITEM_15_PROPERTIES = 436,
			PLAYER_VISIBLE_ITEM_15_PAD = 437,
			PLAYER_VISIBLE_ITEM_16_CREATOR = 438,
			PLAYER_VISIBLE_ITEM_16_0 = 440,
			PLAYER_VISIBLE_ITEM_16_PROPERTIES = 448,
			PLAYER_VISIBLE_ITEM_16_PAD = 449,
			PLAYER_VISIBLE_ITEM_17_CREATOR = 450,
			PLAYER_VISIBLE_ITEM_17_0 = 452,
			PLAYER_VISIBLE_ITEM_17_PROPERTIES = 460,
			PLAYER_VISIBLE_ITEM_17_PAD = 461,
			PLAYER_VISIBLE_ITEM_18_CREATOR = 462,
			PLAYER_VISIBLE_ITEM_18_0 = 464,
			PLAYER_VISIBLE_ITEM_18_PROPERTIES = 472,
			PLAYER_VISIBLE_ITEM_18_PAD = 473,
			PLAYER_VISIBLE_ITEM_19_CREATOR = 474,
			PLAYER_VISIBLE_ITEM_19_0 = 476,
			PLAYER_VISIBLE_ITEM_19_PROPERTIES = 484,
			PLAYER_VISIBLE_ITEM_19_PAD = 485,
			PLAYER_FIELD_INV_SLOT_HEAD = 486,
			PLAYER_FIELD_PACK_SLOT_1 = 532,
			PLAYER_FIELD_BANK_SLOT_1 = 564,
			PLAYER_FIELD_BANKBAG_SLOT_1 = 612,
			PLAYER_FIELD_VENDORBUYBACK_SLOT_1 = 624,
			PLAYER_FIELD_KEYRING_SLOT_1 = 648,
			PLAYER_FARSIGHT = 712,
			PLAYER__FIELD_COMBO_TARGET = 714,
			PLAYER_XP = 716,
			PLAYER_NEXT_LEVEL_XP = 717,
			PLAYER_SKILL_INFO_1_1 = 718,
			PLAYER_CHARACTER_POINTS1 = 1102,
			PLAYER_CHARACTER_POINTS2 = 1103,
			PLAYER_TRACK_CREATURES = 1104,
			PLAYER_TRACK_RESOURCES = 1105,
			PLAYER_BLOCK_PERCENTAGE = 1106,
			PLAYER_DODGE_PERCENTAGE = 1107,
			PLAYER_PARRY_PERCENTAGE = 1108,
			PLAYER_CRIT_PERCENTAGE = 1109,
			PLAYER_RANGED_CRIT_PERCENTAGE = 1110,
			PLAYER_EXPLORED_ZONES_1 = 1111,
			PLAYER_REST_STATE_EXPERIENCE = 1175,
			PLAYER_FIELD_COINAGE = 1176,
			PLAYER_FIELD_POSSTAT0 = 1177,
			PLAYER_FIELD_POSSTAT1 = 1178,
			PLAYER_FIELD_POSSTAT2 = 1179,
			PLAYER_FIELD_POSSTAT3 = 191,
			PLAYER_FIELD_POSSTAT4 = 236,
			PLAYER_FIELD_NEGSTAT0 = 956,
			PLAYER_FIELD_NEGSTAT1 = 12476,
			PLAYER_FIELD_NEGSTAT2 = 196796,
			PLAYER_FIELD_NEGSTAT3 = 3145916,
			PLAYER_FIELD_NEGSTAT4 = 50331836,
			PLAYER_FIELD_RESISTANCEBUFFMODSPOSITIVE = 805306556,
			PLAYER_FIELD_RESISTANCEBUFFMODSNEGATIVE = 1194,
			PLAYER_FIELD_MOD_DAMAGE_DONE_POS = 1201,
			PLAYER_FIELD_MOD_DAMAGE_DONE_NEG = 1208,
			PLAYER_FIELD_MOD_DAMAGE_DONE_PCT = 1215,
			PLAYER_FIELD_BYTES = 1222,
			PLAYER_AMMO_ID = 1223,
			PLAYER_SELF_RES_SPELL = 1224,
			PLAYER_FIELD_PVP_MEDALS = 1225,
			PLAYER_FIELD_BUYBACK_PRICE_1 = 1226,
			PLAYER_FIELD_BUYBACK_TIMESTAMP_1 = 1238,
			PLAYER_FIELD_SESSION_KILLS = 1250,
			PLAYER_FIELD_YESTERDAY_KILLS = 1251,
			PLAYER_FIELD_LAST_WEEK_KILLS = 1252,
			PLAYER_FIELD_THIS_WEEK_KILLS = 1253,
			PLAYER_FIELD_THIS_WEEK_CONTRIBUTION = 1254,
			PLAYER_FIELD_LIFETIME_HONORBALE_KILLS = 1255,
			PLAYER_FIELD_LIFETIME_DISHONORBALE_KILLS = 1256,
			PLAYER_FIELD_YESTERDAY_CONTRIBUTION = 1257,
			PLAYER_FIELD_LAST_WEEK_CONTRIBUTION = 1258,
			PLAYER_FIELD_LAST_WEEK_RANK = 1259,
			PLAYER_FIELD_BYTES2 = 1260,
			PLAYER_FIELD_WATCHED_FACTION_INDEX = 1261,
			PLAYER_FIELD_COMBAT_RATING_1 = 1262,
			MAX = 1282 - 4,
		}
	}
}
