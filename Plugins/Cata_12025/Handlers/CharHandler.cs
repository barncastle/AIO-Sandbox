using System;
using System.Linq;
using Common.Commands;
using Common.Constants;
using Common.Cryptography;
using Common.Extensions;
using Common.Interfaces;
using Common.Interfaces.Handlers;
using Common.Structs;

namespace WotLK_12025.Handlers
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
            writer.WriteUInt8((byte)result.Count());

            foreach (Character c in result)
            {
                writer.WriteUInt64(c.Guid);
                writer.WriteString(c.Name);

                writer.WriteUInt8(c.Race);
                writer.WriteUInt8(c.Class);
                writer.WriteUInt8(c.Gender);
                writer.WriteUInt8(c.Skin);
                writer.WriteUInt8(c.Face);
                writer.WriteUInt8(c.HairStyle);
                writer.WriteUInt8(c.HairColor);
                writer.WriteUInt8(c.FacialHair);
                writer.WriteUInt8((byte)c.Level);

                writer.WriteUInt32(c.Zone);
                writer.WriteUInt32(c.Location.Map);

                writer.WriteFloat(c.Location.X);
                writer.WriteFloat(c.Location.Y);
                writer.WriteFloat(c.Location.Z);

                writer.WriteUInt32(0);
                writer.WriteUInt32(0);
                writer.WriteUInt32(0);

                writer.WriteUInt8(0);
                writer.WriteUInt32(0);
                writer.WriteUInt32(0);
                writer.WriteUInt32(0);

                // Items
                int inventorySize = Authenticator.ClientBuild < 12025 ? 0x14 : 0x17;
                for (int j = 0; j < inventorySize; j++)
                {
                    writer.WriteUInt32(0);    // DisplayId
                    writer.WriteUInt8(0);     // InventoryType
                    writer.WriteUInt32(0);    // Enchant
                }
            }

            manager.Send(writer);
        }

        public void HandleMessageChat(ref IPacketReader packet, ref IWorldManager manager)
        {
            var character = manager.Account.ActiveCharacter;

            PacketWriter writer = new PacketWriter(Sandbox.Instance.Opcodes[global::Opcodes.SMSG_MESSAGECHAT], "SMSG_MESSAGECHAT");
            writer.WriteUInt8((byte)packet.ReadInt32()); // System Message
            packet.ReadUInt32();
            writer.WriteUInt32(0); // Language: General
            writer.WriteUInt64(character.Guid);

            string message = packet.ReadString();
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
