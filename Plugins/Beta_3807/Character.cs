using System;
using Common.Extensions;
using Common.Interfaces;
using Common.Structs;

namespace Beta_3807
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
            SetField(Fields.UNIT_FIELD_POWER1, Mana);
            SetField(Fields.UNIT_FIELD_POWER2, 0);
            SetField(Fields.UNIT_FIELD_POWER3, Focus);
            SetField(Fields.UNIT_FIELD_POWER4, Energy);
            SetField(Fields.UNIT_FIELD_MAXHEALTH, Health);
            SetField(Fields.UNIT_FIELD_MAXPOWER1, Mana);
            SetField(Fields.UNIT_FIELD_MAXPOWER2, Rage);
            SetField(Fields.UNIT_FIELD_MAXPOWER3, Focus);
            SetField(Fields.UNIT_FIELD_MAXPOWER4, Energy);
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
            OBJECT_END = 6,
            UNIT_FIELD_CHARM = 6,
            UNIT_FIELD_SUMMON = 8,
            UNIT_FIELD_CHARMEDBY = 10,
            UNIT_FIELD_SUMMONEDBY = 12,
            UNIT_FIELD_CREATEDBY = 14,
            UNIT_FIELD_TARGET = 16,
            UNIT_FIELD_CHANNEL_OBJECT = 18,
            UNIT_FIELD_HEALTH = 20,
            UNIT_FIELD_POWER1 = 21,
            UNIT_FIELD_POWER2 = 22,
            UNIT_FIELD_POWER3 = 23,
            UNIT_FIELD_POWER4 = 24,
            UNIT_FIELD_POWER5 = 25,
            UNIT_FIELD_MAXHEALTH = 26,
            UNIT_FIELD_MAXPOWER1 = 27,
            UNIT_FIELD_MAXPOWER2 = 28,
            UNIT_FIELD_MAXPOWER3 = 29,
            UNIT_FIELD_MAXPOWER4 = 30,
            UNIT_FIELD_MAXPOWER5 = 31,
            UNIT_FIELD_LEVEL = 32,
            UNIT_FIELD_FACTIONTEMPLATE = 33,
            UNIT_FIELD_BYTES_0 = 34,
            UNIT_VIRTUAL_ITEM_SLOT_DISPLAY = 35,
            UNIT_VIRTUAL_ITEM_INFO = 38,
            UNIT_FIELD_FLAGS = 44,
            UNIT_FIELD_AURA = 45,
            UNIT_FIELD_AURALEVELS = 101,
            UNIT_FIELD_AURAAPPLICATIONS = 111,
            UNIT_FIELD_AURAFLAGS = 121,
            UNIT_FIELD_AURASTATE = 128,
            UNIT_FIELD_BASEATTACKTIME = 129,
            UNIT_FIELD_BOUNDINGRADIUS = 131,
            UNIT_FIELD_COMBATREACH = 132,
            UNIT_FIELD_WEAPONREACH = 133,
            UNIT_FIELD_DISPLAYID = 134,
            UNIT_FIELD_MOUNTDISPLAYID = 135,
            UNIT_FIELD_MINDAMAGE = 136,
            UNIT_FIELD_MAXDAMAGE = 137,
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
            UNIT_FIELD_ATTACKPOWER = 162,
            UNIT_FIELD_BASE_MANA = 163,
            PLAYER_SELECTION = 164,
            PLAYER_DUEL_ARBITER = 166,
            PLAYER_GUILDID = 168,
            PLAYER_GUILDRANK = 169,
            PLAYER_BYTES = 170,
            PLAYER_BYTES_2 = 171,
            PLAYER_BYTES_3 = 172,
            PLAYER_DUEL_TEAM = 173,
            PLAYER_GUILD_TIMESTAMP = 174,
            PLAYER_FIELD_PAD_0 = 175,
            PLAYER_FIELD_INV_SLOT_HEAD = 176,
            PLAYER_FIELD_PACK_SLOT_1 = 222,
            PLAYER_FIELD_BANK_SLOT_1 = 254,
            PLAYER_FIELD_BANKBAG_SLOT_1 = 302,
            PLAYER_FARSIGHT = 314,
            PLAYER__FIELD_COMBO_TARGET = 316,
            PLAYER_XP = 318,
            PLAYER_NEXT_LEVEL_XP = 319,
            PLAYER_SKILL_INFO_1_1 = 320,
            PLAYER_QUEST_LOG_1_1 = 704,
            PLAYER_CHARACTER_POINTS1 = 764,
            PLAYER_CHARACTER_POINTS2 = 765,
            PLAYER_TRACK_CREATURES = 766,
            PLAYER_TRACK_RESOURCES = 767,
            PLAYER_CHAT_FILTERS = 768,
            PLAYER_BLOCK_PERCENTAGE = 769,
            PLAYER_DODGE_PERCENTAGE = 770,
            PLAYER_PARRY_PERCENTAGE = 771,
            PLAYER_CRIT_PERCENTAGE = 772,
            PLAYER_EXPLORED_ZONES_1 = 773,
            PLAYER_REST_STATE_EXPERIENCE = 805,
            PLAYER_FIELD_COINAGE = 806,
            PLAYER_FIELD_POSSTAT0 = 807,
            PLAYER_FIELD_POSSTAT1 = 808,
            PLAYER_FIELD_POSSTAT2 = 809,
            PLAYER_FIELD_POSSTAT3 = 810,
            PLAYER_FIELD_POSSTAT4 = 811,
            PLAYER_FIELD_NEGSTAT0 = 812,
            PLAYER_FIELD_NEGSTAT1 = 813,
            PLAYER_FIELD_NEGSTAT2 = 814,
            PLAYER_FIELD_NEGSTAT3 = 815,
            PLAYER_FIELD_NEGSTAT4 = 816,
            PLAYER_FIELD_RESISTANCEBUFFMODSPOSITIVE = 817,
            PLAYER_FIELD_RESISTANCEBUFFMODSNEGATIVE = 824,
            PLAYER_FIELD_MOD_DAMAGE_DONE_POS = 831,
            PLAYER_FIELD_MOD_DAMAGE_DONE_NEG = 838,
            PLAYER_FIELD_MOD_DAMAGE_DONE_PCT = 845,
            PLAYER_FIELD_BYTES = 852,
            PLAYER_FIELD_ATTACKPOWERMODPOS = 853,
            PLAYER_FIELD_ATTACKPOWERMODNEG = 854,
            PLAYER_AMMO_ID = 855,
            MAX = 856,
        }
    }
}
