using Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alpha_3494
{
	public class Opcodes : IOpcodes
	{
		readonly IDictionary<global::Opcodes, uint> opcodes = new Dictionary<global::Opcodes, uint>()
		{
			{ global::Opcodes.SMSG_AUTH_CHALLENGE, 0x01DE },
			{ global::Opcodes.SMSG_AUTH_RESPONSE, 0x01E0 },
			{ global::Opcodes.CMSG_AUTH_SESSION, 0x01DF },
			{ global::Opcodes.CMSG_CHAR_CREATE, 0x036 },
			{ global::Opcodes.SMSG_CHAR_CREATE, 0x03A },
			{ global::Opcodes.CMSG_CHAR_DELETE, 0x038 },
			{ global::Opcodes.SMSG_CHAR_DELETE, 0x03C },
			{ global::Opcodes.CMSG_CHAR_ENUM, 0x037 },
			{ global::Opcodes.SMSG_CHAR_ENUM, 0x03B },
			{ global::Opcodes.CMSG_PING, 0x1CD },
			{ global::Opcodes.SMSG_PONG, 0x1CE },
			{ global::Opcodes.CMSG_PLAYER_LOGIN, 0x03D },
			{ global::Opcodes.SMSG_UPDATE_OBJECT, 0x0A9 },
			{ global::Opcodes.CMSG_NAME_QUERY, 0x050 },
			{ global::Opcodes.SMSG_NAME_QUERY_RESPONSE, 0x051 },
			{ global::Opcodes.CMSG_LOGOUT_REQUEST, 0x04B },
			{ global::Opcodes.SMSG_LOGOUT_COMPLETE, 0x04D },
			{ global::Opcodes.CMSG_WORLD_TELEPORT, 0x008 },
			{ global::Opcodes.SMSG_NEW_WORLD, 0x03E },
			{ global::Opcodes.SMSG_TRANSFER_PENDING, 0x003F },
			{ global::Opcodes.MSG_MOVE_START_FORWARD, 0x0B5 },
			{ global::Opcodes.MSG_MOVE_START_BACKWARD, 0x0B6 },
			{ global::Opcodes.MSG_MOVE_STOP, 0x0B7 },
			{ global::Opcodes.MSG_MOVE_START_STRAFE_LEFT, 0x0B8 },
			{ global::Opcodes.MSG_MOVE_START_STRAFE_RIGHT, 0x0B9 },
			{ global::Opcodes.MSG_MOVE_STOP_STRAFE, 0x0BA },
			{ global::Opcodes.MSG_MOVE_JUMP, 0x0BB },
			{ global::Opcodes.MSG_MOVE_START_TURN_LEFT, 0x0BC },
			{ global::Opcodes.MSG_MOVE_START_TURN_RIGHT, 0x0BD },
			{ global::Opcodes.MSG_MOVE_STOP_TURN, 0x0BE },
			{ global::Opcodes.MSG_MOVE_START_PITCH_UP, 0x0BF },
			{ global::Opcodes.MSG_MOVE_START_PITCH_DOWN, 0x0C0 },
			{ global::Opcodes.MSG_MOVE_STOP_PITCH, 0x0C1 },
			{ global::Opcodes.MSG_MOVE_SET_RUN_MODE, 0x0C2 },
			{ global::Opcodes.MSG_MOVE_SET_WALK_MODE, 0x0C3 },
			{ global::Opcodes.MOVE_COLLIDE_REDIRECT, 0x0C9 },
			{ global::Opcodes.MOVE_COLLIDE_STUCK, 0x0CA },
			{ global::Opcodes.MSG_MOVE_START_SWIM, 0x0CB },
			{ global::Opcodes.MSG_MOVE_STOP_SWIM, 0x0CC },
			{ global::Opcodes.MSG_MOVE_SET_FACING, 0x0D7 },
			{ global::Opcodes.MSG_MOVE_SET_PITCH, 0x0D8 },
			{ global::Opcodes.MSG_MOVE_ROOT, 0x0E7 },
			{ global::Opcodes.MSG_MOVE_UNROOT, 0x0E8 },
			{ global::Opcodes.MSG_MOVE_HEARTBEAT, 0x0E9 },
			{ global::Opcodes.SMSG_MOVE_WORLDPORT_ACK, 0x0C7 },
			{ global::Opcodes.MSG_MOVE_WORLDPORT_ACK, 0x0D9 },
			{ global::Opcodes.SMSG_MESSAGECHAT, 0x096 },
			{ global::Opcodes.CMSG_MESSAGECHAT, 0x095 },
			{ global::Opcodes.SMSG_FORCE_SPEED_CHANGE, 0x0DF },
			{ global::Opcodes.SMSG_FORCE_SWIM_SPEED_CHANGE, 0x0E1 },
			{ global::Opcodes.SMSG_TUTORIAL_FLAGS,  0xF0 },
			{ global::Opcodes.SMSG_UI_CONFIG_MD5,  0x1FB },
			{ global::Opcodes.CMSG_QUERY_TIME, 0x1BF },
			{ global::Opcodes.SMSG_LOGIN_SETTIMESPEED, 0x42 },
			{ global::Opcodes.CMSG_STANDSTATECHANGE, 0xF4 },
			{ global::Opcodes.CMSG_AREATRIGGER, 0xB4 },
			{ global::Opcodes.CMSG_ZONEUPDATE, 0x01E6 },
			{ global::Opcodes.SMSG_EMOTE, 0x00F6 },
			{ global::Opcodes.CMSG_TEXT_EMOTE, 0x00F7 },
			{ global::Opcodes.SMSG_TEXT_EMOTE, 0x00F8 },
		};

		public uint this[global::Opcodes opcode] => opcodes[opcode];

		public global::Opcodes this[uint opcode] => opcodes.First(x => x.Value == opcode).Key;

		public bool OpcodeExists(uint opcode) => opcodes.Any(x => x.Value == opcode);
	}
}
