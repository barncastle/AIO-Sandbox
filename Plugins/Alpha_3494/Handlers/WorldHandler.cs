using Common.Constants;
using Common.Extensions;
using Common.Interfaces;
using Common.Interfaces.Handlers;
using Common.Logging;

namespace Alpha_3494.Handlers
{
    public class WorldHandler : IWorldHandler
    {
        public void HandlePing(ref IPacketReader packet, ref IWorldManager manager)
        {
            PacketWriter writer = new PacketWriter(Sandbox.Instance.Opcodes[global::Opcodes.SMSG_PONG], "SMSG_PONG");
            writer.WriteUInt32(packet.ReadUInt32());
            manager.Send(writer);
        }

        public void HandlePlayerLogin(ref IPacketReader packet, ref IWorldManager manager)
        {
            ulong guid = packet.ReadUInt64();
            Character character = (Character)manager.Account.SetActiveChar(guid, Sandbox.Instance.Build);
            character.DisplayId = character.GetDisplayId();

            // Tutorial Flags : REQUIRED
            PacketWriter tutorial = new PacketWriter(Sandbox.Instance.Opcodes[global::Opcodes.SMSG_TUTORIAL_FLAGS], "SMSG_TUTORIAL_FLAGS");
            for (int i = 0; i < 8; i++)
                tutorial.WriteInt32(-1);
            manager.Send(tutorial);

            // Enable UI : REQUIRED
            PacketWriter uiconfig = new PacketWriter(Sandbox.Instance.Opcodes[global::Opcodes.SMSG_UI_CONFIG_MD5], "SMSG_UI_CONFIG_MD5");
            uiconfig.Write(new byte[80]);
            manager.Send(uiconfig);

            HandleQueryTime(ref packet, ref manager);

            manager.Send(character.BuildUpdate());
        }

        public void HandleWorldTeleport(ref IPacketReader packet, ref IWorldManager manager)
        {
            packet.ReadUInt32();
            byte zone = packet.ReadUInt8();
            float x = packet.ReadFloat();
            float y = packet.ReadFloat();
            float z = packet.ReadFloat();
            float o = packet.ReadFloat();

            PacketWriter movementStatus = new PacketWriter(Sandbox.Instance.Opcodes[global::Opcodes.SMSG_MOVE_WORLDPORT_ACK], "SMSG_MOVE_WORLDPORT_ACK");
            movementStatus.WriteUInt64(0);
            movementStatus.WriteFloat(0);
            movementStatus.WriteFloat(0);
            movementStatus.WriteFloat(0);
            movementStatus.WriteFloat(0);
            movementStatus.WriteFloat(x);
            movementStatus.WriteFloat(y);
            movementStatus.WriteFloat(z);
            movementStatus.WriteFloat(o);
            movementStatus.WriteFloat(0);
            movementStatus.WriteUInt32(0);
            manager.Send(movementStatus);
        }

        public void HandleQueryTime(ref IPacketReader packet, ref IWorldManager manager)
        {
            PacketWriter queryTime = new PacketWriter(Sandbox.Instance.Opcodes[global::Opcodes.SMSG_LOGIN_SETTIMESPEED], "SMSG_LOGIN_SETTIMESPEED");
            queryTime.WriteInt32(this.GetTime());
            queryTime.WriteFloat(0.01666667f);
            manager.Send(queryTime);
        }

        public void HandleAreaTrigger(ref IPacketReader packet, ref IWorldManager manager)
        {
            uint id = packet.ReadUInt32();
            if (AreaTriggers.Triggers.ContainsKey(id))
            {
                var loc = AreaTriggers.Triggers[id];

                // Hacky override
                switch (id)
                {
                    case 45: // Scarlet Monestary
                        loc = new Common.Structs.Location(77f, -1f, 20f, 0, 44);
                        break;
                }

                manager.Account.ActiveCharacter.Teleport(loc, ref manager);
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
