using System;
using System.Collections.Generic;
using System.IO;
using Common.Interfaces;

namespace Common.Structs
{
    public class Account
    {
        public string Name { get; set; }
        public List<ICharacter> Characters { get; private set; }
        public ICharacter ActiveCharacter { get; private set; }

        private bool _saving = false;

        #region Constructors

        public Account() => Characters = new List<ICharacter>();

        public Account(string name) : this() => Name = name;

        #endregion

        #region Methods

        public ICharacter SetActiveChar(ulong guid, int build)
        {
            Characters.ForEach(x => x.IsOnline = false);

            ActiveCharacter = Characters.Find(x => x.Guid == guid && x.Build == build);
            ActiveCharacter.IsOnline = true;
            return ActiveCharacter;
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
            using (var fs = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.Write))
            using (var bw = new BinaryWriter(fs))
            {
                foreach (BaseCharacter character in Characters)
                    character.Serialize(bw);
            }

            _saving = false;
        }

        public void Load<T>() where T : ICharacter, new()
        {
            Characters.Clear();

            string filename = Path.Combine("Accounts", Name.ToUpper() + ".dat");
            if (!File.Exists(filename))
                return;

            using (var fs = File.OpenRead(filename))
            using (var br = new BinaryReader(fs))
            {
                while (br.BaseStream.Position < br.BaseStream.Length)
                {
                    try
                    {
                        var character = Activator.CreateInstance<T>() as BaseCharacter;
                        character.Deserialize(br);
                        Characters.Add(character);
                    }
                    catch
                    {
                        return;
                    }
                }
            }
        }

        #endregion
    }
}
