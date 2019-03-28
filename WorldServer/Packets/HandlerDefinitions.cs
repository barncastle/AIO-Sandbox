using Common.Network;

namespace WorldServer.Packets
{
    public class HandlerDefinitions
    {
        public static void InitializePacketHandler()
        {
            PacketManager.DefineOpcodeHandler(Opcodes.CMSG_AUTH_SESSION, WorldServer.Sandbox.AuthHandler.HandleAuthSession);
            PacketManager.DefineOpcodeHandler(Opcodes.CMSG_LOGOUT_REQUEST, WorldServer.Sandbox.AuthHandler.HandleLogoutRequest);
            PacketManager.DefineOpcodeHandler(Opcodes.CMSG_CHAR_ENUM, WorldServer.Sandbox.CharHandler.HandleCharEnum);
            PacketManager.DefineOpcodeHandler(Opcodes.CMSG_CHAR_CREATE, WorldServer.Sandbox.CharHandler.HandleCharCreate);
            PacketManager.DefineOpcodeHandler(Opcodes.CMSG_CHAR_DELETE, WorldServer.Sandbox.CharHandler.HandleCharDelete);
            PacketManager.DefineOpcodeHandler(Opcodes.CMSG_PING, WorldServer.Sandbox.WorldHandler.HandlePing);
            PacketManager.DefineOpcodeHandler(Opcodes.CMSG_PLAYER_LOGIN, WorldServer.Sandbox.WorldHandler.HandlePlayerLogin);
            PacketManager.DefineOpcodeHandler(Opcodes.CMSG_QUERY_TIME, WorldServer.Sandbox.WorldHandler.HandleQueryTime);
            PacketManager.DefineOpcodeHandler(Opcodes.CMSG_NAME_QUERY, WorldServer.Sandbox.CharHandler.HandleNameCache);

            PacketManager.DefineOpcodeHandler(Opcodes.CMSG_WORLD_TELEPORT, WorldServer.Sandbox.WorldHandler.HandleWorldTeleport);
            PacketManager.DefineOpcodeHandler(Opcodes.CMSG_AREATRIGGER, WorldServer.Sandbox.WorldHandler.HandleAreaTrigger);
            PacketManager.DefineOpcodeHandler(Opcodes.CMSG_ZONEUPDATE, WorldServer.Sandbox.WorldHandler.HandleZoneUpdate);

            PacketManager.DefineOpcodeHandler(Opcodes.CMSG_MESSAGECHAT, WorldServer.Sandbox.CharHandler.HandleMessageChat);
            PacketManager.DefineOpcodeHandler(Opcodes.CMSG_TEXT_EMOTE, WorldServer.Sandbox.CharHandler.HandleTextEmote);

            PacketManager.DefineOpcodeHandler(Opcodes.MSG_MOVE_START_FORWARD, WorldServer.Sandbox.CharHandler.HandleMovementStatus);
            PacketManager.DefineOpcodeHandler(Opcodes.MSG_MOVE_START_BACKWARD, WorldServer.Sandbox.CharHandler.HandleMovementStatus);
            PacketManager.DefineOpcodeHandler(Opcodes.MSG_MOVE_STOP, WorldServer.Sandbox.CharHandler.HandleMovementStatus);
            PacketManager.DefineOpcodeHandler(Opcodes.MSG_MOVE_START_STRAFE_LEFT, WorldServer.Sandbox.CharHandler.HandleMovementStatus);
            PacketManager.DefineOpcodeHandler(Opcodes.MSG_MOVE_START_STRAFE_RIGHT, WorldServer.Sandbox.CharHandler.HandleMovementStatus);
            PacketManager.DefineOpcodeHandler(Opcodes.MSG_MOVE_STOP_STRAFE, WorldServer.Sandbox.CharHandler.HandleMovementStatus);
            PacketManager.DefineOpcodeHandler(Opcodes.MSG_MOVE_JUMP, WorldServer.Sandbox.CharHandler.HandleMovementStatus);
            PacketManager.DefineOpcodeHandler(Opcodes.MSG_MOVE_START_TURN_LEFT, WorldServer.Sandbox.CharHandler.HandleMovementStatus);
            PacketManager.DefineOpcodeHandler(Opcodes.MSG_MOVE_START_TURN_RIGHT, WorldServer.Sandbox.CharHandler.HandleMovementStatus);
            PacketManager.DefineOpcodeHandler(Opcodes.MSG_MOVE_STOP_TURN, WorldServer.Sandbox.CharHandler.HandleMovementStatus);
            PacketManager.DefineOpcodeHandler(Opcodes.MSG_MOVE_START_PITCH_UP, WorldServer.Sandbox.CharHandler.HandleMovementStatus);
            PacketManager.DefineOpcodeHandler(Opcodes.MSG_MOVE_START_PITCH_DOWN, WorldServer.Sandbox.CharHandler.HandleMovementStatus);
            PacketManager.DefineOpcodeHandler(Opcodes.MSG_MOVE_STOP_PITCH, WorldServer.Sandbox.CharHandler.HandleMovementStatus);
            PacketManager.DefineOpcodeHandler(Opcodes.MSG_MOVE_SET_RUN_MODE, WorldServer.Sandbox.CharHandler.HandleMovementStatus);
            PacketManager.DefineOpcodeHandler(Opcodes.MSG_MOVE_SET_WALK_MODE, WorldServer.Sandbox.CharHandler.HandleMovementStatus);
            PacketManager.DefineOpcodeHandler(Opcodes.MSG_MOVE_START_SWIM, WorldServer.Sandbox.CharHandler.HandleMovementStatus);
            PacketManager.DefineOpcodeHandler(Opcodes.MSG_MOVE_STOP_SWIM, WorldServer.Sandbox.CharHandler.HandleMovementStatus);
            PacketManager.DefineOpcodeHandler(Opcodes.MSG_MOVE_SET_FACING, WorldServer.Sandbox.CharHandler.HandleMovementStatus);
            PacketManager.DefineOpcodeHandler(Opcodes.MSG_MOVE_SET_PITCH, WorldServer.Sandbox.CharHandler.HandleMovementStatus);
            PacketManager.DefineOpcodeHandler(Opcodes.MSG_MOVE_ROOT, WorldServer.Sandbox.CharHandler.HandleMovementStatus);
            PacketManager.DefineOpcodeHandler(Opcodes.MSG_MOVE_UNROOT, WorldServer.Sandbox.CharHandler.HandleMovementStatus);
            PacketManager.DefineOpcodeHandler(Opcodes.MSG_MOVE_HEARTBEAT, WorldServer.Sandbox.CharHandler.HandleMovementStatus);
            PacketManager.DefineOpcodeHandler(Opcodes.MSG_MOVE_FALL_LAND, WorldServer.Sandbox.CharHandler.HandleMovementStatus);
            PacketManager.DefineOpcodeHandler(Opcodes.MSG_MOVE_START_ASCEND, WorldServer.Sandbox.CharHandler.HandleMovementStatus);
            PacketManager.DefineOpcodeHandler(Opcodes.MSG_MOVE_STOP_ASCEND, WorldServer.Sandbox.CharHandler.HandleMovementStatus);

            PacketManager.DefineOpcodeHandler(Opcodes.CMSG_STANDSTATECHANGE, WorldServer.Sandbox.CharHandler.HandleStandState);
        }
    }
}
