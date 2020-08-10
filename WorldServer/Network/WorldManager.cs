using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Common.Constants;
using Common.Extensions;
using Common.Interfaces;
using Common.Logging;
using Common.Network;
using Common.Structs;

namespace WorldServer.Network
{
    public class WorldManager : IWorldManager
    {
        public static WorldSocket WorldSession { get; set; }

        public Account Account { get; set; }
        public Socket Socket { get; set; }
        public ISandbox SandboxHost => WorldServer.Sandbox;

        private DateTime? LastPacket;        


        public void Recieve()
        {
            Send(WorldServer.Sandbox.AuthHandler.HandleAuthChallenge()); // SMSG_AUTH_CHALLENGE

            Task.Run(DoAutoSaveAsync);

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

                        buffer = buffer.AsSpan().Slice((int)pkt.Size).ToArray();
                    }

                    LastPacket = DateTime.Now;
                }
            }

            // save the account and close the socket
            Account?.Save();
            Log.Message(LogType.DEBUG, "CLIENT DISCONNECTED {0}", Account?.Name);
            Socket.Close();
        }

        public void Send(IPacketWriter packet) => Socket.SendData(packet, packet.Name);

        public void Handshake()
        {
            if (SandboxHost.Expansion < Expansions.MoP)
                return;
            
            string handshake = "WORLD OF WARCRAFT CONNECTION - SERVER TO CLIENT\0";

            byte[] data = new byte[handshake.Length + 2];
            data[0] = (byte)handshake.Length;
            System.Text.Encoding.UTF8.GetBytes(handshake).CopyTo(data, 2);

            Socket.Send(data, 0, data.Length, SocketFlags.None);
        }


        private async Task DoAutoSaveAsync()
        {
            await Task.Delay(45000); // initial delay

            while (Socket?.Connected == true)
            {
                Account?.Save();
                await Task.Delay(45000);

                // check for disconnect, pings should be every ~20 seconds
                if (LastPacket.HasValue && (DateTime.Now - LastPacket.Value).TotalSeconds > 45)
                    Socket?.Disconnect(false);
            }
        }
    }
}
