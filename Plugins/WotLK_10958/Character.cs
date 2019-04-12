using System;
using Common.Constants;
using Common.Extensions;
using Common.Interfaces;
using Common.Structs;

namespace WotLK_10958
{
    public class Character : BaseCharacter
    {
        public override int Build { get; set; } = Sandbox.Instance.Build;

        public Character() => Level = 55; // enable DKs

        public override IPacketWriter BuildUpdate()
        {
            MaskSize = ((int)Fields.MAX + 31) / 32;
            FieldData.Clear();
            MaskArray = new byte[MaskSize * 4];

            PacketWriter writer = new PacketWriter(Sandbox.Instance.Opcodes[global::Opcodes.SMSG_UPDATE_OBJECT], "SMSG_UPDATE_OBJECT");
            writer.WriteUInt32(1); // Number of transactions

            writer.WriteUInt8(3); // UpdateType
            writer.WritePackedGUID(Guid);
            writer.WriteUInt8(4); // ObjectType, 4 = Player

            writer.WriteUInt16(0x71); // UpdateFlags
            writer.WriteUInt32(0);  // MovementFlagMask
            writer.WriteUInt16(0); // MoveFlags2? 
            writer.WriteUInt32((uint)Environment.TickCount);

            writer.WriteFloat(Location.X);  // x
            writer.WriteFloat(Location.Y);  // y
            writer.WriteFloat(Location.Z);  // z
            writer.WriteFloat(Location.O);  // w (o)
            writer.WriteInt32(0); // falltime

            writer.WriteFloat(2.5f); // WalkSpeed
            writer.WriteFloat(7.0f); // RunSpeed
            writer.WriteFloat(4.5f); // Backwards WalkSpeed
            writer.WriteFloat(4.7222f); // SwimSpeed
            writer.WriteFloat(2.5f); // Backwards SwimSpeed
            writer.WriteFloat(7.0f); // FlySpeed
            writer.WriteFloat(4.5f); // Backwards FlySpeed
            writer.WriteFloat(3.14f); // TurnSpeed
            writer.WriteFloat(7f); // PitchRate

            writer.Write(0);

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
            SetField(Fields.UNIT_FIELD_FLAGS_2, 0x800);
            SetField(Fields.UNIT_FIELD_BASE_MANA, Mana);
            SetField(Fields.UNIT_FIELD_DISPLAYID, DisplayId);
            SetField(Fields.UNIT_FIELD_NATIVEDISPLAYID, this.GetDisplayId());
            SetField(Fields.UNIT_FIELD_MOUNTDISPLAYID, MountDisplayId);
            SetField(Fields.UNIT_FIELD_BYTES_1, ToUInt32((byte)StandState));
            SetField(Fields.UNIT_FIELD_BYTES_2, 10240);
            SetField(Fields.PLAYER_BYTES, ToUInt32(Skin, Face, HairStyle, HairColor));
            SetField(Fields.PLAYER_BYTES_2, ToUInt32(FacialHair, b4: RestedState));
            SetField(Fields.PLAYER_BYTES_3, ToUInt32(Gender));
            SetField(Fields.PLAYER_XP, 47);
            SetField(Fields.PLAYER_NEXT_LEVEL_XP, 200);
            SetField(Fields.PLAYER_FLAGS, 0);

            SetField(Fields.UNIT_FIELD_ATTACK_POWER, 1);
            SetField(Fields.UNIT_FIELD_ATTACK_POWER_MODS, 0);
            SetField(Fields.UNIT_FIELD_RANGED_ATTACK_POWER, 1);
            SetField(Fields.UNIT_FIELD_RANGED_ATTACK_POWER_MODS, 0);

            for (int i = 0; i < 0x80; i++)
                SetField(Fields.PLAYER_EXPLORED_ZONES_1 + i, 0xFFFFFFFF);

            // send language skills so we can type commands
            SetField(Fields.PLAYER_SKILL_INFO_1_1, CharacterData.COMMON_SKILL_ID);
            SetField(Fields.PLAYER_SKILL_INFO_1_1 + 1, ToUInt32(300, 300));
            SetField(Fields.PLAYER_SKILL_INFO_1_1 + 2, 0);
            SetField(Fields.PLAYER_SKILL_INFO_1_1 + 3, CharacterData.ORCISH_SKILL_ID);
            SetField(Fields.PLAYER_SKILL_INFO_1_1 + 4, ToUInt32(300, 300));
            SetField(Fields.PLAYER_SKILL_INFO_1_1 + 5, 0);

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
            bool mapchange = Location.Map != map;

            if (!mapchange)
            {
                PacketWriter movementStatus = new PacketWriter(Sandbox.Instance.Opcodes[global::Opcodes.MSG_MOVE_TELEPORT_ACK], "MSG_MOVE_TELEPORT_ACK");
                movementStatus.WritePackedGUID(Guid);
                movementStatus.WriteUInt64(0);
                movementStatus.WriteUInt16(0);
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

                IsFlying = false;
            }

            System.Threading.Thread.Sleep(150); // Pause to factor unsent packets

            Location = new Location(x, y, z, o, map);
            manager.Send(BuildUpdate());

            if (mapchange)
            {
                // retain flight
                manager.Send(BuildFly(IsFlying));

                // send timesync
                PacketWriter timesyncreq = new PacketWriter(Sandbox.Instance.Opcodes[global::Opcodes.SMSG_TIME_SYNC_REQ], "SMSG_TIME_SYNC_REQ");
                timesyncreq.Write(0);
                manager.Send(timesyncreq);
            }

            IsTeleporting = false;
        }

        public override IPacketWriter BuildForceSpeed(float modifier, SpeedType type = SpeedType.Run)
        {
            global::Opcodes opcode;
            switch (type)
            {
                case SpeedType.Fly:
                    opcode = global::Opcodes.SMSG_FORCE_FLIGHT_SPEED_CHANGE;
                    break;
                case SpeedType.Swim:
                    opcode = global::Opcodes.SMSG_FORCE_SWIM_SPEED_CHANGE;
                    break;
                default:
                    opcode = global::Opcodes.SMSG_FORCE_SPEED_CHANGE;
                    break;
            }

            PacketWriter writer = new PacketWriter(Sandbox.Instance.Opcodes[opcode], opcode.ToString());
            writer.WritePackedGUID(Guid);
            writer.Write(0);
            if (type == SpeedType.Run)
                writer.WriteUInt8(0);
            return this.BuildForceSpeed(writer, modifier);
        }

        public override IPacketWriter BuildFly(bool mode)
        {
            IsFlying = mode;

            var opcode = mode ? global::Opcodes.SMSG_MOVE_SET_CAN_FLY : global::Opcodes.SMSG_MOVE_UNSET_CAN_FLY;
            PacketWriter writer = new PacketWriter(Sandbox.Instance.Opcodes[opcode], opcode.ToString());
            writer.WritePackedGUID(Guid);
            writer.Write(2);
            return writer;
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
            UNIT_FIELD_CRITTER = OBJECT_END + 0x0004,
            UNIT_FIELD_CHARMEDBY = OBJECT_END + 0x0006,
            UNIT_FIELD_SUMMONEDBY = OBJECT_END + 0x0008,
            UNIT_FIELD_CREATEDBY = OBJECT_END + 0x000A,
            UNIT_FIELD_TARGET = OBJECT_END + 0x000C,
            UNIT_FIELD_CHANNEL_OBJECT = OBJECT_END + 0x000E,
            UNIT_FIELD_BYTES_0 = OBJECT_END + 0x0010,
            UNIT_FIELD_HEALTH = OBJECT_END + 0x0011,
            UNIT_FIELD_POWER1 = OBJECT_END + 0x0012,
            UNIT_FIELD_POWER2 = OBJECT_END + 0x0013,
            UNIT_FIELD_POWER3 = OBJECT_END + 0x0014,
            UNIT_FIELD_POWER4 = OBJECT_END + 0x0015,
            UNIT_FIELD_POWER5 = OBJECT_END + 0x0016,
            UNIT_FIELD_POWER6 = OBJECT_END + 0x0017,
            UNIT_FIELD_POWER7 = OBJECT_END + 0x0018,
            UNIT_FIELD_MAXHEALTH = OBJECT_END + 0x0019,
            UNIT_FIELD_MAXPOWER1 = OBJECT_END + 0x001A,
            UNIT_FIELD_MAXPOWER2 = OBJECT_END + 0x001B,
            UNIT_FIELD_MAXPOWER3 = OBJECT_END + 0x001C,
            UNIT_FIELD_MAXPOWER4 = OBJECT_END + 0x001D,
            UNIT_FIELD_MAXPOWER5 = OBJECT_END + 0x001E,
            UNIT_FIELD_MAXPOWER6 = OBJECT_END + 0x001F,
            UNIT_FIELD_MAXPOWER7 = OBJECT_END + 0x0020,
            UNIT_FIELD_POWER_REGEN_FLAT_MODIFIER = OBJECT_END + 0x0021,
            UNIT_FIELD_POWER_REGEN_INTERRUPTED_FLAT_MODIFIER = OBJECT_END + 0x0028,
            UNIT_FIELD_LEVEL = OBJECT_END + 0x002F,
            UNIT_FIELD_FACTIONTEMPLATE = OBJECT_END + 0x0030,
            UNIT_VIRTUAL_ITEM_SLOT_ID = OBJECT_END + 0x0031,
            UNIT_FIELD_FLAGS = OBJECT_END + 0x0034,
            UNIT_FIELD_FLAGS_2 = OBJECT_END + 0x0035,
            UNIT_FIELD_AURASTATE = OBJECT_END + 0x0036,
            UNIT_FIELD_BASEATTACKTIME = OBJECT_END + 0x0037,
            UNIT_FIELD_RANGEDATTACKTIME = OBJECT_END + 0x0039,
            UNIT_FIELD_BOUNDINGRADIUS = OBJECT_END + 0x003A,
            UNIT_FIELD_COMBATREACH = OBJECT_END + 0x003B,
            UNIT_FIELD_DISPLAYID = OBJECT_END + 0x003C,
            UNIT_FIELD_NATIVEDISPLAYID = OBJECT_END + 0x003D,
            UNIT_FIELD_MOUNTDISPLAYID = OBJECT_END + 0x003E,
            UNIT_FIELD_MINDAMAGE = OBJECT_END + 0x003F,
            UNIT_FIELD_MAXDAMAGE = OBJECT_END + 0x0040,
            UNIT_FIELD_MINOFFHANDDAMAGE = OBJECT_END + 0x0041,
            UNIT_FIELD_MAXOFFHANDDAMAGE = OBJECT_END + 0x0042,
            UNIT_FIELD_BYTES_1 = OBJECT_END + 0x0043,
            UNIT_FIELD_PETNUMBER = OBJECT_END + 0x0044,
            UNIT_FIELD_PET_NAME_TIMESTAMP = OBJECT_END + 0x0045,
            UNIT_FIELD_PETEXPERIENCE = OBJECT_END + 0x0046,
            UNIT_FIELD_PETNEXTLEVELEXP = OBJECT_END + 0x0047,
            UNIT_DYNAMIC_FLAGS = OBJECT_END + 0x0048,
            UNIT_CHANNEL_SPELL = OBJECT_END + 0x0049,
            UNIT_MOD_CAST_SPEED = OBJECT_END + 0x004A,
            UNIT_CREATED_BY_SPELL = OBJECT_END + 0x004B,
            UNIT_NPC_FLAGS = OBJECT_END + 0x004C,
            UNIT_NPC_EMOTESTATE = OBJECT_END + 0x004D,
            UNIT_FIELD_STAT0 = OBJECT_END + 0x004E,
            UNIT_FIELD_STAT1 = OBJECT_END + 0x004F,
            UNIT_FIELD_STAT2 = OBJECT_END + 0x0050,
            UNIT_FIELD_STAT3 = OBJECT_END + 0x0051,
            UNIT_FIELD_STAT4 = OBJECT_END + 0x0052,
            UNIT_FIELD_POSSTAT0 = OBJECT_END + 0x0053,
            UNIT_FIELD_POSSTAT1 = OBJECT_END + 0x0054,
            UNIT_FIELD_POSSTAT2 = OBJECT_END + 0x0055,
            UNIT_FIELD_POSSTAT3 = OBJECT_END + 0x0056,
            UNIT_FIELD_POSSTAT4 = OBJECT_END + 0x0057,
            UNIT_FIELD_NEGSTAT0 = OBJECT_END + 0x0058,
            UNIT_FIELD_NEGSTAT1 = OBJECT_END + 0x0059,
            UNIT_FIELD_NEGSTAT2 = OBJECT_END + 0x005A,
            UNIT_FIELD_NEGSTAT3 = OBJECT_END + 0x005B,
            UNIT_FIELD_NEGSTAT4 = OBJECT_END + 0x005C,
            UNIT_FIELD_RESISTANCES = OBJECT_END + 0x005D,
            UNIT_FIELD_RESISTANCEBUFFMODSPOSITIVE = OBJECT_END + 0x0064,
            UNIT_FIELD_RESISTANCEBUFFMODSNEGATIVE = OBJECT_END + 0x006B,
            UNIT_FIELD_BASE_MANA = OBJECT_END + 0x0072,
            UNIT_FIELD_BASE_HEALTH = OBJECT_END + 0x0073,
            UNIT_FIELD_BYTES_2 = OBJECT_END + 0x0074,
            UNIT_FIELD_ATTACK_POWER = OBJECT_END + 0x0075,
            UNIT_FIELD_ATTACK_POWER_MODS = OBJECT_END + 0x0076,
            UNIT_FIELD_ATTACK_POWER_MULTIPLIER = OBJECT_END + 0x0077,
            UNIT_FIELD_RANGED_ATTACK_POWER = OBJECT_END + 0x0078,
            UNIT_FIELD_RANGED_ATTACK_POWER_MODS = OBJECT_END + 0x0079,
            UNIT_FIELD_RANGED_ATTACK_POWER_MULTIPLIER = OBJECT_END + 0x007A,
            UNIT_FIELD_MINRANGEDDAMAGE = OBJECT_END + 0x007B,
            UNIT_FIELD_MAXRANGEDDAMAGE = OBJECT_END + 0x007C,
            UNIT_FIELD_POWER_COST_MODIFIER = OBJECT_END + 0x007D,
            UNIT_FIELD_POWER_COST_MULTIPLIER = OBJECT_END + 0x0084,
            UNIT_FIELD_MAXHEALTHMODIFIER = OBJECT_END + 0x008B,
            UNIT_FIELD_HOVERHEIGHT = OBJECT_END + 0x008C,
            UNIT_FIELD_PADDING = OBJECT_END + 0x008D,
            UNIT_END = OBJECT_END + 0x008E,
            PLAYER_DUEL_ARBITER = UNIT_END + 0x0000,
            PLAYER_FLAGS = UNIT_END + 0x0002,
            PLAYER_GUILDID = UNIT_END + 0x0003,
            PLAYER_GUILDRANK = UNIT_END + 0x0004,
            PLAYER_BYTES = UNIT_END + 0x0005,
            PLAYER_BYTES_2 = UNIT_END + 0x0006,
            PLAYER_BYTES_3 = UNIT_END + 0x0007,
            PLAYER_DUEL_TEAM = UNIT_END + 0x0008,
            PLAYER_GUILD_TIMESTAMP = UNIT_END + 0x0009,
            PLAYER_QUEST_LOG_1_1 = UNIT_END + 0x000A,
            PLAYER_QUEST_LOG_1_2 = UNIT_END + 0x000B,
            PLAYER_QUEST_LOG_1_3 = UNIT_END + 0x000C,
            PLAYER_QUEST_LOG_1_4 = UNIT_END + 0x000E,
            PLAYER_QUEST_LOG_2_1 = UNIT_END + 0x000F,
            PLAYER_QUEST_LOG_2_2 = UNIT_END + 0x0010,
            PLAYER_QUEST_LOG_2_3 = UNIT_END + 0x0011,
            PLAYER_QUEST_LOG_2_5 = UNIT_END + 0x0013,
            PLAYER_QUEST_LOG_3_1 = UNIT_END + 0x0014,
            PLAYER_QUEST_LOG_3_2 = UNIT_END + 0x0015,
            PLAYER_QUEST_LOG_3_3 = UNIT_END + 0x0016,
            PLAYER_QUEST_LOG_3_5 = UNIT_END + 0x0018,
            PLAYER_QUEST_LOG_4_1 = UNIT_END + 0x0019,
            PLAYER_QUEST_LOG_4_2 = UNIT_END + 0x001A,
            PLAYER_QUEST_LOG_4_3 = UNIT_END + 0x001B,
            PLAYER_QUEST_LOG_4_5 = UNIT_END + 0x001D,
            PLAYER_QUEST_LOG_5_1 = UNIT_END + 0x001E,
            PLAYER_QUEST_LOG_5_2 = UNIT_END + 0x001F,
            PLAYER_QUEST_LOG_5_3 = UNIT_END + 0x0020,
            PLAYER_QUEST_LOG_5_5 = UNIT_END + 0x0022,
            PLAYER_QUEST_LOG_6_1 = UNIT_END + 0x0023,
            PLAYER_QUEST_LOG_6_2 = UNIT_END + 0x0024,
            PLAYER_QUEST_LOG_6_3 = UNIT_END + 0x0025,
            PLAYER_QUEST_LOG_6_5 = UNIT_END + 0x0027,
            PLAYER_QUEST_LOG_7_1 = UNIT_END + 0x0028,
            PLAYER_QUEST_LOG_7_2 = UNIT_END + 0x0029,
            PLAYER_QUEST_LOG_7_3 = UNIT_END + 0x002A,
            PLAYER_QUEST_LOG_7_5 = UNIT_END + 0x002C,
            PLAYER_QUEST_LOG_8_1 = UNIT_END + 0x002D,
            PLAYER_QUEST_LOG_8_2 = UNIT_END + 0x002E,
            PLAYER_QUEST_LOG_8_3 = UNIT_END + 0x002F,
            PLAYER_QUEST_LOG_8_5 = UNIT_END + 0x0031,
            PLAYER_QUEST_LOG_9_1 = UNIT_END + 0x0032,
            PLAYER_QUEST_LOG_9_2 = UNIT_END + 0x0033,
            PLAYER_QUEST_LOG_9_3 = UNIT_END + 0x0034,
            PLAYER_QUEST_LOG_9_5 = UNIT_END + 0x0036,
            PLAYER_QUEST_LOG_10_1 = UNIT_END + 0x0037,
            PLAYER_QUEST_LOG_10_2 = UNIT_END + 0x0038,
            PLAYER_QUEST_LOG_10_3 = UNIT_END + 0x0039,
            PLAYER_QUEST_LOG_10_5 = UNIT_END + 0x003B,
            PLAYER_QUEST_LOG_11_1 = UNIT_END + 0x003C,
            PLAYER_QUEST_LOG_11_2 = UNIT_END + 0x003D,
            PLAYER_QUEST_LOG_11_3 = UNIT_END + 0x003E,
            PLAYER_QUEST_LOG_11_5 = UNIT_END + 0x0040,
            PLAYER_QUEST_LOG_12_1 = UNIT_END + 0x0041,
            PLAYER_QUEST_LOG_12_2 = UNIT_END + 0x0042,
            PLAYER_QUEST_LOG_12_3 = UNIT_END + 0x0043,
            PLAYER_QUEST_LOG_12_5 = UNIT_END + 0x0045,
            PLAYER_QUEST_LOG_13_1 = UNIT_END + 0x0046,
            PLAYER_QUEST_LOG_13_2 = UNIT_END + 0x0047,
            PLAYER_QUEST_LOG_13_3 = UNIT_END + 0x0048,
            PLAYER_QUEST_LOG_13_5 = UNIT_END + 0x004A,
            PLAYER_QUEST_LOG_14_1 = UNIT_END + 0x004B,
            PLAYER_QUEST_LOG_14_2 = UNIT_END + 0x004C,
            PLAYER_QUEST_LOG_14_3 = UNIT_END + 0x004D,
            PLAYER_QUEST_LOG_14_5 = UNIT_END + 0x004F,
            PLAYER_QUEST_LOG_15_1 = UNIT_END + 0x0050,
            PLAYER_QUEST_LOG_15_2 = UNIT_END + 0x0051,
            PLAYER_QUEST_LOG_15_3 = UNIT_END + 0x0052,
            PLAYER_QUEST_LOG_15_5 = UNIT_END + 0x0054,
            PLAYER_QUEST_LOG_16_1 = UNIT_END + 0x0055,
            PLAYER_QUEST_LOG_16_2 = UNIT_END + 0x0056,
            PLAYER_QUEST_LOG_16_3 = UNIT_END + 0x0057,
            PLAYER_QUEST_LOG_16_5 = UNIT_END + 0x0059,
            PLAYER_QUEST_LOG_17_1 = UNIT_END + 0x005A,
            PLAYER_QUEST_LOG_17_2 = UNIT_END + 0x005B,
            PLAYER_QUEST_LOG_17_3 = UNIT_END + 0x005C,
            PLAYER_QUEST_LOG_17_5 = UNIT_END + 0x005E,
            PLAYER_QUEST_LOG_18_1 = UNIT_END + 0x005F,
            PLAYER_QUEST_LOG_18_2 = UNIT_END + 0x0060,
            PLAYER_QUEST_LOG_18_3 = UNIT_END + 0x0061,
            PLAYER_QUEST_LOG_18_5 = UNIT_END + 0x0063,
            PLAYER_QUEST_LOG_19_1 = UNIT_END + 0x0064,
            PLAYER_QUEST_LOG_19_2 = UNIT_END + 0x0065,
            PLAYER_QUEST_LOG_19_3 = UNIT_END + 0x0066,
            PLAYER_QUEST_LOG_19_5 = UNIT_END + 0x0068,
            PLAYER_QUEST_LOG_20_1 = UNIT_END + 0x0069,
            PLAYER_QUEST_LOG_20_2 = UNIT_END + 0x006A,
            PLAYER_QUEST_LOG_20_3 = UNIT_END + 0x006B,
            PLAYER_QUEST_LOG_20_5 = UNIT_END + 0x006D,
            PLAYER_QUEST_LOG_21_1 = UNIT_END + 0x006E,
            PLAYER_QUEST_LOG_21_2 = UNIT_END + 0x006F,
            PLAYER_QUEST_LOG_21_3 = UNIT_END + 0x0070,
            PLAYER_QUEST_LOG_21_5 = UNIT_END + 0x0072,
            PLAYER_QUEST_LOG_22_1 = UNIT_END + 0x0073,
            PLAYER_QUEST_LOG_22_2 = UNIT_END + 0x0074,
            PLAYER_QUEST_LOG_22_3 = UNIT_END + 0x0075,
            PLAYER_QUEST_LOG_22_5 = UNIT_END + 0x0077,
            PLAYER_QUEST_LOG_23_1 = UNIT_END + 0x0078,
            PLAYER_QUEST_LOG_23_2 = UNIT_END + 0x0079,
            PLAYER_QUEST_LOG_23_3 = UNIT_END + 0x007A,
            PLAYER_QUEST_LOG_23_5 = UNIT_END + 0x007C,
            PLAYER_QUEST_LOG_24_1 = UNIT_END + 0x007D,
            PLAYER_QUEST_LOG_24_2 = UNIT_END + 0x007E,
            PLAYER_QUEST_LOG_24_3 = UNIT_END + 0x007F,
            PLAYER_QUEST_LOG_24_5 = UNIT_END + 0x0081,
            PLAYER_QUEST_LOG_25_1 = UNIT_END + 0x0082,
            PLAYER_QUEST_LOG_25_2 = UNIT_END + 0x0083,
            PLAYER_QUEST_LOG_25_3 = UNIT_END + 0x0084,
            PLAYER_QUEST_LOG_25_5 = UNIT_END + 0x0086,
            PLAYER_VISIBLE_ITEM_1_ENTRYID = UNIT_END + 0x0087,
            PLAYER_VISIBLE_ITEM_1_ENCHANTMENT = UNIT_END + 0x0088,
            PLAYER_VISIBLE_ITEM_2_ENTRYID = UNIT_END + 0x0089,
            PLAYER_VISIBLE_ITEM_2_ENCHANTMENT = UNIT_END + 0x008A,
            PLAYER_VISIBLE_ITEM_3_ENTRYID = UNIT_END + 0x008B,
            PLAYER_VISIBLE_ITEM_3_ENCHANTMENT = UNIT_END + 0x008C,
            PLAYER_VISIBLE_ITEM_4_ENTRYID = UNIT_END + 0x008D,
            PLAYER_VISIBLE_ITEM_4_ENCHANTMENT = UNIT_END + 0x008E,
            PLAYER_VISIBLE_ITEM_5_ENTRYID = UNIT_END + 0x008F,
            PLAYER_VISIBLE_ITEM_5_ENCHANTMENT = UNIT_END + 0x0090,
            PLAYER_VISIBLE_ITEM_6_ENTRYID = UNIT_END + 0x0091,
            PLAYER_VISIBLE_ITEM_6_ENCHANTMENT = UNIT_END + 0x0092,
            PLAYER_VISIBLE_ITEM_7_ENTRYID = UNIT_END + 0x0093,
            PLAYER_VISIBLE_ITEM_7_ENCHANTMENT = UNIT_END + 0x0094,
            PLAYER_VISIBLE_ITEM_8_ENTRYID = UNIT_END + 0x0095,
            PLAYER_VISIBLE_ITEM_8_ENCHANTMENT = UNIT_END + 0x0096,
            PLAYER_VISIBLE_ITEM_9_ENTRYID = UNIT_END + 0x0097,
            PLAYER_VISIBLE_ITEM_9_ENCHANTMENT = UNIT_END + 0x0098,
            PLAYER_VISIBLE_ITEM_10_ENTRYID = UNIT_END + 0x0099,
            PLAYER_VISIBLE_ITEM_10_ENCHANTMENT = UNIT_END + 0x009A,
            PLAYER_VISIBLE_ITEM_11_ENTRYID = UNIT_END + 0x009B,
            PLAYER_VISIBLE_ITEM_11_ENCHANTMENT = UNIT_END + 0x009C,
            PLAYER_VISIBLE_ITEM_12_ENTRYID = UNIT_END + 0x009D,
            PLAYER_VISIBLE_ITEM_12_ENCHANTMENT = UNIT_END + 0x009E,
            PLAYER_VISIBLE_ITEM_13_ENTRYID = UNIT_END + 0x009F,
            PLAYER_VISIBLE_ITEM_13_ENCHANTMENT = UNIT_END + 0x00A0,
            PLAYER_VISIBLE_ITEM_14_ENTRYID = UNIT_END + 0x00A1,
            PLAYER_VISIBLE_ITEM_14_ENCHANTMENT = UNIT_END + 0x00A2,
            PLAYER_VISIBLE_ITEM_15_ENTRYID = UNIT_END + 0x00A3,
            PLAYER_VISIBLE_ITEM_15_ENCHANTMENT = UNIT_END + 0x00A4,
            PLAYER_VISIBLE_ITEM_16_ENTRYID = UNIT_END + 0x00A5,
            PLAYER_VISIBLE_ITEM_16_ENCHANTMENT = UNIT_END + 0x00A6,
            PLAYER_VISIBLE_ITEM_17_ENTRYID = UNIT_END + 0x00A7,
            PLAYER_VISIBLE_ITEM_17_ENCHANTMENT = UNIT_END + 0x00A8,
            PLAYER_VISIBLE_ITEM_18_ENTRYID = UNIT_END + 0x00A9,
            PLAYER_VISIBLE_ITEM_18_ENCHANTMENT = UNIT_END + 0x00AA,
            PLAYER_VISIBLE_ITEM_19_ENTRYID = UNIT_END + 0x00AB,
            PLAYER_VISIBLE_ITEM_19_ENCHANTMENT = UNIT_END + 0x00AC,
            PLAYER_CHOSEN_TITLE = UNIT_END + 0x00AD,
            PLAYER_FAKE_INEBRIATION = UNIT_END + 0x00AE,
            PLAYER_FIELD_PAD_0 = UNIT_END + 0x00AF,
            PLAYER_FIELD_INV_SLOT_HEAD = UNIT_END + 0x00B0,
            PLAYER_FIELD_PACK_SLOT_1 = UNIT_END + 0x00DE,
            PLAYER_FIELD_BANK_SLOT_1 = UNIT_END + 0x00FE,
            PLAYER_FIELD_BANKBAG_SLOT_1 = UNIT_END + 0x0136,
            PLAYER_FIELD_VENDORBUYBACK_SLOT_1 = UNIT_END + 0x0144,
            PLAYER_FIELD_KEYRING_SLOT_1 = UNIT_END + 0x015C,
            PLAYER_FIELD_CURRENCYTOKEN_SLOT_1 = UNIT_END + 0x019C,
            PLAYER_FARSIGHT = UNIT_END + 0x01DC,
            PLAYER__FIELD_KNOWN_TITLES = UNIT_END + 0x01DE,
            PLAYER__FIELD_KNOWN_TITLES1 = UNIT_END + 0x01E0,
            PLAYER__FIELD_KNOWN_TITLES2 = UNIT_END + 0x01E2,
            PLAYER_FIELD_KNOWN_CURRENCIES = UNIT_END + 0x01E4,
            PLAYER_XP = UNIT_END + 0x01E6,
            PLAYER_NEXT_LEVEL_XP = UNIT_END + 0x01E7,
            PLAYER_SKILL_INFO_1_1 = UNIT_END + 0x01E8,
            PLAYER_CHARACTER_POINTS1 = UNIT_END + 0x0368,
            PLAYER_CHARACTER_POINTS2 = UNIT_END + 0x0369,
            PLAYER_TRACK_CREATURES = UNIT_END + 0x036A,
            PLAYER_TRACK_RESOURCES = UNIT_END + 0x036B,
            PLAYER_BLOCK_PERCENTAGE = UNIT_END + 0x036C,
            PLAYER_DODGE_PERCENTAGE = UNIT_END + 0x036D,
            PLAYER_PARRY_PERCENTAGE = UNIT_END + 0x036E,
            PLAYER_EXPERTISE = UNIT_END + 0x036F,
            PLAYER_OFFHAND_EXPERTISE = UNIT_END + 0x0370,
            PLAYER_CRIT_PERCENTAGE = UNIT_END + 0x0371,
            PLAYER_RANGED_CRIT_PERCENTAGE = UNIT_END + 0x0372,
            PLAYER_OFFHAND_CRIT_PERCENTAGE = UNIT_END + 0x0373,
            PLAYER_SPELL_CRIT_PERCENTAGE1 = UNIT_END + 0x0374,
            PLAYER_SHIELD_BLOCK = UNIT_END + 0x037B,
            PLAYER_SHIELD_BLOCK_CRIT_PERCENTAGE = UNIT_END + 0x037C,
            PLAYER_EXPLORED_ZONES_1 = UNIT_END + 0x037D,
            PLAYER_REST_STATE_EXPERIENCE = UNIT_END + 0x03FD,
            PLAYER_FIELD_COINAGE = UNIT_END + 0x03FE,
            PLAYER_FIELD_MOD_DAMAGE_DONE_POS = UNIT_END + 0x03FF,
            PLAYER_FIELD_MOD_DAMAGE_DONE_NEG = UNIT_END + 0x0406,
            PLAYER_FIELD_MOD_DAMAGE_DONE_PCT = UNIT_END + 0x040D,
            PLAYER_FIELD_MOD_HEALING_DONE_POS = UNIT_END + 0x0414,
            PLAYER_FIELD_MOD_HEALING_PCT = UNIT_END + 0x0415,
            PLAYER_FIELD_MOD_HEALING_DONE_PCT = UNIT_END + 0x0416,
            PLAYER_FIELD_MOD_TARGET_RESISTANCE = UNIT_END + 0x0417,
            PLAYER_FIELD_MOD_TARGET_PHYSICAL_RESISTANCE = UNIT_END + 0x0418,
            PLAYER_FIELD_BYTES = UNIT_END + 0x0419,
            PLAYER_AMMO_ID = UNIT_END + 0x041A,
            PLAYER_SELF_RES_SPELL = UNIT_END + 0x041B,
            PLAYER_FIELD_PVP_MEDALS = UNIT_END + 0x041C,
            PLAYER_FIELD_BUYBACK_PRICE_1 = UNIT_END + 0x041D,
            PLAYER_FIELD_BUYBACK_TIMESTAMP_1 = UNIT_END + 0x0429,
            PLAYER_FIELD_KILLS = UNIT_END + 0x0435,
            PLAYER_FIELD_TODAY_CONTRIBUTION = UNIT_END + 0x0436,
            PLAYER_FIELD_YESTERDAY_CONTRIBUTION = UNIT_END + 0x0437,
            PLAYER_FIELD_LIFETIME_HONORBALE_KILLS = UNIT_END + 0x0438,
            PLAYER_FIELD_BYTES2 = UNIT_END + 0x0439,
            PLAYER_FIELD_WATCHED_FACTION_INDEX = UNIT_END + 0x043A,
            PLAYER_FIELD_COMBAT_RATING_1 = UNIT_END + 0x043B,
            PLAYER_FIELD_ARENA_TEAM_INFO_1_1 = UNIT_END + 0x0454,
            PLAYER_FIELD_HONOR_CURRENCY = UNIT_END + 0x0469,
            PLAYER_FIELD_ARENA_CURRENCY = UNIT_END + 0x046A,
            PLAYER_FIELD_MAX_LEVEL = UNIT_END + 0x046B,
            PLAYER_FIELD_DAILY_QUESTS_1 = UNIT_END + 0x046C,
            PLAYER_RUNE_REGEN_1 = UNIT_END + 0x0485,
            PLAYER_NO_REAGENT_COST_1 = UNIT_END + 0x0489,
            PLAYER_FIELD_GLYPH_SLOTS_1 = UNIT_END + 0x048C,
            PLAYER_FIELD_GLYPHS_1 = UNIT_END + 0x0492,
            PLAYER_GLYPHS_ENABLED = UNIT_END + 0x0498,
            PLAYER_FIELD_PADDING = UNIT_END + 0x0499,
            MAX = UNIT_END + 0x049A
        }
    }
}
