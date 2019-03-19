using System;
using Common.Extensions;
using Common.Interfaces;
using Common.Structs;

namespace Vanilla_4735
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
            SetField(Fields.UNIT_FIELD_BYTES_0, BitConverter.ToUInt32(new byte[] { Race, Class, Gender, PowerType }, 0));
            SetField(Fields.UNIT_FIELD_STAT0, Strength);
            SetField(Fields.UNIT_FIELD_STAT1, Agility);
            SetField(Fields.UNIT_FIELD_STAT2, Stamina);
            SetField(Fields.UNIT_FIELD_STAT3, Intellect);
            SetField(Fields.UNIT_FIELD_STAT4, Spirit);
            SetField(Fields.UNIT_FIELD_FLAGS, 0);
            SetField(Fields.UNIT_FIELD_BASE_MANA, Mana);
            SetField(Fields.UNIT_FIELD_DISPLAYID, DisplayId);
            SetField(Fields.UNIT_FIELD_MOUNTDISPLAYID, MountDisplayId);
            SetField(Fields.UNIT_FIELD_BYTES_1, BitConverter.ToUInt32(new byte[] { (byte)StandState, 0, 0, 0 }, 0));
            SetField(Fields.PLAYER_SELECTION, (ulong)0);
            SetField(Fields.PLAYER_BYTES, BitConverter.ToUInt32(new byte[] { Skin, Face, HairStyle, HairColor }, 0));
            SetField(Fields.PLAYER_BYTES_2, BitConverter.ToUInt32(new byte[] { 0, FacialHair, 0, RestedState }, 0));
            SetField(Fields.PLAYER_BYTES_3, (uint)Gender);
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
            writer.WriteBytes(MaskArray);
            foreach (var kvp in FieldData)
                writer.WriteBytes(kvp.Value); // Data

            return writer;
        }

        public override IPacketWriter BuildMessage(string text)
        {
            PacketWriter message = new PacketWriter(Sandbox.Instance.Opcodes[global::Opcodes.SMSG_MESSAGECHAT], "SMSG_MESSAGECHAT");
            return this.BuildMessage(message, text, Sandbox.Instance.Build);
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
            UNIT_FIELD_AURAFLAGS = 111,
            UNIT_FIELD_AURALEVELS = 119,
            UNIT_FIELD_AURAAPPLICATIONS = 131,
            UNIT_FIELD_AURASTATE = 143,
            UNIT_FIELD_BASEATTACKTIME = 144,
            UNIT_FIELD_RANGEDATTACKTIME = 146,
            UNIT_FIELD_BOUNDINGRADIUS = 147,
            UNIT_FIELD_COMBATREACH = 148,
            UNIT_FIELD_DISPLAYID = 149,
            UNIT_FIELD_NATIVEDISPLAYID = 150,
            UNIT_FIELD_MOUNTDISPLAYID = 151,
            UNIT_FIELD_MINDAMAGE = 152,
            UNIT_FIELD_MAXDAMAGE = 153,
            UNIT_FIELD_MINOFFHANDDAMAGE = 154,
            UNIT_FIELD_MAXOFFHANDDAMAGE = 155,
            UNIT_FIELD_BYTES_1 = 156,
            UNIT_FIELD_PETNUMBER = 157,
            UNIT_FIELD_PET_NAME_TIMESTAMP = 158,
            UNIT_FIELD_PETEXPERIENCE = 159,
            UNIT_FIELD_PETNEXTLEVELEXP = 160,
            UNIT_DYNAMIC_FLAGS = 161,
            UNIT_CHANNEL_SPELL = 162,
            UNIT_MOD_CAST_SPEED = 163,
            UNIT_CREATED_BY_SPELL = 164,
            UNIT_NPC_FLAGS = 165,
            UNIT_NPC_EMOTESTATE = 166,
            UNIT_TRAINING_POINTS = 167,
            UNIT_FIELD_STAT0 = 168,
            UNIT_FIELD_STAT1 = 169,
            UNIT_FIELD_STAT2 = 170,
            UNIT_FIELD_STAT3 = 171,
            UNIT_FIELD_STAT4 = 172,
            UNIT_FIELD_RESISTANCES = 173,
            UNIT_FIELD_ATTACKPOWER = 180,
            UNIT_FIELD_BASE_MANA = 181,
            UNIT_FIELD_BASE_HEALTH = 182,
            UNIT_FIELD_ATTACK_POWER_MODS = 183,
            UNIT_FIELD_BYTES_2 = 184,
            UNIT_FIELD_RANGEDATTACKPOWER = 185,
            UNIT_FIELD_RANGED_ATTACK_POWER_MODS = 186,
            UNIT_FIELD_MINRANGEDDAMAGE = 187,
            UNIT_FIELD_MAXRANGEDDAMAGE = 188,
            UNIT_FIELD_POWER_COST_MODIFIER = 189,
            UNIT_FIELD_POWER_COST_MULTIPLIER = 196,
            UNIT_FIELD_PADDING = 203,
            UNIT_END = 204,
            PLAYER_SELECTION = 204,
            PLAYER_DUEL_ARBITER = 206,
            PLAYER_FLAGS = 208,
            PLAYER_GUILDID = 209,
            PLAYER_GUILDRANK = 210,
            PLAYER_BYTES = 211,
            PLAYER_BYTES_2 = 212,
            PLAYER_BYTES_3 = 213,
            PLAYER_DUEL_TEAM = 214,
            PLAYER_GUILD_TIMESTAMP = 215,
            PLAYER_QUEST_LOG_1_1 = 216,
            PLAYER_QUEST_LOG_1_2 = 217,
            PLAYER_QUEST_LOG_2_1 = 219,
            PLAYER_QUEST_LOG_2_2 = 220,
            PLAYER_QUEST_LOG_3_1 = 222,
            PLAYER_QUEST_LOG_3_2 = 223,
            PLAYER_QUEST_LOG_4_1 = 225,
            PLAYER_QUEST_LOG_4_2 = 226,
            PLAYER_QUEST_LOG_5_1 = 228,
            PLAYER_QUEST_LOG_5_2 = 229,
            PLAYER_QUEST_LOG_6_1 = 231,
            PLAYER_QUEST_LOG_6_2 = 232,
            PLAYER_QUEST_LOG_7_1 = 234,
            PLAYER_QUEST_LOG_7_2 = 235,
            PLAYER_QUEST_LOG_8_1 = 237,
            PLAYER_QUEST_LOG_8_2 = 238,
            PLAYER_QUEST_LOG_9_1 = 240,
            PLAYER_QUEST_LOG_9_2 = 241,
            PLAYER_QUEST_LOG_10_1 = 243,
            PLAYER_QUEST_LOG_10_2 = 244,
            PLAYER_QUEST_LOG_11_1 = 246,
            PLAYER_QUEST_LOG_11_2 = 247,
            PLAYER_QUEST_LOG_12_1 = 249,
            PLAYER_QUEST_LOG_12_2 = 250,
            PLAYER_QUEST_LOG_13_1 = 252,
            PLAYER_QUEST_LOG_13_2 = 253,
            PLAYER_QUEST_LOG_14_1 = 255,
            PLAYER_QUEST_LOG_14_2 = 256,
            PLAYER_QUEST_LOG_15_1 = 258,
            PLAYER_QUEST_LOG_15_2 = 259,
            PLAYER_QUEST_LOG_16_1 = 261,
            PLAYER_QUEST_LOG_16_2 = 262,
            PLAYER_QUEST_LOG_17_1 = 264,
            PLAYER_QUEST_LOG_17_2 = 265,
            PLAYER_QUEST_LOG_18_1 = 267,
            PLAYER_QUEST_LOG_18_2 = 268,
            PLAYER_QUEST_LOG_19_1 = 270,
            PLAYER_QUEST_LOG_19_2 = 271,
            PLAYER_QUEST_LOG_20_1 = 273,
            PLAYER_QUEST_LOG_20_2 = 274,
            PLAYER_VISIBLE_ITEM_1_CREATOR = 276,
            PLAYER_VISIBLE_ITEM_1_0 = 278,
            PLAYER_VISIBLE_ITEM_1_PROPERTIES = 286,
            PLAYER_VISIBLE_ITEM_1_PAD = 287,
            PLAYER_VISIBLE_ITEM_2_CREATOR = 288,
            PLAYER_VISIBLE_ITEM_2_0 = 290,
            PLAYER_VISIBLE_ITEM_2_PROPERTIES = 298,
            PLAYER_VISIBLE_ITEM_2_PAD = 299,
            PLAYER_VISIBLE_ITEM_3_CREATOR = 300,
            PLAYER_VISIBLE_ITEM_3_0 = 302,
            PLAYER_VISIBLE_ITEM_3_PROPERTIES = 310,
            PLAYER_VISIBLE_ITEM_3_PAD = 311,
            PLAYER_VISIBLE_ITEM_4_CREATOR = 312,
            PLAYER_VISIBLE_ITEM_4_0 = 314,
            PLAYER_VISIBLE_ITEM_4_PROPERTIES = 322,
            PLAYER_VISIBLE_ITEM_4_PAD = 323,
            PLAYER_VISIBLE_ITEM_5_CREATOR = 324,
            PLAYER_VISIBLE_ITEM_5_0 = 326,
            PLAYER_VISIBLE_ITEM_5_PROPERTIES = 334,
            PLAYER_VISIBLE_ITEM_5_PAD = 335,
            PLAYER_VISIBLE_ITEM_6_CREATOR = 336,
            PLAYER_VISIBLE_ITEM_6_0 = 338,
            PLAYER_VISIBLE_ITEM_6_PROPERTIES = 346,
            PLAYER_VISIBLE_ITEM_6_PAD = 347,
            PLAYER_VISIBLE_ITEM_7_CREATOR = 348,
            PLAYER_VISIBLE_ITEM_7_0 = 350,
            PLAYER_VISIBLE_ITEM_7_PROPERTIES = 358,
            PLAYER_VISIBLE_ITEM_7_PAD = 359,
            PLAYER_VISIBLE_ITEM_8_CREATOR = 360,
            PLAYER_VISIBLE_ITEM_8_0 = 362,
            PLAYER_VISIBLE_ITEM_8_PROPERTIES = 370,
            PLAYER_VISIBLE_ITEM_8_PAD = 371,
            PLAYER_VISIBLE_ITEM_9_CREATOR = 372,
            PLAYER_VISIBLE_ITEM_9_0 = 374,
            PLAYER_VISIBLE_ITEM_9_PROPERTIES = 382,
            PLAYER_VISIBLE_ITEM_9_PAD = 383,
            PLAYER_VISIBLE_ITEM_10_CREATOR = 384,
            PLAYER_VISIBLE_ITEM_10_0 = 386,
            PLAYER_VISIBLE_ITEM_10_PROPERTIES = 394,
            PLAYER_VISIBLE_ITEM_10_PAD = 395,
            PLAYER_VISIBLE_ITEM_11_CREATOR = 396,
            PLAYER_VISIBLE_ITEM_11_0 = 398,
            PLAYER_VISIBLE_ITEM_11_PROPERTIES = 406,
            PLAYER_VISIBLE_ITEM_11_PAD = 407,
            PLAYER_VISIBLE_ITEM_12_CREATOR = 408,
            PLAYER_VISIBLE_ITEM_12_0 = 410,
            PLAYER_VISIBLE_ITEM_12_PROPERTIES = 418,
            PLAYER_VISIBLE_ITEM_12_PAD = 419,
            PLAYER_VISIBLE_ITEM_13_CREATOR = 420,
            PLAYER_VISIBLE_ITEM_13_0 = 422,
            PLAYER_VISIBLE_ITEM_13_PROPERTIES = 430,
            PLAYER_VISIBLE_ITEM_13_PAD = 431,
            PLAYER_VISIBLE_ITEM_14_CREATOR = 432,
            PLAYER_VISIBLE_ITEM_14_0 = 434,
            PLAYER_VISIBLE_ITEM_14_PROPERTIES = 442,
            PLAYER_VISIBLE_ITEM_14_PAD = 443,
            PLAYER_VISIBLE_ITEM_15_CREATOR = 444,
            PLAYER_VISIBLE_ITEM_15_0 = 446,
            PLAYER_VISIBLE_ITEM_15_PROPERTIES = 454,
            PLAYER_VISIBLE_ITEM_15_PAD = 455,
            PLAYER_VISIBLE_ITEM_16_CREATOR = 456,
            PLAYER_VISIBLE_ITEM_16_0 = 458,
            PLAYER_VISIBLE_ITEM_16_PROPERTIES = 466,
            PLAYER_VISIBLE_ITEM_16_PAD = 467,
            PLAYER_VISIBLE_ITEM_17_CREATOR = 468,
            PLAYER_VISIBLE_ITEM_17_0 = 470,
            PLAYER_VISIBLE_ITEM_17_PROPERTIES = 478,
            PLAYER_VISIBLE_ITEM_17_PAD = 479,
            PLAYER_VISIBLE_ITEM_18_CREATOR = 480,
            PLAYER_VISIBLE_ITEM_18_0 = 482,
            PLAYER_VISIBLE_ITEM_18_PROPERTIES = 490,
            PLAYER_VISIBLE_ITEM_18_PAD = 491,
            PLAYER_VISIBLE_ITEM_19_CREATOR = 492,
            PLAYER_VISIBLE_ITEM_19_0 = 494,
            PLAYER_VISIBLE_ITEM_19_PROPERTIES = 502,
            PLAYER_VISIBLE_ITEM_19_PAD = 503,
            PLAYER_FIELD_INV_SLOT_HEAD = 504,
            PLAYER_FIELD_PACK_SLOT_1 = 550,
            PLAYER_FIELD_BANK_SLOT_1 = 582,
            PLAYER_FIELD_BANKBAG_SLOT_1 = 630,
            PLAYER_FIELD_VENDORBUYBACK_SLOT_1 = 642,
            PLAYER_FARSIGHT = 666,
            PLAYER__FIELD_COMBO_TARGET = 668,
            PLAYER_XP = 670,
            PLAYER_NEXT_LEVEL_XP = 671,
            PLAYER_SKILL_INFO_1_1 = 672,
            PLAYER_CHARACTER_POINTS1 = 1056,
            PLAYER_CHARACTER_POINTS2 = 1057,
            PLAYER_TRACK_CREATURES = 1058,
            PLAYER_TRACK_RESOURCES = 1059,
            PLAYER_BLOCK_PERCENTAGE = 1060,
            PLAYER_DODGE_PERCENTAGE = 1061,
            PLAYER_PARRY_PERCENTAGE = 1062,
            PLAYER_CRIT_PERCENTAGE = 1063,
            PLAYER_RANGED_CRIT_PERCENTAGE = 1064,
            PLAYER_EXPLORED_ZONES_1 = 1065,
            PLAYER_REST_STATE_EXPERIENCE = 1129,
            PLAYER_FIELD_COINAGE = 1130,
            PLAYER_FIELD_POSSTAT0 = 1131,
            PLAYER_FIELD_POSSTAT1 = 1132,
            PLAYER_FIELD_POSSTAT2 = 1133,
            PLAYER_FIELD_POSSTAT3 = 1134,
            PLAYER_FIELD_POSSTAT4 = 1135,
            PLAYER_FIELD_NEGSTAT0 = 1136,
            PLAYER_FIELD_NEGSTAT1 = 1137,
            PLAYER_FIELD_NEGSTAT2 = 1138,
            PLAYER_FIELD_NEGSTAT3 = 1139,
            PLAYER_FIELD_NEGSTAT4 = 1140,
            PLAYER_FIELD_RESISTANCEBUFFMODSPOSITIVE = 1141,
            PLAYER_FIELD_RESISTANCEBUFFMODSNEGATIVE = 1148,
            PLAYER_FIELD_MOD_DAMAGE_DONE_POS = 1155,
            PLAYER_FIELD_MOD_DAMAGE_DONE_NEG = 1162,
            PLAYER_FIELD_MOD_DAMAGE_DONE_PCT = 1169,
            PLAYER_FIELD_BYTES = 1176,
            PLAYER_AMMO_ID = 1177,
            PLAYER_SELF_RES_SPELL = 1178,
            PLAYER_FIELD_PVP_MEDALS = 1179,
            PLAYER_FIELD_BUYBACK_PRICE_1 = 1180,
            PLAYER_FIELD_BUYBACK_TIMESTAMP_1 = 1192,
            PLAYER_FIELD_SESSION_KILLS = 1194,
            PLAYER_FIELD_YESTERDAY_KILLS = 1195,
            PLAYER_FIELD_LAST_WEEK_KILLS = 1206,
            PLAYER_FIELD_THIS_WEEK_KILLS = 1207,
            PLAYER_FIELD_THIS_WEEK_CONTRIBUTION = 1208,
            PLAYER_FIELD_LIFETIME_HONORBALE_KILLS = 1209,
            PLAYER_FIELD_LIFETIME_DISHONORBALE_KILLS = 1210,
            PLAYER_FIELD_YESTERDAY_CONTRIBUTION = 1211,
            PLAYER_FIELD_LAST_WEEK_CONTRIBUTION = 1212,
            PLAYER_FIELD_LAST_WEEK_RANK = 1213,
            PLAYER_FIELD_BYTES2 = 1214,
            PLAYER_FIELD_PADDING = 1215,
            MAX = 1216,
        }
    }
}
