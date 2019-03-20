using System;
using Common.Extensions;
using Common.Interfaces;
using Common.Structs;

namespace TBC_Alpha_5610
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
            writer.WriteUInt8(3); // UpdateType
            writer.WriteUInt8(0xFF); // ??
            writer.WriteUInt64(Guid);
            writer.WriteUInt8(4); // ObjectType, 4 = Player
            writer.WriteUInt8(0x71);

            writer.WriteUInt32(0);  // MovementFlagMask
            writer.WriteUInt32((uint)Environment.TickCount);
            writer.WriteFloat(Location.X);  // x
            writer.WriteFloat(Location.Y);  // y
            writer.WriteFloat(Location.Z);  // z
            writer.WriteFloat(Location.O);  // w (o)
            writer.WriteUInt32(0); // Falltime

            writer.WriteFloat(0f); // Transport vector4??
            writer.WriteFloat(1f);
            writer.WriteFloat(0f);
            writer.WriteFloat(0f);

            writer.WriteFloat(2.5f); // WalkSpeed
            writer.WriteFloat(7.5f); // RunSpeed
            writer.WriteFloat(2.5f); // Backwards WalkSpeed
            writer.WriteFloat(4.722222f); // FlySpeed
            writer.WriteFloat(2.5f); // Backwards FlySpeed
            writer.WriteFloat(3.141593f); // TurnSpeed

            writer.WriteUInt32(1);

            SetField(Fields.OBJECT_FIELD_GUID, Guid);
            SetField(Fields.OBJECT_FIELD_TYPE, (uint)0x19);
            SetField(Fields.OBJECT_FIELD_SCALE_X, Scale);

            SetField(Fields.UNIT_FIELD_HEALTH, Health);
            SetField(Fields.UNIT_FIELD_POWER1, 0);
            SetField(Fields.UNIT_FIELD_MAXHEALTH, Health);
            SetField(Fields.UNIT_FIELD_MAXPOWER1, Rage);

            SetField(Fields.UNIT_FIELD_LEVEL, Level);
            SetField(Fields.UNIT_FIELD_FACTIONTEMPLATE, 0);
            SetField(Fields.UNIT_FIELD_BYTES_0, BitConverter.ToUInt32(new byte[] { Race, Class, Gender, PowerType }, 0));
            SetField(Fields.UNIT_FIELD_FLAGS, 1);

            SetField(Fields.UNIT_FIELD_PETNUMBER, 0);
            SetField(Fields.UNIT_FIELD_PET_NAME_TIMESTAMP, 0);
            SetField(Fields.UNIT_FIELD_PETEXPERIENCE, 0);
            SetField(Fields.UNIT_FIELD_PETNEXTLEVELEXP, 0);
            SetField(Fields.UNIT_DYNAMIC_FLAGS, 0);
            SetField(Fields.UNIT_CHANNEL_SPELL, 0);
            SetField(Fields.UNIT_MOD_CAST_SPEED, 0);

            SetField(Fields.UNIT_NPC_FLAGS, 0);
            SetField(Fields.UNIT_NPC_EMOTESTATE, 0);

            SetField(Fields.UNIT_FIELD_STAT0, Strength);
            SetField(Fields.UNIT_FIELD_STAT1, Agility);
            SetField(Fields.UNIT_FIELD_STAT2, Stamina);
            SetField(Fields.UNIT_FIELD_STAT3, Intellect);
            SetField(Fields.UNIT_FIELD_STAT4, Spirit);

            SetField(Fields.UNIT_FIELD_BASE_MANA, Mana);
            SetField(Fields.UNIT_FIELD_BASE_HEALTH, Health);

            SetField(Fields.UNIT_FIELD_BYTES_2, 0);
            SetField(Fields.UNIT_FIELD_ATTACK_POWER, 0);
            SetField(Fields.UNIT_FIELD_ATTACK_POWER_MODS, 0);
            SetField(Fields.UNIT_FIELD_ATTACK_POWER_MULTIPLIER, 0);
            SetField(Fields.UNIT_FIELD_RANGED_ATTACK_POWER, 0);

            for (int i = 0; i < 4; i++)
                SetField(Fields.UNIT_FIELD_POWER_COST_MODIFIER + 2 + i, 0);
            SetField(Fields.UNIT_FIELD_POWER_COST_MULTIPLIER + 3, 0);

            SetField(Fields.PLAYER_FIELD_RESISTANCEBUFFMODSNEGATIVE + 3, 0);
            SetField((Fields)1353, 0);

            SetField(Fields.UNIT_FIELD_DISPLAYID, DisplayId);
            SetField(Fields.UNIT_FIELD_MOUNTDISPLAYID, MountDisplayId);
            SetField(Fields.UNIT_FIELD_BYTES_1, BitConverter.ToUInt32(new byte[] { (byte)StandState, 0, 0, 0 }, 0));
            SetField(Fields.PLAYER_BYTES, BitConverter.ToUInt32(new byte[] { Skin, Face, HairStyle, HairColor }, 0));
            SetField(Fields.PLAYER_BYTES_2, BitConverter.ToUInt32(new byte[] { 0, FacialHair, 0, RestedState }, 0));
            SetField(Fields.PLAYER_BYTES_3, (uint)Gender);
            SetField(Fields.PLAYER_XP, 47);
            SetField(Fields.PLAYER_NEXT_LEVEL_XP, 200);

            // FillInPartialObjectData
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

        public override IPacketWriter BuildForceSpeed(float modifier, bool swim = false)
        {
            var opcode = swim ? global::Opcodes.SMSG_FORCE_SWIM_SPEED_CHANGE : global::Opcodes.SMSG_FORCE_SPEED_CHANGE;
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
            PLAYER_VISIBLE_ITEM_1_PROPERTIES = 272,
            PLAYER_VISIBLE_ITEM_1_PAD = 273,
            PLAYER_VISIBLE_ITEM_2_CREATOR = 274,
            PLAYER_VISIBLE_ITEM_2_0 = 276,
            PLAYER_VISIBLE_ITEM_2_PROPERTIES = 288,
            PLAYER_VISIBLE_ITEM_2_PAD = 289,
            PLAYER_VISIBLE_ITEM_3_CREATOR = 290,
            PLAYER_VISIBLE_ITEM_3_0 = 292,
            PLAYER_VISIBLE_ITEM_3_PROPERTIES = 304,
            PLAYER_VISIBLE_ITEM_3_PAD = 305,
            PLAYER_VISIBLE_ITEM_4_CREATOR = 306,
            PLAYER_VISIBLE_ITEM_4_0 = 308,
            PLAYER_VISIBLE_ITEM_4_PROPERTIES = 320,
            PLAYER_VISIBLE_ITEM_4_PAD = 321,
            PLAYER_VISIBLE_ITEM_5_CREATOR = 322,
            PLAYER_VISIBLE_ITEM_5_0 = 324,
            PLAYER_VISIBLE_ITEM_5_PROPERTIES = 336,
            PLAYER_VISIBLE_ITEM_5_PAD = 337,
            PLAYER_VISIBLE_ITEM_6_CREATOR = 338,
            PLAYER_VISIBLE_ITEM_6_0 = 340,
            PLAYER_VISIBLE_ITEM_6_PROPERTIES = 352,
            PLAYER_VISIBLE_ITEM_6_PAD = 353,
            PLAYER_VISIBLE_ITEM_7_CREATOR = 354,
            PLAYER_VISIBLE_ITEM_7_0 = 356,
            PLAYER_VISIBLE_ITEM_7_PROPERTIES = 368,
            PLAYER_VISIBLE_ITEM_7_PAD = 369,
            PLAYER_VISIBLE_ITEM_8_CREATOR = 370,
            PLAYER_VISIBLE_ITEM_8_0 = 372,
            PLAYER_VISIBLE_ITEM_8_PROPERTIES = 384,
            PLAYER_VISIBLE_ITEM_8_PAD = 385,
            PLAYER_VISIBLE_ITEM_9_CREATOR = 386,
            PLAYER_VISIBLE_ITEM_9_0 = 388,
            PLAYER_VISIBLE_ITEM_9_PROPERTIES = 400,
            PLAYER_VISIBLE_ITEM_9_PAD = 401,
            PLAYER_VISIBLE_ITEM_10_CREATOR = 402,
            PLAYER_VISIBLE_ITEM_10_0 = 404,
            PLAYER_VISIBLE_ITEM_10_PROPERTIES = 416,
            PLAYER_VISIBLE_ITEM_10_PAD = 417,
            PLAYER_VISIBLE_ITEM_11_CREATOR = 418,
            PLAYER_VISIBLE_ITEM_11_0 = 420,
            PLAYER_VISIBLE_ITEM_11_PROPERTIES = 432,
            PLAYER_VISIBLE_ITEM_11_PAD = 433,
            PLAYER_VISIBLE_ITEM_12_CREATOR = 434,
            PLAYER_VISIBLE_ITEM_12_0 = 436,
            PLAYER_VISIBLE_ITEM_12_PROPERTIES = 448,
            PLAYER_VISIBLE_ITEM_12_PAD = 449,
            PLAYER_VISIBLE_ITEM_13_CREATOR = 450,
            PLAYER_VISIBLE_ITEM_13_0 = 452,
            PLAYER_VISIBLE_ITEM_13_PROPERTIES = 464,
            PLAYER_VISIBLE_ITEM_13_PAD = 465,
            PLAYER_VISIBLE_ITEM_14_CREATOR = 466,
            PLAYER_VISIBLE_ITEM_14_0 = 468,
            PLAYER_VISIBLE_ITEM_14_PROPERTIES = 480,
            PLAYER_VISIBLE_ITEM_14_PAD = 481,
            PLAYER_VISIBLE_ITEM_15_CREATOR = 482,
            PLAYER_VISIBLE_ITEM_15_0 = 484,
            PLAYER_VISIBLE_ITEM_15_PROPERTIES = 496,
            PLAYER_VISIBLE_ITEM_15_PAD = 497,
            PLAYER_VISIBLE_ITEM_16_CREATOR = 498,
            PLAYER_VISIBLE_ITEM_16_0 = 500,
            PLAYER_VISIBLE_ITEM_16_PROPERTIES = 512,
            PLAYER_VISIBLE_ITEM_16_PAD = 513,
            PLAYER_VISIBLE_ITEM_17_CREATOR = 514,
            PLAYER_VISIBLE_ITEM_17_0 = 516,
            PLAYER_VISIBLE_ITEM_17_PROPERTIES = 528,
            PLAYER_VISIBLE_ITEM_17_PAD = 529,
            PLAYER_VISIBLE_ITEM_18_CREATOR = 530,
            PLAYER_VISIBLE_ITEM_18_0 = 532,
            PLAYER_VISIBLE_ITEM_18_PROPERTIES = 544,
            PLAYER_VISIBLE_ITEM_18_PAD = 545,
            PLAYER_VISIBLE_ITEM_19_CREATOR = 546,
            PLAYER_VISIBLE_ITEM_19_0 = 548,
            PLAYER_VISIBLE_ITEM_19_PROPERTIES = 560,
            PLAYER_VISIBLE_ITEM_19_PAD = 561,
            PLAYER_FIELD_INV_SLOT_HEAD = 562,
            PLAYER_FIELD_PACK_SLOT_1 = 608,
            PLAYER_FIELD_BANK_SLOT_1 = 640,
            PLAYER_FIELD_BANKBAG_SLOT_1 = 688,
            PLAYER_FIELD_VENDORBUYBACK_SLOT_1 = 700,
            PLAYER_FIELD_KEYRING_SLOT_1 = 724,
            PLAYER_FARSIGHT = 788,
            PLAYER_FIELD_COMBO_TARGET = 790,
            PLAYER_XP = 792,
            PLAYER_NEXT_LEVEL_XP = 793,
            PLAYER_SKILL_INFO_1_1 = 794,
            PLAYER_CHARACTER_POINTS1 = 1178,
            PLAYER_CHARACTER_POINTS2 = 1179,
            PLAYER_TRACK_CREATURES = 1180,
            PLAYER_TRACK_RESOURCES = 1181,
            PLAYER_BLOCK_PERCENTAGE = 1182,
            PLAYER_DODGE_PERCENTAGE = 1183,
            PLAYER_PARRY_PERCENTAGE = 1184,
            PLAYER_CRIT_PERCENTAGE = 1185,
            PLAYER_RANGED_CRIT_PERCENTAGE = 1186,
            PLAYER_EXPLORED_ZONES_1 = 1187,
            PLAYER_REST_STATE_EXPERIENCE = 1251,
            PLAYER_FIELD_COINAGE = 1252,
            PLAYER_FIELD_POSSTAT0 = 1253,
            PLAYER_FIELD_POSSTAT1 = 1254,
            PLAYER_FIELD_POSSTAT2 = 1255,
            PLAYER_FIELD_POSSTAT3 = 1256,
            PLAYER_FIELD_POSSTAT4 = 1257,
            PLAYER_FIELD_NEGSTAT0 = 1258,
            PLAYER_FIELD_NEGSTAT1 = 1259,
            PLAYER_FIELD_NEGSTAT2 = 1260,
            PLAYER_FIELD_NEGSTAT3 = 1261,
            PLAYER_FIELD_NEGSTAT4 = 1262,
            PLAYER_FIELD_RESISTANCEBUFFMODSPOSITIVE = 1263,
            PLAYER_FIELD_RESISTANCEBUFFMODSNEGATIVE = 1270,
            PLAYER_FIELD_MOD_DAMAGE_DONE_POS = 1277,
            PLAYER_FIELD_MOD_DAMAGE_DONE_NEG = 1284,
            PLAYER_FIELD_MOD_DAMAGE_DONE_PCT = 1291,
            PLAYER_FIELD_BYTES = 1298,
            PLAYER_AMMO_ID = 1299,
            PLAYER_SELF_RES_SPELL = 1300,
            PLAYER_FIELD_PVP_MEDALS = 1301,
            PLAYER_FIELD_BUYBACK_PRICE_1 = 1302,
            PLAYER_FIELD_BUYBACK_TIMESTAMP_1 = 1314,
            PLAYER_FIELD_SESSION_KILLS = 1326,
            PLAYER_FIELD_YESTERDAY_KILLS = 1327,
            PLAYER_FIELD_LAST_WEEK_KILLS = 1328,
            PLAYER_FIELD_THIS_WEEK_KILLS = 1329,
            PLAYER_FIELD_THIS_WEEK_CONTRIBUTION = 1330,
            PLAYER_FIELD_LIFETIME_HONORBALE_KILLS = 1331,
            PLAYER_FIELD_LIFETIME_DISHONORBALE_KILLS = 1332,
            PLAYER_FIELD_YESTERDAY_CONTRIBUTION = 1333,
            PLAYER_FIELD_LAST_WEEK_CONTRIBUTION = 1334,
            PLAYER_FIELD_LAST_WEEK_RANK = 1335,
            PLAYER_FIELD_BYTES2 = 1336,
            PLAYER_FIELD_WATCHED_FACTION_INDEX = 1337,
            PLAYER_FIELD_COMBAT_RATING_1 = 1338,
            PLAYER_FIELD_ARENA_TEAM_INFO_1_1 = 1358,
            PLAYER_FIELD_HONOR_CURRENCY = 1376,
            PLAYER_FIELD_ARENA_CURRENCY = 1377,
            MAX = 1378,
        }
    }
}
