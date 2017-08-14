using Common.Constants;
using Common.Extensions;
using Common.Interfaces;
using Common.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beta_3694
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
        public byte ResetedState { get; set; } = 3;
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
            writer.WriteUInt8(0);
            writer.WriteUInt8(2); //UpdateType
            writer.WriteUInt64(this.Guid);
            writer.WriteUInt8(4); //ObjectType, 4 = Player

            writer.WriteUInt32(0);  //MovementFlagMask
            writer.WriteUInt32((uint)Environment.TickCount);
            writer.WriteFloat(Location.X);  //x
            writer.WriteFloat(Location.Y);  //y
            writer.WriteFloat(Location.Z);  //z
            writer.WriteFloat(Location.O);  //w (o)
            writer.WriteFloat(2.5f); //WalkSpeed
            writer.WriteFloat(7.0f); //RunSpeed
            writer.WriteFloat(2.5f); //Backwards WalkSpeed
            writer.WriteFloat(4.7222f); //SwimSpeed
            writer.WriteFloat(4.7222f); //Backwards SwimSpeed
            writer.WriteFloat(3.14f); //TurnSpeed

            writer.WriteUInt32(1); //Flags, 1 - Player
            writer.WriteUInt32(1); //AttackCycle
            writer.WriteUInt32(0); //TimerId
            writer.WriteUInt64(0); //VictimGuid

            SetField(Fields.OBJECT_FIELD_GUID, this.Guid);
            SetField(Fields.OBJECT_FIELD_TYPE, (uint)0x19);
            SetField(Fields.OBJECT_FIELD_ENTRY, 0);
            SetField(Fields.OBJECT_FIELD_SCALE_X, 1f);
            SetField(Fields.OBJECT_FIELD_PADDING, 0);
            SetField(Fields.UNIT_FIELD_TARGET, (ulong)0);
            SetField(Fields.UNIT_FIELD_HEALTH, this.Health);
            SetField(Fields.UNIT_FIELD_POWER1, this.Mana);
            SetField(Fields.UNIT_FIELD_POWER2, 0);
            SetField(Fields.UNIT_FIELD_POWER3, this.Focus);
            SetField(Fields.UNIT_FIELD_POWER4, this.Energy);
            SetField(Fields.UNIT_FIELD_MAXHEALTH, this.Health);
            SetField(Fields.UNIT_FIELD_MAXPOWER1, this.Mana);
            SetField(Fields.UNIT_FIELD_MAXPOWER2, this.Rage);
            SetField(Fields.UNIT_FIELD_MAXPOWER3, this.Focus);
            SetField(Fields.UNIT_FIELD_MAXPOWER4, this.Energy);
            SetField(Fields.UNIT_FIELD_LEVEL, this.Level);
            SetField(Fields.UNIT_FIELD_BYTES_0, BitConverter.ToUInt32(new byte[] { this.Race, this.Class, this.Gender, this.PowerType }, 0));
            SetField(Fields.PLAYER_FIELD_STAT0, this.Strength);
            SetField(Fields.PLAYER_FIELD_STAT1, this.Agility);
            SetField(Fields.PLAYER_FIELD_STAT2, this.Stamina);
            SetField(Fields.PLAYER_FIELD_STAT3, this.Intellect);
            SetField(Fields.PLAYER_FIELD_STAT4, this.Spirit);
            SetField(Fields.UNIT_FIELD_FLAGS, 0);
            SetField(Fields.PLAYER_BASE_MANA, this.Mana);
            SetField(Fields.UNIT_FIELD_DISPLAYID, DisplayId);
            SetField(Fields.UNIT_FIELD_MOUNTDISPLAYID, MountDisplayId);
            SetField(Fields.UNIT_FIELD_BYTES_1, BitConverter.ToUInt32(new byte[] { (byte)StandState, 0, 0, 0 }, 0));
            SetField(Fields.PLAYER_SELECTION, (ulong)0);
            SetField(Fields.PLAYER_BYTES, BitConverter.ToUInt32(new byte[] { Skin, Face, HairStyle, HairColor }, 0));
            SetField(Fields.PLAYER_BYTES_2, BitConverter.ToUInt32(new byte[] { 0, FacialHair, 0, ResetedState }, 0));
            SetField(Fields.PLAYER_XP, 47);
            SetField(Fields.PLAYER_NEXT_LEVEL_XP, 200);
            SetField(Fields.PLAYER_FIELD_ATTACKPOWER, 10);
            SetField(Fields.PLAYER_FIELD_BYTES, 0xEEEE0000);
            SetField(Fields.UNIT_DYNAMIC_FLAGS, 0x1);
            SetField(Fields.UNIT_FIELD_BASEATTACKTIME, 1f);
            SetField(Fields.UNIT_FIELD_FACTIONTEMPLATE, 35);

            for (int i = 0; i < 32; i++)
                SetField(Fields.PLAYER_EXPLORED_ZONES_1 + i, 0xFFFFFFFF);

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
                PacketWriter movementStatus = new PacketWriter(Sandbox.Instance.Opcodes[global::Opcodes.MSG_MOVE_TELEPORT_ACK], "MSG_MOVE_TELEPORT_ACK");
                movementStatus.WriteUInt64(this.Guid);
                movementStatus.WriteUInt64(0); //Flags
                movementStatus.WriteFloat(x);
                movementStatus.WriteFloat(y);
                movementStatus.WriteFloat(z);
                movementStatus.WriteFloat(o);
                movementStatus.WriteFloat(0);
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
                newWorld.WriteUInt32(map);
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
            writer.WriteUInt64(this.Guid);
            return this.BuildForceSpeed(writer, modifier);
        }

        public void Demorph() => DisplayId = this.GetDisplayId();


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
            UNIT_FIELD_PADDING = 149,
            PLAYER_SELECTION = 150,
            PLAYER_DUEL_ARBITER = 152,
            PLAYER_GUILDID = 154,
            PLAYER_GUILDRANK = 155,
            PLAYER_BYTES = 156,
            PLAYER_BYTES_2 = 157,
            PLAYER_BYTES_3 = 158,
            PLAYER_DUEL_TEAM = 159,
            PLAYER_GUILD_TIMESTAMP = 160,
            PLAYER_FIELD_PAD_0 = 161,
            PLAYER_FIELD_INV_SLOT_HEAD = 162,
            PLAYER_FIELD_PACK_SLOT_1 = 208,
            PLAYER_FIELD_BANK_SLOT_1 = 240,
            PLAYER_FIELD_BANKBAG_SLOT_1 = 288,
            PLAYER_FARSIGHT = 300,
            PLAYER_FIELD_COMBO_TARGET = 302,
            PLAYER_XP = 304,
            PLAYER_NEXT_LEVEL_XP = 305,
            PLAYER_SKILL_INFO_1_1 = 306,
            PLAYER_QUEST_LOG_1_1 = 690,
            PLAYER_CHARACTER_POINTS1 = 770,
            PLAYER_CHARACTER_POINTS2 = 771,
            PLAYER_TRACK_CREATURES = 772,
            PLAYER_TRACK_RESOURCES = 773,
            PLAYER_CHAT_FILTERS = 774,
            PLAYER_BLOCK_PERCENTAGE = 775,
            PLAYER_DODGE_PERCENTAGE = 776,
            PLAYER_PARRY_PERCENTAGE = 777,
            PLAYER_BASE_MANA = 778,
            PLAYER_EXPLORED_ZONES_1 = 779,
            PLAYER_REST_STATE_EXPERIENCE = 811,
            PLAYER_FIELD_COINAGE = 812,
            PLAYER_FIELD_STAT0 = 813,
            PLAYER_FIELD_STAT1 = 814,
            PLAYER_FIELD_STAT2 = 815,
            PLAYER_FIELD_STAT3 = 816,
            PLAYER_FIELD_STAT4 = 817,
            PLAYER_FIELD_BASESTAT0 = 818,
            PLAYER_FIELD_BASESTAT1 = 819,
            PLAYER_FIELD_BASESTAT2 = 820,
            PLAYER_FIELD_BASESTAT3 = 821,
            PLAYER_FIELD_BASESTAT4 = 822,
            PLAYER_FIELD_RESISTANCES = 823,
            PLAYER_FIELD_RESISTANCEBUFFMODSPOSITIVE = 829,
            PLAYER_FIELD_RESISTANCEBUFFMODSNEGATIVE = 835,
            PLAYER_FIELD_MOD_DAMAGE_DONE = 841,
            PLAYER_FIELD_BYTES = 847,
            PLAYER_FIELD_ATTACKPOWER = 848,
            PLAYER_FIELD_ATTACKPOWERMODIFIER = 849,
            MAX = 850,
        }
    }
}
