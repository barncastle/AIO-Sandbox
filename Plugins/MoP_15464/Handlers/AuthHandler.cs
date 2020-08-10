using System;
using System.Net.Sockets;
using System.Text;
using Common.Constants;
using Common.Cryptography;
using Common.Extensions;
using Common.Interfaces;
using Common.Interfaces.Handlers;
using Common.Network;
using Common.Structs;

namespace MoP_15464.Handlers
{
    public class AuthHandler : IAuthHandler
    {
        public IPacketWriter HandleAuthChallenge()
        {
            Authenticator.Clear();
            PacketWriter writer = new PacketWriter(Sandbox.Instance.Opcodes[global::Opcodes.SMSG_AUTH_CHALLENGE], "SMSG_AUTH_CHALLENGE");
            writer.WriteInt16(0); // extended header
            writer.WriteUInt32(0xDEADBABE);
            writer.Write(new byte[32]);
            writer.WriteUInt8(1);
            return writer;
        }

        public IPacketWriter HandleRedirect()
        {
            PacketWriter proxyWriter = new PacketWriter();
            proxyWriter.Write(Encoding.ASCII.GetBytes("127.0.0.1:" + Sandbox.Instance.WorldPort));
            proxyWriter.WriteUInt8(0);
            return proxyWriter;
        }

        public void HandleAuthSession(ref IPacketReader packet, ref IWorldManager manager)
        {
            Authenticator.PacketCrypt.Initialised = true;

            packet.Position = 58;
            int addonsize = packet.ReadInt32();
            int decompressedSize = packet.ReadInt32();
            byte[] addonData = this.GetAddonInfo(packet);       

            // get account name
            packet.Position = 62 + addonsize;
            var bitUnpack = new BitUnpacker(packet);
            int nameLen = bitUnpack.GetBits<int>(12);
            string name = packet.ReadString(nameLen).ToUpper();

            Account account = new Account(name);
            account.Load<Character>();
            manager.Account = account;

            PacketWriter writer = new PacketWriter(Sandbox.Instance.Opcodes[global::Opcodes.SMSG_AUTH_RESPONSE], "SMSG_AUTH_RESPONSE");
            var bitPack = new BitPacker(writer);
            bitPack.Write(0); // IsInQueue
            bitPack.Write(1); // HasAccountData
            bitPack.Flush();
            writer.WriteUInt8(0);
            writer.WriteUInt8(4);
            writer.WriteUInt32(0);
            writer.WriteUInt32(0);
            writer.WriteUInt8(4);
            writer.WriteUInt32(0);
            writer.WriteUInt8(0xC);
            manager.Send(writer);

            // create addoninfo packet
            var addonPacketInfo = new PacketReader(addonData, false);
            var addonPacketResponse = new PacketWriter(Sandbox.Instance.Opcodes[global::Opcodes.SMSG_ADDON_INFO], "SMSG_ADDON_INFO");
            this.WriteAddonInfo(addonPacketInfo, addonPacketResponse, decompressedSize);
            manager.Send(addonPacketResponse);

            // Tutorial Flags : REQUIRED
            PacketWriter tutorial = new PacketWriter(Sandbox.Instance.Opcodes[global::Opcodes.SMSG_TUTORIAL_FLAGS], "SMSG_TUTORIAL_FLAGS");
            for (int i = 0; i < 8; i++)
                tutorial.WriteUInt32(0);
            manager.Send(tutorial);
        }

        public void HandleLogoutRequest(ref IPacketReader packet, ref IWorldManager manager)
        {
            var character = manager.Account.ActiveCharacter;
            if (character != null)
            {
                PacketWriter logoutComplete = new PacketWriter(Sandbox.Instance.Opcodes[global::Opcodes.SMSG_LOGOUT_COMPLETE], "SMSG_LOGOUT_COMPLETE");
                manager.Send(logoutComplete);
                character.IsOnline = false;
                manager.Account.Save();
            }
        }

        public void HandleRealmList(Socket socket)
        {
            while (socket.Connected)
            {
                System.Threading.Thread.Sleep(1);
                if (socket.Available > 0)
                {
                    byte[] buffer = new byte[socket.Available];
                    socket.Receive(buffer, buffer.Length, SocketFlags.None);

                    PacketReader packet = new PacketReader(buffer, false);
                    PacketWriter writer = new PacketWriter();

                    var op = (RealmlistOpcodes)packet.ReadByte();
                    switch (op)
                    {
                        case RealmlistOpcodes.LOGON_CHALLENGE:
                            writer.Write(Authenticator.LogonChallenge(packet));
                            break;

                        case RealmlistOpcodes.RECONNECT_CHALLENGE:
                            writer.Write(Authenticator.Reconnect_Challenge);
                            break;

                        case RealmlistOpcodes.LOGON_PROOF:
                            writer.Write(Authenticator.LogonProof(packet));
                            break;

                        case RealmlistOpcodes.RECONNECT_PROOF:
                            writer.WriteUInt8((byte)RealmlistOpcodes.RECONNECT_PROOF);
                            writer.WriteUInt8(0);
                            break;

                        case RealmlistOpcodes.REALMLIST_REQUEST:
                            // Send Realm List
                            byte[] realmName = Encoding.UTF8.GetBytes(Sandbox.Instance.RealmName);
                            byte[] redirect = Encoding.UTF8.GetBytes("127.0.0.1:" + Sandbox.Instance.WorldPort);

                            writer.WriteUInt8(0x10);
                            writer.WriteUInt16(0); // Packet length

                            writer.WriteUInt32(0);
                            writer.WriteUInt16(1); // Realm count

                            writer.WriteUInt8(0); // Flags
                            writer.WriteUInt8(0); // Locked
                            writer.WriteUInt8(0); // Icon
                            writer.Write(realmName);
                            writer.WriteUInt8(0);
                            writer.Write(redirect);
                            writer.WriteUInt8(0);
                            writer.WriteFloat(0); // Population
                            writer.WriteUInt8(0); // Char count
                            writer.WriteUInt8(1); // TimeZone
                            writer.WriteUInt8(0);

                            writer.WriteUInt16(0x10);

                            writer.BaseStream.Position = 1;
                            writer.WriteUInt16((ushort)(writer.BaseStream.Length - 3)); // Packet length
                            break;
                    }

                    if (writer.BaseStream.Length > 0)
                        socket.SendData(writer, op.ToString());
                }
            }

            socket.Close();
        }
    }
}
