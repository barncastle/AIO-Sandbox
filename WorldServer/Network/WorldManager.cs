using System;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Common.Constants;
using Common.Interfaces;
using Common.Logging;
using Common.Network;
using Common.Structs;

namespace WorldServer.Network
{
    public class WorldManager : IWorldManager
    {
        public Account Account { get; set; }
        public Socket Socket { get; set; }
        public static WorldSocket WorldSession { get; set; }

        private int autosave;

        public void Recieve()
        {
            SetAutosave();

            Send(WorldServer.Sandbox.AuthHandler.HandleAuthChallenge()); // SMSG_AUTH_CHALLENGE

            while (Socket.Connected)
            {
                Thread.Sleep(1);
                if (Socket.Available > 0)
                {
                    byte[] buffer = new byte[Socket.Available];
                    Socket.Receive(buffer, buffer.Length, SocketFlags.None);

                    while (buffer.Length > 0)
                    {
                        IPacketReader pkt = WorldServer.Sandbox.ReadPacket(buffer);
                        if (WorldServer.Sandbox.Opcodes.OpcodeExists(pkt.Opcode))
                        {
                            Opcodes opcode = WorldServer.Sandbox.Opcodes[pkt.Opcode];
                            Log.Message(LogType.DUMP, "RECEIVED OPCODE: {0}, LENGTH: {1}", opcode.ToString(), pkt.Size);
                            PacketManager.InvokeHandler(pkt, this, opcode);
                        }
                        else
                        {
                            Log.Message(LogType.DEBUG, "UNKNOWN OPCODE: 0x{0} ({1}), LENGTH: {2}", pkt.Opcode.ToString("X"), pkt.Opcode, pkt.Size);
                        }

                        if (buffer.Length == pkt.Size)
                            break;

                        buffer = buffer.Skip((int)pkt.Size).ToArray();
                    }
                }

                DoAutosave();
            }

            // save the account and reset the character
            if (Account != null)
            {
                Account.Save();
                if (Account.ActiveCharacter != null)
                    Account.ActiveCharacter.IsOnline = false;
            }

            Log.Message(LogType.DEBUG, "CLIENT DISCONNECTED {0}", Account?.Name);
            Socket.Close();
        }

        public void Send(IPacketWriter packet)
        {
            Socket.SendData(packet, packet.Name);
        }

        private void SetAutosave()
        {
            unchecked { autosave = Environment.TickCount + 60000; }
        }

        private void DoAutosave()
        {
            unchecked
            {
                if (Environment.TickCount >= autosave || Environment.TickCount < (autosave - 60000))
                {
                    SetAutosave();
                    Task.Run(() => Account?.Save());
                }
            }
        }
    }
}
