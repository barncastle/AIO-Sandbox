namespace Common.Interfaces.Handlers
{
    public interface IWorldHandler
    {
        void HandleWorldTeleport(ref IPacketReader packet, ref IWorldManager manager);
        void HandleWorldTeleportAck(ref IPacketReader packet, ref IWorldManager manager);
        void HandleWorldPortAck(ref IPacketReader packet, ref IWorldManager manager);
        void HandlePlayerLogin(ref IPacketReader packet, ref IWorldManager manager);
        void HandlePing(ref IPacketReader packet, ref IWorldManager manager);
        void HandleQueryTime(ref IPacketReader packet, ref IWorldManager manager);
        void HandleAreaTrigger(ref IPacketReader packet, ref IWorldManager manager);
        void HandleZoneUpdate(ref IPacketReader packet, ref IWorldManager manager);
    }
}
