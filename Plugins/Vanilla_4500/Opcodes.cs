using System.Collections.Generic;
using System.Linq;
using Common.Interfaces;

namespace Vanilla_4500
{
    public class Opcodes : IOpcodes
    {
        private readonly IDictionary<global::Opcodes, uint> opcodes = new Dictionary<global::Opcodes, uint>()
        {
            { global::Opcodes.SMSG_AUTH_CHALLENGE, 0x1EC },
            { global::Opcodes.SMSG_AUTH_RESPONSE, 0x1EE },
            { global::Opcodes.CMSG_AUTH_SESSION, 0x1ED },
            { global::Opcodes.CMSG_CHAR_CREATE, 0x36 },
            { global::Opcodes.SMSG_CHAR_CREATE, 0x3A },
            { global::Opcodes.CMSG_CHAR_DELETE, 0x38 },
            { global::Opcodes.SMSG_CHAR_DELETE, 0x3C },
            { global::Opcodes.CMSG_CHAR_ENUM, 0x37 },
            { global::Opcodes.SMSG_CHAR_ENUM, 0x3B },
            { global::Opcodes.CMSG_PING, 0x1DC },
            { global::Opcodes.SMSG_PONG, 0x1DD },
            { global::Opcodes.CMSG_PLAYER_LOGIN, 0x3D },
            { global::Opcodes.SMSG_UPDATE_OBJECT, 0xA9 },
            { global::Opcodes.CMSG_NAME_QUERY, 0x50 },
            { global::Opcodes.SMSG_NAME_QUERY_RESPONSE, 0x51 },
            { global::Opcodes.CMSG_LOGOUT_REQUEST, 0x4B },
            { global::Opcodes.SMSG_LOGOUT_COMPLETE, 0x4D },
            { global::Opcodes.CMSG_WORLD_TELEPORT, 0x8 },
            { global::Opcodes.SMSG_NEW_WORLD, 0x3E },
            { global::Opcodes.SMSG_TRANSFER_PENDING, 0x3F },
            { global::Opcodes.MSG_MOVE_START_FORWARD, 0xB5 },
            { global::Opcodes.MSG_MOVE_START_BACKWARD, 0xB6 },
            { global::Opcodes.MSG_MOVE_STOP, 0xB7 },
            { global::Opcodes.MSG_MOVE_START_STRAFE_LEFT, 0xB8 },
            { global::Opcodes.MSG_MOVE_START_STRAFE_RIGHT, 0xB9 },
            { global::Opcodes.MSG_MOVE_STOP_STRAFE, 0xBA },
            { global::Opcodes.MSG_MOVE_JUMP, 0xBB },
            { global::Opcodes.MSG_MOVE_START_TURN_LEFT, 0xBC },
            { global::Opcodes.MSG_MOVE_START_TURN_RIGHT, 0xBD },
            { global::Opcodes.MSG_MOVE_STOP_TURN, 0xBE },
            { global::Opcodes.MSG_MOVE_START_PITCH_UP, 0xBF },
            { global::Opcodes.MSG_MOVE_START_PITCH_DOWN, 0xC0 },
            { global::Opcodes.MSG_MOVE_STOP_PITCH, 0xC1 },
            { global::Opcodes.MSG_MOVE_SET_RUN_MODE, 0xC2 },
            { global::Opcodes.MSG_MOVE_SET_WALK_MODE, 0xC3 },
            { global::Opcodes.MSG_MOVE_START_SWIM, 0xCA },
            { global::Opcodes.MSG_MOVE_STOP_SWIM, 0xCB },
            { global::Opcodes.MSG_MOVE_SET_FACING, 0xDA },
            { global::Opcodes.MSG_MOVE_SET_PITCH, 0xDB },
            { global::Opcodes.MSG_MOVE_ROOT, 0xEC },
            { global::Opcodes.MSG_MOVE_UNROOT, 0xED },
            { global::Opcodes.MSG_MOVE_HEARTBEAT, 0xEE },
            { global::Opcodes.MSG_MOVE_WORLDPORT_ACK, 0xDC },
            { global::Opcodes.SMSG_MESSAGECHAT, 0x96 },
            { global::Opcodes.CMSG_MESSAGECHAT, 0x95 },
            { global::Opcodes.SMSG_FORCE_SPEED_CHANGE, 0xE2 },
            { global::Opcodes.SMSG_FORCE_SWIM_SPEED_CHANGE, 0xE6 },
            { global::Opcodes.SMSG_TUTORIAL_FLAGS, 0xFD },
            { global::Opcodes.CMSG_QUERY_TIME, 0x1CE },
            { global::Opcodes.SMSG_LOGIN_SETTIMESPEED, 0x42 },
            { global::Opcodes.SMSG_ACCOUNT_DATA_MD5, 0x209 },
            { global::Opcodes.CMSG_STANDSTATECHANGE, 0x101 },
            { global::Opcodes.MSG_MOVE_TELEPORT_ACK, 0xC7 },
            { global::Opcodes.CMSG_AREATRIGGER, 0xB4 },
            { global::Opcodes.CMSG_ZONEUPDATE, 0x1F4 },
            { global::Opcodes.CMSG_UPDATE_ACCOUNT_DATA, 0x20B },
            { global::Opcodes.SMSG_UPDATE_ACCOUNT_DATA, 0x20C },
            { global::Opcodes.SMSG_LOGIN_VERIFY_WORLD, 0x236 },
            { global::Opcodes.SMSG_EMOTE, 0x103 },
            { global::Opcodes.CMSG_TEXT_EMOTE, 0x104 },
            { global::Opcodes.SMSG_TEXT_EMOTE, 0x105 },
        };

        public uint this[global::Opcodes opcode] => opcodes[opcode];

        public global::Opcodes this[uint opcode] => opcodes.First(x => x.Value == opcode).Key;

        public bool OpcodeExists(uint opcode) => opcodes.Any(x => x.Value == opcode);
    }
}
