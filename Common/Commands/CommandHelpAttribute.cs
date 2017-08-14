using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Commands
{
    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
    public class CommandHelpAttribute : Attribute
    {
        public string HelpText { get; set; }

        public CommandHelpAttribute(string helptext)
        {
            this.HelpText = helptext;
        }

        public override string ToString()
        {
            return HelpText;
        }
    }
}
