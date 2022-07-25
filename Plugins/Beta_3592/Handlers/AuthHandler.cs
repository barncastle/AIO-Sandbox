﻿using System.Net.Sockets;
using System.Text;
using Common.Constants;
using Common.Cryptography;
using Common.Extensions;
using Common.Interfaces;
using Common.Interfaces.Handlers;
using Common.Structs;

namespace Beta_3592.Handlers
{
    public class AuthHandler : IAuthHandler
    {
        public IPacketWriter HandleAuthChallenge()
        {
            PacketWriter writer = new PacketWriter(Sandbox.Instance.Opcodes[global::Opcodes.SMSG_AUTH_CHALLENGE], "SMSG_AUTH_CHALLENGE");
            writer.WriteInt32(0);
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
            packet.ReadUInt64();
            string name = packet.ReadString().ToUpper();

            Account account = new Account(name);
            account.Load<Character>();
            manager.Account = account;

            PacketWriter writer = new PacketWriter(Sandbox.Instance.Opcodes[global::Opcodes.SMSG_AUTH_RESPONSE], "SMSG_AUTH_RESPONSE");
            writer.WriteUInt8(0x0C); // AUTH_OK
            manager.Send(writer);
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
                            byte[] realmName = Encoding.ASCII.GetBytes(Sandbox.Instance.RealmName);
                            byte[] redirect = Encoding.ASCII.GetBytes("127.0.0.1:" + Sandbox.Instance.RedirectPort);

                            writer.WriteUInt8(0x10);
                            writer.WriteUInt16((ushort)(11 + realmName.Length + redirect.Length));
                            writer.WriteUInt32(0);
                            writer.WriteUInt8(1); // Realm count
                            writer.Write(realmName);
                            writer.WriteUInt8(0);
                            writer.Write(redirect);
                            writer.WriteUInt8(0);
                            writer.WriteUInt32(0);
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
