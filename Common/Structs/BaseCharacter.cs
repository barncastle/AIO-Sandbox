using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Common.Constants;
using Common.Interfaces;

namespace Common.Structs
{
    public abstract class BaseCharacter : ICharacter
    {
        public abstract int Build { get; set; }
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
        public float Scale { get; set; }
        public bool IsFlying { get; set; }

        protected SortedList<int, byte[]> FieldData;
        protected byte MaskSize;
        protected byte[] MaskArray;

        public BaseCharacter() => FieldData = new SortedList<int, byte[]>();

        #region Methods

        public abstract IPacketWriter BuildForceSpeed(float modifier, SpeedType type = SpeedType.Run);

        public abstract IPacketWriter BuildMessage(string text);

        public abstract IPacketWriter BuildUpdate();

        public abstract void Teleport(float x, float y, float z, float o, uint map, ref IWorldManager manager);

        public virtual IPacketWriter BuildFly(bool mode) => null;

        #endregion

        #region Helpers

        protected void SetField<TEnum, TValue>(TEnum index, TValue value) where TEnum : Enum where TValue : unmanaged
        {
            int field = (int)(object)index;
            int size = Marshal.SizeOf<TValue>();

            FieldData[field] = new byte[size];
            MemoryMarshal.Write(FieldData[field], ref value);

            for (int i = 0; i < (size / 4); i++)
                MaskArray[field / 8] |= (byte)(1 << ((field + i) % 8));
        }

        protected uint ToUInt32(byte b1 = 0, byte b2 = 0, byte b3 = 0, byte b4 = 0)
        {
            return (uint)(b1 | (b2 << 8) | (b3 << 16) | (b4 << 24));
        }

        protected uint ToUInt32(ushort u1 = 0, ushort u2 = 0)
        {
            return (uint)(u1 | (u2 << 16));
        }

        #endregion

        #region Serialization

        internal void Serialize(BinaryWriter bw)
        {
            bw.Write(Build);
            bw.Write(Class);
            bw.Write(DisplayId);
            bw.Write(Face);
            bw.Write(FacialHair);
            bw.Write(Gender);
            bw.Write(Guid);
            bw.Write(HairColor);
            bw.Write(HairStyle);
            bw.Write(Location.X);
            bw.Write(Location.Y);
            bw.Write(Location.Z);
            bw.Write(Location.O);
            bw.Write(Location.Map);
            bw.Write(Name);
            bw.Write(PowerType);
            bw.Write(Race);
            bw.Write(Skin);
            bw.Write(Zone);
            bw.Write(Scale);
            bw.Write(IsFlying);
        }

        internal void Deserialize(BinaryReader br)
        {
            Build = br.ReadInt32();
            Class = br.ReadByte();
            DisplayId = br.ReadUInt32();
            Face = br.ReadByte();
            FacialHair = br.ReadByte();
            Gender = br.ReadByte();
            Guid = br.ReadUInt64();
            HairColor = br.ReadByte();
            HairStyle = br.ReadByte();

            Location = new Location()
            {
                X = br.ReadSingle(),
                Y = br.ReadSingle(),
                Z = br.ReadSingle(),
                O = br.ReadSingle(),
                Map = br.ReadUInt32(),
            };

            Name = br.ReadString();
            PowerType = br.ReadByte();
            Race = br.ReadByte();
            Skin = br.ReadByte();
            Zone = br.ReadUInt32();
            Scale = br.ReadSingle();
            IsFlying = br.ReadBoolean();
        }

        #endregion Serialization
    }
}
