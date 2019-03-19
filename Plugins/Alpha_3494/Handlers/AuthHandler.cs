using System.Linq;
using System.Net.Sockets;
using System.Text;
using Common.Constants;
using Common.Interfaces;
using Common.Interfaces.Handlers;
using Common.Structs;

namespace Alpha_3494.Handlers
{
    public class AuthHandler : IAuthHandler
    {
        public IPacketWriter HandleAuthChallenge()
        {
            PacketWriter writer = new PacketWriter(Sandbox.Instance.Opcodes[global::Opcodes.SMSG_AUTH_CHALLENGE], "SMSG_AUTH_CHALLENGE");
            writer.WriteUInt8(0xC);
            return writer;
        }

        public IPacketWriter HandleRedirect()
        {
            PacketWriter proxyWriter = new PacketWriter();
            proxyWriter.WriteBytes(Encoding.ASCII.GetBytes("127.0.0.1:" + Sandbox.Instance.WorldPort));
            proxyWriter.WriteUInt8(0);
            return proxyWriter;
        }

        public void HandleAuthSession(ref IPacketReader packet, ref IWorldManager manager)
        {
            packet.ReadUInt64();
            byte[] data = packet.ReadToEnd().TakeWhile(x => x != 10 && x != 13).ToArray();
            string name = Encoding.UTF8.GetString(data).ToUpper();

            Account account = new Account(name);
            account.Load<Character>();
            manager.Account = account;

            PacketWriter writer = new PacketWriter(Sandbox.Instance.Opcodes[global::Opcodes.SMSG_AUTH_RESPONSE], "SMSG_AUTH_RESPONSE");
            writer.WriteUInt8(0x0C); //AUTH_OK
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
            byte[] realmName = Encoding.ASCII.GetBytes(Sandbox.Instance.RealmName);
            byte[] redirect = Encoding.ASCII.GetBytes("127.0.0.1:" + Sandbox.Instance.RedirectPort);

            PacketWriter writer = new PacketWriter();
            writer.PreAuth = false;
            writer.WriteUInt16(0);
            writer.WriteUInt8(0x10);
            writer.WriteUInt8(1); //Realm count
            writer.WriteBytes(realmName);
            writer.WriteUInt8(0);
            writer.WriteBytes(redirect);
            writer.WriteUInt8(0);
            writer.WriteUInt32(0);
            socket.SendData(writer, "REALMLIST_REQUEST");
            socket.Close();
        }
    }
}
