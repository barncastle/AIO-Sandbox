using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Common.Interfaces;
using Common.Logging;

namespace WorldServer.Packets
{
    public static class DanceStudioHandler
    {
        private static List<DanceCache> _danceCache;
        private static readonly Random _random = new Random();

        public static void LoadDanceStudio(ref IWorldManager manager)
        {
            if (!File.Exists("Resources\\dancecache.json"))
            {
                Directory.CreateDirectory("Resources");
                File.WriteAllBytes("Resources\\dancecache.json", Common.Properties.Resources.dancecache);
            }
                
            ulong guid = manager.Account.ActiveCharacter.Guid;
            uint i = 1;

            // load locally defined dance routines since the editor isn't implemented
            // correct all calculated and dynamic fields
            var options = new JsonSerializerOptions { IncludeFields = true };
            _danceCache = JsonSerializer.Deserialize<List<DanceCache>>(File.ReadAllText("Resources\\dancecache.json"), options);
            _danceCache.RemoveAll(x => x.Routine.Length == 0);
            _danceCache.ForEach(x =>
            {
                x.Id = i++;
                x.Guid = guid;
                x.MoveCount = (uint)x.Routine.Length;
                x.Checksum = GetCacheChecksum(x);
            });

            // enable all dance moves, 1 bit per LockId => max LockId is 4 in DBC
            // moveUnlocked = (LockId == 0 || ((CGDanceStudio::m_activePlayersLearnedDanceMoves >> (LockId - 1)) & 1) != 0);
            SendLearnedDancedMoves(0b1111, ref manager);

            // tell the client each dance was saved and available
            foreach (DanceCache dance in _danceCache)
                SendNotifyDance(dance.Id, DanceNotifyFlags.SAVE_DANCE, ref manager);
        }

        public static void SendNotifyDance(uint danceId, DanceNotifyFlags flags, ref IWorldManager manager)
        {
            DanceCache dance = _danceCache.Find(x => x.Id == danceId);

            if (dance == null)
                flags |= DanceNotifyFlags.HAS_ERRORED;

            IPacketWriter writer = manager.SandboxHost.GetPacketWriter(Opcodes.SMSG_NOTIFY_DANCE);
            writer.WriteUInt32((uint)flags);

            if (!flags.HasFlag(DanceNotifyFlags.HAS_ERRORED))
            {
                writer.WriteUInt32(dance.Id);
                writer.WriteString(dance.Name);
                writer.WriteUInt32(dance.Checksum);
            }
            else
            {
                writer.WriteUInt32((flags & ~DanceNotifyFlags.HAS_ERRORED) switch
                {
                    DanceNotifyFlags.DUPLICATE_DANCE => 0, // ERR_DANCE_CREATE_DUPLICATE
                    DanceNotifyFlags.SAVE_DANCE => 1,      // ERR_DANCE_SAVE_FAILED
                    DanceNotifyFlags.DELETE_DANCE => 2,    // ERR_DANCE_DELETE_FAILED
                    _ => 0
                });
            }

            manager.Send(writer);
        }

        public static void HandlePlayDance(ref IPacketReader packet, ref IWorldManager manager)
        {
            uint danceId = packet.ReadUInt32();
            uint checksum = packet.ReadUInt32();

            DanceCache dance = _danceCache.Find(x => x.Id == danceId);
            if (dance == null)
                return;

            IPacketWriter writer = manager.SandboxHost.GetPacketWriter(Opcodes.SMSG_PLAY_DANCE);
            writer.WriteUInt64(manager.Account.ActiveCharacter.Guid);
            writer.WriteUInt32(dance.Id);
            writer.WriteUInt32((uint)_random.Next(0, dance.Routine.Length)); // first move index (randomised)
            writer.WriteUInt32((uint)_random.Next()); // seed for move chances
            writer.WriteUInt32(dance.Checksum);

            manager.Send(writer);
        }

        public static void HandleStopDance(ref IPacketReader packet, ref IWorldManager manager)
        {
            // Notifies everyone around like SMSG_EMOTE
            IPacketWriter writer = manager.SandboxHost.GetPacketWriter(Opcodes.SMSG_STOP_DANCE);
            writer.WritePackedGUID(manager.Account.ActiveCharacter.Guid);

            manager.Send(writer);
        }

        public static void HandleSyncDance(ref IPacketReader _, ref IWorldManager _1)
        {
            Log.Message(LogType.NORMAL, "Dance Move Completed");
        }

        public static void HandleDanceQuery(ref IPacketReader packet, ref IWorldManager manager)
        {
            uint danceId = packet.ReadUInt32();

            if (_danceCache.FindIndex(x => x.Id == danceId) == -1)
            {
                SendInvalidateDance(danceId, ref manager);
                return;
            }

            SendDanceQueryResponse(danceId, ref manager);
        }

        public static void SendDanceQueryResponse(uint danceId, ref IWorldManager manager)
        {
            DanceCache dance = _danceCache.Find(x => x.Id == danceId);

            // not found, invalidate
            if (dance == null)
            {
                SendInvalidateDance(danceId, ref manager);
                return;
            }

            IPacketWriter writer = manager.SandboxHost.GetPacketWriter(Opcodes.SMSG_DANCE_QUERY_RESPONSE);
            writer.WriteUInt32(dance.Id);
            writer.WritePackedGUID(manager.Account.ActiveCharacter.Guid);
            writer.WriteString(dance.Name);
            writer.WriteUInt32(dance.MoveCount);

            for (int i = 0; i < dance.Routine.Length; i++)
            {
                writer.WriteUInt16(dance.Routine[i].MoveId);
                writer.WriteUInt8(dance.Routine[i].Chance);
                writer.WriteUInt8(dance.Routine[i].UseFallback);
            }

            writer.WriteUInt32(dance.Checksum);

            manager.Send(writer);
        }

        public static void SendInvalidateDance(uint danceId, ref IWorldManager manager)
        {
            IPacketWriter writer = manager.SandboxHost.GetPacketWriter(Opcodes.SMSG_INVALIDATE_DANCE);
            writer.WriteUInt32(danceId);

            manager.Send(writer);
        }

        public static void SendLearnedDancedMoves(ulong danceMoveMask, ref IWorldManager manager)
        {
            IPacketWriter writer = manager.SandboxHost.GetPacketWriter(Opcodes.SMSG_LEARNED_DANCE_MOVES);
            writer.WriteUInt64(danceMoveMask);

            manager.Send(writer);
        }

        // FNV-like checksum, Blizzard implementation unknown
        private static uint GetCacheChecksum(DanceCache dance)
        {
            unchecked
            {
                uint hash = 0x811C9DC5u;
                hash = (hash ^ (uint)dance.GetHashCode()) * 0x1000193u;
                for (int i = 0; i < dance.Routine.Length; i++)
                    hash = (hash ^ (uint)dance.Routine[i].GetHashCode()) * 0x1000193u;

                return hash;
            }
        }
    }

    [Flags]
    public enum DanceNotifyFlags
    {
        DUPLICATE_DANCE = 1,
        SAVE_DANCE = 2,
        DELETE_DANCE = 4,
        HAS_ERRORED = 8
    }

    record DanceCache
    {
        public uint Id;
        public ulong Guid;
        public string Name; // char[128]
        public DanceMove[] Routine;
        public uint MoveCount;
        public uint Checksum; // checksum of the routine, used by Blizzard to verify the routine matches cached data
    };

    record DanceMove
    {
        public ushort MoveId; // DanceMoves.dbc ID
        public byte Chance; // Random seed to display move: 0-100
        public byte UseFallback; // forces the fallback move
    };
}
