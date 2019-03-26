using Common.Constants;
using Common.Extensions;
using Common.Interfaces;
using Common.Structs;

namespace Alpha_3368
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
            writer.WriteUInt8(2); // UpdateType
            writer.WriteUInt64(Guid); // ObjectGuid
            writer.WriteUInt8(4); // ObjectType, 4 = Player

            writer.WriteUInt64(0); // TransportGuid
            writer.WriteFloat(0);  // TransportX
            writer.WriteFloat(0);  // TransportY
            writer.WriteFloat(0);  // TransportZ
            writer.WriteFloat(0);  // TransportW (TransportO)

            writer.WriteFloat(Location.X);  // x
            writer.WriteFloat(Location.Y);  // y
            writer.WriteFloat(Location.Z);  // z
            writer.WriteFloat(Location.O);  // w (o)

            writer.WriteFloat(0);  // Pitch

            writer.WriteUInt32(0);  // MovementFlagMask
            writer.WriteUInt32(0); // FallTime

            writer.WriteFloat(2.5f); // WalkSpeed
            writer.WriteFloat(7.0f); // RunSpeed
            writer.WriteFloat(4.7222f); // SwimSpeed
            writer.WriteFloat(3.14f); // TurnSpeed

            writer.WriteUInt32(1); // Flags, 1 - Player
            writer.WriteUInt32(1); // AttackCycle
            writer.WriteUInt32(0); // TimerId
            writer.WriteUInt64(0); // VictimGuid

            SetField(Fields.OBJECT_FIELD_GUID, Guid);
            SetField(Fields.OBJECT_FIELD_TYPE, 0x19);
            SetField(Fields.OBJECT_FIELD_ENTRY, 0);
            SetField(Fields.OBJECT_FIELD_SCALE_X, Scale);
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
            SetField(Fields.UNIT_FIELD_BYTES_0, ToUInt32(Race, Class, Gender, PowerType));
            SetField(Fields.UNIT_FIELD_BYTES_1, (uint)StandState);
            SetField(Fields.UNIT_FIELD_STAT0, Strength);
            SetField(Fields.UNIT_FIELD_STAT1, Agility);
            SetField(Fields.UNIT_FIELD_STAT2, Stamina);
            SetField(Fields.UNIT_FIELD_STAT3, Intellect);
            SetField(Fields.UNIT_FIELD_STAT4, Spirit);
            SetField(Fields.UNIT_FIELD_BASESTAT0, Strength);
            SetField(Fields.UNIT_FIELD_BASESTAT1, Agility);
            SetField(Fields.UNIT_FIELD_BASESTAT2, Stamina);
            SetField(Fields.UNIT_FIELD_BASESTAT3, Intellect);
            SetField(Fields.UNIT_FIELD_BASESTAT4, Spirit);
            SetField(Fields.UNIT_FIELD_DISPLAYID, DisplayId);
            SetField(Fields.UNIT_FIELD_MOUNTDISPLAYID, MountDisplayId);
            SetField(Fields.PLAYER_FIELD_NUM_INV_SLOTS, 14);
            SetField(Fields.PLAYER_BYTES, ToUInt32(Skin, Face, HairStyle, HairColor));
            SetField(Fields.PLAYER_XP, 47);
            SetField(Fields.PLAYER_NEXT_LEVEL_XP, 200);
            SetField(Fields.PLAYER_BYTES_2, ToUInt32(0, FacialHair));
            SetField(Fields.PLAYER_BASE_MANA, Mana);

            // FillInPartialObjectData
            writer.WriteUInt8(MaskSize); // UpdateMaskBlocks, 20
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
                PacketWriter movementStatus = new PacketWriter(Sandbox.Instance.Opcodes[global::Opcodes.SMSG_MOVE_WORLDPORT_ACK], "SMSG_MOVE_WORLDPORT_ACK");
                movementStatus.WriteUInt64(0); // Transport ID
                movementStatus.WriteFloat(0); // Transport
                movementStatus.WriteFloat(0);
                movementStatus.WriteFloat(0);
                movementStatus.WriteFloat(0);
                movementStatus.WriteFloat(x); // Player
                movementStatus.WriteFloat(y);
                movementStatus.WriteFloat(z);
                movementStatus.WriteFloat(o);
                movementStatus.WriteFloat(0); // ?
                movementStatus.WriteUInt32(0x08000000);
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
                newWorld.WriteUInt8((byte)map);
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
            UNIT_FIELD_COMBO_TARGET = 18,
            UNIT_FIELD_CHANNEL_OBJECT = 20,
            UNIT_FIELD_HEALTH = 22,
            UNIT_FIELD_POWER1 = 23,
            UNIT_FIELD_POWER2 = 24,
            UNIT_FIELD_POWER3 = 25,
            UNIT_FIELD_POWER4 = 26,
            UNIT_FIELD_MAXHEALTH = 27,
            UNIT_FIELD_MAXPOWER1 = 28,
            UNIT_FIELD_MAXPOWER2 = 29,
            UNIT_FIELD_MAXPOWER3 = 30,
            UNIT_FIELD_MAXPOWER4 = 31,
            UNIT_FIELD_LEVEL = 32,
            UNIT_FIELD_FACTIONTEMPLATE = 33,
            UNIT_FIELD_BYTES_0 = 34,
            UNIT_FIELD_STAT0 = 35,
            UNIT_FIELD_STAT1 = 36,
            UNIT_FIELD_STAT2 = 37,
            UNIT_FIELD_STAT3 = 38,
            UNIT_FIELD_STAT4 = 39,
            UNIT_FIELD_BASESTAT0 = 40,
            UNIT_FIELD_BASESTAT1 = 41,
            UNIT_FIELD_BASESTAT2 = 42,
            UNIT_FIELD_BASESTAT3 = 43,
            UNIT_FIELD_BASESTAT4 = 44,
            UNIT_VIRTUAL_ITEM_SLOT_DISPLAY = 45,
            UNIT_VIRTUAL_ITEM_INFO = 48,
            UNIT_FIELD_FLAGS = 54,
            UNIT_FIELD_COINAGE = 55,
            UNIT_FIELD_AURA = 56,
            UNIT_FIELD_AURAFLAGS = 112,
            UNIT_FIELD_AURASTATE = 119,
            UNIT_FIELD_MOD_DAMAGE_DONE = 120,
            UNIT_FIELD_MOD_DAMAGE_TAKEN = 126,
            UNIT_FIELD_MOD_CREATURE_DAMAGE_DONE = 132,
            UNIT_FIELD_BASEATTACKTIME = 140,
            UNIT_FIELD_RESISTANCES = 142,
            UNIT_FIELD_BOUNDINGRADIUS = 148,
            UNIT_FIELD_COMBATREACH = 149,
            UNIT_FIELD_WEAPONREACH = 150,
            UNIT_FIELD_DISPLAYID = 151,
            UNIT_FIELD_MOUNTDISPLAYID = 152,
            UNIT_FIELD_DAMAGE = 153,
            UNIT_FIELD_RESISTANCEBUFFMODSPOSITIVE = 154,
            UNIT_FIELD_RESISTANCEBUFFMODSNEGATIVE = 160,
            UNIT_FIELD_RESISTANCEITEMMODS = 166,
            UNIT_FIELD_BYTES_1 = 172,
            UNIT_FIELD_PETNUMBER = 173,
            UNIT_FIELD_PET_NAME_TIMESTAMP = 174,
            UNIT_FIELD_PETEXPERIENCE = 175,
            UNIT_FIELD_PETNEXTLEVELEXP = 176,
            UNIT_DYNAMIC_FLAGS = 177,
            UNIT_EMOTE_STATE = 178,
            UNIT_CHANNEL_SPELL = 179,
            UNIT_MOD_CAST_SPEED = 180,
            UNIT_CREATED_BY_SPELL = 181,
            UNIT_FIELD_BYTES_2 = 182,
            UNIT_FIELD_PADDING = 183,
            PLAYER_FIELD_INV_SLOT_1 = 184,
            PLAYER_FIELD_PACK_SLOT_1 = 230,
            PLAYER_FIELD_BANK_SLOT_1 = 262,
            PLAYER_FIELD_BANKBAG_SLOT_1 = 310,
            PLAYER_SELECTION = 322,
            PLAYER_FARSIGHT = 324,
            PLAYER_DUEL_ARBITER = 326,
            PLAYER_FIELD_NUM_INV_SLOTS = 328,
            PLAYER_GUILDID = 329,
            PLAYER_GUILDRANK = 330,
            PLAYER_BYTES = 331,
            PLAYER_XP = 332,
            PLAYER_NEXT_LEVEL_XP = 333,
            PLAYER_SKILL_INFO_1_1 = 334,
            PLAYER_BYTES_2 = 526,
            PLAYER_QUEST_LOG_1_1 = 527,
            PLAYER_CHARACTER_POINTS1 = 623,
            PLAYER_CHARACTER_POINTS2 = 624,
            PLAYER_TRACK_CREATURES = 625,
            PLAYER_TRACK_RESOURCES = 626,
            PLAYER_CHAT_FILTERS = 627,
            PLAYER_DUEL_TEAM = 628,
            PLAYER_BLOCK_PERCENTAGE = 629,
            PLAYER_DODGE_PERCENTAGE = 630,
            PLAYER_PARRY_PERCENTAGE = 631,
            PLAYER_BASE_MANA = 632,
            PLAYER_GUILD_TIMESTAMP = 633,
            MAX = 634,
        }
    }
}
