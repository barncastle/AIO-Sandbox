using System;
using System.Linq;
using Common.Commands;
using Common.Constants;
using Common.Extensions;
using Common.Interfaces;
using Common.Interfaces.Handlers;
using Common.Network;
using Common.Structs;

namespace MoP_15464.Handlers
{
    public class CharHandler : ICharHandler
    {
        public void HandleCharCreate(ref IPacketReader packet, ref IWorldManager manager)
        {
            string name = packet.ReadString();

            Character cha = new Character()
            {
                Name = name.ToUpperFirst(),
                Race = packet.ReadByte(),
                Class = packet.ReadByte(),
                Gender = packet.ReadByte(),
                Skin = packet.ReadByte(),
                Face = packet.ReadByte(),
                HairStyle = packet.ReadByte(),
                HairColor = packet.ReadByte(),
                FacialHair = packet.ReadByte()
            };

            // HACK neutral panda faction template doesn't work
            if (cha.Race == (byte)Races.PANDAREN_NEUTRAL)
                cha.Race = (byte)Races.PANDAREN_ALLI;

            var result = manager.Account.Characters.Where(x => x.Build == Sandbox.Instance.Build);
            PacketWriter writer = new PacketWriter(Sandbox.Instance.Opcodes[global::Opcodes.SMSG_CHAR_CREATE], "SMSG_CHAR_CREATE");

            if (result.Any(x => x.Name.Equals(cha.Name, StringComparison.CurrentCultureIgnoreCase)))
            {
                writer.WriteUInt8(0x32); // Duplicate name
                manager.Send(writer);
                return;
            }

            cha.Guid = (ulong)(manager.Account.Characters.Count + 1);
            cha.Location = new Location(-8949.95f, -132.493f, 83.5312f, 0, 0);
            cha.RestedState = (byte)new Random().Next(1, 2);
            cha.SetDefaultValues();

            manager.Account.Characters.Add(cha);
            manager.Account.Save();

            // Success
            writer.WriteUInt8(0x2F);
            manager.Send(writer);
        }

        public void HandleCharDelete(ref IPacketReader packet, ref IWorldManager manager)
        {
            ulong guid = packet.ReadUInt64();
            var character = manager.Account.GetCharacter(guid, Sandbox.Instance.Build);

            PacketWriter writer = new PacketWriter(Sandbox.Instance.Opcodes[global::Opcodes.SMSG_CHAR_DELETE], "SMSG_CHAR_DELETE");
            writer.WriteUInt8(0x47);
            manager.Send(writer);

            if (character != null)
            {
                manager.Account.Characters.Remove(character);
                manager.Account.Save();
            }
        }

        public void HandleCharEnum(ref IPacketReader packet, ref IWorldManager manager)
        {
            var account = manager.Account;
            var result = account.Characters.Where(x => x.Build == Sandbox.Instance.Build);

            PacketWriter writer = new PacketWriter(Sandbox.Instance.Opcodes[global::Opcodes.SMSG_CHAR_ENUM], "SMSG_CHAR_ENUM");
            BitPacker bitPacker = new BitPacker(writer);

            bitPacker.Write(1);
            bitPacker.Write(0, 23);
            bitPacker.Write(result.Count(), 17);

            foreach (Character c in result)
            {
                bitPacker.Write(0, 3);
                bitPacker.Write(c.Guid & 0xFF);
                bitPacker.Write((c.Guid >> 8) & 0xFF);
                bitPacker.Write(0);
                bitPacker.Write(c.Name.Length, 7);
                bitPacker.Write(0, 4);
                bitPacker.Write((c.Guid >> 16) & 0xFF);
                bitPacker.Write((c.Guid >> 24) & 0xFF);
                bitPacker.Write(0, 5);
            }

            bitPacker.Flush();

            foreach (Character c in result)
            {
                writer.WriteUInt8(c.Gender);
                writer.WriteUInt8((byte)c.Level);

                // items
                for (int i = 0; i < 0x17; i++)
                {
                    writer.WriteUInt32(0);
                    writer.WriteUInt32(0);
                    writer.WriteUInt8(0);
                }

                writer.WriteGUIDByte(c.Guid, 0);
                writer.WriteFloat(c.Location.Z);
                writer.WriteUInt8(c.HairColor);
                writer.WriteUInt32(c.Zone);
                writer.WriteGUIDByte(c.Guid, 3);
                writer.WriteGUIDByte(c.Guid, 1);
                writer.WriteUInt32(0);
                writer.WriteFloat(c.Location.X);
                writer.WriteUInt8(0);
                writer.WriteUInt8(c.HairStyle);
                writer.WriteUInt32(c.Location.Map);
                writer.WriteUInt8(c.Face);
                writer.WriteUInt8(c.Skin);
                writer.Write(System.Text.Encoding.ASCII.GetBytes(c.Name));
                writer.WriteUInt8(c.FacialHair);
                writer.WriteUInt8(c.Class);
                writer.WriteUInt32(0);
                writer.WriteUInt8(c.Race);
                writer.WriteFloat(c.Location.Y);
                writer.WriteGUIDByte(c.Guid, 2);
                writer.WriteUInt32(0); // customise flags
                writer.WriteUInt32(0); // char flags
                writer.WriteUInt32(0);
            }

            manager.Send(writer);
        }

        public void HandleMessageChat(ref IPacketReader packet, ref IWorldManager manager)
        {
            var character = manager.Account.ActiveCharacter;

            var bitunpack = new BitUnpacker(packet);
            var language = packet.ReadUInt32();
            var message = packet.ReadString(bitunpack.GetBits<int>(9));

            PacketWriter writer = new PacketWriter(Sandbox.Instance.Opcodes[global::Opcodes.SMSG_MESSAGECHAT], "SMSG_MESSAGECHAT");
            writer.WriteUInt8(1); // System Message
            writer.WriteUInt32(0); // Language: General
            writer.WriteUInt64(character.Guid);
            writer.WriteUInt32(0);
            writer.WriteUInt64(0);
            writer.WriteInt32(message.Length + 1);
            writer.WriteString(message);
            writer.WriteUInt8(0);

            if (!CommandManager.InvokeHandler(message, manager))
                manager.Send(writer);
        }

        public void HandleMovementStatus(ref IPacketReader packet, ref IWorldManager manager)
        {
            if (manager.Account.ActiveCharacter.IsTeleporting)
                return;

            uint opcode = packet.Opcode;
            long pos = packet.Position;

            var character = manager.Account.ActiveCharacter;

            packet.ReadPackedGUID();
            packet.Position += 10;
            character.Location.Update(packet, true);

            //packet.Position = pos;
            //PacketWriter writer = new PacketWriter(opcode, Sandbox.Instance.Opcodes[opcode].ToString());
            //writer.Write(packet.ReadToEnd());
            //manager.Send(writer);
        }

        public void HandleNameCache(ref IPacketReader packet, ref IWorldManager manager)
        {
            ulong guid = packet.ReadUInt64();
            var character = manager.Account.GetCharacter(guid, Sandbox.Instance.Build);

            if (character == null)
                return;

            PacketWriter nameCache = new PacketWriter(Sandbox.Instance.Opcodes[global::Opcodes.SMSG_NAME_QUERY_RESPONSE], "SMSG_NAME_QUERY_RESPONSE");
            nameCache.WritePackedGUID(guid);
            nameCache.WriteUInt8(0);
            nameCache.WriteString(character.Name);
            nameCache.WriteUInt8(0);
            nameCache.WriteUInt8(character.Race);
            nameCache.WriteUInt8(character.Gender);
            nameCache.WriteUInt8(character.Class);
            nameCache.WriteUInt8(0);
            manager.Send(nameCache);
        }

        public void HandleStandState(ref IPacketReader packet, ref IWorldManager manager)
        {
            manager.Account.ActiveCharacter.StandState = (StandState)packet.ReadUInt32();
            manager.Send(manager.Account.ActiveCharacter.BuildUpdate());
        }

        public void HandleTextEmote(ref IPacketReader packet, ref IWorldManager manager)
        {
            uint emote = packet.ReadUInt32();
            uint emotenum = packet.ReadUInt32();
            ulong guid = packet.ReadUInt64();
            uint emoteId = Emotes.Get((TextEmotes)emote);
            Character character = (Character)manager.Account.ActiveCharacter;

            PacketWriter pw = new PacketWriter(Sandbox.Instance.Opcodes[global::Opcodes.SMSG_TEXT_EMOTE], "SMSG_TEXT_EMOTE");
            pw.Write(character.Guid);
            pw.Write(emote);
            pw.Write(emotenum);
            pw.WriteUInt32(0);
            pw.WriteUInt8(0);
            manager.Send(pw);

            switch ((TextEmotes)emote)
            {
                case TextEmotes.EMOTE_SIT:
                    character.StandState = StandState.SITTING;
                    manager.Send(character.BuildUpdate());
                    return;

                case TextEmotes.EMOTE_STAND:
                    character.StandState = StandState.STANDING;
                    manager.Send(character.BuildUpdate());
                    return;

                case TextEmotes.EMOTE_SLEEP:
                    character.StandState = StandState.SLEEPING;
                    manager.Send(character.BuildUpdate());
                    return;

                case TextEmotes.EMOTE_KNEEL:
                    character.StandState = StandState.KNEEL;
                    manager.Send(character.BuildUpdate());
                    return;
            }

            if (emoteId > 0)
            {
                pw = new PacketWriter(Sandbox.Instance.Opcodes[global::Opcodes.SMSG_EMOTE], "SMSG_EMOTE");
                pw.WriteUInt32(emoteId);
                pw.WriteUInt64(character.Guid);
                manager.Send(pw);
            }
        }
    }
}
