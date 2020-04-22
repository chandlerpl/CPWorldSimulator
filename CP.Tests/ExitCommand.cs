using CP.Common.Commands;
using System;
using System.Collections.Generic;
using System.Text;

namespace CP.Tests
{
    class ExitCommand : CPCommand
    {
        public override bool Init()
        {
            Name = "Exit";
            Desc = "Closes the Application";
            Aliases = new List<string>() { "exit" };
            ProperUse = "exit";

            return true;
        }

        public override bool Execute(object obj, List<string> args)
        {
            Environment.Exit(0);

            return true;
        }
    }
}
