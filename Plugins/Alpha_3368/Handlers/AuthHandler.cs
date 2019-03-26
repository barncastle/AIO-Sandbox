using System;
using System.Net.Sockets;
using System.Text;
using Common.Constants;
using Common.Interfaces;
using Common.Interfaces.Handlers;
using Common.Structs;

namespace Alpha_3368.Handlers
{
    public class AuthHandler : IAuthHandler
    {
        public IPacketWriter HandleAuthChallenge()
        {
            PacketWriter writer = new PacketWriter(Sandbox.Instance.Opcodes[global::Opcodes.SMSG_AUTH_CHALLENGE], "SMSG_AUTH_CHALLENGE");
            writer.WriteUInt8(0);
            writer.WriteUInt8(0);
            writer.WriteUInt8(0);
            writer.WriteUInt8(0);
            writer.WriteUInt8(0);
            writer.WriteUInt8(0);
            return writer;
        }

        public void HandleRealmList(Socket socket)
        {
            byte[] realmName = Encoding.ASCII.GetBytes(Sandbox.Instance.RealmName);
            byte[] redirect = Encoding.ASCII.GetBytes("127.0.0.1:" + Sandbox.Instance.RedirectPort);

            PacketWriter writer = new PacketWriter();
            writer.WriteUInt8(1);
            writer.Write(realmName);
            writer.WriteUInt8(0);
            writer.Write(redirect);
            writer.WriteUInt8(0);
            writer.WriteUInt32(0);

            socket.SendData(writer, "REALMLIST_REQUEST");
            socket.Close();
        }

        public void HandleAuthSession(ref IPacketReader packet, ref IWorldManager manager)
        {
            packet.Position = 6;
            Common.Cryptography.ClientAuth.ClientBuild = packet.ReadUInt16();

            packet.Position = 14;
            string login = packet.ReadString();
            string username = login.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries)[0];

            Account account = new Account(username.ToUpper());
            account.Load<Character>();
            manager.Account = account;

            PacketWriter writer = new PacketWriter(Sandbox.Instance.Opcodes[global::Opcodes.SMSG_AUTH_RESPONSE], "SMSG_AUTH_RESPONSE");
            writer.WriteUInt8(0x0C); // AUTH_OK
            manager.Send(writer);
        }

        public void HandleLogoutRequest(ref IPacketReader packet, ref IWorldManager manager)
        {
            PacketWriter logoutComplete = new PacketWriter(Sandbox.Instance.Opcodes[global::Opcodes.SMSG_LOGOUT_COMPLETE], "SMSG_LOGOUT_COMPLETE");
            manager.Send(logoutComplete);

            var character = manager.Account.ActiveCharacter;
            if (character != null)
                character.IsOnline = false;
        }

        public IPacketWriter HandleRedirect()
        {
            PacketWriter proxyWriter = new PacketWriter();
            proxyWriter.Write(Encoding.ASCII.GetBytes("127.0.0.1:" + Sandbox.Instance.WorldPort));
            proxyWriter.WriteUInt8(0);
            return proxyWriter;
        }
    }
}
