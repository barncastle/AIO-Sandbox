using System.Collections.Generic;
using System.Linq;
using Common.Interfaces;

namespace MoP_15464
{
    public class Opcodes : IOpcodes
    {
        private readonly IDictionary<global::Opcodes, uint> opcodes = new Dictionary<global::Opcodes, uint>()
        {
              // <!-- verified
              { global::Opcodes.SMSG_AUTH_CHALLENGE, 0xF94 },
              { global::Opcodes.SMSG_AUTH_RESPONSE, 0xB27 },
              { global::Opcodes.CMSG_AUTH_SESSION, 0x737 },
              { global::Opcodes.CMSG_CHAR_CREATE, 0x1D2 },
              { global::Opcodes.SMSG_CHAR_CREATE, 0x874 },
              { global::Opcodes.CMSG_CHAR_DELETE, 0x9F7 },
              { global::Opcodes.SMSG_CHAR_DELETE, 0x2D7 },
              { global::Opcodes.CMSG_CHAR_ENUM, 0xCFF },
              { global::Opcodes.SMSG_CHAR_ENUM, 0x824 },
              { global::Opcodes.CMSG_PING, 0x5B6 },
              { global::Opcodes.SMSG_PONG, 0xFB4 },
              { global::Opcodes.CMSG_PLAYER_LOGIN, 0x7D2 },
              { global::Opcodes.SMSG_UPDATE_OBJECT, 0x150 },
              { global::Opcodes.CMSG_NAME_QUERY, 0x9F4 },
              { global::Opcodes.SMSG_ADDON_INFO, 0xF4 },
              { global::Opcodes.MSG_VERIFY_CONNECTIVITY, 0x4F57 },
              { global::Opcodes.SMSG_TUTORIAL_FLAGS, 0x376 },
              { global::Opcodes.CMSG_UPDATE_ACCOUNT_DATA, 0xB5E },
              { global::Opcodes.CMSG_QUERY_TIME, 0x367 },
              { global::Opcodes.SMSG_ACCOUNT_DATA_MD5, 0x0172 },
              { global::Opcodes.SMSG_LOGIN_VERIFY_WORLD, 0x0147 },
              { global::Opcodes.SMSG_LOGIN_SETTIMESPEED, 0x0B64 },
              { global::Opcodes.SMSG_INITIAL_SPELLS, 0x01F2 },
              { global::Opcodes.CMSG_MESSAGECHAT, 0xC2F },
              { global::Opcodes.SMSG_MESSAGECHAT, 0xB42 },
              { global::Opcodes.SMSG_NEW_WORLD, 0x7E },
              { global::Opcodes.SMSG_TRANSFER_PENDING, 0x82C },
              { global::Opcodes.SMSG_TIME_SYNC_REQ, 0x9A7 },
              { global::Opcodes.SMSG_UPDATE_ACCOUNT_DATA, 0x0172 },
              { global::Opcodes.SMSG_NAME_QUERY_RESPONSE, 0x9D9 },
              { global::Opcodes.SMSG_MOVE_SET_CAN_FLY, 0x128 },
              { global::Opcodes.SMSG_MOVE_UNSET_CAN_FLY, 0x18A },
              { global::Opcodes.SMSG_FORCE_SPEED_CHANGE, 0x396 },
              { global::Opcodes.SMSG_FORCE_FLIGHT_SPEED_CHANGE , 0xAF },
              { global::Opcodes.SMSG_FORCE_SWIM_SPEED_CHANGE, 0x91B },
              { global::Opcodes.MSG_MOVE_TELEPORT_ACK, 0x907 },
              // -->

              // TODO!
              { global::Opcodes.MSG_MOVE_WORLDPORT_ACK, 0xD20A },             
              { global::Opcodes.CMSG_LOGOUT_REQUEST, 0x1424 },
              { global::Opcodes.SMSG_LOGOUT_COMPLETE, 0xD270 },              
              { global::Opcodes.CMSG_STANDSTATECHANGE, 0x448 },
              { global::Opcodes.CMSG_AREATRIGGER, 0xFFFF },
              { global::Opcodes.CMSG_ZONEUPDATE, 0x9FE7 },
              { global::Opcodes.SMSG_EMOTE, 0x1438 },
              { global::Opcodes.CMSG_TEXT_EMOTE, 0x9D68 },
              { global::Opcodes.SMSG_TEXT_EMOTE, 0x2B61 },
              { global::Opcodes.MSG_MOVE_START_ASCEND, 0x905C },
              { global::Opcodes.MSG_MOVE_STOP_ASCEND, 0x1254 },
              { global::Opcodes.MSG_MOVE_START_FORWARD, 0x860E },
              { global::Opcodes.MSG_MOVE_START_BACKWARD, 0x8612 },
              { global::Opcodes.MSG_MOVE_STOP, 0x927C },
              { global::Opcodes.MSG_MOVE_START_STRAFE_LEFT, 0x8254 },
              { global::Opcodes.MSG_MOVE_START_STRAFE_RIGHT, 0x8272 },
              { global::Opcodes.MSG_MOVE_STOP_STRAFE, 0x443E },
              { global::Opcodes.MSG_MOVE_JUMP, 0x9F6A },
              { global::Opcodes.MSG_MOVE_START_TURN_LEFT, 0x420C },
              { global::Opcodes.MSG_MOVE_START_TURN_RIGHT, 0x650 },
              { global::Opcodes.MSG_MOVE_STOP_TURN, 0x8424 },
              { global::Opcodes.MSG_MOVE_START_PITCH_UP, 0x4012 },
              { global::Opcodes.MSG_MOVE_START_PITCH_DOWN, 0xC212 },
              { global::Opcodes.MSG_MOVE_STOP_PITCH, 0xC624 },
              { global::Opcodes.MSG_MOVE_SET_RUN_MODE, 0xD65C },
              { global::Opcodes.MSG_MOVE_SET_WALK_MODE, 0x4462 },
              { global::Opcodes.MSG_MOVE_START_SWIM, 0x9644 },
              { global::Opcodes.MSG_MOVE_STOP_SWIM, 0x5210 },
              { global::Opcodes.MSG_MOVE_FALL_LAND, 0x9068 },
              { global::Opcodes.MSG_MOVE_SET_FACING, 0x902C },
              { global::Opcodes.MSG_MOVE_SET_PITCH, 0x0006 },
              { global::Opcodes.MSG_MOVE_ROOT, 0x501C },
              { global::Opcodes.MSG_MOVE_UNROOT, 0x9644 },
              { global::Opcodes.MSG_MOVE_HEARTBEAT, 0x8672 },
        };

        public uint this[global::Opcodes opcode] => opcodes[opcode];

        public global::Opcodes this[uint opcode] => opcodes.First(x => x.Value == opcode).Key;

        public bool OpcodeExists(uint opcode) => opcodes.Any(x => x.Value == opcode);
    }
}
