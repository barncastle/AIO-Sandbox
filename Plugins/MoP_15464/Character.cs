using System;
using Common.Constants;
using Common.Extensions;
using Common.Interfaces;
using Common.Network;
using Common.Structs;

namespace MoP_15464
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
            BitPacker packer = new BitPacker(writer);

            writer.WriteUInt16((ushort)Location.Map);
            writer.WriteUInt32(1); // Number of transactions
            writer.WriteUInt8(2); // UpdateType
            writer.WritePackedGUID(Guid);
            writer.WriteUInt8(4); // ObjectType, 4 = Player

            packer.Write(0); // ?
            packer.Write(0); // ?
            packer.Write(1); // UPDATEFLAG_SELF
            packer.Write(0); // UPDATEFLAG_ANIMKITS
            packer.Write(0); // false
            packer.Write(0); // UPDATEFLAG_STATIONARY_POSITION
            packer.Write(1); // ?
            packer.Write(0); // UPDATEFLAG_GO_TRANSPORT_POSITION
            packer.Write(1); // UPDATEFLAG_ROTATION
            packer.Write(0); // false
            packer.Write(0); // ?
            packer.Write(1); // UPDATEFLAG_LIVING
            packer.Write(0); // false
            packer.Write(0, 24); // byte counter
            packer.Write(0); // UPDATEFLAG_VEHICLE
            packer.Write(0); // ?
            packer.Write(0); // ?
            packer.Write(0); // UPDATEFLAG_TRANSPORT
            packer.Write(0); // UPDATEFLAG_HAS_TARGET
            packer.Write(0); // ?

            #region UPDATEFLAGS

            // UPDATEFLAG_LIVING
            packer.Write(0); // isTransport
            packer.Write(1); // !(has pitch)
            packer.WriteMask(Guid, 5, 3);
            packer.Write(1); // !(is missing time)
            packer.Write(0); // MOVEMENTFLAG2_INTERPOLATED_TURNING
            packer.Write(0); // false
            packer.WriteMask(Guid, 2);
            packer.Write(1); // !MOVEFLAG_SPLINE_ELEVATION
            packer.Write(0); // false
            packer.WriteMask(Guid, 1);
            packer.Write(1); // !MOVEMENT_FLAGS_2
            packer.WriteMask(Guid, 4);
            packer.Write(0); // Has spline data
            packer.WriteMask(Guid, 6, 0, 7);
            packer.Write(1); // !MOVEMENT_FLAGS
            packer.Write(0); // !Rotation
            packer.Flush();

            writer.WriteFloat(4.5f); // Backwards FlySpeed
            writer.WriteGUIDByte(Guid, 2);
            writer.WriteFloat(4.5f); // Backwards WalkSpeed
            writer.WriteFloat(3.14f); // TurnSpeed
            writer.WriteFloat(Location.X);
            writer.WriteFloat(2.5f); // WalkSpeed
            writer.WriteGUIDByte(Guid, 1);
            writer.WriteGUIDByte(Guid, 6);
            writer.WriteGUIDByte(Guid, 7);
            writer.WriteFloat(4.7222f); // SwimSpeed
            writer.WriteGUIDByte(Guid, 3);
            writer.WriteGUIDByte(Guid, 5);
            writer.WriteFloat(7f); // PitchRate  
            writer.WriteFloat(7.0f); // FlySpeed
            writer.WriteFloat(Location.Z);
            writer.WriteFloat(Location.O); // if Rotation
            writer.WriteGUIDByte(Guid, 0);
            writer.WriteFloat(2.5f); // Backwards SwimSpeed
            writer.WriteFloat(7.0f); // RunSpeed
            writer.WriteGUIDByte(Guid, 4);
            writer.WriteFloat(Location.Y);

            // UPDATEFLAG_ROTATION
            writer.WriteUInt64(0);

            #endregion

            SetField(Fields.OBJECT_FIELD_GUID, Guid);
            SetField(Fields.OBJECT_FIELD_DATA, 0);
            SetField(Fields.OBJECT_FIELD_ENTRYID, 0);
            SetField(Fields.OBJECT_FIELD_TYPE, (uint)0x19);
            SetField(Fields.OBJECT_FIELD_SCALE, Scale);
            SetField(Fields.UNIT_FIELD_TARGET, (ulong)0);
            SetField(Fields.UNIT_FIELD_BASEHEALTH, Health);
            SetField(Fields.UNIT_FIELD_HEALTH, Health);
            SetField(Fields.UNIT_FIELD_MAXHEALTH, Health);
            SetField(Fields.UNIT_FIELD_LEVEL, Level);
            SetField(Fields.UNIT_FIELD_FACTIONTEMPLATE, this.GetFactionTemplate());
            SetField(Fields.UNIT_FIELD_DISPLAYPOWER, ToUInt32(Race, Class, Gender, PowerType));
            SetField(Fields.UNIT_FIELD_STATS, Strength);
            SetField(Fields.UNIT_FIELD_STATS + 1, Agility);
            SetField(Fields.UNIT_FIELD_STATS + 2, Stamina);
            SetField(Fields.UNIT_FIELD_STATS + 3, Intellect);
            SetField(Fields.UNIT_FIELD_STATS + 4, Spirit);
            SetField(Fields.UNIT_FIELD_FLAGS, 8);
            SetField(Fields.UNIT_FIELD_FLAGS2, 0x800);
            SetField(Fields.UNIT_FIELD_BASEMANA, Mana);
            SetField(Fields.UNIT_FIELD_DISPLAYID, DisplayId);
            SetField(Fields.UNIT_FIELD_NATIVEDISPLAYID, this.GetDisplayId());
            SetField(Fields.UNIT_FIELD_MOUNTDISPLAYID, MountDisplayId);
            SetField(Fields.UNIT_FIELD_ANIMTIER, ToUInt32((byte)StandState));
            SetField(Fields.UNIT_FIELD_SHAPESHIFTFORM, 10240);
            SetField(Fields.PLAYER_FIELD_HAIRCOLORID, ToUInt32(Skin, Face, HairStyle, HairColor));
            SetField(Fields.PLAYER_FIELD_RESTSTATE, ToUInt32(FacialHair, b4: RestedState));
            SetField(Fields.PLAYER_FIELD_ARENAFACTION, ToUInt32(Gender));
            SetField(Fields.PLAYER_FIELD_XP, 47);
            SetField(Fields.PLAYER_FIELD_NEXTLEVELXP, 200);

            SetField(Fields.UNIT_FIELD_ATTACKPOWER, 1);
            SetField(Fields.UNIT_FIELD_RANGEDATTACKPOWER, 1);

            for (int i = 0; i < 0x9C; i++)
                SetField(Fields.PLAYER_FIELD_EXPLOREDZONES + i, 0xFFFFFFFF);

            // send language skills so we can type commands
            SetField(Fields.PLAYER_FIELD_SKILL, CharacterData.COMMON_SKILL_ID);
            SetField(Fields.PLAYER_FIELD_SKILL + 1, CharacterData.ORCISH_SKILL_ID);
            SetField(Fields.PLAYER_FIELD_SKILL + 2, CharacterData.PANDAREN_SKILL_ID);

            // misc
            SetField(Fields.PLAYER_FIELD_CHARACTERPOINTS, 0);
            SetField(Fields.PLAYER_FIELD_CHARACTERPOINTS + 1, 0);
            SetField(Fields.PLAYER_FIELD_TRACKCREATUREMASK, 0);
            SetField(Fields.PLAYER_FIELD_TRACKRESOURCEMASK, 0);
            SetField(Fields.PLAYER_FIELD_EXPERTISE, 0);
            SetField(Fields.PLAYER_FIELD_OFFHANDEXPERTISE, 0);
            SetField(Fields.PLAYER_FIELD_BLOCKPERCENTAGE, 0);
            SetField(Fields.PLAYER_FIELD_DODGEPERCENTAGE, 0);
            SetField(Fields.PLAYER_FIELD_PARRYPERCENTAGE, 0);
            SetField(Fields.PLAYER_FIELD_CRITPERCENTAGE, 0);
            SetField(Fields.PLAYER_FIELD_RANGEDCRITPERCENTAGE, 0);
            SetField(Fields.PLAYER_FIELD_OFFHANDCRITPERCENTAGE, 0);

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
                BitPacker bitPacker = new BitPacker(movementStatus);
                bitPacker.WriteMask(Guid, 4, 5, 3, 6);
                bitPacker.Write(0); // byte28
                bitPacker.Write(0); // byte38
                bitPacker.WriteMask(Guid, 1, 2, 0, 7);
                bitPacker.Flush();
                movementStatus.WriteGUIDByte(Guid, 6);
                movementStatus.WriteFloat(o);
                movementStatus.WriteGUIDByte(Guid, 3);
                movementStatus.WriteFloat(y);
                movementStatus.WriteGUIDByte(Guid, 4);
                movementStatus.WriteFloat(z);
                movementStatus.WriteGUIDByte(Guid, 1);
                movementStatus.WriteGUIDByte(Guid, 5);
                movementStatus.WriteGUIDByte(Guid, 7);
                movementStatus.WriteGUIDByte(Guid, 0);
                movementStatus.WriteFloat(x);
                movementStatus.WriteUInt32(map);

                manager.Send(movementStatus);
            }
            else
            {
                // Loading screen
                PacketWriter transferPending = new PacketWriter(Sandbox.Instance.Opcodes[global::Opcodes.SMSG_TRANSFER_PENDING], "SMSG_TRANSFER_PENDING");
                transferPending.WriteUInt32(map);
                transferPending.WriteUInt8(0); // customLoadScreenSpell, hasTransport bits
                manager.Send(transferPending);

                // New world transfer
                PacketWriter newWorld = new PacketWriter(Sandbox.Instance.Opcodes[global::Opcodes.SMSG_NEW_WORLD], "SMSG_NEW_WORLD");
                newWorld.WriteFloat(y);
                newWorld.WriteFloat(x);
                newWorld.WriteFloat(z);
                newWorld.WriteUInt32(map);
                newWorld.WriteFloat(o);
                manager.Send(newWorld);

                IsFlying = false;
            }

            System.Threading.Thread.Sleep(150); // Pause to factor unsent packets

            Location = new Location(x, y, z, o, map);
            manager.Send(BuildUpdate());

            // retain flight
            manager.Send(BuildFly(IsFlying));

            if (mapchange)
            {
                // send timesync
                PacketWriter timesyncreq = new PacketWriter(Sandbox.Instance.Opcodes[global::Opcodes.SMSG_TIME_SYNC_REQ], "SMSG_TIME_SYNC_REQ");
                timesyncreq.Write(0);
                manager.Send(timesyncreq);
            }

            IsTeleporting = false;
        }

        public override IPacketWriter BuildForceSpeed(float modifier, SpeedType type = SpeedType.Run)
        {
            PacketWriter writer;
            BitPacker bitPacker;
            switch (type)
            {
                case SpeedType.Fly:
                    writer = new PacketWriter(Sandbox.Instance.Opcodes[global::Opcodes.SMSG_FORCE_FLIGHT_SPEED_CHANGE], "SMSG_FORCE_FLIGHT_SPEED_CHANGE");
                    bitPacker = new BitPacker(writer);
                    writer.WriteFloat(modifier * 7f);
                    writer.WriteInt32(0);
                    bitPacker.WriteMask(Guid, 5, 0, 2, 1, 4, 3, 6, 7);
                    bitPacker.Flush();
                    writer.WriteGUIDByte(Guid, 2);
                    writer.WriteGUIDByte(Guid, 7);
                    writer.WriteGUIDByte(Guid, 6);
                    writer.WriteGUIDByte(Guid, 3);
                    writer.WriteGUIDByte(Guid, 0);
                    writer.WriteGUIDByte(Guid, 5);
                    writer.WriteGUIDByte(Guid, 1);
                    writer.WriteGUIDByte(Guid, 4);
                    break;
                case SpeedType.Swim:
                    writer = new PacketWriter(Sandbox.Instance.Opcodes[global::Opcodes.SMSG_FORCE_SWIM_SPEED_CHANGE], "SMSG_FORCE_SWIM_SPEED_CHANGE");
                    bitPacker = new BitPacker(writer);
                    bitPacker.WriteMask(Guid, 3, 7, 0, 1, 4, 2, 6, 5);
                    bitPacker.Flush();
                    writer.WriteGUIDByte(Guid, 4);
                    writer.WriteGUIDByte(Guid, 7);
                    writer.WriteGUIDByte(Guid, 2);
                    writer.WriteGUIDByte(Guid, 0);
                    writer.WriteGUIDByte(Guid, 1);
                    writer.WriteFloat(modifier * 7f);
                    writer.WriteInt32(0);
                    writer.WriteGUIDByte(Guid, 3);
                    writer.WriteGUIDByte(Guid, 5);
                    writer.WriteGUIDByte(Guid, 6);
                    break;
                default:
                    writer = new PacketWriter(Sandbox.Instance.Opcodes[global::Opcodes.SMSG_FORCE_SPEED_CHANGE], "SMSG_FORCE_SPEED_CHANGE");
                    bitPacker = new BitPacker(writer);
                    writer.WriteInt32(0);
                    writer.WriteFloat(modifier * 7f);
                    bitPacker.WriteMask(Guid, 6, 5, 3, 7, 4, 1, 0, 2);
                    bitPacker.Flush();
                    writer.WriteGUIDByte(Guid, 4);
                    writer.WriteGUIDByte(Guid, 6);
                    writer.WriteGUIDByte(Guid, 5);
                    writer.WriteGUIDByte(Guid, 0);
                    writer.WriteGUIDByte(Guid, 1);
                    writer.WriteGUIDByte(Guid, 7);
                    writer.WriteGUIDByte(Guid, 3);
                    writer.WriteGUIDByte(Guid, 2);
                    break;
            }

            return writer;
        }

        public override IPacketWriter BuildFly(bool mode)
        {
            IsFlying = mode;

            var opcode = mode ? global::Opcodes.SMSG_MOVE_SET_CAN_FLY : global::Opcodes.SMSG_MOVE_UNSET_CAN_FLY;
            PacketWriter writer = new PacketWriter(Sandbox.Instance.Opcodes[opcode], opcode.ToString());
            BitPacker bitPacker = new BitPacker(writer);

            if (mode)
            {
                writer.WriteInt32(2);
                bitPacker.WriteMask(Guid, 3, 1, 2, 5, 6, 7, 0, 4);
                bitPacker.Flush();
                writer.WriteGUIDByte(Guid, 5);
                writer.WriteGUIDByte(Guid, 0);
                writer.WriteGUIDByte(Guid, 1);
                writer.WriteGUIDByte(Guid, 2);
                writer.WriteGUIDByte(Guid, 6);
                writer.WriteGUIDByte(Guid, 7);
                writer.WriteGUIDByte(Guid, 4);
                writer.WriteGUIDByte(Guid, 3);
            }
            else
            {
                bitPacker.WriteMask(Guid, 6, 5, 3, 4, 0, 2, 7, 1);
                bitPacker.Flush();
                writer.WriteGUIDByte(Guid, 7);
                writer.WriteGUIDByte(Guid, 4);
                writer.WriteGUIDByte(Guid, 6);
                writer.WriteGUIDByte(Guid, 0);
                writer.WriteInt32(2);
                writer.WriteGUIDByte(Guid, 1);
                writer.WriteGUIDByte(Guid, 2);
                writer.WriteGUIDByte(Guid, 5);
                writer.WriteGUIDByte(Guid, 3);
            }

            return writer;
        }

        internal enum Fields
        {
            OBJECT_FIELD_GUID = 0x0,
            OBJECT_FIELD_DATA = 0x2,
            OBJECT_FIELD_TYPE = 0x4,
            OBJECT_FIELD_ENTRYID = 0x5,
            OBJECT_FIELD_SCALE = 0x6,

            UNIT_FIELD_CHARM = 0x7,
            UNIT_FIELD_SUMMON = 0x9,
            UNIT_FIELD_CRITTER = 0xB,
            UNIT_FIELD_CHARMEDBY = 0xD,
            UNIT_FIELD_SUMMONEDBY = 0xF,
            UNIT_FIELD_CREATEDBY = 0x11,
            UNIT_FIELD_TARGET = 0x13,
            UNIT_FIELD_CHANNELOBJECT = 0x15,
            UNIT_FIELD_CHANNELSPELL = 0x17,
            UNIT_FIELD_DISPLAYPOWER = 0x18,
            UNIT_FIELD_HEALTH = 0x19,
            UNIT_FIELD_POWER = 0x1A,
            UNIT_FIELD_MAXHEALTH = 0x1F,
            UNIT_FIELD_MAXPOWER = 0x20,
            UNIT_FIELD_POWERREGENFLATMODIFIER = 0x25,
            UNIT_FIELD_POWERREGENINTERRUPTEDFLATMODIFIER = 0x2A,
            UNIT_FIELD_LEVEL = 0x2F,
            UNIT_FIELD_FACTIONTEMPLATE = 0x30,
            UNIT_FIELD_VIRTUALITEMID = 0x31,
            UNIT_FIELD_FLAGS = 0x34,
            UNIT_FIELD_FLAGS2 = 0x35,
            UNIT_FIELD_AURASTATE = 0x36,
            UNIT_FIELD_ATTACKROUNDBASETIME = 0x37,
            UNIT_FIELD_RANGEDATTACKROUNDBASETIME = 0x39,
            UNIT_FIELD_BOUNDINGRADIUS = 0x3A,
            UNIT_FIELD_COMBATREACH = 0x3B,
            UNIT_FIELD_DISPLAYID = 0x3C,
            UNIT_FIELD_NATIVEDISPLAYID = 0x3D,
            UNIT_FIELD_MOUNTDISPLAYID = 0x3E,
            UNIT_FIELD_MINDAMAGE = 0x3F,
            UNIT_FIELD_MAXDAMAGE = 0x40,
            UNIT_FIELD_MINOFFHANDDAMAGE = 0x41,
            UNIT_FIELD_MAXOFFHANDDAMAGE = 0x42,
            UNIT_FIELD_ANIMTIER = 0x43,
            UNIT_FIELD_PETNUMBER = 0x44,
            UNIT_FIELD_PETNAMETIMESTAMP = 0x45,
            UNIT_FIELD_PETEXPERIENCE = 0x46,
            UNIT_FIELD_PETNEXTLEVELEXPERIENCE = 0x47,
            UNIT_FIELD_DYNAMICFLAGS = 0x48,
            UNIT_FIELD_MODCASTINGSPEED = 0x49,
            UNIT_FIELD_MODSPELLHASTE = 0x4A,
            UNIT_FIELD_CREATEDBYSPELL = 0x4B,
            UNIT_FIELD_NPCFLAGS = 0x4C,
            UNIT_FIELD_EMOTESTATE = 0x4D,
            UNIT_FIELD_STATS = 0x4E,
            UNIT_FIELD_STATPOSBUFF = 0x53,
            UNIT_FIELD_STATNEGBUFF = 0x58,
            UNIT_FIELD_RESISTANCES = 0x5D,
            UNIT_FIELD_RESISTANCEBUFFMODSPOSITIVE = 0x64,
            UNIT_FIELD_RESISTANCEBUFFMODSNEGATIVE = 0x6B,
            UNIT_FIELD_BASEMANA = 0x72,
            UNIT_FIELD_BASEHEALTH = 0x73,
            UNIT_FIELD_SHAPESHIFTFORM = 0x74,
            UNIT_FIELD_ATTACKPOWER = 0x75,
            UNIT_FIELD_ATTACKPOWERMODPOS = 0x76,
            UNIT_FIELD_ATTACKPOWERMODNEG = 0x77,
            UNIT_FIELD_ATTACKPOWERMULTIPLIER = 0x78,
            UNIT_FIELD_RANGEDATTACKPOWER = 0x79,
            UNIT_FIELD_RANGEDATTACKPOWERMODPOS = 0x7A,
            UNIT_FIELD_RANGEDATTACKPOWERMODNEG = 0x7B,
            UNIT_FIELD_RANGEDATTACKPOWERMULTIPLIER = 0x7C,
            UNIT_FIELD_MINRANGEDDAMAGE = 0x7D,
            UNIT_FIELD_MAXRANGEDDAMAGE = 0x7E,
            UNIT_FIELD_POWERCOSTMODIFIER = 0x7F,
            UNIT_FIELD_POWERCOSTMULTIPLIER = 0x86,
            UNIT_FIELD_MAXHEALTHMODIFIER = 0x8D,
            UNIT_FIELD_HOVERHEIGHT = 0x8E,
            UNIT_FIELD_MAXITEMLEVEL = 0x8F,
            UNIT_FIELD_WILDBATTLEPETLEVEL = 0x90,
            UNIT_FIELD_BATTLEPETCOMPANIONID = 0x91,
            UNIT_FIELD_BATTLEPETCOMPANIONNAMETIMESTAMP = 0x92,

            PLAYER_FIELD_DUELARBITER = 0x93,
            PLAYER_FIELD_PLAYERFLAGS = 0x95,
            PLAYER_FIELD_GUILDRANKID = 0x96,
            PLAYER_FIELD_GUILDDELETEDATE = 0x97,
            PLAYER_FIELD_GUILDLEVEL = 0x98,
            PLAYER_FIELD_HAIRCOLORID = 0x99,
            PLAYER_FIELD_RESTSTATE = 0x9A,
            PLAYER_FIELD_ARENAFACTION = 0x9B,
            PLAYER_FIELD_DUELTEAM = 0x9C,
            PLAYER_FIELD_GUILDTIMESTAMP = 0x9D,
            PLAYER_FIELD_QUESTLOG = 0x9E,
            PLAYER_FIELD_VISIBLEITEMS = 0x38C,
            PLAYER_FIELD_PLAYERTITLE = 0x3B2,
            PLAYER_FIELD_FAKEINEBRIATION = 0x3B3,
            PLAYER_FIELD_INVSLOTS = 0x3B4,
            PLAYER_FIELD_FARSIGHTOBJECT = 0x460,
            PLAYER_FIELD_KNOWNTITLES = 0x462,
            PLAYER_FIELD_XP = 0x46A,
            PLAYER_FIELD_NEXTLEVELXP = 0x46B,
            PLAYER_FIELD_SKILL = 0x46C,
            PLAYER_FIELD_CHARACTERPOINTS = 0x5EC,
            PLAYER_FIELD_TRACKCREATUREMASK = 0x5EE,
            PLAYER_FIELD_TRACKRESOURCEMASK = 0x5EF,
            PLAYER_FIELD_EXPERTISE = 0x5F0,
            PLAYER_FIELD_OFFHANDEXPERTISE = 0x5F1,
            PLAYER_FIELD_BLOCKPERCENTAGE = 0x5F2,
            PLAYER_FIELD_DODGEPERCENTAGE = 0x5F3,
            PLAYER_FIELD_PARRYPERCENTAGE = 0x5F4,
            PLAYER_FIELD_CRITPERCENTAGE = 0x5F5,
            PLAYER_FIELD_RANGEDCRITPERCENTAGE = 0x5F6,
            PLAYER_FIELD_OFFHANDCRITPERCENTAGE = 0x5F7,
            PLAYER_FIELD_SPELLCRITPERCENTAGE = 0x5F8,
            PLAYER_FIELD_SHIELDBLOCK = 0x5FF,
            PLAYER_FIELD_SHIELDBLOCKCRITPERCENTAGE = 0x600,
            PLAYER_FIELD_MASTERY = 0x601,
            PLAYER_FIELD_PVPPOWER = 0x602,
            PLAYER_FIELD_EXPLOREDZONES = 0x603,
            PLAYER_FIELD_RESTSTATEBONUSPOOL = 0x69F,
            PLAYER_FIELD_COINAGE = 0x6A0,
            PLAYER_FIELD_MODDAMAGEDONEPOS = 0x6A2,
            PLAYER_FIELD_MODDAMAGEDONENEG = 0x6A9,
            PLAYER_FIELD_MODDAMAGEDONEPERCENT = 0x6B0,
            PLAYER_FIELD_MODHEALINGDONEPOS = 0x6B7,
            PLAYER_FIELD_MODHEALINGDONEPERCENT = 0x6B9,
            PLAYER_FIELD_WEAPONDMGMULTIPLIERS = 0x6BA,
            PLAYER_FIELD_MODSPELLPOWERPERCENT = 0x6BD,
            PLAYER_FIELD_OVERRIDESPELLPOWERBYAPPERCENT = 0x6BE,
            PLAYER_FIELD_MODTARGETRESISTANCE = 0x6BF,
            PLAYER_FIELD_MODTARGETPHYSICALRESISTANCE = 0x6C0,
            PLAYER_FIELD_LIFETIMEMAXRANK = 0x6C1,
            PLAYER_FIELD_SELFRESSPELL = 0x6C2,
            PLAYER_FIELD_PVPMEDALS = 0x6C3,
            PLAYER_FIELD_BUYBACKPRICE = 0x6C4,
            PLAYER_FIELD_BUYBACKTIMESTAMP = 0x6D0,
            PLAYER_FIELD_YESTERDAYHONORABLEKILLS = 0x6DC,
            PLAYER_FIELD_LIFETIMEHONORABLEKILLS = 0x6DD,
            PLAYER_FIELD_LOCALREGENFLAGS = 0x6DE,
            PLAYER_FIELD_WATCHEDFACTIONINDEX = 0x6DF,
            PLAYER_FIELD_COMBATRATINGS = 0x6E0,
            PLAYER_FIELD_ARENATEAMS = 0x6FB,
            PLAYER_FIELD_BATTLEGROUNDRATING = 0x710,
            PLAYER_FIELD_MAXLEVEL = 0x711,
            PLAYER_FIELD_DAILYQUESTS = 0x712,
            PLAYER_FIELD_RUNEREGEN = 0x72B,
            PLAYER_FIELD_NOREAGENTCOSTMASK = 0x72F,
            PLAYER_FIELD_GLYPHSLOTS = 0x733,
            PLAYER_FIELD_GLYPHS = 0x73C,
            PLAYER_FIELD_GLYPHSLOTSENABLED = 0x745,
            PLAYER_FIELD_PETSPELLPOWER = 0x746,
            PLAYER_FIELD_RESEARCHING = 0x747,
            PLAYER_FIELD_PROFESSIONSKILLLINE = 0x74F,
            PLAYER_FIELD_UIHITMODIFIER = 0x751,
            PLAYER_FIELD_UISPELLHITMODIFIER = 0x752,
            PLAYER_FIELD_HOMEREALMTIMEOFFSET = 0x753,
            PLAYER_FIELD_MODHASTE = 0x754,
            PLAYER_FIELD_MODRANGEDHASTE = 0x755,
            PLAYER_FIELD_MODPETHASTE = 0x756,
            PLAYER_FIELD_MODHASTEREGEN = 0x757,
            MAX = 0x758
        }
    }
}
