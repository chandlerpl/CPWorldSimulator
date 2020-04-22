using CP.Common.Commands;
using System;
using System.Collections.Generic;
using System.Text;

namespace CP.Tests
{
    class TestCommand : CPCommand
    {
        public override bool Init()
        {
            Name = "Test";
            Desc = "This is a test command!";
            Aliases = new List<string>() { "test" };
            ProperUse = "test";

            return true;
        }

        public override bool Execute(object obj, List<string> args)
        {
            if (args.Count > 1)
                return false;

            Console.WriteLine("This is a simple Command test.");
            return true;
        }
    }
}
