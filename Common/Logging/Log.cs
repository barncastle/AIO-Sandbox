using System;

namespace Common.Logging
{
    public class Log
    {
        public static void Message()
        {
            SetLogger(LogType.DEFAULT, "");
        }

        public static void Message(LogType type, string text, params object[] args)
        {
            SetLogger(type, text, args);
        }

        public static void SetType(LogType type)
        {
            switch (type)
            {
                case LogType.NORMAL:
                    Console.ForegroundColor = ConsoleColor.Green;
                    break;

                case LogType.ERROR:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;

                case LogType.DUMP:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;

                case LogType.INIT:
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    break;

                case LogType.MISC:
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;

                case LogType.CMD:
                    Console.ForegroundColor = ConsoleColor.Green;
                    break;

                case LogType.DEBUG:
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    break;

                default:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
            }
        }

        private static void SetLogger(LogType type, string text, params object[] args)
        {
            if (type != LogType.DEFAULT && string.IsNullOrWhiteSpace(text))
                return;

            SetType(type);

            switch (type)
            {
                case LogType.NORMAL:
                    text = text.Insert(0, "System: [" + DateTime.Now.ToLongTimeString() + "] ");
                    break;

                case LogType.ERROR:
                    text = text.Insert(0, "Error: [" + DateTime.Now.ToLongTimeString() + "] ");
                    break;

                case LogType.MISC:
                    text = text.Insert(0, "[" + DateTime.Now.ToLongTimeString() + "] ");
                    break;
            }

            Console.WriteLine(text, args);
        }
    }

    public enum LogType
    {
        NORMAL = 0,
        ERROR = 1,
        DUMP = 2,
        INIT = 3,
        MISC = 4,
        CMD = 5,
        DEBUG = 6,
        DEFAULT = 0xFF,
    }
}
