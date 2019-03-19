using System;
using System.Collections.Generic;
using Common.Constants;
using Common.Extensions;
using Common.Interfaces;
using Common.Structs;

namespace Vanilla_4062
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
            PLAYER_FIELD_INV_SLOT_HEAD = 188,
            PLAYER_FIELD_PACK_SLOT_1 = 234,
            PLAYER_FIELD_BANK_SLOT_1 = 266,
            PLAYER_FIELD_BANKBAG_SLOT_1 = 314,
            PLAYER_FIELD_VENDORBUYBACK_SLOT = 326,
            PLAYER_FARSIGHT = 328,
            PLAYER__FIELD_COMBO_TARGET = 330,
            PLAYER_FIELD_BUYBACK_NPC = 332,
            PLAYER_XP = 334,
            PLAYER_NEXT_LEVEL_XP = 335,
            PLAYER_SKILL_INFO_1_1 = 336,
            PLAYER_QUEST_LOG_1_1 = 720,
            PLAYER_CHARACTER_POINTS1 = 780,
            PLAYER_CHARACTER_POINTS2 = 781,
            PLAYER_TRACK_CREATURES = 782,
            PLAYER_TRACK_RESOURCES = 783,
            PLAYER_CHAT_FILTERS = 784,
            PLAYER_BLOCK_PERCENTAGE = 785,
            PLAYER_DODGE_PERCENTAGE = 786,
            PLAYER_PARRY_PERCENTAGE = 787,
            PLAYER_CRIT_PERCENTAGE = 788,
            PLAYER_EXPLORED_ZONES_1 = 789,
            PLAYER_REST_STATE_EXPERIENCE = 821,
            PLAYER_FIELD_COINAGE = 822,
            PLAYER_FIELD_POSSTAT0 = 823,
            PLAYER_FIELD_POSSTAT1 = 824,
            PLAYER_FIELD_POSSTAT2 = 825,
            PLAYER_FIELD_POSSTAT3 = 826,
            PLAYER_FIELD_POSSTAT4 = 827,
            PLAYER_FIELD_NEGSTAT0 = 828,
            PLAYER_FIELD_NEGSTAT1 = 829,
            PLAYER_FIELD_NEGSTAT2 = 830,
            PLAYER_FIELD_NEGSTAT3 = 831,
            PLAYER_FIELD_NEGSTAT4 = 832,
            PLAYER_FIELD_RESISTANCEBUFFMODSPOSITIVE = 833,
            PLAYER_FIELD_RESISTANCEBUFFMODSNEGATIVE = 840,
            PLAYER_FIELD_MOD_DAMAGE_DONE_POS = 847,
            PLAYER_FIELD_MOD_DAMAGE_DONE_NEG = 854,
            PLAYER_FIELD_MOD_DAMAGE_DONE_PCT = 861,
            PLAYER_FIELD_BYTES = 868,
            PLAYER_AMMO_ID = 869,
            PLAYER_FIELD_PVP_MEDALS = 870,
            PLAYER_FIELD_BUYBACK_ITEM_ID = 871,
            PLAYER_FIELD_BUYBACK_RANDOM_PROPERTIES_ID = 872,
            PLAYER_FIELD_BUYBACK_SEED = 873,
            PLAYER_FIELD_BUYBACK_PRICE = 874,
            PLAYER_FIELD_PADDING = 875,
            MAX = 876,
        }
    }
}
