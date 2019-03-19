using System;
using System.Collections.Generic;
using System.IO;
using Common.Interfaces;

namespace Common.Structs
{
    [Serializable]
    public class Account
    {
        public string Name { get; set; }
        public List<ICharacter> Characters { get; set; }
        public ICharacter ActiveCharacter => Characters.Find(x => x.IsOnline);

        private bool _saving = false;

        public Account()
        {
            Characters = new List<ICharacter>();
        }

        public Account(string name) : this()
        {
            Name = name;
        }


        public ICharacter SetActiveChar(ulong guid, int build)
        {
            Characters.ForEach(x => x.IsOnline = false);

            var cha = Characters.Find(x => x.Guid == guid && x.Build == build);
            cha.IsOnline = true;
            return cha;
        }
        public ICharacter GetCharacter(ulong guid, int build)
        {
            return Characters.Find(x => x.Guid == guid && x.Build == build);
        }


        public void Save()
        {
            if (_saving)
                return;

            _saving = true;

            Directory.CreateDirectory("Accounts");

            string filename = Path.Combine("Accounts", Name.ToUpper() + ".dat");

            using (var fs = File.Create(filename))
            using (var bw = new BinaryWriter(fs))
            {
                foreach (var c in Characters)
                {
                    bw.Write(c.Build);
                    bw.Write(c.Class);
                    bw.Write(c.DisplayId);
                    bw.Write(c.Face);
                    bw.Write(c.FacialHair);
                    bw.Write(c.Gender);
                    bw.Write(c.Guid);
                    bw.Write(c.HairColor);
                    bw.Write(c.HairStyle);
                    bw.Write(c.Location.X);
                    bw.Write(c.Location.Y);
                    bw.Write(c.Location.Z);
                    bw.Write(c.Location.O);
                    bw.Write(c.Location.Map);
                    bw.Write(c.Name);
                    bw.Write(c.PowerType);
                    bw.Write(c.Race);
                    bw.Write(c.Skin);
                    bw.Write(c.Zone);
                    bw.Write(c.Scale);
                }
            }

            _saving = false;
        }

        public void Load<T>() where T : ICharacter, new()
        {
            Characters = new List<ICharacter>();

            string filename = Path.Combine("Accounts", Name.ToUpper() + ".dat");

            if (!File.Exists(filename))
                return;

            using (var fs = File.OpenRead(filename))
            using (var br = new BinaryReader(fs))
            {
                while (br.BaseStream.Position < br.BaseStream.Length)
                {
                    ICharacter c = (ICharacter)Activator.CreateInstance<T>();

                    try
                    {
                        c.Build = br.ReadInt32();
                        c.Class = br.ReadByte();
                        c.DisplayId = br.ReadUInt32();
                        c.Face = br.ReadByte();
                        c.FacialHair = br.ReadByte();
                        c.Gender = br.ReadByte();
                        c.Guid = br.ReadUInt64();
                        c.HairColor = br.ReadByte();
                        c.HairStyle = br.ReadByte();

                        c.Location = new Location()
                        {
                            X = br.ReadSingle(),
                            Y = br.ReadSingle(),
                            Z = br.ReadSingle(),
                            O = br.ReadSingle(),
                            Map = br.ReadUInt32(),
                        };

                        c.Name = br.ReadString();
                        c.PowerType = br.ReadByte();
                        c.Race = br.ReadByte();
                        c.Skin = br.ReadByte();
                        c.Zone = br.ReadUInt32();
                        c.Scale = br.ReadSingle();
                    }
                    catch
                    {
                        return;
                    }

                    Characters.Add(c);
                }
            }
        }
    }
}
