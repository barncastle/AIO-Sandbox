using System;
using Common.Extensions;
using Common.Interfaces;
using Common.Structs;

namespace TBC_5894
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
            SetField(Fields.UNIT_FIELD_LEVEL, 0);// this.Level);
            SetField(Fields.UNIT_FIELD_BYTES_0, BitConverter.ToUInt32(new byte[] { Race, Class, Gender, PowerType }, 0));
            SetField(Fields.UNIT_FIELD_STAT0, Strength);
            SetField(Fields.UNIT_FIELD_STAT1, Agility);
            SetField(Fields.UNIT_FIELD_STAT2, Stamina);
            SetField(Fields.UNIT_FIELD_STAT3, Intellect);
            SetField(Fields.UNIT_FIELD_STAT4, Spirit);
            SetField(Fields.UNIT_FIELD_FLAGS, 8);
            SetField(Fields.UNIT_FIELD_BASE_MANA, Mana);
            SetField(Fields.UNIT_FIELD_DISPLAYID, DisplayId);
            SetField(Fields.UNIT_FIELD_MOUNTDISPLAYID, MountDisplayId);
            SetField(Fields.UNIT_FIELD_BYTES_1, BitConverter.ToUInt32(new byte[] { (byte)StandState, 0, 0, 0 }, 0));
            SetField(Fields.UNIT_FIELD_BYTES_2, 0);
            SetField(Fields.PLAYER_BYTES, BitConverter.ToUInt32(new byte[] { Skin, Face, HairStyle, HairColor }, 0));
            SetField(Fields.PLAYER_BYTES_2, BitConverter.ToUInt32(new byte[] { FacialHair, 0, 0, RestedState }, 0));
            SetField(Fields.PLAYER_BYTES_3, (uint)Gender);
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

        public override IPacketWriter BuildForceSpeed(float modifier, bool swim = false)
        {
            var opcode = swim ? global::Opcodes.SMSG_FORCE_SWIM_SPEED_CHANGE : global::Opcodes.SMSG_FORCE_SPEED_CHANGE;
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
            UNIT_FIELD_AURAFLAGS = OBJECT_END + 0x0061,
            UNIT_FIELD_AURALEVELS = OBJECT_END + 0x0068,
            UNIT_FIELD_AURAAPPLICATIONS = OBJECT_END + 0x0076,
            UNIT_FIELD_AURASTATE = OBJECT_END + 0x0084,
            UNIT_FIELD_BASEATTACKTIME = OBJECT_END + 0x0085,
            UNIT_FIELD_RANGEDATTACKTIME = OBJECT_END + 0x0087,
            UNIT_FIELD_BOUNDINGRADIUS = OBJECT_END + 0x0088,
            UNIT_FIELD_COMBATREACH = OBJECT_END + 0x0089,
            UNIT_FIELD_DISPLAYID = OBJECT_END + 0x008A,
            UNIT_FIELD_NATIVEDISPLAYID = OBJECT_END + 0x008B,
            UNIT_FIELD_MOUNTDISPLAYID = OBJECT_END + 0x008C,
            UNIT_FIELD_MINDAMAGE = OBJECT_END + 0x008D,
            UNIT_FIELD_MAXDAMAGE = OBJECT_END + 0x008E,
            UNIT_FIELD_MINOFFHANDDAMAGE = OBJECT_END + 0x008F,
            UNIT_FIELD_MAXOFFHANDDAMAGE = OBJECT_END + 0x0090,
            UNIT_FIELD_BYTES_1 = OBJECT_END + 0x0091,
            UNIT_FIELD_PETNUMBER = OBJECT_END + 0x0092,
            UNIT_FIELD_PET_NAME_TIMESTAMP = OBJECT_END + 0x0093,
            UNIT_FIELD_PETEXPERIENCE = OBJECT_END + 0x0094,
            UNIT_FIELD_PETNEXTLEVELEXP = OBJECT_END + 0x0095,
            UNIT_DYNAMIC_FLAGS = OBJECT_END + 0x0096,
            UNIT_CHANNEL_SPELL = OBJECT_END + 0x0097,
            UNIT_MOD_CAST_SPEED = OBJECT_END + 0x0098,
            UNIT_CREATED_BY_SPELL = OBJECT_END + 0x0099,
            UNIT_NPC_FLAGS = OBJECT_END + 0x009A,
            UNIT_NPC_EMOTESTATE = OBJECT_END + 0x009B,
            UNIT_TRAINING_POINTS = OBJECT_END + 0x009C,
            UNIT_FIELD_STAT0 = OBJECT_END + 0x009D,
            UNIT_FIELD_STAT1 = OBJECT_END + 0x009E,
            UNIT_FIELD_STAT2 = OBJECT_END + 0x009F,
            UNIT_FIELD_STAT3 = OBJECT_END + 0x00A0,
            UNIT_FIELD_STAT4 = OBJECT_END + 0x00A1,
            UNIT_FIELD_RESISTANCES = OBJECT_END + 0x00A2,
            UNIT_FIELD_BASE_MANA = OBJECT_END + 0x00A9,
            UNIT_FIELD_BASE_HEALTH = OBJECT_END + 0x00AA,
            UNIT_FIELD_BYTES_2 = OBJECT_END + 0x00AB,
            UNIT_FIELD_ATTACK_POWER = OBJECT_END + 0x00AC,
            UNIT_FIELD_ATTACK_POWER_MODS = OBJECT_END + 0x00AD,
            UNIT_FIELD_ATTACK_POWER_MULTIPLIER = OBJECT_END + 0x00AE,
            UNIT_FIELD_RANGED_ATTACK_POWER = OBJECT_END + 0x00AF,
            UNIT_FIELD_RANGED_ATTACK_POWER_MODS = OBJECT_END + 0x00B0,
            UNIT_FIELD_RANGED_ATTACK_POWER_MULTIPLIER = OBJECT_END + 0x00B1,
            UNIT_FIELD_MINRANGEDDAMAGE = OBJECT_END + 0x00B2,
            UNIT_FIELD_MAXRANGEDDAMAGE = OBJECT_END + 0x00B3,
            UNIT_FIELD_POWER_COST_MODIFIER = OBJECT_END + 0x00B4,
            UNIT_FIELD_POWER_COST_MULTIPLIER = OBJECT_END + 0x00BB,
            UNIT_END = OBJECT_END + 0x00C2,
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
            PLAYER_QUEST_LOG_2_1 = UNIT_END + 0x000D,
            PLAYER_QUEST_LOG_2_2 = UNIT_END + 0x000E,
            PLAYER_QUEST_LOG_3_1 = UNIT_END + 0x0010,
            PLAYER_QUEST_LOG_3_2 = UNIT_END + 0x0011,
            PLAYER_QUEST_LOG_4_1 = UNIT_END + 0x0013,
            PLAYER_QUEST_LOG_4_2 = UNIT_END + 0x0014,
            PLAYER_QUEST_LOG_5_1 = UNIT_END + 0x0016,
            PLAYER_QUEST_LOG_5_2 = UNIT_END + 0x0017,
            PLAYER_QUEST_LOG_6_1 = UNIT_END + 0x0019,
            PLAYER_QUEST_LOG_6_2 = UNIT_END + 0x001A,
            PLAYER_QUEST_LOG_7_1 = UNIT_END + 0x001C,
            PLAYER_QUEST_LOG_7_2 = UNIT_END + 0x001D,
            PLAYER_QUEST_LOG_8_1 = UNIT_END + 0x001F,
            PLAYER_QUEST_LOG_8_2 = UNIT_END + 0x0020,
            PLAYER_QUEST_LOG_9_1 = UNIT_END + 0x0022,
            PLAYER_QUEST_LOG_9_2 = UNIT_END + 0x0023,
            PLAYER_QUEST_LOG_10_1 = UNIT_END + 0x0025,
            PLAYER_QUEST_LOG_10_2 = UNIT_END + 0x0026,
            PLAYER_QUEST_LOG_11_1 = UNIT_END + 0x0028,
            PLAYER_QUEST_LOG_11_2 = UNIT_END + 0x0029,
            PLAYER_QUEST_LOG_12_1 = UNIT_END + 0x002B,
            PLAYER_QUEST_LOG_12_2 = UNIT_END + 0x002C,
            PLAYER_QUEST_LOG_13_1 = UNIT_END + 0x002E,
            PLAYER_QUEST_LOG_13_2 = UNIT_END + 0x002F,
            PLAYER_QUEST_LOG_14_1 = UNIT_END + 0x0031,
            PLAYER_QUEST_LOG_14_2 = UNIT_END + 0x0032,
            PLAYER_QUEST_LOG_15_1 = UNIT_END + 0x0034,
            PLAYER_QUEST_LOG_15_2 = UNIT_END + 0x0035,
            PLAYER_QUEST_LOG_16_1 = UNIT_END + 0x0037,
            PLAYER_QUEST_LOG_16_2 = UNIT_END + 0x0038,
            PLAYER_QUEST_LOG_17_1 = UNIT_END + 0x003A,
            PLAYER_QUEST_LOG_17_2 = UNIT_END + 0x003B,
            PLAYER_QUEST_LOG_18_1 = UNIT_END + 0x003D,
            PLAYER_QUEST_LOG_18_2 = UNIT_END + 0x003E,
            PLAYER_QUEST_LOG_19_1 = UNIT_END + 0x0040,
            PLAYER_QUEST_LOG_19_2 = UNIT_END + 0x0041,
            PLAYER_QUEST_LOG_20_1 = UNIT_END + 0x0043,
            PLAYER_QUEST_LOG_20_2 = UNIT_END + 0x0044,
            PLAYER_QUEST_LOG_21_1 = UNIT_END + 0x0046,
            PLAYER_QUEST_LOG_21_2 = UNIT_END + 0x0047,
            PLAYER_QUEST_LOG_22_1 = UNIT_END + 0x0049,
            PLAYER_QUEST_LOG_22_2 = UNIT_END + 0x004A,
            PLAYER_QUEST_LOG_23_1 = UNIT_END + 0x004C,
            PLAYER_QUEST_LOG_23_2 = UNIT_END + 0x004D,
            PLAYER_QUEST_LOG_24_1 = UNIT_END + 0x004F,
            PLAYER_QUEST_LOG_24_2 = UNIT_END + 0x0050,
            PLAYER_QUEST_LOG_25_1 = UNIT_END + 0x0052,
            PLAYER_QUEST_LOG_25_2 = UNIT_END + 0x0053,
            PLAYER_VISIBLE_ITEM_1_CREATOR = UNIT_END + 0x0055,
            PLAYER_VISIBLE_ITEM_1_0 = UNIT_END + 0x0057,
            PLAYER_VISIBLE_ITEM_1_PROPERTIES = UNIT_END + 0x0063,
            PLAYER_VISIBLE_ITEM_1_PAD = UNIT_END + 0x0064,
            PLAYER_VISIBLE_ITEM_2_CREATOR = UNIT_END + 0x0065,
            PLAYER_VISIBLE_ITEM_2_0 = UNIT_END + 0x0067,
            PLAYER_VISIBLE_ITEM_2_PROPERTIES = UNIT_END + 0x0073,
            PLAYER_VISIBLE_ITEM_2_PAD = UNIT_END + 0x0074,
            PLAYER_VISIBLE_ITEM_3_CREATOR = UNIT_END + 0x0075,
            PLAYER_VISIBLE_ITEM_3_0 = UNIT_END + 0x0077,
            PLAYER_VISIBLE_ITEM_3_PROPERTIES = UNIT_END + 0x0083,
            PLAYER_VISIBLE_ITEM_3_PAD = UNIT_END + 0x0084,
            PLAYER_VISIBLE_ITEM_4_CREATOR = UNIT_END + 0x0085,
            PLAYER_VISIBLE_ITEM_4_0 = UNIT_END + 0x0087,
            PLAYER_VISIBLE_ITEM_4_PROPERTIES = UNIT_END + 0x0093,
            PLAYER_VISIBLE_ITEM_4_PAD = UNIT_END + 0x0094,
            PLAYER_VISIBLE_ITEM_5_CREATOR = UNIT_END + 0x0095,
            PLAYER_VISIBLE_ITEM_5_0 = UNIT_END + 0x0097,
            PLAYER_VISIBLE_ITEM_5_PROPERTIES = UNIT_END + 0x00A3,
            PLAYER_VISIBLE_ITEM_5_PAD = UNIT_END + 0x00A4,
            PLAYER_VISIBLE_ITEM_6_CREATOR = UNIT_END + 0x00A5,
            PLAYER_VISIBLE_ITEM_6_0 = UNIT_END + 0x00A7,
            PLAYER_VISIBLE_ITEM_6_PROPERTIES = UNIT_END + 0x00B3,
            PLAYER_VISIBLE_ITEM_6_PAD = UNIT_END + 0x00B4,
            PLAYER_VISIBLE_ITEM_7_CREATOR = UNIT_END + 0x00B5,
            PLAYER_VISIBLE_ITEM_7_0 = UNIT_END + 0x00B7,
            PLAYER_VISIBLE_ITEM_7_PROPERTIES = UNIT_END + 0x00C3,
            PLAYER_VISIBLE_ITEM_7_PAD = UNIT_END + 0x00C4,
            PLAYER_VISIBLE_ITEM_8_CREATOR = UNIT_END + 0x00C5,
            PLAYER_VISIBLE_ITEM_8_0 = UNIT_END + 0x00C7,
            PLAYER_VISIBLE_ITEM_8_PROPERTIES = UNIT_END + 0x00D3,
            PLAYER_VISIBLE_ITEM_8_PAD = UNIT_END + 0x00D4,
            PLAYER_VISIBLE_ITEM_9_CREATOR = UNIT_END + 0x00D5,
            PLAYER_VISIBLE_ITEM_9_0 = UNIT_END + 0x00D7,
            PLAYER_VISIBLE_ITEM_9_PROPERTIES = UNIT_END + 0x00E3,
            PLAYER_VISIBLE_ITEM_9_PAD = UNIT_END + 0x00E4,
            PLAYER_VISIBLE_ITEM_10_CREATOR = UNIT_END + 0x00E5,
            PLAYER_VISIBLE_ITEM_10_0 = UNIT_END + 0x00E7,
            PLAYER_VISIBLE_ITEM_10_PROPERTIES = UNIT_END + 0x00F3,
            PLAYER_VISIBLE_ITEM_10_PAD = UNIT_END + 0x00F4,
            PLAYER_VISIBLE_ITEM_11_CREATOR = UNIT_END + 0x00F5,
            PLAYER_VISIBLE_ITEM_11_0 = UNIT_END + 0x00F7,
            PLAYER_VISIBLE_ITEM_11_PROPERTIES = UNIT_END + 0x0103,
            PLAYER_VISIBLE_ITEM_11_PAD = UNIT_END + 0x0104,
            PLAYER_VISIBLE_ITEM_12_CREATOR = UNIT_END + 0x0105,
            PLAYER_VISIBLE_ITEM_12_0 = UNIT_END + 0x0107,
            PLAYER_VISIBLE_ITEM_12_PROPERTIES = UNIT_END + 0x0113,
            PLAYER_VISIBLE_ITEM_12_PAD = UNIT_END + 0x0114,
            PLAYER_VISIBLE_ITEM_13_CREATOR = UNIT_END + 0x0115,
            PLAYER_VISIBLE_ITEM_13_0 = UNIT_END + 0x0117,
            PLAYER_VISIBLE_ITEM_13_PROPERTIES = UNIT_END + 0x0123,
            PLAYER_VISIBLE_ITEM_13_PAD = UNIT_END + 0x0124,
            PLAYER_VISIBLE_ITEM_14_CREATOR = UNIT_END + 0x0125,
            PLAYER_VISIBLE_ITEM_14_0 = UNIT_END + 0x0127,
            PLAYER_VISIBLE_ITEM_14_PROPERTIES = UNIT_END + 0x0133,
            PLAYER_VISIBLE_ITEM_14_PAD = UNIT_END + 0x0134,
            PLAYER_VISIBLE_ITEM_15_CREATOR = UNIT_END + 0x0135,
            PLAYER_VISIBLE_ITEM_15_0 = UNIT_END + 0x0137,
            PLAYER_VISIBLE_ITEM_15_PROPERTIES = UNIT_END + 0x0143,
            PLAYER_VISIBLE_ITEM_15_PAD = UNIT_END + 0x0144,
            PLAYER_VISIBLE_ITEM_16_CREATOR = UNIT_END + 0x0145,
            PLAYER_VISIBLE_ITEM_16_0 = UNIT_END + 0x0147,
            PLAYER_VISIBLE_ITEM_16_PROPERTIES = UNIT_END + 0x0153,
            PLAYER_VISIBLE_ITEM_16_PAD = UNIT_END + 0x0154,
            PLAYER_VISIBLE_ITEM_17_CREATOR = UNIT_END + 0x0155,
            PLAYER_VISIBLE_ITEM_17_0 = UNIT_END + 0x0157,
            PLAYER_VISIBLE_ITEM_17_PROPERTIES = UNIT_END + 0x0163,
            PLAYER_VISIBLE_ITEM_17_PAD = UNIT_END + 0x0164,
            PLAYER_VISIBLE_ITEM_18_CREATOR = UNIT_END + 0x0165,
            PLAYER_VISIBLE_ITEM_18_0 = UNIT_END + 0x0167,
            PLAYER_VISIBLE_ITEM_18_PROPERTIES = UNIT_END + 0x0173,
            PLAYER_VISIBLE_ITEM_18_PAD = UNIT_END + 0x0174,
            PLAYER_VISIBLE_ITEM_19_CREATOR = UNIT_END + 0x0175,
            PLAYER_VISIBLE_ITEM_19_0 = UNIT_END + 0x0177,
            PLAYER_VISIBLE_ITEM_19_PROPERTIES = UNIT_END + 0x0183,
            PLAYER_VISIBLE_ITEM_19_PAD = UNIT_END + 0x0184,
            PLAYER_CHOSEN_TITLE = UNIT_END + 0x0185,
            PLAYER_FIELD_INV_SLOT_HEAD = UNIT_END + 0x0186,
            PLAYER_FIELD_PACK_SLOT_1 = UNIT_END + 0x01B4,
            PLAYER_FIELD_BANK_SLOT_1 = UNIT_END + 0x01D4,
            PLAYER_FIELD_BANKBAG_SLOT_1 = UNIT_END + 0x0204,
            PLAYER_FIELD_VENDORBUYBACK_SLOT_1 = UNIT_END + 0x0210,
            PLAYER_FIELD_KEYRING_SLOT_1 = UNIT_END + 0x0228,
            PLAYER_FARSIGHT = UNIT_END + 0x0268,
            PLAYER__FIELD_COMBO_TARGET = UNIT_END + 0x026A,
            PLAYER__FIELD_KNOWN_TITLES = UNIT_END + 0x026C,
            PLAYER_XP = UNIT_END + 0x026E,
            PLAYER_NEXT_LEVEL_XP = UNIT_END + 0x026F,
            PLAYER_SKILL_INFO_1_1 = UNIT_END + 0x0270,
            PLAYER_CHARACTER_POINTS1 = UNIT_END + 0x03F0,
            PLAYER_CHARACTER_POINTS2 = UNIT_END + 0x03F1,
            PLAYER_TRACK_CREATURES = UNIT_END + 0x03F2,
            PLAYER_TRACK_RESOURCES = UNIT_END + 0x03F3,
            PLAYER_BLOCK_PERCENTAGE = UNIT_END + 0x03F4,
            PLAYER_DODGE_PERCENTAGE = UNIT_END + 0x03F5,
            PLAYER_PARRY_PERCENTAGE = UNIT_END + 0x03F6,
            PLAYER_CRIT_PERCENTAGE = UNIT_END + 0x03F7,
            PLAYER_RANGED_CRIT_PERCENTAGE = UNIT_END + 0x03F8,
            PLAYER_SPELL_CRIT_PERCENTAGE1 = UNIT_END + 0x03F9,
            PLAYER_EXPLORED_ZONES_1 = UNIT_END + 0x0400,
            PLAYER_REST_STATE_EXPERIENCE = UNIT_END + 0x0440,
            PLAYER_FIELD_COINAGE = UNIT_END + 0x0441,
            PLAYER_FIELD_POSSTAT0 = UNIT_END + 0x0442,
            PLAYER_FIELD_POSSTAT1 = UNIT_END + 0x0443,
            PLAYER_FIELD_POSSTAT2 = UNIT_END + 0x0444,
            PLAYER_FIELD_POSSTAT3 = UNIT_END + 0x0445,
            PLAYER_FIELD_POSSTAT4 = UNIT_END + 0x0446,
            PLAYER_FIELD_NEGSTAT0 = UNIT_END + 0x0447,
            PLAYER_FIELD_NEGSTAT1 = UNIT_END + 0x0448,
            PLAYER_FIELD_NEGSTAT2 = UNIT_END + 0x0449,
            PLAYER_FIELD_NEGSTAT3 = UNIT_END + 0x044A,
            PLAYER_FIELD_NEGSTAT4 = UNIT_END + 0x044B,
            PLAYER_FIELD_RESISTANCEBUFFMODSPOSITIVE = UNIT_END + 0x044C,
            PLAYER_FIELD_RESISTANCEBUFFMODSNEGATIVE = UNIT_END + 0x0453,
            PLAYER_FIELD_MOD_DAMAGE_DONE_POS = UNIT_END + 0x045A,
            PLAYER_FIELD_MOD_DAMAGE_DONE_NEG = UNIT_END + 0x0461,
            PLAYER_FIELD_MOD_DAMAGE_DONE_PCT = UNIT_END + 0x0468,
            PLAYER_FIELD_MOD_HEALING_DONE_POS = UNIT_END + 0x046F,
            PLAYER_FIELD_MOD_TARGET_RESISTANCE = UNIT_END + 0x0470,
            PLAYER_FIELD_BYTES = UNIT_END + 0x0471,
            PLAYER_AMMO_ID = UNIT_END + 0x0472,
            PLAYER_SELF_RES_SPELL = UNIT_END + 0x0473,
            PLAYER_FIELD_PVP_MEDALS = UNIT_END + 0x0474,
            PLAYER_FIELD_BUYBACK_PRICE_1 = UNIT_END + 0x0475,
            PLAYER_FIELD_BUYBACK_TIMESTAMP_1 = UNIT_END + 0x0481,
            PLAYER_FIELD_SESSION_KILLS = UNIT_END + 0x048D,
            PLAYER_FIELD_YESTERDAY_KILLS = UNIT_END + 0x048E,
            PLAYER_FIELD_LAST_WEEK_KILLS = UNIT_END + 0x048F,
            PLAYER_FIELD_THIS_WEEK_KILLS = UNIT_END + 0x0490,
            PLAYER_FIELD_THIS_WEEK_CONTRIBUTION = UNIT_END + 0x0491,
            PLAYER_FIELD_LIFETIME_HONORBALE_KILLS = UNIT_END + 0x0492,
            PLAYER_FIELD_LIFETIME_DISHONORBALE_KILLS = UNIT_END + 0x0493,
            PLAYER_FIELD_YESTERDAY_CONTRIBUTION = UNIT_END + 0x0494,
            PLAYER_FIELD_LAST_WEEK_CONTRIBUTION = UNIT_END + 0x0495,
            PLAYER_FIELD_LAST_WEEK_RANK = UNIT_END + 0x0496,
            PLAYER_FIELD_BYTES2 = UNIT_END + 0x0497,
            PLAYER_FIELD_WATCHED_FACTION_INDEX = UNIT_END + 0x0498,
            PLAYER_FIELD_COMBAT_RATING_1 = UNIT_END + 0x0499,
            PLAYER_FIELD_ARENA_TEAM_INFO_1_1 = UNIT_END + 0x04AD,
            PLAYER_FIELD_HONOR_CURRENCY = UNIT_END + 0x04B6,
            PLAYER_FIELD_ARENA_CURRENCY = UNIT_END + 0x04B7,
            MAX = UNIT_END + 0x04B8
        }
    }
}
