using CP.Common.Commands;
using System;
using System.Collections.Generic;
using System.Text;

namespace CP.Tests
{
    class HelpCommand : CPCommand
    {
        public override bool Init()
        {
            Name = "Help";
            Desc = "Shows all available commands.";
            Aliases = new List<string>() { "help", "h" };
            ProperUse = "help";

            return true;
        }

        public override bool Execute(object obj, List<string> args)
        {
			if (args == null || args.Count == 1)
			{
				StringBuilder message = new StringBuilder();

				foreach (CPCommand command in Command.Commands.Values)
				{
					message.Append(command.Name);
					message.Append(" - ");
					message.Append(command.Desc);
					message.Append("\n");
				}

				Console.WriteLine(message);
			} else
			{
				string arg = args[1];
				if(Command.Commands.ContainsKey(arg.ToLower()))
				{
					Console.WriteLine(Command.Commands[arg.ToLower()]);
				}
			}

			return true;
		}
    }
}
