using System;
using System.IO;
using System.IO.Compression;
using Common.Interfaces;
using Common.Interfaces.Handlers;
using Common.Network;

namespace Common.Extensions
{
    public static class HandlerExtensions
    {
        public static int GetTime(this IWorldHandler worldhandler)
        {
            DateTime now = DateTime.Now;
            int year = (now.Year - 2000) << 24;
            int month = (now.Month - 1) << 20;
            int day = (now.Day - 1) << 14;
            int dow = (int)now.DayOfWeek << 11;
            int hour = now.Hour << 6;

            return now.Minute + hour + dow + day + month + year;
        }

        public static TReader GetAddonInfoPacket<TReader>(this IAuthHandler authhandler, IPacketReader packet) where TReader : BasePacketReader, new()
        {
            byte[] data = packet.ReadBytes((int)(packet.Size - packet.Position));

            using (var msIn = new MemoryStream(data, 2, data.Length - 2)) // skip zlib header
            using (var dfltStream = new DeflateStream(msIn, CompressionMode.Decompress))
            using (var msOut = new MemoryStream())
            {
                dfltStream.CopyTo(msOut);
                return (TReader)Activator.CreateInstance(typeof(TReader), msOut.ToArray(), false);
            }
        }
    }
}
