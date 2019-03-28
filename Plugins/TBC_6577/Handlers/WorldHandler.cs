using System;
using Common.Constants;
using Common.Extensions;
using Common.Interfaces;
using Common.Interfaces.Handlers;
using Common.Logging;

namespace TBC_6577.Handlers
{
    public class WorldHandler : IWorldHandler
    {
        public void HandlePing(ref IPacketReader packet, ref IWorldManager manager)
        {
            PacketWriter writer = new PacketWriter(Sandbox.Instance.Opcodes[global::Opcodes.SMSG_PONG], "SMSG_PONG");
            writer.WriteUInt32(packet.ReadUInt32());
            manager.Send(writer);
        }

        public void HandleQueryTime(ref IPacketReader packet, ref IWorldManager manager)
        {
            PacketWriter queryTime = new PacketWriter(Sandbox.Instance.Opcodes[global::Opcodes.SMSG_LOGIN_SETTIMESPEED], "SMSG_LOGIN_SETTIMESPEED");
            queryTime.WriteInt32(this.GetTime());
            queryTime.WriteFloat(0.01666667f);
            manager.Send(queryTime);
        }

        public void HandlePlayerLogin(ref IPacketReader packet, ref IWorldManager manager)
        {
            ulong guid = packet.ReadUInt64();
            Character character = (Character)manager.Account.SetActiveChar(guid, Sandbox.Instance.Build);
            character.DisplayId = character.GetDisplayId();

            // Verify World : REQUIRED
            PacketWriter verify = new PacketWriter(Sandbox.Instance.Opcodes[global::Opcodes.SMSG_LOGIN_VERIFY_WORLD], "SMSG_LOGIN_VERIFY_WORLD");
            verify.WriteUInt32(character.Location.Map);
            verify.WriteFloat(character.Location.X);
            verify.WriteFloat(character.Location.Y);
            verify.WriteFloat(character.Location.Z);
            verify.WriteFloat(character.Location.O);
            manager.Send(verify);

            // Account Data Hash : REQUIRED
            PacketWriter accountdata = new PacketWriter(Sandbox.Instance.Opcodes[global::Opcodes.SMSG_ACCOUNT_DATA_MD5], "SMSG_ACCOUNT_DATA_MD5");
            for (int i = 0; i < 31; i++)
                accountdata.WriteInt32(0);
            manager.Send(accountdata);

            // Tutorial Flags : REQUIRED
            PacketWriter tutorial = new PacketWriter(Sandbox.Instance.Opcodes[global::Opcodes.SMSG_TUTORIAL_FLAGS], "SMSG_TUTORIAL_FLAGS");
            for (int i = 0; i < 8; i++)
                tutorial.WriteInt32(-1);
            manager.Send(tutorial);

            HandleQueryTime(ref packet, ref manager);

            manager.Send(character.BuildUpdate());

            // handle flying
            manager.Send(character.BuildFly(character.IsFlying));

            // Force timesync : REQUIRED
            PacketWriter timesyncreq = new PacketWriter(Sandbox.Instance.Opcodes[global::Opcodes.SMSG_TIME_SYNC_REQ], "SMSG_TIME_SYNC_REQ");
            timesyncreq.Write(0);
            manager.Send(timesyncreq);
        }

        public void HandleWorldTeleport(ref IPacketReader packet, ref IWorldManager manager)
        {
            throw new NotImplementedException();
        }

        public void HandleAreaTrigger(ref IPacketReader packet, ref IWorldManager manager)
        {
            uint id = packet.ReadUInt32();
            if (AreaTriggers.Triggers.ContainsKey(id))
            {
                var loc = AreaTriggers.Triggers[id];
                manager.Account.ActiveCharacter.Teleport(loc.X, loc.Y, loc.Z, loc.O, loc.Map, ref manager);
            }
            else
                Log.Message(LogType.ERROR, "AreaTrigger for {0} missing.", id);
        }

        public void HandleZoneUpdate(ref IPacketReader packet, ref IWorldManager manager)
        {
            manager.Account.ActiveCharacter.Zone = packet.ReadUInt32();
        }
    }
}
