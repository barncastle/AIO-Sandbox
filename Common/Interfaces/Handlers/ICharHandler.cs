using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common.Interfaces.Handlers
{
    public interface ICharHandler
    {
        void HandleCharEnum(ref IPacketReader packet, ref IWorldManager manager);
        void HandleCharCreate(ref IPacketReader packet, ref IWorldManager manager);
        void HandleCharDelete(ref IPacketReader packet, ref IWorldManager manager);
        void HandleNameCache(ref IPacketReader packet, ref IWorldManager manager);
        void HandleMovementStatus(ref IPacketReader packet, ref IWorldManager manager);
        void HandleMessageChat(ref IPacketReader packet, ref IWorldManager manager);
        void HandleStandState(ref IPacketReader packet, ref IWorldManager manager);
        void HandleTextEmote(ref IPacketReader packet, ref IWorldManager manager);
    }
}
