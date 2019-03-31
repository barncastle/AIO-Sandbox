using System;
using Common.Constants;
using Common.Extensions;
using Common.Interfaces;
using Common.Structs;

namespace TBC_8089
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
            writer.WriteUInt32(0);  // MovementFlagMask
            writer.WriteUInt8(0); // MoveFlags2?
            writer.WriteUInt32((uint)Environment.TickCount);

            writer.WriteFloat(Location.X);  // x
            writer.WriteFloat(Location.Y);  // y
            writer.WriteFloat(Location.Z);  // z
            writer.WriteFloat(Location.O);  // w (o)
            writer.WriteInt32(0); // falltime

            writer.WriteFloat(2.5f); // WalkSpeed
            writer.WriteFloat(7.0f); // RunSpeed
            writer.WriteFloat(2.5f); // Backwards WalkSpeed
            writer.WriteFloat(4.7222f); // SwimSpeed
            writer.WriteFloat(4.7222f); // Backwards SwimSpeed
            writer.WriteFloat(7.0f); // FlySpeed
            writer.WriteFloat(4.7222f); // Backwards FlySpeed
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
            SetField(Fields.PLAYER_BYTES_2, ToUInt32(FacialHair, b4: RestedState));
            SetField(Fields.PLAYER_BYTES_3, ToUInt32(Gender));
            SetField(Fields.PLAYER_XP, 47);
            SetField(Fields.PLAYER_NEXT_LEVEL_XP, 200);
            SetField(Fields.PLAYER_FLAGS, 0);

            SetField(Fields.UNIT_FIELD_ATTACK_POWER, 1);
            SetField(Fields.UNIT_FIELD_ATTACK_POWER_MODS, 0);
            SetField(Fields.UNIT_FIELD_RANGED_ATTACK_POWER, 1);
            SetField(Fields.UNIT_FIELD_RANGED_ATTACK_POWER_MODS, 0);

            for (int i = 0; i < 64; i++)
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
                movementStatus.WriteUInt8(0);
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
            UNIT_FIELD_FLAGS_2 = OBJECT_END + 0x0029,
            UNIT_FIELD_AURA = OBJECT_END + 0x002A,
            UNIT_FIELD_AURAFLAGS = OBJECT_END + 0x0062,
            UNIT_FIELD_AURALEVELS = OBJECT_END + 0x0070,
            UNIT_FIELD_AURAAPPLICATIONS = OBJECT_END + 0x007E,
            UNIT_FIELD_AURASTATE = OBJECT_END + 0x008C,
            UNIT_FIELD_BASEATTACKTIME = OBJECT_END + 0x008D,
            UNIT_FIELD_RANGEDATTACKTIME = OBJECT_END + 0x008F,
            UNIT_FIELD_BOUNDINGRADIUS = OBJECT_END + 0x0090,
            UNIT_FIELD_COMBATREACH = OBJECT_END + 0x0091,
            UNIT_FIELD_DISPLAYID = OBJECT_END + 0x0092,
            UNIT_FIELD_NATIVEDISPLAYID = OBJECT_END + 0x0093,
            UNIT_FIELD_MOUNTDISPLAYID = OBJECT_END + 0x0094,
            UNIT_FIELD_MINDAMAGE = OBJECT_END + 0x0095,
            UNIT_FIELD_MAXDAMAGE = OBJECT_END + 0x0096,
            UNIT_FIELD_MINOFFHANDDAMAGE = OBJECT_END + 0x0097,
            UNIT_FIELD_MAXOFFHANDDAMAGE = OBJECT_END + 0x0098,
            UNIT_FIELD_BYTES_1 = OBJECT_END + 0x0099,
            UNIT_FIELD_PETNUMBER = OBJECT_END + 0x009A,
            UNIT_FIELD_PET_NAME_TIMESTAMP = OBJECT_END + 0x009B,
            UNIT_FIELD_PETEXPERIENCE = OBJECT_END + 0x009C,
            UNIT_FIELD_PETNEXTLEVELEXP = OBJECT_END + 0x009D,
            UNIT_DYNAMIC_FLAGS = OBJECT_END + 0x009E,
            UNIT_CHANNEL_SPELL = OBJECT_END + 0x009F,
            UNIT_MOD_CAST_SPEED = OBJECT_END + 0x00A0,
            UNIT_CREATED_BY_SPELL = OBJECT_END + 0x00A1,
            UNIT_NPC_FLAGS = OBJECT_END + 0x00A2,
            UNIT_NPC_EMOTESTATE = OBJECT_END + 0x00A3,
            UNIT_TRAINING_POINTS = OBJECT_END + 0x00A4,
            UNIT_FIELD_STAT0 = OBJECT_END + 0x00A5,
            UNIT_FIELD_STAT1 = OBJECT_END + 0x00A6,
            UNIT_FIELD_STAT2 = OBJECT_END + 0x00A7,
            UNIT_FIELD_STAT3 = OBJECT_END + 0x00A8,
            UNIT_FIELD_STAT4 = OBJECT_END + 0x00A9,
            UNIT_FIELD_POSSTAT0 = OBJECT_END + 0x00AA,
            UNIT_FIELD_POSSTAT1 = OBJECT_END + 0x00AB,
            UNIT_FIELD_POSSTAT2 = OBJECT_END + 0x00AC,
            UNIT_FIELD_POSSTAT3 = OBJECT_END + 0x00AD,
            UNIT_FIELD_POSSTAT4 = OBJECT_END + 0x00AE,
            UNIT_FIELD_NEGSTAT0 = OBJECT_END + 0x00AF,
            UNIT_FIELD_NEGSTAT1 = OBJECT_END + 0x00B0,
            UNIT_FIELD_NEGSTAT2 = OBJECT_END + 0x00B1,
            UNIT_FIELD_NEGSTAT3 = OBJECT_END + 0x00B2,
            UNIT_FIELD_NEGSTAT4 = OBJECT_END + 0x00B3,
            UNIT_FIELD_RESISTANCES = OBJECT_END + 0x00B4,
            UNIT_FIELD_RESISTANCEBUFFMODSPOSITIVE = OBJECT_END + 0x00BB,
            UNIT_FIELD_RESISTANCEBUFFMODSNEGATIVE = OBJECT_END + 0x00C2,
            UNIT_FIELD_BASE_MANA = OBJECT_END + 0x00C9,
            UNIT_FIELD_BASE_HEALTH = OBJECT_END + 0x00CA,
            UNIT_FIELD_BYTES_2 = OBJECT_END + 0x00CB,
            UNIT_FIELD_ATTACK_POWER = OBJECT_END + 0x00CC,
            UNIT_FIELD_ATTACK_POWER_MODS = OBJECT_END + 0x00CD,
            UNIT_FIELD_ATTACK_POWER_MULTIPLIER = OBJECT_END + 0x00CE,
            UNIT_FIELD_RANGED_ATTACK_POWER = OBJECT_END + 0x00CF,
            UNIT_FIELD_RANGED_ATTACK_POWER_MODS = OBJECT_END + 0x00D0,
            UNIT_FIELD_RANGED_ATTACK_POWER_MULTIPLIER = OBJECT_END + 0x00D1,
            UNIT_FIELD_MINRANGEDDAMAGE = OBJECT_END + 0x00D2,
            UNIT_FIELD_MAXRANGEDDAMAGE = OBJECT_END + 0x00D3,
            UNIT_FIELD_POWER_COST_MODIFIER = OBJECT_END + 0x00D4,
            UNIT_FIELD_POWER_COST_MULTIPLIER = OBJECT_END + 0x00DB,
            UNIT_FIELD_MAXHEALTHMODIFIER = OBJECT_END + 0x00E2,
            UNIT_FIELD_PADDING = OBJECT_END + 0x00E3,
            UNIT_END = OBJECT_END + 0x00E4,
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
            PLAYER_QUEST_LOG_1_4 = UNIT_END + 0x000D,
            PLAYER_QUEST_LOG_2_1 = UNIT_END + 0x000E,
            PLAYER_QUEST_LOG_2_2 = UNIT_END + 0x000F,
            PLAYER_QUEST_LOG_2_3 = UNIT_END + 0x0010,
            PLAYER_QUEST_LOG_2_4 = UNIT_END + 0x0011,
            PLAYER_QUEST_LOG_3_1 = UNIT_END + 0x0012,
            PLAYER_QUEST_LOG_3_2 = UNIT_END + 0x0013,
            PLAYER_QUEST_LOG_3_3 = UNIT_END + 0x0014,
            PLAYER_QUEST_LOG_3_4 = UNIT_END + 0x0015,
            PLAYER_QUEST_LOG_4_1 = UNIT_END + 0x0016,
            PLAYER_QUEST_LOG_4_2 = UNIT_END + 0x0017,
            PLAYER_QUEST_LOG_4_3 = UNIT_END + 0x0018,
            PLAYER_QUEST_LOG_4_4 = UNIT_END + 0x0019,
            PLAYER_QUEST_LOG_5_1 = UNIT_END + 0x001A,
            PLAYER_QUEST_LOG_5_2 = UNIT_END + 0x001B,
            PLAYER_QUEST_LOG_5_3 = UNIT_END + 0x001C,
            PLAYER_QUEST_LOG_5_4 = UNIT_END + 0x001D,
            PLAYER_QUEST_LOG_6_1 = UNIT_END + 0x001E,
            PLAYER_QUEST_LOG_6_2 = UNIT_END + 0x001F,
            PLAYER_QUEST_LOG_6_3 = UNIT_END + 0x0020,
            PLAYER_QUEST_LOG_6_4 = UNIT_END + 0x0021,
            PLAYER_QUEST_LOG_7_1 = UNIT_END + 0x0022,
            PLAYER_QUEST_LOG_7_2 = UNIT_END + 0x0023,
            PLAYER_QUEST_LOG_7_3 = UNIT_END + 0x0024,
            PLAYER_QUEST_LOG_7_4 = UNIT_END + 0x0025,
            PLAYER_QUEST_LOG_8_1 = UNIT_END + 0x0026,
            PLAYER_QUEST_LOG_8_2 = UNIT_END + 0x0027,
            PLAYER_QUEST_LOG_8_3 = UNIT_END + 0x0028,
            PLAYER_QUEST_LOG_8_4 = UNIT_END + 0x0029,
            PLAYER_QUEST_LOG_9_1 = UNIT_END + 0x002A,
            PLAYER_QUEST_LOG_9_2 = UNIT_END + 0x002B,
            PLAYER_QUEST_LOG_9_3 = UNIT_END + 0x002C,
            PLAYER_QUEST_LOG_9_4 = UNIT_END + 0x002D,
            PLAYER_QUEST_LOG_10_1 = UNIT_END + 0x002E,
            PLAYER_QUEST_LOG_10_2 = UNIT_END + 0x002F,
            PLAYER_QUEST_LOG_10_3 = UNIT_END + 0x0030,
            PLAYER_QUEST_LOG_10_4 = UNIT_END + 0x0031,
            PLAYER_QUEST_LOG_11_1 = UNIT_END + 0x0032,
            PLAYER_QUEST_LOG_11_2 = UNIT_END + 0x0033,
            PLAYER_QUEST_LOG_11_3 = UNIT_END + 0x0034,
            PLAYER_QUEST_LOG_11_4 = UNIT_END + 0x0035,
            PLAYER_QUEST_LOG_12_1 = UNIT_END + 0x0036,
            PLAYER_QUEST_LOG_12_2 = UNIT_END + 0x0037,
            PLAYER_QUEST_LOG_12_3 = UNIT_END + 0x0038,
            PLAYER_QUEST_LOG_12_4 = UNIT_END + 0x0039,
            PLAYER_QUEST_LOG_13_1 = UNIT_END + 0x003A,
            PLAYER_QUEST_LOG_13_2 = UNIT_END + 0x003B,
            PLAYER_QUEST_LOG_13_3 = UNIT_END + 0x003C,
            PLAYER_QUEST_LOG_13_4 = UNIT_END + 0x003D,
            PLAYER_QUEST_LOG_14_1 = UNIT_END + 0x003E,
            PLAYER_QUEST_LOG_14_2 = UNIT_END + 0x003F,
            PLAYER_QUEST_LOG_14_3 = UNIT_END + 0x0040,
            PLAYER_QUEST_LOG_14_4 = UNIT_END + 0x0041,
            PLAYER_QUEST_LOG_15_1 = UNIT_END + 0x0042,
            PLAYER_QUEST_LOG_15_2 = UNIT_END + 0x0043,
            PLAYER_QUEST_LOG_15_3 = UNIT_END + 0x0044,
            PLAYER_QUEST_LOG_15_4 = UNIT_END + 0x0045,
            PLAYER_QUEST_LOG_16_1 = UNIT_END + 0x0046,
            PLAYER_QUEST_LOG_16_2 = UNIT_END + 0x0047,
            PLAYER_QUEST_LOG_16_3 = UNIT_END + 0x0048,
            PLAYER_QUEST_LOG_16_4 = UNIT_END + 0x0049,
            PLAYER_QUEST_LOG_17_1 = UNIT_END + 0x004A,
            PLAYER_QUEST_LOG_17_2 = UNIT_END + 0x004B,
            PLAYER_QUEST_LOG_17_3 = UNIT_END + 0x004C,
            PLAYER_QUEST_LOG_17_4 = UNIT_END + 0x004D,
            PLAYER_QUEST_LOG_18_1 = UNIT_END + 0x004E,
            PLAYER_QUEST_LOG_18_2 = UNIT_END + 0x004F,
            PLAYER_QUEST_LOG_18_3 = UNIT_END + 0x0050,
            PLAYER_QUEST_LOG_18_4 = UNIT_END + 0x0051,
            PLAYER_QUEST_LOG_19_1 = UNIT_END + 0x0052,
            PLAYER_QUEST_LOG_19_2 = UNIT_END + 0x0053,
            PLAYER_QUEST_LOG_19_3 = UNIT_END + 0x0054,
            PLAYER_QUEST_LOG_19_4 = UNIT_END + 0x0055,
            PLAYER_QUEST_LOG_20_1 = UNIT_END + 0x0056,
            PLAYER_QUEST_LOG_20_2 = UNIT_END + 0x0057,
            PLAYER_QUEST_LOG_20_3 = UNIT_END + 0x0058,
            PLAYER_QUEST_LOG_20_4 = UNIT_END + 0x0059,
            PLAYER_QUEST_LOG_21_1 = UNIT_END + 0x005A,
            PLAYER_QUEST_LOG_21_2 = UNIT_END + 0x005B,
            PLAYER_QUEST_LOG_21_3 = UNIT_END + 0x005C,
            PLAYER_QUEST_LOG_21_4 = UNIT_END + 0x005D,
            PLAYER_QUEST_LOG_22_1 = UNIT_END + 0x005E,
            PLAYER_QUEST_LOG_22_2 = UNIT_END + 0x005F,
            PLAYER_QUEST_LOG_22_3 = UNIT_END + 0x0060,
            PLAYER_QUEST_LOG_22_4 = UNIT_END + 0x0061,
            PLAYER_QUEST_LOG_23_1 = UNIT_END + 0x0062,
            PLAYER_QUEST_LOG_23_2 = UNIT_END + 0x0063,
            PLAYER_QUEST_LOG_23_3 = UNIT_END + 0x0064,
            PLAYER_QUEST_LOG_23_4 = UNIT_END + 0x0065,
            PLAYER_QUEST_LOG_24_1 = UNIT_END + 0x0066,
            PLAYER_QUEST_LOG_24_2 = UNIT_END + 0x0067,
            PLAYER_QUEST_LOG_24_3 = UNIT_END + 0x0068,
            PLAYER_QUEST_LOG_24_4 = UNIT_END + 0x0069,
            PLAYER_QUEST_LOG_25_1 = UNIT_END + 0x006A,
            PLAYER_QUEST_LOG_25_2 = UNIT_END + 0x006B,
            PLAYER_QUEST_LOG_25_3 = UNIT_END + 0x006C,
            PLAYER_QUEST_LOG_25_4 = UNIT_END + 0x006D,
            PLAYER_VISIBLE_ITEM_1_CREATOR = UNIT_END + 0x006E,
            PLAYER_VISIBLE_ITEM_1_0 = UNIT_END + 0x0070,
            PLAYER_VISIBLE_ITEM_1_PROPERTIES = UNIT_END + 0x007C,
            PLAYER_VISIBLE_ITEM_1_PAD = UNIT_END + 0x007D,
            PLAYER_VISIBLE_ITEM_2_CREATOR = UNIT_END + 0x007E,
            PLAYER_VISIBLE_ITEM_2_0 = UNIT_END + 0x0080,
            PLAYER_VISIBLE_ITEM_2_PROPERTIES = UNIT_END + 0x008C,
            PLAYER_VISIBLE_ITEM_2_PAD = UNIT_END + 0x008D,
            PLAYER_VISIBLE_ITEM_3_CREATOR = UNIT_END + 0x008E,
            PLAYER_VISIBLE_ITEM_3_0 = UNIT_END + 0x0090,
            PLAYER_VISIBLE_ITEM_3_PROPERTIES = UNIT_END + 0x009C,
            PLAYER_VISIBLE_ITEM_3_PAD = UNIT_END + 0x009D,
            PLAYER_VISIBLE_ITEM_4_CREATOR = UNIT_END + 0x009E,
            PLAYER_VISIBLE_ITEM_4_0 = UNIT_END + 0x00A0,
            PLAYER_VISIBLE_ITEM_4_PROPERTIES = UNIT_END + 0x00AC,
            PLAYER_VISIBLE_ITEM_4_PAD = UNIT_END + 0x00AD,
            PLAYER_VISIBLE_ITEM_5_CREATOR = UNIT_END + 0x00AE,
            PLAYER_VISIBLE_ITEM_5_0 = UNIT_END + 0x00B0,
            PLAYER_VISIBLE_ITEM_5_PROPERTIES = UNIT_END + 0x00BC,
            PLAYER_VISIBLE_ITEM_5_PAD = UNIT_END + 0x00BD,
            PLAYER_VISIBLE_ITEM_6_CREATOR = UNIT_END + 0x00BE,
            PLAYER_VISIBLE_ITEM_6_0 = UNIT_END + 0x00C0,
            PLAYER_VISIBLE_ITEM_6_PROPERTIES = UNIT_END + 0x00CC,
            PLAYER_VISIBLE_ITEM_6_PAD = UNIT_END + 0x00CD,
            PLAYER_VISIBLE_ITEM_7_CREATOR = UNIT_END + 0x00CE,
            PLAYER_VISIBLE_ITEM_7_0 = UNIT_END + 0x00D0,
            PLAYER_VISIBLE_ITEM_7_PROPERTIES = UNIT_END + 0x00DC,
            PLAYER_VISIBLE_ITEM_7_PAD = UNIT_END + 0x00DD,
            PLAYER_VISIBLE_ITEM_8_CREATOR = UNIT_END + 0x00DE,
            PLAYER_VISIBLE_ITEM_8_0 = UNIT_END + 0x00E0,
            PLAYER_VISIBLE_ITEM_8_PROPERTIES = UNIT_END + 0x00EC,
            PLAYER_VISIBLE_ITEM_8_PAD = UNIT_END + 0x00ED,
            PLAYER_VISIBLE_ITEM_9_CREATOR = UNIT_END + 0x00EE,
            PLAYER_VISIBLE_ITEM_9_0 = UNIT_END + 0x00F0,
            PLAYER_VISIBLE_ITEM_9_PROPERTIES = UNIT_END + 0x00FC,
            PLAYER_VISIBLE_ITEM_9_PAD = UNIT_END + 0x00FD,
            PLAYER_VISIBLE_ITEM_10_CREATOR = UNIT_END + 0x00FE,
            PLAYER_VISIBLE_ITEM_10_0 = UNIT_END + 0x0100,
            PLAYER_VISIBLE_ITEM_10_PROPERTIES = UNIT_END + 0x010C,
            PLAYER_VISIBLE_ITEM_10_PAD = UNIT_END + 0x010D,
            PLAYER_VISIBLE_ITEM_11_CREATOR = UNIT_END + 0x010E,
            PLAYER_VISIBLE_ITEM_11_0 = UNIT_END + 0x0110,
            PLAYER_VISIBLE_ITEM_11_PROPERTIES = UNIT_END + 0x011C,
            PLAYER_VISIBLE_ITEM_11_PAD = UNIT_END + 0x011D,
            PLAYER_VISIBLE_ITEM_12_CREATOR = UNIT_END + 0x011E,
            PLAYER_VISIBLE_ITEM_12_0 = UNIT_END + 0x0120,
            PLAYER_VISIBLE_ITEM_12_PROPERTIES = UNIT_END + 0x012C,
            PLAYER_VISIBLE_ITEM_12_PAD = UNIT_END + 0x012D,
            PLAYER_VISIBLE_ITEM_13_CREATOR = UNIT_END + 0x012E,
            PLAYER_VISIBLE_ITEM_13_0 = UNIT_END + 0x0130,
            PLAYER_VISIBLE_ITEM_13_PROPERTIES = UNIT_END + 0x013C,
            PLAYER_VISIBLE_ITEM_13_PAD = UNIT_END + 0x013D,
            PLAYER_VISIBLE_ITEM_14_CREATOR = UNIT_END + 0x013E,
            PLAYER_VISIBLE_ITEM_14_0 = UNIT_END + 0x0140,
            PLAYER_VISIBLE_ITEM_14_PROPERTIES = UNIT_END + 0x014C,
            PLAYER_VISIBLE_ITEM_14_PAD = UNIT_END + 0x014D,
            PLAYER_VISIBLE_ITEM_15_CREATOR = UNIT_END + 0x014E,
            PLAYER_VISIBLE_ITEM_15_0 = UNIT_END + 0x0150,
            PLAYER_VISIBLE_ITEM_15_PROPERTIES = UNIT_END + 0x015C,
            PLAYER_VISIBLE_ITEM_15_PAD = UNIT_END + 0x015D,
            PLAYER_VISIBLE_ITEM_16_CREATOR = UNIT_END + 0x015E,
            PLAYER_VISIBLE_ITEM_16_0 = UNIT_END + 0x0160,
            PLAYER_VISIBLE_ITEM_16_PROPERTIES = UNIT_END + 0x016C,
            PLAYER_VISIBLE_ITEM_16_PAD = UNIT_END + 0x016D,
            PLAYER_VISIBLE_ITEM_17_CREATOR = UNIT_END + 0x016E,
            PLAYER_VISIBLE_ITEM_17_0 = UNIT_END + 0x0170,
            PLAYER_VISIBLE_ITEM_17_PROPERTIES = UNIT_END + 0x017C,
            PLAYER_VISIBLE_ITEM_17_PAD = UNIT_END + 0x017D,
            PLAYER_VISIBLE_ITEM_18_CREATOR = UNIT_END + 0x017E,
            PLAYER_VISIBLE_ITEM_18_0 = UNIT_END + 0x0180,
            PLAYER_VISIBLE_ITEM_18_PROPERTIES = UNIT_END + 0x018C,
            PLAYER_VISIBLE_ITEM_18_PAD = UNIT_END + 0x018D,
            PLAYER_VISIBLE_ITEM_19_CREATOR = UNIT_END + 0x018E,
            PLAYER_VISIBLE_ITEM_19_0 = UNIT_END + 0x0190,
            PLAYER_VISIBLE_ITEM_19_PROPERTIES = UNIT_END + 0x019C,
            PLAYER_VISIBLE_ITEM_19_PAD = UNIT_END + 0x019D,
            PLAYER_CHOSEN_TITLE = UNIT_END + 0x019E,
            PLAYER_FIELD_PAD_0 = UNIT_END + 0x019F,
            PLAYER_FIELD_INV_SLOT_HEAD = UNIT_END + 0x01A0,
            PLAYER_FIELD_PACK_SLOT_1 = UNIT_END + 0x01CE,
            PLAYER_FIELD_BANK_SLOT_1 = UNIT_END + 0x01EE,
            PLAYER_FIELD_BANKBAG_SLOT_1 = UNIT_END + 0x0226,
            PLAYER_FIELD_VENDORBUYBACK_SLOT_1 = UNIT_END + 0x0234,
            PLAYER_FIELD_KEYRING_SLOT_1 = UNIT_END + 0x024C,
            PLAYER_FIELD_VANITYPET_SLOT_1 = UNIT_END + 0x028C,
            PLAYER_FARSIGHT = UNIT_END + 0x02B0,
            PLAYER__FIELD_KNOWN_TITLES = UNIT_END + 0x02B2,
            PLAYER_XP = UNIT_END + 0x02B4,
            PLAYER_NEXT_LEVEL_XP = UNIT_END + 0x02B5,
            PLAYER_SKILL_INFO_1_1 = UNIT_END + 0x02B6,
            PLAYER_CHARACTER_POINTS1 = UNIT_END + 0x0436,
            PLAYER_CHARACTER_POINTS2 = UNIT_END + 0x0437,
            PLAYER_TRACK_CREATURES = UNIT_END + 0x0438,
            PLAYER_TRACK_RESOURCES = UNIT_END + 0x0439,
            PLAYER_BLOCK_PERCENTAGE = UNIT_END + 0x043A,
            PLAYER_DODGE_PERCENTAGE = UNIT_END + 0x043B,
            PLAYER_PARRY_PERCENTAGE = UNIT_END + 0x043C,
            PLAYER_EXPERTISE = UNIT_END + 0x043D,
            PLAYER_OFFHAND_EXPERTISE = UNIT_END + 0x043E,
            PLAYER_CRIT_PERCENTAGE = UNIT_END + 0x043F,
            PLAYER_RANGED_CRIT_PERCENTAGE = UNIT_END + 0x0440,
            PLAYER_OFFHAND_CRIT_PERCENTAGE = UNIT_END + 0x0441,
            PLAYER_SPELL_CRIT_PERCENTAGE1 = UNIT_END + 0x0442,
            PLAYER_SHIELD_BLOCK = UNIT_END + 0x0449,
            PLAYER_EXPLORED_ZONES_1 = UNIT_END + 0x044A,
            PLAYER_REST_STATE_EXPERIENCE = UNIT_END + 0x048A,
            PLAYER_FIELD_COINAGE = UNIT_END + 0x048B,
            PLAYER_FIELD_MOD_DAMAGE_DONE_POS = UNIT_END + 0x048C,
            PLAYER_FIELD_MOD_DAMAGE_DONE_NEG = UNIT_END + 0x0493,
            PLAYER_FIELD_MOD_DAMAGE_DONE_PCT = UNIT_END + 0x049A,
            PLAYER_FIELD_MOD_HEALING_DONE_POS = UNIT_END + 0x04A1,
            PLAYER_FIELD_MOD_TARGET_RESISTANCE = UNIT_END + 0x04A2,
            PLAYER_FIELD_MOD_TARGET_PHYSICAL_RESISTANCE = UNIT_END + 0x04A3,
            PLAYER_FIELD_BYTES = UNIT_END + 0x04A4,
            PLAYER_AMMO_ID = UNIT_END + 0x04A5,
            PLAYER_SELF_RES_SPELL = UNIT_END + 0x04A6,
            PLAYER_FIELD_PVP_MEDALS = UNIT_END + 0x04A7,
            PLAYER_FIELD_BUYBACK_PRICE_1 = UNIT_END + 0x04A8,
            PLAYER_FIELD_BUYBACK_TIMESTAMP_1 = UNIT_END + 0x04B4,
            PLAYER_FIELD_KILLS = UNIT_END + 0x04C0,
            PLAYER_FIELD_TODAY_CONTRIBUTION = UNIT_END + 0x04C1,
            PLAYER_FIELD_YESTERDAY_CONTRIBUTION = UNIT_END + 0x04C2,
            PLAYER_FIELD_LIFETIME_HONORBALE_KILLS = UNIT_END + 0x04C3,
            PLAYER_FIELD_BYTES2 = UNIT_END + 0x04C4,
            PLAYER_FIELD_WATCHED_FACTION_INDEX = UNIT_END + 0x04C5,
            PLAYER_FIELD_COMBAT_RATING_1 = UNIT_END + 0x04C6,
            PLAYER_FIELD_ARENA_TEAM_INFO_1_1 = UNIT_END + 0x04DE,
            PLAYER_FIELD_HONOR_CURRENCY = UNIT_END + 0x04F0,
            PLAYER_FIELD_ARENA_CURRENCY = UNIT_END + 0x04F1,
            PLAYER_FIELD_MOD_MANA_REGEN = UNIT_END + 0x04F2,
            PLAYER_FIELD_MOD_MANA_REGEN_INTERRUPT = UNIT_END + 0x04F3,
            PLAYER_FIELD_MAX_LEVEL = UNIT_END + 0x04F4,
            PLAYER_FIELD_DAILY_QUESTS_1 = UNIT_END + 0x04F5,
            MAX = UNIT_END + 0x050E
        }
    }
}
