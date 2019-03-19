using System;
using System.Collections.Generic;
using Common.Constants;
using Common.Extensions;
using Common.Interfaces;
using Common.Structs;

namespace Vanilla_4341
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
            byte maskSize = ((int)Fields.MAX + 31) / 32;
            SortedDictionary<int, byte[]> fieldData = new SortedDictionary<int, byte[]>();
            byte[] maskArray = new byte[maskSize * 4];

            Action<Fields, object> SetField = (place, value) => this.SetField((int)place, value, ref fieldData, ref maskArray);

            PacketWriter writer = new PacketWriter(Sandbox.Instance.Opcodes[global::Opcodes.SMSG_UPDATE_OBJECT], "SMSG_UPDATE_OBJECT");
            writer.WriteUInt32(1); //Number of transactions
            writer.WriteUInt8(0);
            writer.WriteUInt8(2); //UpdateType
            writer.WriteUInt64(this.Guid);
            writer.WriteUInt8(4); //ObjectType, 4 = Player

            writer.WriteUInt32(0);  //MovementFlagMask
            writer.WriteUInt32((uint)Environment.TickCount);
            writer.WriteFloat(Location.X);  //x
            writer.WriteFloat(Location.Y);  //y
            writer.WriteFloat(Location.Z);  //z
            writer.WriteFloat(Location.O);  //w (o)
            writer.WriteFloat(0);
            writer.WriteFloat(2.5f); //WalkSpeed
            writer.WriteFloat(7.0f); //RunSpeed
            writer.WriteFloat(2.5f); //Backwards WalkSpeed
            writer.WriteFloat(4.7222f); //SwimSpeed
            writer.WriteFloat(4.7222f); //Backwards SwimSpeed
            writer.WriteFloat(3.14f); //TurnSpeed

            writer.WriteUInt32(1); //Flags, 1 - Player
            writer.WriteUInt32(1); //AttackCycle
            writer.WriteUInt32(0); //TimerId
            writer.WriteUInt64(0); //VictimGuid

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
            SetField(Fields.UNIT_FIELD_LEVEL, this.Level);
            SetField(Fields.UNIT_FIELD_BYTES_0, BitConverter.ToUInt32(new byte[] { this.Race, this.Class, this.Gender, this.PowerType }, 0));
            SetField(Fields.UNIT_FIELD_STAT0, this.Strength);
            SetField(Fields.UNIT_FIELD_STAT1, this.Agility);
            SetField(Fields.UNIT_FIELD_STAT2, this.Stamina);
            SetField(Fields.UNIT_FIELD_STAT3, this.Intellect);
            SetField(Fields.UNIT_FIELD_STAT4, this.Spirit);
            SetField(Fields.UNIT_FIELD_FLAGS, 0);
            SetField(Fields.UNIT_FIELD_BASE_MANA, this.Mana);
            SetField(Fields.UNIT_FIELD_DISPLAYID, DisplayId);
            SetField(Fields.UNIT_FIELD_MOUNTDISPLAYID, MountDisplayId);
            SetField(Fields.UNIT_FIELD_BYTES_1, BitConverter.ToUInt32(new byte[] { (byte)StandState, 0, 0, 0 }, 0));
            SetField(Fields.PLAYER_SELECTION, (ulong)0);
            SetField(Fields.PLAYER_BYTES, BitConverter.ToUInt32(new byte[] { Skin, Face, HairStyle, HairColor }, 0));
            SetField(Fields.PLAYER_BYTES_2, BitConverter.ToUInt32(new byte[] { 0, FacialHair, 0, RestedState }, 0));
            SetField(Fields.PLAYER_BYTES_3, (uint)this.Gender);
            SetField(Fields.PLAYER_XP, 47);
            SetField(Fields.PLAYER_NEXT_LEVEL_XP, 200);

            SetField(Fields.UNIT_FIELD_ATTACKPOWER, 1);
            SetField(Fields.UNIT_FIELD_ATTACK_POWER_MODS, 0);
            SetField(Fields.UNIT_FIELD_RANGEDATTACKPOWER, 1);
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
            UNIT_FIELD_AURALEVELS = 103,
            UNIT_FIELD_AURAAPPLICATIONS = 113,
            UNIT_FIELD_AURAFLAGS = 123,
            UNIT_FIELD_AURASTATE = 130,
            UNIT_FIELD_BASEATTACKTIME = 131,
            UNIT_FIELD_RANGEDATTACKTIME = 133,
            UNIT_FIELD_BOUNDINGRADIUS = 134,
            UNIT_FIELD_COMBATREACH = 135,
            UNIT_FIELD_DISPLAYID = 136,
            UNIT_FIELD_NATIVEDISPLAYID = 137,
            UNIT_FIELD_MOUNTDISPLAYID = 138,
            UNIT_FIELD_MINDAMAGE = 139,
            UNIT_FIELD_MAXDAMAGE = 140,
            UNIT_FIELD_MINOFFHANDDAMAGE = 141,
            UNIT_FIELD_MAXOFFHANDDAMAGE = 142,
            UNIT_FIELD_BYTES_1 = 143,
            UNIT_FIELD_PETNUMBER = 144,
            UNIT_FIELD_PET_NAME_TIMESTAMP = 145,
            UNIT_FIELD_PETEXPERIENCE = 146,
            UNIT_FIELD_PETNEXTLEVELEXP = 147,
            UNIT_DYNAMIC_FLAGS = 148,
            UNIT_CHANNEL_SPELL = 149,
            UNIT_MOD_CAST_SPEED = 150,
            UNIT_CREATED_BY_SPELL = 151,
            UNIT_NPC_FLAGS = 152,
            UNIT_NPC_EMOTESTATE = 153,
            UNIT_TRAINING_POINTS = 154,
            UNIT_FIELD_STAT0 = 155,
            UNIT_FIELD_STAT1 = 156,
            UNIT_FIELD_STAT2 = 157,
            UNIT_FIELD_STAT3 = 158,
            UNIT_FIELD_STAT4 = 159,
            UNIT_FIELD_RESISTANCES = 160,
            UNIT_FIELD_ATTACKPOWER = 167,
            UNIT_FIELD_BASE_MANA = 168,
            UNIT_FIELD_ATTACK_POWER_MODS = 169,
            UNIT_FIELD_BYTES_2 = 170,
            UNIT_FIELD_RANGEDATTACKPOWER = 171,
            UNIT_FIELD_RANGED_ATTACK_POWER_MODS = 172,
            UNIT_FIELD_MINRANGEDDAMAGE = 173,
            UNIT_FIELD_MAXRANGEDDAMAGE = 174,
            UNIT_FIELD_PADDING = 175,
            UNIT_END = 176,
            PLAYER_SELECTION = 176,
            PLAYER_DUEL_ARBITER = 178,
            PLAYER_FLAGS = 180,
            PLAYER_GUILDID = 181,
            PLAYER_GUILDRANK = 182,
            PLAYER_BYTES = 183,
            PLAYER_BYTES_2 = 184,
            PLAYER_BYTES_3 = 185,
            PLAYER_DUEL_TEAM = 186,
            PLAYER_GUILD_TIMESTAMP = 187,
            PLAYER_QUEST_LOG_1_1 = 188,
            PLAYER_QUEST_LOG_1_2 = 189,
            PLAYER_QUEST_LOG_2_1 = 191,
            PLAYER_QUEST_LOG_2_2 = 192,
            PLAYER_QUEST_LOG_3_1 = 194,
            PLAYER_QUEST_LOG_3_2 = 195,
            PLAYER_QUEST_LOG_4_1 = 197,
            PLAYER_QUEST_LOG_4_2 = 198,
            PLAYER_QUEST_LOG_5_1 = 200,
            PLAYER_QUEST_LOG_5_2 = 201,
            PLAYER_QUEST_LOG_6_1 = 203,
            PLAYER_QUEST_LOG_6_2 = 204,
            PLAYER_QUEST_LOG_7_1 = 206,
            PLAYER_QUEST_LOG_7_2 = 207,
            PLAYER_QUEST_LOG_8_1 = 209,
            PLAYER_QUEST_LOG_8_2 = 210,
            PLAYER_QUEST_LOG_9_1 = 212,
            PLAYER_QUEST_LOG_9_2 = 213,
            PLAYER_QUEST_LOG_10_1 = 215,
            PLAYER_QUEST_LOG_10_2 = 216,
            PLAYER_QUEST_LOG_11_1 = 218,
            PLAYER_QUEST_LOG_11_2 = 219,
            PLAYER_QUEST_LOG_12_1 = 221,
            PLAYER_QUEST_LOG_12_2 = 222,
            PLAYER_QUEST_LOG_13_1 = 224,
            PLAYER_QUEST_LOG_13_2 = 225,
            PLAYER_QUEST_LOG_14_1 = 227,
            PLAYER_QUEST_LOG_14_2 = 228,
            PLAYER_QUEST_LOG_15_1 = 230,
            PLAYER_QUEST_LOG_15_2 = 231,
            PLAYER_QUEST_LOG_16_1 = 233,
            PLAYER_QUEST_LOG_16_2 = 234,
            PLAYER_QUEST_LOG_17_1 = 236,
            PLAYER_QUEST_LOG_17_2 = 237,
            PLAYER_QUEST_LOG_18_1 = 239,
            PLAYER_QUEST_LOG_18_2 = 240,
            PLAYER_QUEST_LOG_19_1 = 242,
            PLAYER_QUEST_LOG_19_2 = 243,
            PLAYER_QUEST_LOG_20_1 = 245,
            PLAYER_QUEST_LOG_20_2 = 246,
            PLAYER_VISIBLE_ITEM_1_0 = 248,
            PLAYER_VISIBLE_ITEM_1_PROPERTIES = 256,
            PLAYER_VISIBLE_ITEM_1_CREATOR = 257,
            PLAYER_VISIBLE_ITEM_2_0 = 259,
            PLAYER_VISIBLE_ITEM_2_PROPERTIES = 267,
            PLAYER_VISIBLE_ITEM_2_CREATOR = 268,
            PLAYER_VISIBLE_ITEM_3_0 = 270,
            PLAYER_VISIBLE_ITEM_3_PROPERTIES = 278,
            PLAYER_VISIBLE_ITEM_3_CREATOR = 279,
            PLAYER_VISIBLE_ITEM_4_0 = 281,
            PLAYER_VISIBLE_ITEM_4_PROPERTIES = 289,
            PLAYER_VISIBLE_ITEM_4_CREATOR = 290,
            PLAYER_VISIBLE_ITEM_5_0 = 292,
            PLAYER_VISIBLE_ITEM_5_PROPERTIES = 300,
            PLAYER_VISIBLE_ITEM_5_CREATOR = 301,
            PLAYER_VISIBLE_ITEM_6_0 = 303,
            PLAYER_VISIBLE_ITEM_6_PROPERTIES = 311,
            PLAYER_VISIBLE_ITEM_6_CREATOR = 312,
            PLAYER_VISIBLE_ITEM_7_0 = 314,
            PLAYER_VISIBLE_ITEM_7_PROPERTIES = 322,
            PLAYER_VISIBLE_ITEM_7_CREATOR = 323,
            PLAYER_VISIBLE_ITEM_8_0 = 325,
            PLAYER_VISIBLE_ITEM_8_PROPERTIES = 333,
            PLAYER_VISIBLE_ITEM_8_CREATOR = 334,
            PLAYER_VISIBLE_ITEM_9_0 = 336,
            PLAYER_VISIBLE_ITEM_9_PROPERTIES = 344,
            PLAYER_VISIBLE_ITEM_9_CREATOR = 345,
            PLAYER_VISIBLE_ITEM_10_0 = 347,
            PLAYER_VISIBLE_ITEM_10_PROPERTIES = 355,
            PLAYER_VISIBLE_ITEM_10_CREATOR = 356,
            PLAYER_VISIBLE_ITEM_11_0 = 358,
            PLAYER_VISIBLE_ITEM_11_PROPERTIES = 366,
            PLAYER_VISIBLE_ITEM_11_CREATOR = 367,
            PLAYER_VISIBLE_ITEM_12_0 = 369,
            PLAYER_VISIBLE_ITEM_12_PROPERTIES = 377,
            PLAYER_VISIBLE_ITEM_12_CREATOR = 378,
            PLAYER_VISIBLE_ITEM_13_0 = 380,
            PLAYER_VISIBLE_ITEM_13_PROPERTIES = 388,
            PLAYER_VISIBLE_ITEM_13_CREATOR = 389,
            PLAYER_VISIBLE_ITEM_14_0 = 391,
            PLAYER_VISIBLE_ITEM_14_PROPERTIES = 399,
            PLAYER_VISIBLE_ITEM_14_CREATOR = 400,
            PLAYER_VISIBLE_ITEM_15_0 = 402,
            PLAYER_VISIBLE_ITEM_15_PROPERTIES = 410,
            PLAYER_VISIBLE_ITEM_15_CREATOR = 411,
            PLAYER_VISIBLE_ITEM_16_0 = 413,
            PLAYER_VISIBLE_ITEM_16_PROPERTIES = 421,
            PLAYER_VISIBLE_ITEM_16_CREATOR = 422,
            PLAYER_VISIBLE_ITEM_17_0 = 424,
            PLAYER_VISIBLE_ITEM_17_PROPERTIES = 432,
            PLAYER_VISIBLE_ITEM_17_CREATOR = 433,
            PLAYER_VISIBLE_ITEM_18_0 = 435,
            PLAYER_VISIBLE_ITEM_18_PROPERTIES = 443,
            PLAYER_VISIBLE_ITEM_18_CREATOR = 444,
            PLAYER_VISIBLE_ITEM_19_0 = 446,
            PLAYER_VISIBLE_ITEM_19_PROPERTIES = 454,
            PLAYER_VISIBLE_ITEM_19_CREATOR = 455,
            PLAYER_FIELD_PAD_0 = 457,
            PLAYER_FIELD_INV_SLOT_HEAD = 458,
            PLAYER_FIELD_PACK_SLOT_1 = 504,
            PLAYER_FIELD_BANK_SLOT_1 = 536,
            PLAYER_FIELD_BANKBAG_SLOT_1 = 584,
            PLAYER_FIELD_VENDORBUYBACK_SLOT = 596,
            PLAYER_FARSIGHT = 598,
            PLAYER__FIELD_COMBO_TARGET = 600,
            PLAYER_FIELD_BUYBACK_NPC = 602,
            PLAYER_XP = 604,
            PLAYER_NEXT_LEVEL_XP = 605,
            PLAYER_SKILL_INFO_1_1 = 606,
            PLAYER_CHARACTER_POINTS1 = 990,
            PLAYER_CHARACTER_POINTS2 = 991,
            PLAYER_TRACK_CREATURES = 992,
            PLAYER_TRACK_RESOURCES = 993,
            PLAYER_BLOCK_PERCENTAGE = 994,
            PLAYER_DODGE_PERCENTAGE = 995,
            PLAYER_PARRY_PERCENTAGE = 996,
            PLAYER_CRIT_PERCENTAGE = 997,
            PLAYER_RANGED_CRIT_PERCENTAGE = 998,
            PLAYER_EXPLORED_ZONES_1 = 999,
            PLAYER_REST_STATE_EXPERIENCE = 1063,
            PLAYER_FIELD_COINAGE = 1064,
            PLAYER_FIELD_POSSTAT0 = 1065,
            PLAYER_FIELD_POSSTAT1 = 1066,
            PLAYER_FIELD_POSSTAT2 = 1067,
            PLAYER_FIELD_POSSTAT3 = 1068,
            PLAYER_FIELD_POSSTAT4 = 1069,
            PLAYER_FIELD_NEGSTAT0 = 1070,
            PLAYER_FIELD_NEGSTAT1 = 1071,
            PLAYER_FIELD_NEGSTAT2 = 1072,
            PLAYER_FIELD_NEGSTAT3 = 1073,
            PLAYER_FIELD_NEGSTAT4 = 1074,
            PLAYER_FIELD_RESISTANCEBUFFMODSPOSITIVE = 1075,
            PLAYER_FIELD_RESISTANCEBUFFMODSNEGATIVE = 1082,
            PLAYER_FIELD_MOD_DAMAGE_DONE_POS = 1089,
            PLAYER_FIELD_MOD_DAMAGE_DONE_NEG = 1096,
            PLAYER_FIELD_MOD_DAMAGE_DONE_PCT = 1103,
            PLAYER_FIELD_BYTES = 1110,
            PLAYER_AMMO_ID = 1111,
            PLAYER_FIELD_PVP_MEDALS = 1112,
            PLAYER_FIELD_BUYBACK_ITEM_ID = 1113,
            PLAYER_FIELD_BUYBACK_RANDOM_PROPERTIES_ID = 1114,
            PLAYER_FIELD_BUYBACK_SEED = 1115,
            PLAYER_FIELD_BUYBACK_PRICE = 1116,
            PLAYER_FIELD_BUYBACK_DURABILITY = 1117,
            PLAYER_FIELD_BUYBACK_COUNT = 1118,
            PLAYER_FIELD_SESSION_KILLS = 1119,
            PLAYER_FIELD_YESTERDAY_KILLS = 1120,
            PLAYER_FIELD_LAST_WEEK_KILLS = 1121,
            PLAYER_FIELD_LIFETIME_HONORBALE_KILLS = 1122,
            PLAYER_FIELD_LIFETIME_DISHONORBALE_KILLS = 1123,
            PLAYER_FIELD_YESTERDAY_CONTRIBUTION = 1124,
            PLAYER_FIELD_LAST_WEEK_CONTRIBUTION = 1125,
            PLAYER_FIELD_LAST_WEEK_RANK = 1126,
            PLAYER_FIELD_PADDING = 1127,
            MAX = 1128,
        }
    }
}
