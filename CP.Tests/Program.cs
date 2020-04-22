using CP.Common.Commands;
using System;

namespace CP.Tests
{
    class Program
    {
        static Command CommandSystem;

        static void Main(string[] args)
        {
            CommandSystem = new Command();

            while(true)
            {
                Console.Write("Please enter a command: ");
                CommandSystem.commandInterface(null, Console.ReadLine().Split(' '));
            }
        }
    }
}
