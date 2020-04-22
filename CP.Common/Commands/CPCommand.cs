using System.Collections.Generic;
using System.Text;

namespace CP.Common.Commands
{
    public abstract class CPCommand
    {
        public string Name { get; protected set; }
        public string ProperUse { get; protected set; }
        public string Desc { get; protected set; }
        public List<string> Aliases { get; protected set; }

        public CPCommand()
        {
            Init();
        }

        public abstract bool Init();

        public abstract bool Execute(object obj, List<string> args);

        public override string ToString()
        {
            StringBuilder message = new StringBuilder();
            message.Append("Help for: ");
            message.Append(Name);
            message.Append("\nProper Use: ");
            message.Append(ProperUse);
            message.Append("\nDescription: ");
            message.Append(Desc);
            message.Append("\nAlternatives: ");
            message.Append(Aliases.ToArray().ToString());

            return message.ToString();
        }
    }
}