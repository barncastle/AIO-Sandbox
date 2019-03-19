using System;

namespace Common.Commands
{
    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
    public class CommandHelpAttribute : Attribute
    {
        public readonly string HelpText;

        public CommandHelpAttribute(string helptext) => HelpText = helptext;

        public override string ToString()
        {
            return HelpText;
        }
    }
}
