using System;
using System.Configuration;
using Common.Cryptography;
using Common.Logging;
using WorldServer.Network;
using WorldServer.Packets;
using WorldServer.Plugins;

namespace WorldServer
{
    internal class WorldServer
    {
        public static SandboxHost Sandbox;

        private static void Main()
        {
            Log.Message(LogType.INIT, "                AIO SANDBOX                ");
            Log.Message(LogType.INIT, "             REALM/PROXY/WORLD             ");
            Log.Message();
            Log.Message(LogType.NORMAL, "Starting AIO Sandbox WorldServer...");
            Log.Message();

            // Load Plugins
            PluginHandler.GetPlugins();
            Sandbox = PluginHandler.SandboxSelector();

            RealmManager.RealmSession = new RealmSocket();
            WorldManager.WorldSession = new WorldSocket();

            if (WorldManager.WorldSession.Start() && RealmManager.RealmSession.Start())
            {
                RealmManager.RealmSession.StartRealmThread();
                RealmManager.RealmSession.StartProxyThread();
                WorldManager.WorldSession.StartConnectionThread();

                Log.Message();
                Log.Message(LogType.NORMAL, "Loading {0}", Sandbox.RealmName);
                Log.Message(LogType.NORMAL, "RealmProxy listening on {0} port(s) {1}.", "127.0.0.1", Sandbox.RealmPort);
                Log.Message(LogType.NORMAL, "RedirectServer listening on {0} port {1}.", "127.0.0.1", Sandbox.RedirectPort);
                Log.Message(LogType.NORMAL, "WorldServer listening on {0} port {1}.", "127.0.0.1", Sandbox.WorldPort);
                Log.Message(LogType.NORMAL, "Started {0}", Sandbox.RealmName);
                Log.Message();

                ClientAuth.Password = ConfigurationManager.AppSettings["Password"];
                if (!string.IsNullOrEmpty(ClientAuth.Password))
                {
                    Log.Message(LogType.NORMAL, "Default client password set to \"{0}\"", ClientAuth.Password);
                    Log.Message();
                }

                HandlerDefinitions.InitializePacketHandler();
            }
            else
            {
                if (!WorldManager.WorldSession.Started)
                    Log.Message(LogType.ERROR, "WorldServer couldn't be started.");
                if (!RealmManager.RealmSession.Started)
                    Log.Message(LogType.ERROR, "RealmServer couldn't be started.");

                Log.Message();
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }
}
