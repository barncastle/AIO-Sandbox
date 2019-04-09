using System;
using System.Net.Sockets;
using Common.Interfaces;
using Common.Logging;

namespace Common.Extensions
{
    public static class Extensions
    {
        public static void SendData(this Socket socket, IPacketWriter packet, string packetname = "")
        {
            byte[] buffer = packet.ReadDataToSend();
            try
            {
                socket.Send(buffer, 0, buffer.Length, SocketFlags.None);
                if (!string.IsNullOrEmpty(packetname))
                    Log.Message(LogType.DUMP, "SENT {0}.", packetname);
            }
            catch (Exception e)
            {
                Log.Message(LogType.ERROR, "{0}", e.Message);
            }
        }

        public static string ToUpperFirst(this string s)
        {
            if (string.IsNullOrEmpty(s))
                return string.Empty;

            return char.ToUpper(s[0]) + s.ToLower().Substring(1);
        }

        public static byte Clamp(this byte i, byte min, byte max) => Math.Max(Math.Min(i, max), min);

    }
}
