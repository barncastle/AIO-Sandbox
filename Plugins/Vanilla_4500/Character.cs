using System;
using Common.Constants;
using Common.Extensions;
using Common.Interfaces;
using Common.Structs;

namespace Vanilla_4500
{
    public class Character : BaseCharacter
    {
        public override int Build { get; set; } = Sandbox.Instance.Build;

        public override IPacketWriter BuildUpdate()
        {
            MaskSize = ((int)Fields.MAX + 31) / 32;
            FieldData.Clear();
            MaskArray = new byte[MaskSize * 4];

            PacketWriter writer = new PacketWriter(Sandbox.Instance.Opcodes[global::Opcodes.SMSG_UPDATE_OBJECT], "SMSG_UPDATE_OBJECT");
            writer.WriteUInt32(1); // Number of transactions
            writer.WriteUInt8(0);
            writer.WriteUInt8(2); // UpdateType
            writer.WriteUInt64(Guid);
            writer.WriteUInt8(4); // ObjectType, 4 = Player

            writer.WriteUInt32(0);  // MovementFlagMask
            writer.WriteUInt32((uint)Environment.TickCount);
            writer.WriteFloat(Location.X);  // x
            writer.WriteFloat(Location.Y);  // y
            writer.WriteFloat(Location.Z);  // z
            writer.WriteFloat(Location.O);  // w (o)
            writer.WriteFloat(0);
            writer.WriteFloat(2.5f); // WalkSpeed
            writer.WriteFloat(7.0f); // RunSpeed
            writer.WriteFloat(2.5f); // Backwards WalkSpeed
            writer.WriteFloat(4.7222f); // SwimSpeed
            writer.WriteFloat(4.7222f); // Backwards SwimSpeed
            writer.WriteFloat(3.14f); // TurnSpeed

            writer.WriteUInt32(1); // Flags, 1 - Player
            writer.WriteUInt32(1); // AttackCycle
            writer.WriteUInt32(0); // TimerId
            writer.WriteUInt64(0); // VictimGuid

            SetField(Fields.OBJECT_FIELD_GUID, Guid);
            SetField(Fields.OBJECT_FIELD_TYPE, (uint)0x19);
            SetField(Fields.OBJECT_FIELD_ENTRY, 0);
            SetField(Fields.OBJECT_FIELD_SCALE_X, Scale);
            SetField(Fields.OBJECT_FIELD_PADDING, 0);
            SetField(Fields.UNIT_FIELD_TARGET, (ulong)0);
            SetField(Fields.UNIT_FIELD_HEALTH, Health);
            SetField(Fields.UNIT_FIELD_POWER2, 0);
            SetField(Fields.UNIT_FIELD_MAXHEALTH, Health);
            SetField(Fields.UNIT_FIELD_MAXPOWER2, Rage);
            SetField(Fields.UNIT_FIELD_LEVEL, Level);
            SetField(Fields.UNIT_FIELD_BYTES_0, ToUInt32(Race, Class, Gender, PowerType));
            SetField(Fields.UNIT_FIELD_STAT0, Strength);
            SetField(Fields.UNIT_FIELD_STAT1, Agility);
            SetField(Fields.UNIT_FIELD_STAT2, Stamina);
            SetField(Fields.UNIT_FIELD_STAT3, Intellect);
            SetField(Fields.UNIT_FIELD_STAT4, Spirit);
            SetField(Fields.UNIT_FIELD_FLAGS, 0);
            SetField(Fields.UNIT_FIELD_BASE_MANA, Mana);
            SetField(Fields.UNIT_FIELD_DISPLAYID, DisplayId);
            SetField(Fields.UNIT_FIELD_MOUNTDISPLAYID, MountDisplayId);
            SetField(Fields.UNIT_FIELD_BYTES_1, ToUInt32((byte)StandState));
            SetField(Fields.PLAYER_SELECTION, (ulong)0);
            SetField(Fields.PLAYER_BYTES, ToUInt32(Skin, Face, HairStyle, HairColor));
            SetField(Fields.PLAYER_BYTES_2, ToUInt32(0, FacialHair, 0, RestedState));
            SetField(Fields.PLAYER_BYTES_3, ToUInt32(Gender));
            SetField(Fields.PLAYER_XP, 47);
            SetField(Fields.PLAYER_NEXT_LEVEL_XP, 200);

            SetField(Fields.UNIT_FIELD_ATTACKPOWER, 1);
            SetField(Fields.UNIT_FIELD_ATTACK_POWER_MODS, 0);
            SetField(Fields.UNIT_FIELD_RANGEDATTACKPOWER, 1);
            SetField(Fields.UNIT_FIELD_RANGED_ATTACK_POWER_MODS, 0);

            for (int i = 0; i < 32; i++)
                SetField(Fields.PLAYER_EXPLORED_ZONES_1 + i, 0xFFFFFFFF);

            // FillInPartialObjectData
            writer.WriteUInt8(MaskSize); // UpdateMaskBlocks
            writer.Write(MaskArray);
            foreach (var kvp in FieldData)
                writer.Write(kvp.Value); // Data

            return writer;
        }

        public override IPacketWriter BuildMessage(string text)
        {
            PacketWriter message = new PacketWriter(Sandbox.Instance.Opcodes[global::Opcodes.SMSG_MESSAGECHAT], "SMSG_MESSAGECHAT");
            return this.BuildMessage(message, text);
        }

        public override void Teleport(float x, float y, float z, float o, uint map, ref IWorldManager manager)
        {
            IsTeleporting = true;

            if (Location.Map == map)
            {
                PacketWriter movementStatus = new PacketWriter(Sandbox.Instance.Opcodes[global::Opcodes.MSG_MOVE_TELEPORT_ACK], "MSG_MOVE_TELEPORT_ACK");
                movementStatus.WriteUInt64(Guid);
                movementStatus.WriteUInt64(0); // Flags
                movementStatus.WriteFloat(x);
                movementStatus.WriteFloat(y);
                movementStatus.WriteFloat(z);
                movementStatus.WriteFloat(o);
                movementStatus.WriteFloat(0);
                manager.Send(movementStatus);
            }
            else
            {
                // Loading screen
                PacketWriter transferPending = new PacketWriter(Sandbox.Instance.Opcodes[global::Opcodes.SMSG_TRANSFER_PENDING], "SMSG_TRANSFER_PENDING");
                transferPending.WriteUInt32(map);
                manager.Send(transferPending);

                // New world transfer
                PacketWriter newWorld = new PacketWriter(Sandbox.Instance.Opcodes[global::Opcodes.SMSG_NEW_WORLD], "SMSG_NEW_WORLD");
                newWorld.WriteUInt32(map);
                newWorld.WriteFloat(x);
                newWorld.WriteFloat(y);
                newWorld.WriteFloat(z);
                newWorld.WriteFloat(o);
                manager.Send(newWorld);
            }

            System.Threading.Thread.Sleep(150); // Pause to factor unsent packets

            Location = new Location(x, y, z, o, map);
            manager.Send(BuildUpdate());

            IsTeleporting = false;
        }

        public override IPacketWriter BuildForceSpeed(float modifier, SpeedType type = SpeedType.Run)
        {
            var opcode = type == SpeedType.Swim ? global::Opcodes.SMSG_FORCE_SWIM_SPEED_CHANGE : global::Opcodes.SMSG_FORCE_SPEED_CHANGE;
            PacketWriter writer = new PacketWriter(Sandbox.Instance.Opcodes[opcode], opcode.ToString());
            writer.WriteUInt64(Guid);
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
            PLAYER_VISIBLE_ITEM_1_CREATOR = 248,
            PLAYER_VISIBLE_ITEM_1_0 = 250,
            PLAYER_VISIBLE_ITEM_1_PROPERTIES = 258,
            PLAYER_VISIBLE_ITEM_1_PAD = 259,
            PLAYER_VISIBLE_ITEM_2_CREATOR = 260,
            PLAYER_VISIBLE_ITEM_2_0 = 262,
            PLAYER_VISIBLE_ITEM_2_PROPERTIES = 270,
            PLAYER_VISIBLE_ITEM_2_PAD = 271,
            PLAYER_VISIBLE_ITEM_3_CREATOR = 272,
            PLAYER_VISIBLE_ITEM_3_0 = 274,
            PLAYER_VISIBLE_ITEM_3_PROPERTIES = 282,
            PLAYER_VISIBLE_ITEM_3_PAD = 283,
            PLAYER_VISIBLE_ITEM_4_CREATOR = 284,
            PLAYER_VISIBLE_ITEM_4_0 = 286,
            PLAYER_VISIBLE_ITEM_4_PROPERTIES = 294,
            PLAYER_VISIBLE_ITEM_4_PAD = 295,
            PLAYER_VISIBLE_ITEM_5_CREATOR = 296,
            PLAYER_VISIBLE_ITEM_5_0 = 298,
            PLAYER_VISIBLE_ITEM_5_PROPERTIES = 306,
            PLAYER_VISIBLE_ITEM_5_PAD = 307,
            PLAYER_VISIBLE_ITEM_6_CREATOR = 308,
            PLAYER_VISIBLE_ITEM_6_0 = 310,
            PLAYER_VISIBLE_ITEM_6_PROPERTIES = 318,
            PLAYER_VISIBLE_ITEM_6_PAD = 319,
            PLAYER_VISIBLE_ITEM_7_CREATOR = 320,
            PLAYER_VISIBLE_ITEM_7_0 = 322,
            PLAYER_VISIBLE_ITEM_7_PROPERTIES = 330,
            PLAYER_VISIBLE_ITEM_7_PAD = 331,
            PLAYER_VISIBLE_ITEM_8_CREATOR = 332,
            PLAYER_VISIBLE_ITEM_8_0 = 334,
            PLAYER_VISIBLE_ITEM_8_PROPERTIES = 342,
            PLAYER_VISIBLE_ITEM_8_PAD = 343,
            PLAYER_VISIBLE_ITEM_9_CREATOR = 344,
            PLAYER_VISIBLE_ITEM_9_0 = 346,
            PLAYER_VISIBLE_ITEM_9_PROPERTIES = 354,
            PLAYER_VISIBLE_ITEM_9_PAD = 355,
            PLAYER_VISIBLE_ITEM_10_CREATOR = 356,
            PLAYER_VISIBLE_ITEM_10_0 = 358,
            PLAYER_VISIBLE_ITEM_10_PROPERTIES = 366,
            PLAYER_VISIBLE_ITEM_10_PAD = 367,
            PLAYER_VISIBLE_ITEM_11_CREATOR = 368,
            PLAYER_VISIBLE_ITEM_11_0 = 370,
            PLAYER_VISIBLE_ITEM_11_PROPERTIES = 378,
            PLAYER_VISIBLE_ITEM_11_PAD = 379,
            PLAYER_VISIBLE_ITEM_12_CREATOR = 380,
            PLAYER_VISIBLE_ITEM_12_0 = 382,
            PLAYER_VISIBLE_ITEM_12_PROPERTIES = 390,
            PLAYER_VISIBLE_ITEM_12_PAD = 391,
            PLAYER_VISIBLE_ITEM_13_CREATOR = 392,
            PLAYER_VISIBLE_ITEM_13_0 = 394,
            PLAYER_VISIBLE_ITEM_13_PROPERTIES = 402,
            PLAYER_VISIBLE_ITEM_13_PAD = 403,
            PLAYER_VISIBLE_ITEM_14_CREATOR = 404,
            PLAYER_VISIBLE_ITEM_14_0 = 406,
            PLAYER_VISIBLE_ITEM_14_PROPERTIES = 414,
            PLAYER_VISIBLE_ITEM_14_PAD = 415,
            PLAYER_VISIBLE_ITEM_15_CREATOR = 416,
            PLAYER_VISIBLE_ITEM_15_0 = 418,
            PLAYER_VISIBLE_ITEM_15_PROPERTIES = 426,
            PLAYER_VISIBLE_ITEM_15_PAD = 427,
            PLAYER_VISIBLE_ITEM_16_CREATOR = 428,
            PLAYER_VISIBLE_ITEM_16_0 = 430,
            PLAYER_VISIBLE_ITEM_16_PROPERTIES = 438,
            PLAYER_VISIBLE_ITEM_16_PAD = 439,
            PLAYER_VISIBLE_ITEM_17_CREATOR = 440,
            PLAYER_VISIBLE_ITEM_17_0 = 442,
            PLAYER_VISIBLE_ITEM_17_PROPERTIES = 450,
            PLAYER_VISIBLE_ITEM_17_PAD = 451,
            PLAYER_VISIBLE_ITEM_18_CREATOR = 452,
            PLAYER_VISIBLE_ITEM_18_0 = 454,
            PLAYER_VISIBLE_ITEM_18_PROPERTIES = 462,
            PLAYER_VISIBLE_ITEM_18_PAD = 463,
            PLAYER_VISIBLE_ITEM_19_CREATOR = 464,
            PLAYER_VISIBLE_ITEM_19_0 = 466,
            PLAYER_VISIBLE_ITEM_19_PROPERTIES = 474,
            PLAYER_VISIBLE_ITEM_19_PAD = 475,
            PLAYER_FIELD_INV_SLOT_HEAD = 476,
            PLAYER_FIELD_PACK_SLOT_1 = 522,
            PLAYER_FIELD_BANK_SLOT_1 = 554,
            PLAYER_FIELD_BANKBAG_SLOT_1 = 602,
            PLAYER_FIELD_VENDORBUYBACK_SLOT = 614,
            PLAYER_FARSIGHT = 616,
            PLAYER__FIELD_COMBO_TARGET = 618,
            PLAYER_FIELD_BUYBACK_NPC = 620,
            PLAYER_XP = 622,
            PLAYER_NEXT_LEVEL_XP = 623,
            PLAYER_SKILL_INFO_1_1 = 624,
            PLAYER_CHARACTER_POINTS1 = 1008,
            PLAYER_CHARACTER_POINTS2 = 1009,
            PLAYER_TRACK_CREATURES = 1010,
            PLAYER_TRACK_RESOURCES = 1011,
            PLAYER_BLOCK_PERCENTAGE = 1012,
            PLAYER_DODGE_PERCENTAGE = 1013,
            PLAYER_PARRY_PERCENTAGE = 1014,
            PLAYER_CRIT_PERCENTAGE = 1015,
            PLAYER_RANGED_CRIT_PERCENTAGE = 1016,
            PLAYER_EXPLORED_ZONES_1 = 1017,
            PLAYER_REST_STATE_EXPERIENCE = 1081,
            PLAYER_FIELD_COINAGE = 1082,
            PLAYER_FIELD_POSSTAT0 = 1083,
            PLAYER_FIELD_POSSTAT1 = 1084,
            PLAYER_FIELD_POSSTAT2 = 1085,
            PLAYER_FIELD_POSSTAT3 = 1086,
            PLAYER_FIELD_POSSTAT4 = 1087,
            PLAYER_FIELD_NEGSTAT0 = 1088,
            PLAYER_FIELD_NEGSTAT1 = 1089,
            PLAYER_FIELD_NEGSTAT2 = 1090,
            PLAYER_FIELD_NEGSTAT3 = 1091,
            PLAYER_FIELD_NEGSTAT4 = 1092,
            PLAYER_FIELD_RESISTANCEBUFFMODSPOSITIVE = 1093,
            PLAYER_FIELD_RESISTANCEBUFFMODSNEGATIVE = 1100,
            PLAYER_FIELD_MOD_DAMAGE_DONE_POS = 1107,
            PLAYER_FIELD_MOD_DAMAGE_DONE_NEG = 1114,
            PLAYER_FIELD_MOD_DAMAGE_DONE_PCT = 1121,
            PLAYER_FIELD_BYTES = 1128,
            PLAYER_AMMO_ID = 1129,
            PLAYER_SELF_RES_SPELL = 1130,
            PLAYER_FIELD_PVP_MEDALS = 1131,
            PLAYER_FIELD_BUYBACK_ITEM_ID = 1132,
            PLAYER_FIELD_BUYBACK_RANDOM_PROPERTIES_ID = 1133,
            PLAYER_FIELD_BUYBACK_SEED = 1134,
            PLAYER_FIELD_BUYBACK_PRICE = 1135,
            PLAYER_FIELD_BUYBACK_DURABILITY = 1136,
            PLAYER_FIELD_BUYBACK_COUNT = 1137,
            PLAYER_FIELD_SESSION_KILLS = 1138,
            PLAYER_FIELD_YESTERDAY_KILLS = 1139,
            PLAYER_FIELD_LAST_WEEK_KILLS = 1140,
            PLAYER_FIELD_THIS_WEEK_KILLS = 1141,
            PLAYER_FIELD_THIS_WEEK_CONTRIBUTION = 1142,
            PLAYER_FIELD_LIFETIME_HONORBALE_KILLS = 1143,
            PLAYER_FIELD_LIFETIME_DISHONORBALE_KILLS = 1144,
            PLAYER_FIELD_YESTERDAY_CONTRIBUTION = 1145,
            PLAYER_FIELD_LAST_WEEK_CONTRIBUTION = 1146,
            PLAYER_FIELD_LAST_WEEK_RANK = 1147,
            PLAYER_FIELD_BYTES2 = 1148,
            PLAYER_FIELD_PADDING = 1149,
            MAX = 1150,
        }
    }
}
