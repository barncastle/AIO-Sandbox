﻿using Common.Constants;
using Common.Extensions;
using Common.Interfaces;
using Common.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Beta_3592
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

        public IPacketWriter BuildUpdate()
        {
            byte maskSize = ((int)Fields.MAX + 31) / 32;
            SortedDictionary<int, byte[]> fieldData = new SortedDictionary<int, byte[]>();
            byte[] maskArray = new byte[maskSize * 4];

            Action<Fields, object> SetField = (place, value) => this.SetField((int)place, value, ref fieldData, ref maskArray);

            PacketWriter writer = new PacketWriter(Sandbox.Instance.Opcodes[global::Opcodes.SMSG_UPDATE_OBJECT], "SMSG_UPDATE_OBJECT");
            writer.WriteUInt32(1); //Number of transactions
            writer.WriteUInt8(2); //UpdateType
            writer.WriteUInt64(this.Guid); //ObjectGuid
            writer.WriteUInt8(4); //ObjectType, 4 = Player

            writer.WriteUInt32(0);  //MovementFlagMask
            writer.WriteFloat(Location.X);  //x
            writer.WriteFloat(Location.Y);  //y
            writer.WriteFloat(Location.Z);  //z
            writer.WriteFloat(Location.O);  //w (o)
            writer.WriteFloat(2.5f); //WalkSpeed
            writer.WriteFloat(7.0f); //RunSpeed
            writer.WriteFloat(4.7222f); //SwimSpeed
            writer.WriteFloat(3.14f); //TurnSpeed

            writer.WriteUInt32(1); //Flags, 1 - Player
            writer.WriteUInt32(1); //AttackCycle
            writer.WriteUInt32(0); //TimerId
            writer.WriteUInt64(0); //VictimGuid

            SetField(Fields.GUID, this.Guid);
            SetField(Fields.HIER_TYPE, (uint)0x19);
            SetField(Fields.ENTRY, 0);
            SetField(Fields.SCALE, 1f);
            SetField(Fields.TARGET, (ulong)0);
            SetField(Fields.HEALTH, this.Health);
            SetField(Fields.MANA, this.Mana);
            SetField(Fields.RAGE, 0);
            SetField(Fields.FOCUS, this.Focus);
            SetField(Fields.ENERGY, this.Energy);
            SetField(Fields.MAX_HEALTH, this.Health);
            SetField(Fields.MAX_MANA, this.Mana);
            SetField(Fields.MAX_RAGE, this.Rage);
            SetField(Fields.MAX_FOCUS, this.Focus);
            SetField(Fields.MAX_ENERGY, this.Energy);
            SetField(Fields.LEVEL, this.Level);
            SetField(Fields.FACTION, 35);
            SetField(Fields.UNIT_BYTES_0, BitConverter.ToUInt32(new byte[] { this.Race, this.Class, this.Gender, this.PowerType }, 0));
            SetField(Fields.STRENGTH, this.Strength);
            SetField(Fields.AGILITY, this.Agility);
            SetField(Fields.STAMINA, this.Stamina);
            SetField(Fields.INTELLECT, this.Intellect);
            SetField(Fields.SPIRIT, this.Spirit);
            SetField(Fields.BASE_STRENGTH, this.Strength);
            SetField(Fields.BASE_AGILITY, this.Agility);
            SetField(Fields.BASE_STAMINA, this.Stamina);
            SetField(Fields.BASE_INTELLECT, this.Intellect);
            SetField(Fields.BASE_SPIRIT, this.Spirit);
            SetField(Fields.FLAGS, 0);
            SetField(Fields.DISPLAYID, DisplayId);
            SetField(Fields.MOUNT_DISPLAYID, MountDisplayId);
            SetField(Fields.UNIT_BYTES_1, BitConverter.ToUInt32(new byte[] { (byte)StandState, 0, 0, 0 }, 0));
            SetField(Fields.SELECTION, (ulong)0);
            SetField(Fields.PLAYER_BYTES_1, BitConverter.ToUInt32(new byte[] { Skin, Face, HairStyle, HairColor }, 0));
            SetField(Fields.PLAYER_BYTES_2, BitConverter.ToUInt32(new byte[] { 0, FacialHair, 0, RestedState }, 0));
            SetField(Fields.XP, 47);
            SetField(Fields.NEXTLEVEL_XP, 200);
            SetField(Fields.MIN_DAMAGE, 0);
            SetField(Fields.BASEATTACKTIME0, 1f);

            for (int i = 0; i < 32; i++)
                SetField(Fields.EXPLORED_ZONES + i, 0xFFFFFFFF);

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
            return this.BuildMessage(message, text);
        }

        public void Teleport(float x, float y, float z, float o, uint map, ref IWorldManager manager)
        {
            IsTeleporting = true;

            if (Location.Map == map)
            {
                PacketWriter movementStatus = new PacketWriter(Sandbox.Instance.Opcodes[global::Opcodes.SMSG_MOVE_WORLDPORT_ACK], "SMSG_MOVE_WORLDPORT_ACK");
                movementStatus.WriteUInt32(0); //Transport ID
                movementStatus.WriteFloat(x);
                movementStatus.WriteFloat(y);
                movementStatus.WriteFloat(z);
                movementStatus.WriteFloat(o);
                movementStatus.WriteFloat(0);
                movementStatus.WriteUInt32(0); //Movement Flags
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
                newWorld.WriteUInt8((byte)map);
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
            return this.BuildForceSpeed(writer, modifier);
        }

        public void Demorph() => DisplayId = this.GetDisplayId();


        internal enum Fields
        {
            GUID = 0,
            HIER_TYPE = 2,
            ENTRY = 3,
            SCALE = 4,
            PADDING = 5,
            CHARMED = 6,
            SUMMON = 8,
            CHARMEDBY = 10,
            SUMMONEDBY = 12,
            CREATEDBY = 14,
            TARGET = 16,
            COMBO_TARGET = 18,
            CHANNEL_OBJECT = 20,
            HEALTH = 22,
            POWER0 = 23,
            MANA = 23,
            RAGE = 24,
            FOCUS = 25,
            ENERGY = 26,
            MAX_HEALTH = 27,
            MAX_POWER0 = 28,
            MAX_MANA = 28,
            MAX_RAGE = 29,
            MAX_FOCUS = 30,
            MAX_ENERGY = 31,
            LEVEL = 32,
            FACTION = 33,
            UNIT_BYTES_0 = 34,
            STATS0 = 35,
            STRENGTH = 35,
            AGILITY = 36,
            STAMINA = 37,
            INTELLECT = 38,
            SPIRIT = 39,
            BASE_STRENGTH = 40,
            BASE_AGILITY = 41,
            BASE_STAMINA = 42,
            BASE_INTELLECT = 43,
            BASE_SPIRIT = 44,
            VIRTUAL_ITEMSLOTDISPLAY = 45,
            VIRTUAL_ITEMINFO = 48,
            FLAGS = 54,
            COINAGE = 55,
            AURA = 56,
            AURA_LEVELS = 112,
            AURA_APPLICATIONS = 122,
            AURA_FLAGS = 132,
            AURA_STATE = 139,
            BASEATTACKTIME0 = 140,
            BASEATTACKTIME1 = 141,
            RESISTANCE = 142,
            RESIST_PHYSICAL = 142,
            RESIST_HOLY = 143,
            RESIST_FIRE = 144,
            RESIST_NATURE = 145,
            RESIST_FROST = 146,
            RESIST_SHADOW = 147,
            BOUNDING_RADIUS = 148,
            COMBAT_REACH = 149,
            WEAPON_REACH = 150,
            DISPLAYID = 151,
            MOUNT_DISPLAYID = 152,
            MIN_DAMAGE = 153,
            MAX_DAMAGE = 154,
            MOD_DAMAGE_DONE = 155,
            RESISTANCE_BUFF_POSITIVE = 161,
            RESISTANCE_BUFF_NEGATIVE = 167,
            RESISTANCE_ITEM_MODS = 173,
            UNIT_BYTES_1 = 179,
            PET_NUMBER = 180,
            PET_NAME_TIMESTAMP = 181,
            PET_EXPERIENCE = 182,
            PET_NEXT_LEVE_EXP = 183,
            DYNAMIC_FLAGS = 184,
            EMOTE_STATE = 185,
            CHANNEL_SPELL = 186,
            MOD_CAST_SPEED = 187,
            CREATED_BY_SPELL = 188,
            NPC_FLAGS = 189,
            UNIT_BYTES_2 = 190,
            ATTACKPOWER = 191,
            ATTACKPOWERMODIFIER = 192,
            UNIT_PADDING = 193,
            SELECTION = 194,
            DUEL_ARBITER = 196,
            GUILD_ID = 198,
            GUILD_RANK = 199,
            PLAYER_BYTES_1 = 200,
            PLAYER_BYTES_2 = 201,
            DUEL_TEAM = 202,
            GUILD_TIMESTAMP = 203,
            INV_SLOTS = 204,
            PACK_SLOTS = 250,
            BANK_SLOTS = 282,
            BANKBAG_SLOTS = 330,
            FARSIGHT = 342,
            XP = 344,
            NEXTLEVEL_XP = 345,
            SKILL_INFO = 346,
            QUEST_INFO = 730,
            TALENT_POINTS = 810,
            SKILL_POINTS = 811,
            TRACK_CREATURES = 812,
            TRACK_RESOURCES = 813,
            CHAT_FILTERS = 814,
            BLOCK_PERCENTAGE = 815,
            DODGE_PERCENTAGE = 816,
            PARRY_PERCENTAGE = 817,
            BASE_MANA = 818,
            EXPLORED_ZONES = 819,
            REST_STATE_EXPERIENCE = 851,
            MAX = 852
        }
    }
}
