using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common.Constants
{
    public enum Classes : byte
    {
        WARRIOR = 1,
        PALADIN = 2,
        HUNTER = 3,
        ROGUE = 4,
        PRIEST = 5,
        SHAMAN = 7,
        MAGE = 8,
        WARLOCK = 9,
        DRUID = 11
    }

    public enum PowerTypes : uint
    {
        MANA = 0,
        RAGE = 1,
        FOCUS = 2,
        ENERGY = 3,
        HAPPINESS = 4,
        POWER_HEALTH = 0xFFFFFFFE
    }

    public enum StandState : uint
    {
        STANDING = 0x0,
        SITTING = 0x1,
        SITTINGCHAIR = 0x2,
        SLEEPING = 0x3,
        SITTINGCHAIRLOW = 0x4,
        FIRSTCHAIRSIT = 0x4,
        SITTINGCHAIRMEDIUM = 0x5,
        SITTINGCHAIRHIGH = 0x6,
        LASTCHAIRSIT = 0x6,
        DEAD = 0x7,
        KNEEL = 0x8,
    };

    public enum RealmlistOpcodes : byte
    {
        LOGON_CHALLENGE = 0,
        LOGON_PROOF = 1,
        RECONNECT_CHALLENGE = 2,
        RECONNECT_PROOF = 3,
        REALMLIST_REQUEST = 16,
    }
}
