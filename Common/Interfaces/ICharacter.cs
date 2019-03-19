using Common.Constants;
using Common.Structs;

namespace Common.Interfaces
{
    public interface ICharacter
    {
        int Build { get; set; }

        ulong Guid { get; set; }
        string Name { get; set; }
        byte Race { get; set; }
        byte Class { get; set; }
        byte Gender { get; set; }
        byte Skin { get; set; }
        byte Face { get; set; }
        byte HairStyle { get; set; }
        byte HairColor { get; set; }
        byte FacialHair { get; set; }
        uint Level { get; set; }
        uint Zone { get; set; }
        Location Location { get; set; }
        bool IsOnline { get; set; }
        uint Health { get; set; }
        uint Mana { get; set; }
        uint Rage { get; set; }
        uint Focus { get; set; }
        uint Energy { get; set; }
        uint Strength { get; set; }
        uint Agility { get; set; }
        uint Stamina { get; set; }
        uint Intellect { get; set; }
        uint Spirit { get; set; }
        byte PowerType { get; set; }
        StandState StandState { get; set; }
        bool IsTeleporting { get; set; }
        uint DisplayId { get; set; }
        uint MountDisplayId { get; set; }
        float Scale { get; set; }

        void Teleport(float x, float y, float z, float o, uint map, ref IWorldManager manager);

        IPacketWriter BuildForceSpeed(float modifier, bool swim = false);

        IPacketWriter BuildMessage(string text);

        IPacketWriter BuildUpdate();
    }
}
