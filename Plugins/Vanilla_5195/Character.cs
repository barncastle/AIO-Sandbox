using System;
using Common.Constants;
using Common.Extensions;
using Common.Interfaces;
using Common.Structs;

namespace Vanilla_5195
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
            writer.WritePackedGUID(Guid);
            writer.WriteUInt8(4); // ObjectType, 4 = Player

            writer.WriteUInt8(0x71); // UpdateFlags
            writer.WriteUInt32(0x2000);  // MovementFlagMask
            writer.WriteUInt32((uint)Environment.TickCount);

            writer.WriteFloat(Location.X);  // x
            writer.WriteFloat(Location.Y);  // y
            writer.WriteFloat(Location.Z);  // z
            writer.WriteFloat(Location.O);  // w (o)
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
            SetField(Fields.UNIT_FIELD_FACTIONTEMPLATE, this.GetFactionTemplate());
            SetField(Fields.UNIT_FIELD_BYTES_0, ToUInt32(Race, Class, Gender, PowerType));
            SetField(Fields.UNIT_FIELD_STAT0, Strength);
            SetField(Fields.UNIT_FIELD_STAT1, Agility);
            SetField(Fields.UNIT_FIELD_STAT2, Stamina);
            SetField(Fields.UNIT_FIELD_STAT3, Intellect);
            SetField(Fields.UNIT_FIELD_STAT4, Spirit);
            SetField(Fields.UNIT_FIELD_FLAGS, 8);
            SetField(Fields.UNIT_FIELD_BASE_MANA, Mana);
            SetField(Fields.UNIT_FIELD_DISPLAYID, DisplayId);
            SetField(Fields.UNIT_FIELD_MOUNTDISPLAYID, MountDisplayId);
            SetField(Fields.UNIT_FIELD_BYTES_1, ToUInt32((byte)StandState));
            SetField(Fields.UNIT_FIELD_BYTES_2, 0);
            SetField(Fields.PLAYER_BYTES, ToUInt32(Skin, Face, HairStyle, HairColor));
            SetField(Fields.PLAYER_BYTES_2, ToUInt32(FacialHair, 0, 0, RestedState));
            SetField(Fields.PLAYER_BYTES_3, ToUInt32(Gender));
            SetField(Fields.PLAYER_XP, 47);
            SetField(Fields.PLAYER_NEXT_LEVEL_XP, 200);
            SetField(Fields.PLAYER_FLAGS, 0);

            SetField(Fields.UNIT_FIELD_ATTACK_POWER, 1);
            SetField(Fields.UNIT_FIELD_ATTACK_POWER_MODS, 0);
            SetField(Fields.UNIT_FIELD_RANGED_ATTACK_POWER, 1);
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
                movementStatus.WritePackedGUID(Guid);
                movementStatus.WriteUInt64(0);
                movementStatus.WriteUInt32(0);
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
            writer.WritePackedGUID(Guid);
            writer.Write(0);
            return this.BuildForceSpeed(writer, modifier);
        }

        internal enum Fields
        {
            OBJECT_FIELD_GUID = 0x0000,
            OBJECT_FIELD_TYPE = 0x0002,
            OBJECT_FIELD_ENTRY = 0x0003,
            OBJECT_FIELD_SCALE_X = 0x0004,
            OBJECT_FIELD_PADDING = 0x0005,
            OBJECT_END = 0x0006,
            UNIT_FIELD_CHARM = OBJECT_END + 0x0000,
            UNIT_FIELD_SUMMON = OBJECT_END + 0x0002,
            UNIT_FIELD_CHARMEDBY = OBJECT_END + 0x0004,
            UNIT_FIELD_SUMMONEDBY = OBJECT_END + 0x0006,
            UNIT_FIELD_CREATEDBY = OBJECT_END + 0x0008,
            UNIT_FIELD_TARGET = OBJECT_END + 0x000A,
            UNIT_FIELD_PERSUADED = OBJECT_END + 0x000C,
            UNIT_FIELD_CHANNEL_OBJECT = OBJECT_END + 0x000E,
            UNIT_FIELD_HEALTH = OBJECT_END + 0x0010,
            UNIT_FIELD_POWER1 = OBJECT_END + 0x0011,
            UNIT_FIELD_POWER2 = OBJECT_END + 0x0012,
            UNIT_FIELD_POWER3 = OBJECT_END + 0x0013,
            UNIT_FIELD_POWER4 = OBJECT_END + 0x0014,
            UNIT_FIELD_POWER5 = OBJECT_END + 0x0015,
            UNIT_FIELD_MAXHEALTH = OBJECT_END + 0x0016,
            UNIT_FIELD_MAXPOWER1 = OBJECT_END + 0x0017,
            UNIT_FIELD_MAXPOWER2 = OBJECT_END + 0x0018,
            UNIT_FIELD_MAXPOWER3 = OBJECT_END + 0x0019,
            UNIT_FIELD_MAXPOWER4 = OBJECT_END + 0x001A,
            UNIT_FIELD_MAXPOWER5 = OBJECT_END + 0x001B,
            UNIT_FIELD_LEVEL = OBJECT_END + 0x001C,
            UNIT_FIELD_FACTIONTEMPLATE = OBJECT_END + 0x001D,
            UNIT_FIELD_BYTES_0 = OBJECT_END + 0x001E,
            UNIT_VIRTUAL_ITEM_SLOT_DISPLAY = OBJECT_END + 0x001F,
            UNIT_VIRTUAL_ITEM_INFO = OBJECT_END + 0x0022,
            UNIT_FIELD_FLAGS = OBJECT_END + 0x0028,
            UNIT_FIELD_AURA = OBJECT_END + 0x0029,
            UNIT_FIELD_AURAFLAGS = OBJECT_END + 0x0059,
            UNIT_FIELD_AURALEVELS = OBJECT_END + 0x005F,
            UNIT_FIELD_AURAAPPLICATIONS = OBJECT_END + 0x006B,
            UNIT_FIELD_AURASTATE = OBJECT_END + 0x0077,
            UNIT_FIELD_BASEATTACKTIME = OBJECT_END + 0x0078,
            UNIT_FIELD_RANGEDATTACKTIME = OBJECT_END + 0x007A,
            UNIT_FIELD_BOUNDINGRADIUS = OBJECT_END + 0x007B,
            UNIT_FIELD_COMBATREACH = OBJECT_END + 0x007C,
            UNIT_FIELD_DISPLAYID = OBJECT_END + 0x007D,
            UNIT_FIELD_NATIVEDISPLAYID = OBJECT_END + 0x007E,
            UNIT_FIELD_MOUNTDISPLAYID = OBJECT_END + 0x007F,
            UNIT_FIELD_MINDAMAGE = OBJECT_END + 0x0080,
            UNIT_FIELD_MAXDAMAGE = OBJECT_END + 0x0081,
            UNIT_FIELD_MINOFFHANDDAMAGE = OBJECT_END + 0x0082,
            UNIT_FIELD_MAXOFFHANDDAMAGE = OBJECT_END + 0x0083,
            UNIT_FIELD_BYTES_1 = OBJECT_END + 0x0084,
            UNIT_FIELD_PETNUMBER = OBJECT_END + 0x0085,
            UNIT_FIELD_PET_NAME_TIMESTAMP = OBJECT_END + 0x0086,
            UNIT_FIELD_PETEXPERIENCE = OBJECT_END + 0x0087,
            UNIT_FIELD_PETNEXTLEVELEXP = OBJECT_END + 0x0088,
            UNIT_DYNAMIC_FLAGS = OBJECT_END + 0x0089,
            UNIT_CHANNEL_SPELL = OBJECT_END + 0x008A,
            UNIT_MOD_CAST_SPEED = OBJECT_END + 0x008B,
            UNIT_CREATED_BY_SPELL = OBJECT_END + 0x008C,
            UNIT_NPC_FLAGS = OBJECT_END + 0x008D,
            UNIT_NPC_EMOTESTATE = OBJECT_END + 0x008E,
            UNIT_TRAINING_POINTS = OBJECT_END + 0x008F,
            UNIT_FIELD_STAT0 = OBJECT_END + 0x0090,
            UNIT_FIELD_STAT1 = OBJECT_END + 0x0091,
            UNIT_FIELD_STAT2 = OBJECT_END + 0x0092,
            UNIT_FIELD_STAT3 = OBJECT_END + 0x0093,
            UNIT_FIELD_STAT4 = OBJECT_END + 0x0094,
            UNIT_FIELD_RESISTANCES = OBJECT_END + 0x0095,
            UNIT_FIELD_BASE_MANA = OBJECT_END + 0x009C,
            UNIT_FIELD_BASE_HEALTH = OBJECT_END + 0x009D,
            UNIT_FIELD_BYTES_2 = OBJECT_END + 0x009E,
            UNIT_FIELD_ATTACK_POWER = OBJECT_END + 0x009F,
            UNIT_FIELD_ATTACK_POWER_MODS = OBJECT_END + 0x00A0,
            UNIT_FIELD_ATTACK_POWER_MULTIPLIER = OBJECT_END + 0x00A1,
            UNIT_FIELD_RANGED_ATTACK_POWER = OBJECT_END + 0x00A2,
            UNIT_FIELD_RANGED_ATTACK_POWER_MODS = OBJECT_END + 0x00A3,
            UNIT_FIELD_RANGED_ATTACK_POWER_MULTIPLIER = OBJECT_END + 0x00A4,
            UNIT_FIELD_MINRANGEDDAMAGE = OBJECT_END + 0x00A5,
            UNIT_FIELD_MAXRANGEDDAMAGE = OBJECT_END + 0x00A6,
            UNIT_FIELD_POWER_COST_MODIFIER = OBJECT_END + 0x00A7,
            UNIT_FIELD_POWER_COST_MULTIPLIER = OBJECT_END + 0x00AE,
            UNIT_FIELD_PADDING = OBJECT_END + 0x00B5,
            UNIT_END = OBJECT_END + 0x00B6,
            PLAYER_SELECTION = UNIT_END + 0x0000,
            PLAYER_DUEL_ARBITER = UNIT_END + 0x0002,
            PLAYER_FLAGS = UNIT_END + 0x0004,
            PLAYER_GUILDID = UNIT_END + 0x0005,
            PLAYER_GUILDRANK = UNIT_END + 0x0006,
            PLAYER_BYTES = UNIT_END + 0x0007,
            PLAYER_BYTES_2 = UNIT_END + 0x0008,
            PLAYER_BYTES_3 = UNIT_END + 0x0009,
            PLAYER_DUEL_TEAM = UNIT_END + 0x000A,
            PLAYER_GUILD_TIMESTAMP = UNIT_END + 0x000B,
            PLAYER_QUEST_LOG_1_1 = UNIT_END + 0x000C,
            PLAYER_QUEST_LOG_1_2 = UNIT_END + 0x000D,
            PLAYER_QUEST_LOG_2_1 = UNIT_END + 0x000F,
            PLAYER_QUEST_LOG_2_2 = UNIT_END + 0x0010,
            PLAYER_QUEST_LOG_3_1 = UNIT_END + 0x0012,
            PLAYER_QUEST_LOG_3_2 = UNIT_END + 0x0013,
            PLAYER_QUEST_LOG_4_1 = UNIT_END + 0x0015,
            PLAYER_QUEST_LOG_4_2 = UNIT_END + 0x0016,
            PLAYER_QUEST_LOG_5_1 = UNIT_END + 0x0018,
            PLAYER_QUEST_LOG_5_2 = UNIT_END + 0x0019,
            PLAYER_QUEST_LOG_6_1 = UNIT_END + 0x001B,
            PLAYER_QUEST_LOG_6_2 = UNIT_END + 0x001C,
            PLAYER_QUEST_LOG_7_1 = UNIT_END + 0x001E,
            PLAYER_QUEST_LOG_7_2 = UNIT_END + 0x001F,
            PLAYER_QUEST_LOG_8_1 = UNIT_END + 0x0021,
            PLAYER_QUEST_LOG_8_2 = UNIT_END + 0x0022,
            PLAYER_QUEST_LOG_9_1 = UNIT_END + 0x0024,
            PLAYER_QUEST_LOG_9_2 = UNIT_END + 0x0025,
            PLAYER_QUEST_LOG_10_1 = UNIT_END + 0x0027,
            PLAYER_QUEST_LOG_10_2 = UNIT_END + 0x0028,
            PLAYER_QUEST_LOG_11_1 = UNIT_END + 0x002A,
            PLAYER_QUEST_LOG_11_2 = UNIT_END + 0x002B,
            PLAYER_QUEST_LOG_12_1 = UNIT_END + 0x002D,
            PLAYER_QUEST_LOG_12_2 = UNIT_END + 0x002E,
            PLAYER_QUEST_LOG_13_1 = UNIT_END + 0x0030,
            PLAYER_QUEST_LOG_13_2 = UNIT_END + 0x0031,
            PLAYER_QUEST_LOG_14_1 = UNIT_END + 0x0033,
            PLAYER_QUEST_LOG_14_2 = UNIT_END + 0x0034,
            PLAYER_QUEST_LOG_15_1 = UNIT_END + 0x0036,
            PLAYER_QUEST_LOG_15_2 = UNIT_END + 0x0037,
            PLAYER_QUEST_LOG_16_1 = UNIT_END + 0x0039,
            PLAYER_QUEST_LOG_16_2 = UNIT_END + 0x003A,
            PLAYER_QUEST_LOG_17_1 = UNIT_END + 0x003C,
            PLAYER_QUEST_LOG_17_2 = UNIT_END + 0x003D,
            PLAYER_QUEST_LOG_18_1 = UNIT_END + 0x003F,
            PLAYER_QUEST_LOG_18_2 = UNIT_END + 0x0040,
            PLAYER_QUEST_LOG_19_1 = UNIT_END + 0x0042,
            PLAYER_QUEST_LOG_19_2 = UNIT_END + 0x0043,
            PLAYER_QUEST_LOG_20_1 = UNIT_END + 0x0045,
            PLAYER_QUEST_LOG_20_2 = UNIT_END + 0x0046,
            PLAYER_VISIBLE_ITEM_1_CREATOR = UNIT_END + 0x0048,
            PLAYER_VISIBLE_ITEM_1_0 = UNIT_END + 0x004A,
            PLAYER_VISIBLE_ITEM_1_PROPERTIES = UNIT_END + 0x0052,
            PLAYER_VISIBLE_ITEM_1_PAD = UNIT_END + 0x0053,
            PLAYER_VISIBLE_ITEM_2_CREATOR = UNIT_END + 0x0054,
            PLAYER_VISIBLE_ITEM_2_0 = UNIT_END + 0x0056,
            PLAYER_VISIBLE_ITEM_2_PROPERTIES = UNIT_END + 0x005E,
            PLAYER_VISIBLE_ITEM_2_PAD = UNIT_END + 0x005F,
            PLAYER_VISIBLE_ITEM_3_CREATOR = UNIT_END + 0x0060,
            PLAYER_VISIBLE_ITEM_3_0 = UNIT_END + 0x0062,
            PLAYER_VISIBLE_ITEM_3_PROPERTIES = UNIT_END + 0x006A,
            PLAYER_VISIBLE_ITEM_3_PAD = UNIT_END + 0x006B,
            PLAYER_VISIBLE_ITEM_4_CREATOR = UNIT_END + 0x006C,
            PLAYER_VISIBLE_ITEM_4_0 = UNIT_END + 0x006E,
            PLAYER_VISIBLE_ITEM_4_PROPERTIES = UNIT_END + 0x0076,
            PLAYER_VISIBLE_ITEM_4_PAD = UNIT_END + 0x0077,
            PLAYER_VISIBLE_ITEM_5_CREATOR = UNIT_END + 0x0078,
            PLAYER_VISIBLE_ITEM_5_0 = UNIT_END + 0x007A,
            PLAYER_VISIBLE_ITEM_5_PROPERTIES = UNIT_END + 0x0082,
            PLAYER_VISIBLE_ITEM_5_PAD = UNIT_END + 0x0083,
            PLAYER_VISIBLE_ITEM_6_CREATOR = UNIT_END + 0x0084,
            PLAYER_VISIBLE_ITEM_6_0 = UNIT_END + 0x0086,
            PLAYER_VISIBLE_ITEM_6_PROPERTIES = UNIT_END + 0x008E,
            PLAYER_VISIBLE_ITEM_6_PAD = UNIT_END + 0x008F,
            PLAYER_VISIBLE_ITEM_7_CREATOR = UNIT_END + 0x0090,
            PLAYER_VISIBLE_ITEM_7_0 = UNIT_END + 0x0092,
            PLAYER_VISIBLE_ITEM_7_PROPERTIES = UNIT_END + 0x009A,
            PLAYER_VISIBLE_ITEM_7_PAD = UNIT_END + 0x009B,
            PLAYER_VISIBLE_ITEM_8_CREATOR = UNIT_END + 0x009C,
            PLAYER_VISIBLE_ITEM_8_0 = UNIT_END + 0x009E,
            PLAYER_VISIBLE_ITEM_8_PROPERTIES = UNIT_END + 0x00A6,
            PLAYER_VISIBLE_ITEM_8_PAD = UNIT_END + 0x00A7,
            PLAYER_VISIBLE_ITEM_9_CREATOR = UNIT_END + 0x00A8,
            PLAYER_VISIBLE_ITEM_9_0 = UNIT_END + 0x00AA,
            PLAYER_VISIBLE_ITEM_9_PROPERTIES = UNIT_END + 0x00B2,
            PLAYER_VISIBLE_ITEM_9_PAD = UNIT_END + 0x00B3,
            PLAYER_VISIBLE_ITEM_10_CREATOR = UNIT_END + 0x00B4,
            PLAYER_VISIBLE_ITEM_10_0 = UNIT_END + 0x00B6,
            PLAYER_VISIBLE_ITEM_10_PROPERTIES = UNIT_END + 0x00BE,
            PLAYER_VISIBLE_ITEM_10_PAD = UNIT_END + 0x00BF,
            PLAYER_VISIBLE_ITEM_11_CREATOR = UNIT_END + 0x00C0,
            PLAYER_VISIBLE_ITEM_11_0 = UNIT_END + 0x00C2,
            PLAYER_VISIBLE_ITEM_11_PROPERTIES = UNIT_END + 0x00CA,
            PLAYER_VISIBLE_ITEM_11_PAD = UNIT_END + 0x00CB,
            PLAYER_VISIBLE_ITEM_12_CREATOR = UNIT_END + 0x00CC,
            PLAYER_VISIBLE_ITEM_12_0 = UNIT_END + 0x00CE,
            PLAYER_VISIBLE_ITEM_12_PROPERTIES = UNIT_END + 0x00D6,
            PLAYER_VISIBLE_ITEM_12_PAD = UNIT_END + 0x00D7,
            PLAYER_VISIBLE_ITEM_13_CREATOR = UNIT_END + 0x00D8,
            PLAYER_VISIBLE_ITEM_13_0 = UNIT_END + 0x00DA,
            PLAYER_VISIBLE_ITEM_13_PROPERTIES = UNIT_END + 0x00E2,
            PLAYER_VISIBLE_ITEM_13_PAD = UNIT_END + 0x00E3,
            PLAYER_VISIBLE_ITEM_14_CREATOR = UNIT_END + 0x00E4,
            PLAYER_VISIBLE_ITEM_14_0 = UNIT_END + 0x00E6,
            PLAYER_VISIBLE_ITEM_14_PROPERTIES = UNIT_END + 0x00EE,
            PLAYER_VISIBLE_ITEM_14_PAD = UNIT_END + 0x00EF,
            PLAYER_VISIBLE_ITEM_15_CREATOR = UNIT_END + 0x00F0,
            PLAYER_VISIBLE_ITEM_15_0 = UNIT_END + 0x00F2,
            PLAYER_VISIBLE_ITEM_15_PROPERTIES = UNIT_END + 0x00FA,
            PLAYER_VISIBLE_ITEM_15_PAD = UNIT_END + 0x00FB,
            PLAYER_VISIBLE_ITEM_16_CREATOR = UNIT_END + 0x00FC,
            PLAYER_VISIBLE_ITEM_16_0 = UNIT_END + 0x00FE,
            PLAYER_VISIBLE_ITEM_16_PROPERTIES = UNIT_END + 0x0106,
            PLAYER_VISIBLE_ITEM_16_PAD = UNIT_END + 0x0107,
            PLAYER_VISIBLE_ITEM_17_CREATOR = UNIT_END + 0x0108,
            PLAYER_VISIBLE_ITEM_17_0 = UNIT_END + 0x010A,
            PLAYER_VISIBLE_ITEM_17_PROPERTIES = UNIT_END + 0x0112,
            PLAYER_VISIBLE_ITEM_17_PAD = UNIT_END + 0x0113,
            PLAYER_VISIBLE_ITEM_18_CREATOR = UNIT_END + 0x0114,
            PLAYER_VISIBLE_ITEM_18_0 = UNIT_END + 0x0116,
            PLAYER_VISIBLE_ITEM_18_PROPERTIES = UNIT_END + 0x011E,
            PLAYER_VISIBLE_ITEM_18_PAD = UNIT_END + 0x011F,
            PLAYER_VISIBLE_ITEM_19_CREATOR = UNIT_END + 0x0120,
            PLAYER_VISIBLE_ITEM_19_0 = UNIT_END + 0x0122,
            PLAYER_VISIBLE_ITEM_19_PROPERTIES = UNIT_END + 0x012A,
            PLAYER_VISIBLE_ITEM_19_PAD = UNIT_END + 0x012B,
            PLAYER_FIELD_INV_SLOT_HEAD = UNIT_END + 0x012C,
            PLAYER_FIELD_PACK_SLOT_1 = UNIT_END + 0x015A,
            PLAYER_FIELD_BANK_SLOT_1 = UNIT_END + 0x017A,
            PLAYER_FIELD_BANKBAG_SLOT_1 = UNIT_END + 0x01AA,
            PLAYER_FIELD_VENDORBUYBACK_SLOT_1 = UNIT_END + 0x01B6,
            PLAYER_FARSIGHT = UNIT_END + 0x01CE,
            PLAYER__FIELD_COMBO_TARGET = UNIT_END + 0x01D0,
            PLAYER_XP = UNIT_END + 0x01D2,
            PLAYER_NEXT_LEVEL_XP = UNIT_END + 0x01D3,
            PLAYER_SKILL_INFO_1_1 = UNIT_END + 0x01D4,
            PLAYER_CHARACTER_POINTS1 = UNIT_END + 0x0354,
            PLAYER_CHARACTER_POINTS2 = UNIT_END + 0x0355,
            PLAYER_TRACK_CREATURES = UNIT_END + 0x0356,
            PLAYER_TRACK_RESOURCES = UNIT_END + 0x0357,
            PLAYER_BLOCK_PERCENTAGE = UNIT_END + 0x0358,
            PLAYER_DODGE_PERCENTAGE = UNIT_END + 0x0359,
            PLAYER_PARRY_PERCENTAGE = UNIT_END + 0x035A,
            PLAYER_CRIT_PERCENTAGE = UNIT_END + 0x035B,
            PLAYER_RANGED_CRIT_PERCENTAGE = UNIT_END + 0x035C,
            PLAYER_EXPLORED_ZONES_1 = UNIT_END + 0x035D,
            PLAYER_REST_STATE_EXPERIENCE = UNIT_END + 0x039D,
            PLAYER_FIELD_COINAGE = UNIT_END + 0x039E,
            PLAYER_FIELD_POSSTAT0 = UNIT_END + 0x039F,
            PLAYER_FIELD_POSSTAT1 = UNIT_END + 0x03A0,
            PLAYER_FIELD_POSSTAT2 = UNIT_END + 0x03A1,
            PLAYER_FIELD_POSSTAT3 = UNIT_END + 0x03A2,
            PLAYER_FIELD_POSSTAT4 = UNIT_END + 0x03A3,
            PLAYER_FIELD_NEGSTAT0 = UNIT_END + 0x03A4,
            PLAYER_FIELD_NEGSTAT1 = UNIT_END + 0x03A5,
            PLAYER_FIELD_NEGSTAT2 = UNIT_END + 0x03A6,
            PLAYER_FIELD_NEGSTAT3 = UNIT_END + 0x03A7,
            PLAYER_FIELD_NEGSTAT4 = UNIT_END + 0x03A8,
            PLAYER_FIELD_RESISTANCEBUFFMODSPOSITIVE = UNIT_END + 0x03A9,
            PLAYER_FIELD_RESISTANCEBUFFMODSNEGATIVE = UNIT_END + 0x03B0,
            PLAYER_FIELD_MOD_DAMAGE_DONE_POS = UNIT_END + 0x03B7,
            PLAYER_FIELD_MOD_DAMAGE_DONE_NEG = UNIT_END + 0x03BE,
            PLAYER_FIELD_MOD_DAMAGE_DONE_PCT = UNIT_END + 0x03C5,
            PLAYER_FIELD_BYTES = UNIT_END + 0x03CC,
            PLAYER_AMMO_ID = UNIT_END + 0x03CD,
            PLAYER_SELF_RES_SPELL = UNIT_END + 0x03CE,
            PLAYER_FIELD_PVP_MEDALS = UNIT_END + 0x03CF,
            PLAYER_FIELD_BUYBACK_PRICE_1 = UNIT_END + 0x03D0,
            PLAYER_FIELD_BUYBACK_TIMESTAMP_1 = UNIT_END + 0x03DC,
            PLAYER_FIELD_SESSION_KILLS = UNIT_END + 0x03E8,
            PLAYER_FIELD_YESTERDAY_KILLS = UNIT_END + 0x03E9,
            PLAYER_FIELD_LAST_WEEK_KILLS = UNIT_END + 0x03EA,
            PLAYER_FIELD_THIS_WEEK_KILLS = UNIT_END + 0x03EB,
            PLAYER_FIELD_THIS_WEEK_CONTRIBUTION = UNIT_END + 0x03EC,
            PLAYER_FIELD_LIFETIME_HONORBALE_KILLS = UNIT_END + 0x03ED,
            PLAYER_FIELD_LIFETIME_DISHONORBALE_KILLS = UNIT_END + 0x03EE,
            PLAYER_FIELD_YESTERDAY_CONTRIBUTION = UNIT_END + 0x03EF,
            PLAYER_FIELD_LAST_WEEK_CONTRIBUTION = UNIT_END + 0x03F0,
            PLAYER_FIELD_LAST_WEEK_RANK = UNIT_END + 0x03F1,
            PLAYER_FIELD_BYTES2 = UNIT_END + 0x03F2,
            PLAYER_FIELD_WATCHED_FACTION_INDEX = UNIT_END + 0x03F3,
            MAX = UNIT_END + 0x03F4
        }
    }
}
