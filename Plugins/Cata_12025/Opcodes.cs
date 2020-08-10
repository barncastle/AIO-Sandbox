using System.Collections.Generic;
using System.Linq;
using Common.Interfaces;

namespace Cata_12025
{
    public class Opcodes : IOpcodes
    {
        private readonly IDictionary<global::Opcodes, uint> opcodes = new Dictionary<global::Opcodes, uint>()
        {
            { global::Opcodes.SMSG_AUTH_CHALLENGE, 0x1DE3 },
            { global::Opcodes.SMSG_AUTH_RESPONSE, 0x0FEF },
            { global::Opcodes.CMSG_AUTH_SESSION, 0x99EC },
            { global::Opcodes.CMSG_CHAR_CREATE, 0x7F61 },
            { global::Opcodes.SMSG_CHAR_CREATE, 0x9DEF },
            { global::Opcodes.CMSG_CHAR_DELETE, 0x4BE6 },
            { global::Opcodes.SMSG_CHAR_DELETE, 0x7DED },
            { global::Opcodes.CMSG_CHAR_ENUM, 0x0D65 },
            { global::Opcodes.SMSG_CHAR_ENUM, 0xFD67 },
            { global::Opcodes.CMSG_PING, 0x7FE5 },
            { global::Opcodes.SMSG_PONG, 0xDD61 },
            { global::Opcodes.CMSG_PLAYER_LOGIN, 0x8BEC },
            { global::Opcodes.SMSG_UPDATE_OBJECT, 0x3DE2 },
            { global::Opcodes.CMSG_NAME_QUERY, 0x0BE5 },
            { global::Opcodes.SMSG_NAME_QUERY_RESPONSE, 0x1964 },
            { global::Opcodes.CMSG_LOGOUT_REQUEST, 0xDB67 },
            { global::Opcodes.SMSG_LOGOUT_COMPLETE, 0x8F61 },
            { global::Opcodes.SMSG_NEW_WORLD, 0x2D62 },
            { global::Opcodes.SMSG_TRANSFER_PENDING, 0x29EC },
            { global::Opcodes.MSG_MOVE_START_FORWARD, 0x1FE8 },
            { global::Opcodes.MSG_MOVE_START_BACKWARD, 0x2FE1 },
            { global::Opcodes.MSG_MOVE_STOP, 0xFD6F },
            { global::Opcodes.MSG_MOVE_START_STRAFE_LEFT, 0x39E7 },
            { global::Opcodes.MSG_MOVE_START_STRAFE_RIGHT, 0x9FE5 },
            { global::Opcodes.MSG_MOVE_STOP_STRAFE, 0x1DEE },
            { global::Opcodes.MSG_MOVE_JUMP, 0x9F6A },
            { global::Opcodes.MSG_MOVE_START_TURN_LEFT, 0x4D69 },
            { global::Opcodes.MSG_MOVE_START_TURN_RIGHT, 0x5DEA },
            { global::Opcodes.MSG_MOVE_STOP_TURN, 0xEFE6 },
            { global::Opcodes.MSG_MOVE_START_PITCH_UP, 0xA9E2 },
            { global::Opcodes.MSG_MOVE_START_PITCH_DOWN, 0xFD63 },
            { global::Opcodes.MSG_MOVE_STOP_PITCH, 0x796A },
            { global::Opcodes.MSG_MOVE_SET_RUN_MODE, 0x7DE1 },
            { global::Opcodes.MSG_MOVE_SET_WALK_MODE, 0x2FE7 },
            { global::Opcodes.MSG_MOVE_START_SWIM, 0x7FEF },
            { global::Opcodes.MSG_MOVE_STOP_SWIM, 0xEDE3 },
            { global::Opcodes.MSG_MOVE_FALL_LAND, 0x6BE7 },
            { global::Opcodes.MSG_MOVE_SET_FACING, 0xEF6C },
            { global::Opcodes.MSG_MOVE_SET_PITCH, 0x2D6A },
            { global::Opcodes.MSG_MOVE_ROOT, 0xDF67 },
            { global::Opcodes.MSG_MOVE_UNROOT, 0x2FEC },
            { global::Opcodes.MSG_MOVE_HEARTBEAT, 0xAD6E },
            { global::Opcodes.MSG_MOVE_WORLDPORT_ACK, 0xCF64 },
            { global::Opcodes.SMSG_MESSAGECHAT, 0x9B63 },
            { global::Opcodes.CMSG_MESSAGECHAT, 0x69ED },
            { global::Opcodes.SMSG_FORCE_SPEED_CHANGE, 0x9968 },
            { global::Opcodes.SMSG_FORCE_SWIM_SPEED_CHANGE, 0x0B60 },
            { global::Opcodes.SMSG_TUTORIAL_FLAGS, 0x5D62 },
            { global::Opcodes.CMSG_QUERY_TIME, 0xF967 },
            { global::Opcodes.SMSG_LOGIN_SETTIMESPEED, 0xBF66 },
            { global::Opcodes.SMSG_ACCOUNT_DATA_MD5, 0x2B6D },
            { global::Opcodes.CMSG_STANDSTATECHANGE, 0x1960 },
            { global::Opcodes.MSG_MOVE_TELEPORT_ACK, 0xB9E7 },
            { global::Opcodes.CMSG_AREATRIGGER, 0x7969 },
            { global::Opcodes.CMSG_ZONEUPDATE, 0x9FE7 },
            { global::Opcodes.CMSG_UPDATE_ACCOUNT_DATA, 0x3D6C },
            { global::Opcodes.SMSG_UPDATE_ACCOUNT_DATA, 0x5B67 },
            { global::Opcodes.SMSG_LOGIN_VERIFY_WORLD, 0xF96E },
            { global::Opcodes.SMSG_EMOTE, 0xAFEA },
            { global::Opcodes.CMSG_TEXT_EMOTE, 0x9D68 },
            { global::Opcodes.SMSG_TEXT_EMOTE, 0x2B61 },
            { global::Opcodes.SMSG_FORCE_FLIGHT_SPEED_CHANGE , 0x09E6 },
            { global::Opcodes.SMSG_MOVE_SET_CAN_FLY, 0xE965 },
            { global::Opcodes.SMSG_MOVE_UNSET_CAN_FLY, 0xCDE8 },
            { global::Opcodes.MSG_MOVE_START_ASCEND, 0x8B61 },
            { global::Opcodes.MSG_MOVE_STOP_ASCEND, 0xDBE3 },
            { global::Opcodes.SMSG_TIME_SYNC_REQ, 0x9D6A },
            { global::Opcodes.SMSG_INITIAL_SPELLS, 0xEB63 },
            { global::Opcodes.SMSG_ADDON_INFO, 0x3B63 },
        };

        public uint this[global::Opcodes opcode] => opcodes[opcode];

        public global::Opcodes this[uint opcode] => opcodes.First(x => x.Value == opcode).Key;

        public bool OpcodeExists(uint opcode) => opcodes.Any(x => x.Value == opcode);
    }
}
